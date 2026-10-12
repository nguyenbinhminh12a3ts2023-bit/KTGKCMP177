using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using aspnetboilerplate.com.Toeic.Dto;

namespace aspnetboilerplate.com.Toeic.Services
{
    public interface IToeicDocxParserService
    {
        DocxValidationResultDto ParseAndValidate(Stream docxStream);
        DocxValidationResultDto ParseAndValidate(string filePath);
    }

    public class ToeicDocxParserService : IToeicDocxParserService, Abp.Dependency.ITransientDependency
    {
        private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        public DocxValidationResultDto ParseAndValidate(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return ParseAndValidate(stream);
            }
        }

        public DocxValidationResultDto ParseAndValidate(Stream docxStream)
        {
            var result = new DocxValidationResultDto
            {
                DurationMinutes = 75,
                TotalQuestions = 100,
                Errors = new List<FormatErrorDto>()
            };

            List<string> lines;
            try
            {
                lines = ExtractLinesFromDocx(docxStream);
            }
            catch (Exception ex)
            {
                result.Errors.Add(new FormatErrorDto
                {
                    Location = "Tệp tin",
                    Position = "Đầu tệp",
                    Title = "Lỗi đọc tệp .docx",
                    Problem = $"Không thể mở hoặc giải nén tệp Word: {ex.Message}",
                    Fix = "Vui lòng kiểm tra lại định dạng tệp .docx hợp lệ."
                });
                result.IsValid = false;
                return result;
            }

            if (!lines.Any())
            {
                result.Errors.Add(new FormatErrorDto
                {
                    Location = "Tệp tin",
                    Position = "Nội dung",
                    Title = "Tệp Word trống",
                    Problem = "Tài liệu Word không có nội dung văn bản nào.",
                    Fix = "Hãy soạn thảo nội dung đề thi với các thẻ tags chuẩn."
                });
                result.IsValid = false;
                return result;
            }

            ParseContent(lines, result);

            result.IsValid = result.Errors.Count == 0 &&
                             result.ValidQuestionsCount == 100 &&
                             result.Part5Count == 30 &&
                             result.Part6Count == 16 &&
                             result.Part7Count == 54;

            return result;
        }

