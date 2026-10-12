using System;
using System.Collections.Generic;

namespace aspnetboilerplate.com.Toeic.Dto
{
    public class ToeicExamDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public int DurationMinutes { get; set; } = 75;
        public int TotalQuestions { get; set; } = 100;
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public List<ToeicPartDto> Parts { get; set; } = new List<ToeicPartDto>();
    }

    public class ToeicPartDto
    {
        public int Id { get; set; }
        public int PartNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string QuestionRange { get; set; }
        public int QuestionCount { get; set; }
        public List<ToeicPassageGroupDto> Groups { get; set; } = new List<ToeicPassageGroupDto>();
        public List<ToeicQuestionDto> Questions { get; set; } = new List<ToeicQuestionDto>();
    }

    public class ToeicPassageGroupDto
    {
        public int Id { get; set; }
        public string GroupRange { get; set; }
        public string Title { get; set; }
        public string PassageText { get; set; }
        public string ImageUrl { get; set; }
        public List<ToeicQuestionDto> Questions { get; set; } = new List<ToeicQuestionDto>();
    }

    public class ToeicQuestionDto
    {
        public int Id { get; set; }
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; }
        public int? BlankNumber { get; set; }
        public string Explanation { get; set; }
        public List<ToeicOptionDto> Options { get; set; } = new List<ToeicOptionDto>();
        public string CorrectAnswer { get; set; }
    }

    public class ToeicOptionDto
    {
        public int Id { get; set; }
        public string Label { get; set; } // "A", "B", "C", "D"
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class FormatErrorDto
    {
        public string Location { get; set; }   // e.g., "Part 6 · Nhóm 131–134"
        public string Position { get; set; }   // e.g., "Đầu đoạn đọc"
        public string Title { get; set; }      // e.g., "Thiếu thẻ đoạn văn"
        public string Problem { get; set; }    // e.g., "Nhóm câu 131–134 chưa được liên kết với đoạn đọc."
        public string Fix { get; set; }        // e.g., "Bổ sung thẻ đoạn văn [PASSAGE]..."
    }

    public class DocxValidationResultDto
    {
        public bool IsValid { get; set; }
        public string ExamTitle { get; set; }
        public int DurationMinutes { get; set; } = 75;
        public int TotalQuestions { get; set; } = 100;
        public int ValidQuestionsCount { get; set; }
        public int Part5Count { get; set; }
        public int Part6Count { get; set; }
        public int Part7Count { get; set; }
        public List<FormatErrorDto> Errors { get; set; } = new List<FormatErrorDto>();
        public ToeicExamDto ParsedExam { get; set; }
    }

    public class SaveExamInputDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int DurationMinutes { get; set; } = 75;
        public ToeicExamDto ExamData { get; set; }
    }

    public class UpdateQuestionInputDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectAnswer { get; set; }
        public string PassageText { get; set; }
        public int? PassageGroupId { get; set; }
    }

    public class SubmitExamInputDto
    {
        public int ExamId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public int TimeSpentSeconds { get; set; }
        public bool IsAutoSubmitted { get; set; }
        public Dictionary<int, string> Answers { get; set; } = new Dictionary<int, string>(); // QuestionNumber -> "A", "B", "C", "D"
    }

    public class ExamResultDto
    {
        public int SubmissionId { get; set; }
        public string ExamTitle { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public int TotalQuestions { get; set; } = 100;
        public int TotalCorrect { get; set; }
        public int TotalIncorrect { get; set; }
        public int TotalSkipped { get; set; }
        public int TimeSpentSeconds { get; set; }
        public bool IsAutoSubmitted { get; set; }
        public int EstimatedScore { get; set; }
        public PartScoreDto Part5 { get; set; } = new PartScoreDto();
        public PartScoreDto Part6 { get; set; } = new PartScoreDto();
        public PartScoreDto Part7 { get; set; } = new PartScoreDto();
        public List<QuestionReviewDto> QuestionReviews { get; set; } = new List<QuestionReviewDto>();
    }

    public class PartScoreDto
    {
        public int PartNumber { get; set; }
        public string Title { get; set; }
        public int TotalQuestions { get; set; }
        public int Correct { get; set; }
        public int Incorrect { get; set; }
        public int Skipped { get; set; }
    }

    public class QuestionReviewDto
    {
        public int QuestionNumber { get; set; }
        public string UserAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsSkipped { get; set; }
    }
}
