using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using aspnetboilerplate.com.Toeic.Dto;
using aspnetboilerplate.com.Toeic.Entities;
using Microsoft.EntityFrameworkCore;

namespace aspnetboilerplate.com.Toeic.Services
{
    public class ToeicExamAppService : ApplicationService, IToeicExamAppService
    {
        private readonly IRepository<ToeicExam, int> _examRepository;
        private readonly IRepository<ToeicPart, int> _partRepository;
        private readonly IRepository<ToeicPassageGroup, int> _groupRepository;
        private readonly IRepository<ToeicQuestion, int> _questionRepository;
        private readonly IRepository<ToeicAnswerOption, int> _optionRepository;
        private readonly IRepository<ExamSubmission, int> _submissionRepository;
        private readonly IRepository<ExamSubmissionDetail, int> _detailRepository;
        private readonly IToeicDocxParserService _docxParser;

        public ToeicExamAppService(
            IRepository<ToeicExam, int> examRepository,
            IRepository<ToeicPart, int> partRepository,
            IRepository<ToeicPassageGroup, int> groupRepository,
            IRepository<ToeicQuestion, int> questionRepository,
            IRepository<ToeicAnswerOption, int> optionRepository,
            IRepository<ExamSubmission, int> submissionRepository,
            IRepository<ExamSubmissionDetail, int> detailRepository,
            IToeicDocxParserService docxParser)
        {
            _examRepository = examRepository;
            _partRepository = partRepository;
            _groupRepository = groupRepository;
            _questionRepository = questionRepository;
            _optionRepository = optionRepository;
            _submissionRepository = submissionRepository;
            _detailRepository = detailRepository;
            _docxParser = docxParser;
        }

        public DocxValidationResultDto ValidateDocxStream(Stream stream)
        {
            return _docxParser.ParseAndValidate(stream);
        }

        public DocxValidationResultDto ValidateDocxFile(string filePath)
        {
            return _docxParser.ParseAndValidate(filePath);
        }

        public async Task<ToeicExamDto> SaveExam(SaveExamInputDto input)
        {
            if (input == null || input.ExamData == null)
            {
                throw new UserFriendlyException("Dữ liệu đề thi không hợp lệ.");
            }

            var examData = input.ExamData;

            var exam = new ToeicExam
            {
                Title = !string.IsNullOrWhiteSpace(input.Title) ? input.Title : examData.Title,
                Code = "TEST_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                DurationMinutes = input.DurationMinutes > 0 ? input.DurationMinutes : 75,
                TotalQuestions = 100,
                Description = input.Description ?? examData.Description ?? "Đề thi TOEIC Reading chuẩn ETS",
                IsActive = true,
                CreationTime = DateTime.Now
            };

            var examId = await _examRepository.InsertAndGetIdAsync(exam);

            foreach (var partDto in examData.Parts)
            {
                var part = new ToeicPart
                {
                    ExamId = examId,
                    PartNumber = partDto.PartNumber,
                    Title = partDto.Title,
                    Description = partDto.Description,
                    QuestionCount = partDto.QuestionCount,
                    OrderIndex = partDto.PartNumber
                };
                var partId = await _partRepository.InsertAndGetIdAsync(part);

                // Map passage groups
                var groupMap = new Dictionary<string, int>();
                if (partDto.Groups != null)
                {
                    int grpIndex = 1;
                    foreach (var grpDto in partDto.Groups)
                    {
                        var group = new ToeicPassageGroup
                        {
                            PartId = partId,
                            GroupRange = grpDto.GroupRange,
                            Title = grpDto.Title,
                            PassageText = grpDto.PassageText,
                            ImageUrl = grpDto.ImageUrl,
                            OrderIndex = grpIndex++
                        };
                        var groupId = await _groupRepository.InsertAndGetIdAsync(group);
                        groupMap[grpDto.GroupRange] = groupId;

                        // Insert questions in this group
                        if (grpDto.Questions != null)
                        {
                            foreach (var qDto in grpDto.Questions)
                            {
                                await InsertQuestionAsync(examId, partId, groupId, qDto);
                            }
                        }
                    }
                }

                // Insert questions in part that might not be in groups (Part 5)
                if (partDto.Questions != null && partDto.PartNumber == 5)
                {
                    foreach (var qDto in partDto.Questions)
                    {
                        await InsertQuestionAsync(examId, partId, null, qDto);
                    }
                }
            }

            return await GetExamById(examId);
        }