        private List<string> ExtractLinesFromDocx(Stream stream)
        {
            var lines = new List<string>();
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                var entry = zip.GetEntry("word/document.xml");
                if (entry == null)
                    throw new Exception("Không tìm thấy word/document.xml trong tệp Word.");

                using (var entryStream = entry.Open())
                {
                    var doc = XDocument.Load(entryStream);
                    foreach (var p in doc.Descendants(W + "p"))
                    {
                        var text = string.Concat(p.Descendants(W + "t").Select(t => t.Value)).Trim();
                        if (!string.IsNullOrEmpty(text))
                        {
                            lines.Add(text);
                        }
                    }
                }
            }
            return lines;
        }

        private void ParseContent(List<string> lines, DocxValidationResultDto result)
        {
            var exam = new ToeicExamDto
            {
                Title = "TOEIC Reading Test",
                DurationMinutes = 75,
                TotalQuestions = 100,
                Parts = new List<ToeicPartDto>()
            };

            int currentPartNumber = 0;
            ToeicPartDto currentPart = null;
            ToeicPassageGroupDto currentGroup = null;
            ToeicQuestionDto currentQuestion = null;
            bool insidePassage = false;
            var passageLines = new List<string>();

            var encounteredQuestionNumbers = new HashSet<int>();
            var allQuestions = new List<ToeicQuestionDto>();

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                // [TEST]
                if (line.Equals("[TEST]", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // [TITLE: ...]
                var titleMatch = Regex.Match(line, @"^\[TITLE:\s*(.*?)\]$", RegexOptions.IgnoreCase);
                if (titleMatch.Success)
                {
                    exam.Title = titleMatch.Groups[1].Value.Trim();
                    result.ExamTitle = exam.Title;
                    continue;
                }

                // [TIME: ...]
                var timeMatch = Regex.Match(line, @"^\[TIME:\s*(\d+)\]$", RegexOptions.IgnoreCase);
                if (timeMatch.Success)
                {
                    exam.DurationMinutes = int.Parse(timeMatch.Groups[1].Value);
                    result.DurationMinutes = exam.DurationMinutes;
                    continue;
                }

                // [PART: 5/6/7]
                var partMatch = Regex.Match(line, @"^\[PART:\s*(\d+)\]$", RegexOptions.IgnoreCase);
                if (partMatch.Success)
                {
                    currentPartNumber = int.Parse(partMatch.Groups[1].Value);
                    currentPart = new ToeicPartDto
                    {
                        PartNumber = currentPartNumber,
                        Title = $"Part {currentPartNumber}",
                        Groups = new List<ToeicPassageGroupDto>(),
                        Questions = new List<ToeicQuestionDto>()
                    };
                    exam.Parts.Add(currentPart);
                    currentGroup = null;
                    currentQuestion = null;
                    insidePassage = false;
                    continue;
                }

                // [PART_TITLE: ...]
                var partTitleMatch = Regex.Match(line, @"^\[PART_TITLE:\s*(.*?)\]$", RegexOptions.IgnoreCase);
                if (partTitleMatch.Success)
                {
                    if (currentPart != null)
                    {
                        currentPart.Title = $"Part {currentPart.PartNumber} · {partTitleMatch.Groups[1].Value.Trim()}";
                    }
                    continue;
                }

                // [DESCRIPTION: ...]
                var descMatch = Regex.Match(line, @"^\[DESCRIPTION:\s*(.*?)\]$", RegexOptions.IgnoreCase);
                if (descMatch.Success)
                {
                    if (currentPart != null)
                    {
                        currentPart.Description = descMatch.Groups[1].Value.Trim();
                    }
                    continue;
                }

                // [GROUP: ...]
                var groupMatch = Regex.Match(line, @"^\[GROUP:\s*(.*?)\]$", RegexOptions.IgnoreCase);
                if (groupMatch.Success)
                {
                    currentGroup = new ToeicPassageGroupDto
                    {
                        GroupRange = groupMatch.Groups[1].Value.Trim(),
                        Questions = new List<ToeicQuestionDto>()
                    };
                    if (currentPart != null)
                    {
                        currentPart.Groups.Add(currentGroup);
                    }
                    currentQuestion = null;
                    insidePassage = false;
                    continue;
                }

                // [PASSAGE_TITLE: ...]
                var passTitleMatch = Regex.Match(line, @"^\[PASSAGE_TITLE:\s*(.*?)\]$", RegexOptions.IgnoreCase);
                if (passTitleMatch.Success)
                {
                    if (currentGroup != null)
                    {
                        currentGroup.Title = passTitleMatch.Groups[1].Value.Trim();
                    }
                    continue;
                }

                // [PASSAGE]
                if (line.Equals("[PASSAGE]", StringComparison.OrdinalIgnoreCase))
                {
                    insidePassage = true;
                    passageLines.Clear();
                    continue;
                }

                // [/PASSAGE]
                if (line.Equals("[/PASSAGE]", StringComparison.OrdinalIgnoreCase))
                {
                    insidePassage = false;
                    if (currentGroup != null)
                    {
                        currentGroup.PassageText = string.Join("\n", passageLines);
                    }
                    continue;
                }

                if (insidePassage)
                {
                    passageLines.Add(line);
                    continue;
                }

                // [QUESTION: 101]
                var qMatch = Regex.Match(line, @"^\[QUESTION:\s*(\d+)\]$", RegexOptions.IgnoreCase);
                if (qMatch.Success)
                {
                    int qNum = int.Parse(qMatch.Groups[1].Value);

                    // Check for duplicate question number
                    if (encounteredQuestionNumbers.Contains(qNum))
                    {
                        result.Errors.Add(new FormatErrorDto
                        {
                            Location = $"Part {currentPartNumber} · Câu {qNum}",
                            Position = $"Vị trí câu {qNum}",
                            Title = $"Trùng số câu {qNum}",
                            Problem = $"Đã phát hiện hai câu hỏi mang cùng số thứ tự {qNum}.",
                            Fix = $"Đổi số câu thứ hai thành số tiếp theo và kiểm tra dãy số liên tục 101–200."
                        });
                    }
                    else
                    {
                        encounteredQuestionNumbers.Add(qNum);
                    }

                    currentQuestion = new ToeicQuestionDto
                    {
                        QuestionNumber = qNum,
                        Options = new List<ToeicOptionDto>()
                    };

                    allQuestions.Add(currentQuestion);

                    if (currentPart != null)
                    {
                        currentPart.Questions.Add(currentQuestion);
                    }

                    if (currentGroup != null)
                    {
                        currentGroup.Questions.Add(currentQuestion);
                    }
                    continue;
                }

                // Option tags [A] [B] [C] [D]
                var optMatch = Regex.Match(line, @"^\[([A-D])\]\s*(.*)$", RegexOptions.IgnoreCase);
                if (optMatch.Success)
                {
                    if (currentQuestion != null)
                    {
                        currentQuestion.Options.Add(new ToeicOptionDto
                        {
                            Label = optMatch.Groups[1].Value.ToUpper(),
                            Text = optMatch.Groups[2].Value.Trim()
                        });
                    }
                    continue;
                }

                // Answer tag [ANSWER: A]
                var ansMatch = Regex.Match(line, @"^\[ANSWER:\s*([A-D])\]$", RegexOptions.IgnoreCase);
                if (ansMatch.Success)
                {
                    if (currentQuestion != null)
                    {
                        currentQuestion.CorrectAnswer = ansMatch.Groups[1].Value.ToUpper();
                        var opt = currentQuestion.Options.FirstOrDefault(o => o.Label == currentQuestion.CorrectAnswer);
                        if (opt != null)
                        {
                            opt.IsCorrect = true;
                        }
                    }
                    continue;
                }

                // Question text or Passage text without tags
                if (currentQuestion != null && currentQuestion.Options.Count == 0)
                {
                    // This is question stem text
                    if (string.IsNullOrEmpty(currentQuestion.QuestionText))
                        currentQuestion.QuestionText = line;
                    else
                        currentQuestion.QuestionText += " " + line;
                    continue;
                }

                // If in Part 6 or 7 group and no [PASSAGE] tag was opened, this might be text
                if (currentGroup != null && currentQuestion == null && string.IsNullOrEmpty(currentGroup.PassageText))
                {
                    // A passage text was placed without [PASSAGE] tag!
                    result.Errors.Add(new FormatErrorDto
                    {
                        Location = $"Part {currentPartNumber} · Nhóm {currentGroup.GroupRange}",
                        Position = "Đầu đoạn đọc",
                        Title = "Thiếu thẻ đoạn văn",
                        Problem = $"Nhóm câu {currentGroup.GroupRange} chưa được bao bọc bởi thẻ [PASSAGE] và [/PASSAGE].",
                        Fix = $"Bổ sung thẻ đoạn văn [PASSAGE] trước nội dung bài đọc và [/PASSAGE] sau khi kết thúc bài đọc."
                    });
                    currentGroup.PassageText = line;
                }
            }

            // Post-validation of all questions
            foreach (var q in allQuestions)
            {
                // Check 4 options
                if (q.Options.Count != 4)
                {
                    result.Errors.Add(new FormatErrorDto
                    {
                        Location = $"Part {(q.QuestionNumber <= 130 ? 5 : (q.QuestionNumber <= 146 ? 6 : 7))} · Câu {q.QuestionNumber}",
                        Position = $"Lựa chọn câu {q.QuestionNumber}",
                        Title = $"Thiếu lựa chọn câu {q.QuestionNumber}",
                        Problem = $"Câu {q.QuestionNumber} chỉ có {q.Options.Count} lựa chọn (cần đủ 4 lựa chọn A, B, C, D).",
                        Fix = $"Bổ sung đầy đủ 4 lựa chọn [A], [B], [C], [D] cho câu hỏi."
                    });
                }

                // Check correct answer
                if (string.IsNullOrEmpty(q.CorrectAnswer) || !new[] { "A", "B", "C", "D" }.Contains(q.CorrectAnswer))
                {
                    result.Errors.Add(new FormatErrorDto
                    {
                        Location = $"Câu {q.QuestionNumber}",
                        Position = $"Đáp án đúng",
                        Title = $"Thiếu hoặc sai đáp án đúng",
                        Problem = $"Câu {q.QuestionNumber} chưa có thẻ [ANSWER: A/B/C/D] hợp lệ.",
                        Fix = $"Thêm thẻ [ANSWER: X] với X là một trong các chữ cái A, B, C, D."
                    });
                }
                else
                {
                    var correctOpt = q.Options.FirstOrDefault(o => o.Label == q.CorrectAnswer);
                    if (correctOpt != null)
                    {
                        correctOpt.IsCorrect = true;
                    }
                }
            }

            // Check continuous numbering 101 to 200
            for (int expected = 101; expected <= 200; expected++)
            {
                if (!encounteredQuestionNumbers.Contains(expected))
                {
                    // If not reported as duplicate, report as missing
                    result.Errors.Add(new FormatErrorDto
                    {
                        Location = $"Đề thi · Câu {expected}",
                        Position = $"Dãy số 101–200",
                        Title = $"Thiếu câu số {expected}",
                        Problem = $"Không tìm thấy câu hỏi số {expected} trong đề thi.",
                        Fix = $"Kiểm tra lại thứ tự đánh số câu để đảm bảo đủ từ 101 đến 200 liên tục."
                    });
                }
            }

            // Summary counts
            result.Part5Count = allQuestions.Count(q => q.QuestionNumber >= 101 && q.QuestionNumber <= 130);
            result.Part6Count = allQuestions.Count(q => q.QuestionNumber >= 131 && q.QuestionNumber <= 146);
            result.Part7Count = allQuestions.Count(q => q.QuestionNumber >= 147 && q.QuestionNumber <= 200);
            result.ValidQuestionsCount = allQuestions.Count(q => q.Options.Count == 4 && !string.IsNullOrEmpty(q.CorrectAnswer));
            result.TotalQuestions = allQuestions.Count;

            if (exam.Parts.Count == 3)
            {
                exam.Parts[0].QuestionRange = "101–130";
                exam.Parts[0].QuestionCount = result.Part5Count;

                exam.Parts[1].QuestionRange = "131–146";
                exam.Parts[1].QuestionCount = result.Part6Count;

                exam.Parts[2].QuestionRange = "147–200";
                exam.Parts[2].QuestionCount = result.Part7Count;
            }

            result.ParsedExam = exam;
        }
    }
}
