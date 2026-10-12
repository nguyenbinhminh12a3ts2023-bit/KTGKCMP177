using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.AspNetCore.Mvc.Controllers;
using aspnetboilerplate.com.Controllers;
using aspnetboilerplate.com.Toeic.Dto;
using aspnetboilerplate.com.Toeic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace aspnetboilerplate.com.Web.Controllers
{
    public class ToeicAdminController : comControllerBase
    {
        private readonly IToeicExamAppService _examAppService;

        public ToeicAdminController(IToeicExamAppService examAppService)
        {
            _examAppService = examAppService;
        }

        // 1. Quản lý đề thi (Figma #4:9787)
        public async Task<IActionResult> Index()
        {
            var exams = await _examAppService.GetExams();
            return View(exams);
        }

        // 2. Nhập đề từ Word (Figma #4:9943)
        public IActionResult Import()
        {
            return View();
        }

        // Upload handler
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile wordFile)
        {
            if (wordFile == null || wordFile.Length == 0)
            {
                ViewBag.ErrorMessage = "Vui lòng chọn tệp Word (.docx) hợp lệ.";
                return View("Import");
            }

            if (!wordFile.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = "Chỉ chấp nhận tệp có định dạng .docx";
                return View("Import");
            }

            DocxValidationResultDto validationResult;
            using (var stream = new MemoryStream())
            {
                await wordFile.CopyToAsync(stream);
                stream.Position = 0;
                validationResult = _examAppService.ValidateDocxStream(stream);
            }

            validationResult.ExamTitle = string.IsNullOrWhiteSpace(validationResult.ExamTitle)
                ? Path.GetFileNameWithoutExtension(wordFile.FileName)
                : validationResult.ExamTitle;

            if (!validationResult.IsValid)
            {
                // Màn hình "Lỗi định dạng" (Figma #4:10071)
                ViewBag.FileName = wordFile.FileName;
                return View("FormatError", validationResult);
            }

            // Màn hình "Kiểm tra thành công" (Figma #4:10191)
            ViewBag.FileName = wordFile.FileName;
            var examJson = JsonSerializer.Serialize(validationResult.ParsedExam);
            HttpContext.Session.SetString("ValidatedExamJson", examJson);
            TempData["ValidatedExamJson"] = examJson;
            TempData["ValidatedFileName"] = wordFile.FileName;

            return View("ValidationSuccess", validationResult);
        }

        // Save Validated Exam to DB
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveValidatedExam()
        {
            var json = HttpContext.Session.GetString("ValidatedExamJson");
            if (string.IsNullOrEmpty(json))
            {
                json = TempData["ValidatedExamJson"] as string;
            }

            if (string.IsNullOrEmpty(json))
            {
                return RedirectToAction("Index");
            }

            var examDto = JsonSerializer.Deserialize<ToeicExamDto>(json);
            var saveInput = new SaveExamInputDto
            {
                Title = examDto.Title,
                Description = examDto.Description,
                DurationMinutes = examDto.DurationMinutes,
                ExamData = examDto
            };

            var saved = await _examAppService.SaveExam(saveInput);
            return RedirectToAction("Index");
        }

        // Alias in case ConfirmSave is called
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ConfirmSave()
        {
            return await SaveValidatedExam();
        }

        // 3. Xem trước đề thi (Figma #4:10337)
        public async Task<IActionResult> Preview(int id)
        {
            var exam = await _examAppService.GetExamById(id);
            return View(exam);
        }

        // 4. Chỉnh sửa đề thi (Figma #4:10505)
        public async Task<IActionResult> Edit(int id, int? questionNum)
        {
            var exam = await _examAppService.GetExamById(id);
            ViewBag.ActiveQuestionNumber = questionNum ?? 101;
            return View(exam);
        }

        // AJAX Update Question
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateQuestion([FromBody] UpdateQuestionInputDto input)
        {
            try
            {
                var success = await _examAppService.UpdateQuestion(input);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