        private async Task InsertQuestionAsync(int examId, int partId, int? groupId, ToeicQuestionDto qDto)
        {
            var question = new ToeicQuestion
            {
                ExamId = examId,
                PartId = partId,
                PassageGroupId = groupId,
                QuestionNumber = qDto.QuestionNumber,
                QuestionText = qDto.QuestionText,
                BlankNumber = qDto.BlankNumber,
                Explanation = qDto.Explanation,
                OrderIndex = qDto.QuestionNumber
            };
            var questionId = await _questionRepository.InsertAndGetIdAsync(question);

            int optIndex = 1;
            foreach (var optDto in qDto.Options)
            {
                var option = new ToeicAnswerOption
                {
                    QuestionId = questionId,
                    OptionLabel = optDto.Label,
                    OptionText = optDto.Text,
                    IsCorrect = optDto.Label.Equals(qDto.CorrectAnswer, StringComparison.OrdinalIgnoreCase) || optDto.IsCorrect,
                    OrderIndex = optIndex++
                };
                await _optionRepository.InsertAsync(option);
            }
        }

        public async Task<List<ToeicExamDto>> GetExams()
        {
            var exams = await _examRepository.GetAll()
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.CreationTime)
                .ToListAsync();

            return exams.Select(e => new ToeicExamDto
            {
                Id = e.Id,
                Title = e.Title,
                Code = e.Code,
                DurationMinutes = e.DurationMinutes,
                TotalQuestions = e.TotalQuestions,
                Description = e.Description,
                CreationTime = e.CreationTime
            }).ToList();
        }

