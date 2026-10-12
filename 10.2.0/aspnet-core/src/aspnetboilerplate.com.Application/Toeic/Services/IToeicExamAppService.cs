using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Abp.Application.Services;
using aspnetboilerplate.com.Toeic.Dto;

namespace aspnetboilerplate.com.Toeic.Services
{
    public interface IToeicExamAppService : IApplicationService
    {
        DocxValidationResultDto ValidateDocxStream(Stream stream);
        DocxValidationResultDto ValidateDocxFile(string filePath);
        Task<ToeicExamDto> SaveExam(SaveExamInputDto input);
        Task<List<ToeicExamDto>> GetExams();
        Task<ToeicExamDto> GetExamById(int id);
        Task<ToeicExamDto> GetExamForStudent(int id);
        Task<bool> UpdateQuestion(UpdateQuestionInputDto input);
        Task<ExamResultDto> SubmitExam(SubmitExamInputDto input);
        Task<ExamResultDto> GetSubmissionResult(int submissionId);
    }
}
