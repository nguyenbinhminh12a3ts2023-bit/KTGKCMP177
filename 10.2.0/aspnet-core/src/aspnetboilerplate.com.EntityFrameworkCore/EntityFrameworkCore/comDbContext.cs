using Abp.Zero.EntityFrameworkCore;
using aspnetboilerplate.com.Authorization.Roles;
using aspnetboilerplate.com.Authorization.Users;
using aspnetboilerplate.com.MultiTenancy;
using aspnetboilerplate.com.Toeic.Entities;
using Microsoft.EntityFrameworkCore;

namespace aspnetboilerplate.com.EntityFrameworkCore;

public class comDbContext : AbpZeroDbContext<Tenant, Role, User, comDbContext>
{
    /* Define a DbSet for each entity of the application */
    public DbSet<ToeicExam> ToeicExams { get; set; }
    public DbSet<ToeicPart> ToeicParts { get; set; }
    public DbSet<ToeicPassageGroup> ToeicPassageGroups { get; set; }
    public DbSet<ToeicQuestion> ToeicQuestions { get; set; }
    public DbSet<ToeicAnswerOption> ToeicAnswerOptions { get; set; }
    public DbSet<ExamSubmission> ExamSubmissions { get; set; }
    public DbSet<ExamSubmissionDetail> ExamSubmissionDetails { get; set; }

    public comDbContext(DbContextOptions<comDbContext> options)
        : base(options)
    {
    }
}