        public async Task<ToeicExamDto> GetExamById(int id)
        {
            var exam = await _examRepository.GetAll()
                .Include(e => e.Parts)
                    .ThenInclude(p => p.PassageGroups)
                .Include(e => e.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
            {
                throw new UserFriendlyException($"Không tìm thấy đề thi với Id = {id}");
            }

            return MapExamToDto(exam, includeAnswers: true);
        }

        public async Task<ToeicExamDto> GetExamForStudent(int id)
        {
            var exam = await _examRepository.GetAll()
                .Include(e => e.Parts)
                    .ThenInclude(p => p.PassageGroups)
                .Include(e => e.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
            {
                throw new UserFriendlyException($"Không tìm thấy đề thi với Id = {id}");
            }

            return MapExamToDto(exam, includeAnswers: false);
        }

        public async Task<bool> UpdateQuestion(UpdateQuestionInputDto input)
        {
            var question = await _questionRepository.GetAll()
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == input.QuestionId);

            if (question == null)
            {
                throw new UserFriendlyException("Không tìm thấy câu hỏi để cập nhật.");
            }

            question.QuestionText = input.QuestionText;

            // Update options
            var optA = question.Options.FirstOrDefault(o => o.OptionLabel == "A");
            if (optA != null && input.OptionA != null) optA.OptionText = input.OptionA;

            var optB = question.Options.FirstOrDefault(o => o.OptionLabel == "B");
            if (optB != null && input.OptionB != null) optB.OptionText = input.OptionB;

            var optC = question.Options.FirstOrDefault(o => o.OptionLabel == "C");
            if (optC != null && input.OptionC != null) optC.OptionText = input.OptionC;

            var optD = question.Options.FirstOrDefault(o => o.OptionLabel == "D");
            if (optD != null && input.OptionD != null) optD.OptionText = input.OptionD;

            if (!string.IsNullOrEmpty(input.CorrectAnswer))
            {
                foreach (var opt in question.Options)
                {
                    opt.IsCorrect = opt.OptionLabel.Equals(input.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
                }
            }

            // Update passage text if specified
            if (input.PassageGroupId.HasValue && !string.IsNullOrEmpty(input.PassageText))
            {
                var group = await _groupRepository.FirstOrDefaultAsync(input.PassageGroupId.Value);
                if (group != null)
                {
                    group.PassageText = input.PassageText;
                }
            }

            await _questionRepository.UpdateAsync(question);
            return true;
        }

        public async Task<ExamResultDto> SubmitExam(SubmitExamInputDto input)
        {
            var exam = await _examRepository.GetAll()
                .Include(e => e.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(e => e.Id == input.ExamId);

            if (exam == null)
            {
                throw new UserFriendlyException("Đề thi không tồn tại.");
            }

            var submission = new ExamSubmission
            {
                ExamId = input.ExamId,
                StudentName = input.StudentName ?? "Thí sinh",
                StudentEmail = input.StudentEmail ?? "student@toeic.edu.vn",
                StartTime = DateTime.Now.AddSeconds(-input.TimeSpentSeconds),
                SubmitTime = DateTime.Now,
                TimeSpentSeconds = input.TimeSpentSeconds,
                TotalQuestions = 100,
                IsAutoSubmitted = input.IsAutoSubmitted,
                CreationTime = DateTime.Now
            };

            var subId = await _submissionRepository.InsertAndGetIdAsync(submission);

            int correctCount = 0;
            int incorrectCount = 0;
            int skippedCount = 0;

            int p5Correct = 0, p5Incorrect = 0, p5Skipped = 0;
            int p6Correct = 0, p6Incorrect = 0, p6Skipped = 0;
            int p7Correct = 0, p7Incorrect = 0, p7Skipped = 0;

            var reviews = new List<QuestionReviewDto>();

            var orderedQuestions = exam.Questions.OrderBy(q => q.QuestionNumber).ToList();

            foreach (var q in orderedQuestions)
            {
                var correctOpt = q.Options.FirstOrDefault(o => o.IsCorrect)?.OptionLabel ?? "A";
                string userAns = null;
                if (input.Answers != null && input.Answers.ContainsKey(q.QuestionNumber))
                {
                    userAns = input.Answers[q.QuestionNumber]?.Trim()?.ToUpper();
                }

                bool isCorrect = false;
                bool isSkipped = string.IsNullOrEmpty(userAns);

                if (isSkipped)
                {
                    skippedCount++;
                    if (q.QuestionNumber <= 130) p5Skipped++;
                    else if (q.QuestionNumber <= 146) p6Skipped++;
                    else p7Skipped++;
                }
                else if (userAns == correctOpt)
                {
                    isCorrect = true;
                    correctCount++;
                    if (q.QuestionNumber <= 130) p5Correct++;
                    else if (q.QuestionNumber <= 146) p6Correct++;
                    else p7Correct++;
                }
                else
                {
                    incorrectCount++;
                    if (q.QuestionNumber <= 130) p5Incorrect++;
                    else if (q.QuestionNumber <= 146) p6Incorrect++;
                    else p7Incorrect++;
                }

                var detail = new ExamSubmissionDetail
                {
                    SubmissionId = subId,
                    QuestionId = q.Id,
                    QuestionNumber = q.QuestionNumber,
                    SelectedOption = userAns,
                    CorrectOption = correctOpt,
                    IsCorrect = isCorrect
                };
                await _detailRepository.InsertAsync(detail);

                reviews.Add(new QuestionReviewDto
                {
                    QuestionNumber = q.QuestionNumber,
                    UserAnswer = userAns,
                    CorrectAnswer = correctOpt,
                    IsCorrect = isCorrect,
                    IsSkipped = isSkipped
                });
            }

            submission.TotalCorrect = correctCount;
            submission.TotalIncorrect = incorrectCount;
            submission.TotalSkipped = skippedCount;
            submission.ScorePart5 = p5Correct;
            submission.ScorePart6 = p6Correct;
            submission.ScorePart7 = p7Correct;
            submission.EstimatedToeicScore = CalculateEstimatedScore(correctCount);

            await _submissionRepository.UpdateAsync(submission);

            return new ExamResultDto
            {
                SubmissionId = subId,
                ExamTitle = exam.Title,
                StudentName = submission.StudentName,
                StudentEmail = submission.StudentEmail,
                TotalQuestions = 100,
                TotalCorrect = correctCount,
                TotalIncorrect = incorrectCount,
                TotalSkipped = skippedCount,
                TimeSpentSeconds = input.TimeSpentSeconds,
                IsAutoSubmitted = input.IsAutoSubmitted,
                EstimatedScore = submission.EstimatedToeicScore,
                Part5 = new PartScoreDto
                {
                    PartNumber = 5,
                    Title = "Part 5 · Incomplete Sentences",
                    TotalQuestions = 30,
                    Correct = p5Correct,
                    Incorrect = p5Incorrect,
                    Skipped = p5Skipped
                },
                Part6 = new PartScoreDto
                {
                    PartNumber = 6,
                    Title = "Part 6 · Text Completion",
                    TotalQuestions = 16,
                    Correct = p6Correct,
                    Incorrect = p6Incorrect,
                    Skipped = p6Skipped
                },
                Part7 = new PartScoreDto
                {
                    PartNumber = 7,
                    Title = "Part 7 · Reading Comprehension",
                    TotalQuestions = 54,
                    Correct = p7Correct,
                    Incorrect = p7Incorrect,
                    Skipped = p7Skipped
                },
                QuestionReviews = reviews
            };
        }

        public async Task<ExamResultDto> GetSubmissionResult(int submissionId)
        {
            var sub = await _submissionRepository.GetAll()
                .Include(s => s.Exam)
                .Include(s => s.Details)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (sub == null)
            {
                throw new UserFriendlyException("Không tìm thấy kết quả bài thi.");
            }

            return new ExamResultDto
            {
                SubmissionId = sub.Id,
                ExamTitle = sub.Exam.Title,
                StudentName = sub.StudentName,
                StudentEmail = sub.StudentEmail,
                TotalQuestions = sub.TotalQuestions,
                TotalCorrect = sub.TotalCorrect,
                TotalIncorrect = sub.TotalIncorrect,
                TotalSkipped = sub.TotalSkipped,
                TimeSpentSeconds = sub.TimeSpentSeconds,
                IsAutoSubmitted = sub.IsAutoSubmitted,
                EstimatedScore = sub.EstimatedToeicScore,
                Part5 = new PartScoreDto
                {
                    PartNumber = 5,
                    Title = "Part 5 · Incomplete Sentences",
                    TotalQuestions = 30,
                    Correct = sub.ScorePart5,
                    Incorrect = 30 - sub.ScorePart5,
                    Skipped = sub.Details.Count(d => d.QuestionNumber <= 130 && string.IsNullOrEmpty(d.SelectedOption))
                },
                Part6 = new PartScoreDto
                {
                    PartNumber = 6,
                    Title = "Part 6 · Text Completion",
                    TotalQuestions = 16,
                    Correct = sub.ScorePart6,
                    Incorrect = 16 - sub.ScorePart6,
                    Skipped = sub.Details.Count(d => d.QuestionNumber >= 131 && d.QuestionNumber <= 146 && string.IsNullOrEmpty(d.SelectedOption))
                },
                Part7 = new PartScoreDto
                {
                    PartNumber = 7,
                    Title = "Part 7 · Reading Comprehension",
                    TotalQuestions = 54,
                    Correct = sub.ScorePart7,
                    Incorrect = 54 - sub.ScorePart7,
                    Skipped = sub.Details.Count(d => d.QuestionNumber >= 147 && string.IsNullOrEmpty(d.SelectedOption))
                },
                QuestionReviews = sub.Details.OrderBy(d => d.QuestionNumber).Select(d => new QuestionReviewDto
                {
                    QuestionNumber = d.QuestionNumber,
                    UserAnswer = d.SelectedOption,
                    CorrectAnswer = d.CorrectOption,
                    IsCorrect = d.IsCorrect,
                    IsSkipped = string.IsNullOrEmpty(d.SelectedOption)
                }).ToList()
            };
        }

        private int CalculateEstimatedScore(int correctCount)
        {
            // ETS standard scale approximation: 0 -> 5 pts, 100 -> 495 pts
            if (correctCount <= 0) return 5;
            if (correctCount >= 100) return 495;
            return (int)Math.Round(5 + (correctCount * 4.9));
        }

        private ToeicExamDto MapExamToDto(ToeicExam exam, bool includeAnswers)
        {
            var examDto = new ToeicExamDto
            {
                Id = exam.Id,
                Title = exam.Title,
                Code = exam.Code,
                DurationMinutes = exam.DurationMinutes,
                TotalQuestions = exam.TotalQuestions,
                Description = exam.Description,
                CreationTime = exam.CreationTime,
                Parts = new List<ToeicPartDto>()
            };

            var questionsByNumber = exam.Questions.ToDictionary(q => q.QuestionNumber);

            foreach (var part in exam.Parts.OrderBy(p => p.PartNumber))
            {
                var partDto = new ToeicPartDto
                {
                    Id = part.Id,
                    PartNumber = part.PartNumber,
                    Title = part.Title,
                    Description = part.Description,
                    QuestionCount = part.QuestionCount,
                    QuestionRange = part.PartNumber == 5 ? "101–130" : (part.PartNumber == 6 ? "131–146" : "147–200"),
                    Groups = new List<ToeicPassageGroupDto>(),
                    Questions = new List<ToeicQuestionDto>()
                };

                // Add groups
                foreach (var grp in part.PassageGroups.OrderBy(g => g.OrderIndex))
                {
                    var grpDto = new ToeicPassageGroupDto
                    {
                        Id = grp.Id,
                        GroupRange = grp.GroupRange,
                        Title = grp.Title,
                        PassageText = grp.PassageText,
                        ImageUrl = grp.ImageUrl,
                        Questions = new List<ToeicQuestionDto>()
                    };

                    var rangeParts = grp.GroupRange.Split('-');
                    if (rangeParts.Length == 2 && int.TryParse(rangeParts[0], out int startQ) && int.TryParse(rangeParts[1], out int endQ))
                    {
                        for (int qn = startQ; qn <= endQ; qn++)
                        {
                            if (questionsByNumber.TryGetValue(qn, out var q))
                            {
                                grpDto.Questions.Add(MapQuestionToDto(q, includeAnswers));
                            }
                        }
                    }

                    partDto.Groups.Add(grpDto);
                }

                // If Part 5 (or any questions without group)
                if (part.PartNumber == 5)
                {
                    for (int qn = 101; qn <= 130; qn++)
                    {
                        if (questionsByNumber.TryGetValue(qn, out var q))
                        {
                            partDto.Questions.Add(MapQuestionToDto(q, includeAnswers));
                        }
                    }
                }

                examDto.Parts.Add(partDto);
            }

            return examDto;
        }

        private ToeicQuestionDto MapQuestionToDto(ToeicQuestion q, bool includeAnswers)
        {
            var correctOpt = q.Options.FirstOrDefault(o => o.IsCorrect)?.OptionLabel;
            return new ToeicQuestionDto
            {
                Id = q.Id,
                QuestionNumber = q.QuestionNumber,
                QuestionText = q.QuestionText,
                BlankNumber = q.BlankNumber,
                Explanation = q.Explanation,
                CorrectAnswer = includeAnswers ? correctOpt : null,
                Options = q.Options.OrderBy(o => o.OptionLabel).Select(o => new ToeicOptionDto
                {
                    Id = o.Id,
                    Label = o.OptionLabel,
                    Text = o.OptionText,
                    IsCorrect = includeAnswers ? o.IsCorrect : false
                }).ToList()
            };
        }
    }
}
