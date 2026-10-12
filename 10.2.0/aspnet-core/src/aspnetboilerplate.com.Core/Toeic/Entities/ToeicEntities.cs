using System;
using System.Collections.Generic;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace aspnetboilerplate.com.Toeic.Entities
{
    public class ToeicExam : Entity<int>, IHasCreationTime
    {
        public string Title { get; set; }
        public string Code { get; set; }
        public int DurationMinutes { get; set; } = 75;
        public int TotalQuestions { get; set; } = 100;
        public string Description { get; set; }
        public string FilePath { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreationTime { get; set; } = DateTime.Now;

        public virtual ICollection<ToeicPart> Parts { get; set; } = new List<ToeicPart>();
        public virtual ICollection<ToeicQuestion> Questions { get; set; } = new List<ToeicQuestion>();
        public virtual ICollection<ExamSubmission> Submissions { get; set; } = new List<ExamSubmission>();
    }

    public class ToeicPart : Entity<int>
    {
        public int ExamId { get; set; }
        public virtual ToeicExam Exam { get; set; }

        public int PartNumber { get; set; } // 5, 6, 7
        public string Title { get; set; }
        public string Description { get; set; }
        public int QuestionCount { get; set; }
        public int OrderIndex { get; set; }

        public virtual ICollection<ToeicPassageGroup> PassageGroups { get; set; } = new List<ToeicPassageGroup>();
        public virtual ICollection<ToeicQuestion> Questions { get; set; } = new List<ToeicQuestion>();
    }

    public class ToeicPassageGroup : Entity<int>
    {
        public int PartId { get; set; }
        public virtual ToeicPart Part { get; set; }

        public string GroupRange { get; set; } // e.g., "131-134", "176-180"
        public string Title { get; set; }
        public string PassageText { get; set; }
        public string ImageUrl { get; set; }
        public int OrderIndex { get; set; }

        public virtual ICollection<ToeicQuestion> Questions { get; set; } = new List<ToeicQuestion>();
    }

    public class ToeicQuestion : Entity<int>
    {
        public int ExamId { get; set; }
        public virtual ToeicExam Exam { get; set; }

        public int PartId { get; set; }
        public virtual ToeicPart Part { get; set; }

        public int? PassageGroupId { get; set; }
        public virtual ToeicPassageGroup PassageGroup { get; set; }

        public int QuestionNumber { get; set; } // 101 - 200
        public string QuestionText { get; set; }
        public int? BlankNumber { get; set; } // for Part 6
        public string Explanation { get; set; }
        public int OrderIndex { get; set; }

        public virtual ICollection<ToeicAnswerOption> Options { get; set; } = new List<ToeicAnswerOption>();
    }

    public class ToeicAnswerOption : Entity<int>
    {
        public int QuestionId { get; set; }
        public virtual ToeicQuestion Question { get; set; }

        public string OptionLabel { get; set; } // "A", "B", "C", "D"
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
        public int OrderIndex { get; set; }
    }

    public class ExamSubmission : Entity<int>, IHasCreationTime
    {
        public int ExamId { get; set; }
        public virtual ToeicExam Exam { get; set; }

        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime SubmitTime { get; set; } = DateTime.Now;
        public int TimeSpentSeconds { get; set; }
        public int TotalQuestions { get; set; } = 100;
        public int TotalCorrect { get; set; }
        public int TotalIncorrect { get; set; }
        public int TotalSkipped { get; set; }

        public int ScorePart5 { get; set; }
        public int ScorePart6 { get; set; }
        public int ScorePart7 { get; set; }
        public int EstimatedToeicScore { get; set; } // e.g. 5-495
        public bool IsAutoSubmitted { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;

        public virtual ICollection<ExamSubmissionDetail> Details { get; set; } = new List<ExamSubmissionDetail>();
    }

    public class ExamSubmissionDetail : Entity<int>
    {
        public int SubmissionId { get; set; }
        public virtual ExamSubmission Submission { get; set; }

        public int QuestionId { get; set; }
        public virtual ToeicQuestion Question { get; set; }

        public int QuestionNumber { get; set; }
        public string SelectedOption { get; set; } // "A", "B", "C", "D" or null
        public string CorrectOption { get; set; }
        public bool IsCorrect { get; set; }
    }
}
