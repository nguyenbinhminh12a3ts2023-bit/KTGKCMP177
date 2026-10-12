using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using aspnetboilerplate.com.Toeic.Entities;
using Microsoft.EntityFrameworkCore;

namespace aspnetboilerplate.com.EntityFrameworkCore.Seed.Host
{
    public class DefaultToeicExamCreator
    {
        private readonly comDbContext _context;

        public DefaultToeicExamCreator(comDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            // Ensure tables exist
            try
            {
                _context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ToeicExams')
BEGIN
    CREATE TABLE [dbo].[ToeicExams] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Title] NVARCHAR(200) NOT NULL,
        [Code] NVARCHAR(50) NULL,
        [DurationMinutes] INT NOT NULL DEFAULT 75,
        [TotalQuestions] INT NOT NULL DEFAULT 100,
        [Description] NVARCHAR(MAX) NULL,
        [FilePath] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreationTime] DATETIME2 NOT NULL DEFAULT GETDATE()
    );

    CREATE TABLE [dbo].[ToeicParts] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ExamId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[ToeicExams]([Id]) ON DELETE CASCADE,
        [PartNumber] INT NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [QuestionCount] INT NOT NULL,
        [OrderIndex] INT NOT NULL
    );

    CREATE TABLE [dbo].[ToeicPassageGroups] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PartId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[ToeicParts]([Id]) ON DELETE CASCADE,
        [GroupRange] NVARCHAR(50) NOT NULL,
        [Title] NVARCHAR(250) NULL,
        [PassageText] NVARCHAR(MAX) NULL,
        [ImageUrl] NVARCHAR(500) NULL,
        [OrderIndex] INT NOT NULL
    );

    CREATE TABLE [dbo].[ToeicQuestions] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ExamId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[ToeicExams]([Id]),
        [PartId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[ToeicParts]([Id]),
        [PassageGroupId] INT NULL FOREIGN KEY REFERENCES [dbo].[ToeicPassageGroups]([Id]),
        [QuestionNumber] INT NOT NULL,
        [QuestionText] NVARCHAR(MAX) NULL,
        [BlankNumber] INT NULL,
        [Explanation] NVARCHAR(MAX) NULL,
        [OrderIndex] INT NOT NULL
    );

    CREATE TABLE [dbo].[ToeicAnswerOptions] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [QuestionId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[ToeicQuestions]([Id]) ON DELETE CASCADE,
        [OptionLabel] NVARCHAR(10) NOT NULL,
        [OptionText] NVARCHAR(MAX) NOT NULL,
        [IsCorrect] BIT NOT NULL DEFAULT 0,
        [OrderIndex] INT NOT NULL
    );

    CREATE TABLE [dbo].[ExamSubmissions] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ExamId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[ToeicExams]([Id]) ON DELETE CASCADE,
        [StudentName] NVARCHAR(150) NULL,
        [StudentEmail] NVARCHAR(150) NULL,
        [StartTime] DATETIME2 NOT NULL,
        [SubmitTime] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [TimeSpentSeconds] INT NOT NULL DEFAULT 0,
        [TotalQuestions] INT NOT NULL DEFAULT 100,
        [TotalCorrect] INT NOT NULL DEFAULT 0,
        [TotalIncorrect] INT NOT NULL DEFAULT 0,
        [TotalSkipped] INT NOT NULL DEFAULT 0,
        [ScorePart5] INT NOT NULL DEFAULT 0,
        [ScorePart6] INT NOT NULL DEFAULT 0,
        [ScorePart7] INT NOT NULL DEFAULT 0,
        [EstimatedToeicScore] INT NOT NULL DEFAULT 5,
        [IsAutoSubmitted] BIT NOT NULL DEFAULT 0,
        [CreationTime] DATETIME2 NOT NULL DEFAULT GETDATE()
    );

    CREATE TABLE [dbo].[ExamSubmissionDetails] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [SubmissionId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[ExamSubmissions]([Id]) ON DELETE CASCADE,
        [QuestionId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[ToeicQuestions]([Id]),
        [QuestionNumber] INT NOT NULL,
        [SelectedOption] NVARCHAR(10) NULL,
        [CorrectOption] NVARCHAR(10) NOT NULL,
        [IsCorrect] BIT NOT NULL DEFAULT 0
    );
END
");
            }
            catch (Exception ex)
            {
                // In case tables already exist or other DB provider
                Console.WriteLine("Table ensure check: " + ex.Message);
            }

            // Seed default exams if empty
            if (!_context.ToeicExams.Any())
            {
                SeedFromDocxOrJson();
            }
        }

        private void SeedFromDocxOrJson()
        {
            try
            {
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "toeic_reading_test_07.json");
                if (!File.Exists(jsonPath))
                {
                    jsonPath = @"d:\BaiTapGiuaKi\toeic_reading_test_07.json";
                }

                if (File.Exists(jsonPath))
                {
                    var json = File.ReadAllText(jsonPath);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    var exam = new ToeicExam
                    {
                        Title = "Reading Test 01",
                        Code = "TEST_01",
                        DurationMinutes = root.GetProperty("durationMinutes").GetInt32(),
                        TotalQuestions = root.GetProperty("totalQuestions").GetInt32(),
                        Description = root.GetProperty("description").GetString(),
                        IsActive = true,
                        CreationTime = DateTime.Now
                    };
                    _context.ToeicExams.Add(exam);
                    _context.SaveChanges();

                    foreach (var partElem in root.GetProperty("parts").EnumerateArray())
                    {
                        int pNum = partElem.GetProperty("partNumber").GetInt32();
                        var part = new ToeicPart
                        {
                            ExamId = exam.Id,
                            PartNumber = pNum,
                            Title = partElem.GetProperty("title").GetString(),
                            Description = partElem.GetProperty("description").GetString(),
                            QuestionCount = partElem.GetProperty("questionCount").GetInt32(),
                            OrderIndex = pNum
                        };
                        _context.ToeicParts.Add(part);
                        _context.SaveChanges();

                        if (partElem.TryGetProperty("groups", out var groupsElem))
                        {
                            int gIdx = 1;
                            foreach (var grpElem in groupsElem.EnumerateArray())
                            {
                                var grp = new ToeicPassageGroup
                                {
                                    PartId = part.Id,
                                    GroupRange = grpElem.GetProperty("groupRange").GetString(),
                                    Title = grpElem.GetProperty("title").GetString(),
                                    PassageText = grpElem.GetProperty("passage").GetString(),
                                    OrderIndex = gIdx++
                                };
                                _context.ToeicPassageGroups.Add(grp);
                                _context.SaveChanges();

                                foreach (var qElem in grpElem.GetProperty("questions").EnumerateArray())
                                {
                                    InsertSeedQuestion(exam.Id, part.Id, grp.Id, qElem);
                                }
                            }
                        }

                        if (partElem.TryGetProperty("questions", out var questionsElem))
                        {
                            foreach (var qElem in questionsElem.EnumerateArray())
                            {
                                InsertSeedQuestion(exam.Id, part.Id, null, qElem);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SeedFromDocxOrJson error: " + ex.Message);
            }
        }

        private void InsertSeedQuestion(int examId, int partId, int? groupId, JsonElement qElem)
        {
            int qNum = qElem.GetProperty("number").GetInt32();
            string qText = qElem.GetProperty("text").GetString();
            string correctAns = qElem.GetProperty("correctAnswer").GetString();

            var question = new ToeicQuestion
            {
                ExamId = examId,
                PartId = partId,
                PassageGroupId = groupId,
                QuestionNumber = qNum,
                QuestionText = qText,
                BlankNumber = (qNum >= 131 && qNum <= 146) ? qNum : (int?)null,
                OrderIndex = qNum
            };
            _context.ToeicQuestions.Add(question);
            _context.SaveChanges();

            var optionsElem = qElem.GetProperty("options");
            int optOrder = 1;
            foreach (var optProp in optionsElem.EnumerateObject())
            {
                var option = new ToeicAnswerOption
                {
                    QuestionId = question.Id,
                    OptionLabel = optProp.Name,
                    OptionText = optProp.Value.GetString(),
                    IsCorrect = optProp.Name.Equals(correctAns, StringComparison.OrdinalIgnoreCase),
                    OrderIndex = optOrder++
                };
                _context.ToeicAnswerOptions.Add(option);
            }
            _context.SaveChanges();
        }
    }
}
