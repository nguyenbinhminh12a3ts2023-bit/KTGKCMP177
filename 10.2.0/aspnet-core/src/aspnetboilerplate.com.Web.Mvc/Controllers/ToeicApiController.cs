using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using aspnetboilerplate.com.Controllers;
using aspnetboilerplate.com.Toeic.Dto;
using aspnetboilerplate.com.Toeic.Services;
using Microsoft.AspNetCore.Mvc;

namespace aspnetboilerplate.com.Web.Controllers
{
    [Route("api/[controller]")]
    public class ToeicApiController : comControllerBase
    {
        private readonly IToeicExamAppService _examAppService;

        public ToeicApiController(IToeicExamAppService examAppService)
        {
            _examAppService = examAppService;
        }

        // GET: /api/toeicapi/exams
        [HttpGet("exams")]
        public async Task<IActionResult> GetExams()
        {
            var exams = await _examAppService.GetExams();
            return Ok(new { success = true, data = exams });
        }

        // GET: /api/toeicapi/exam/{id}
        [HttpGet("exam/{id}")]
        public async Task<IActionResult> GetExam(int id)
        {
            try
            {
                var exam = await _examAppService.GetExamForStudent(id);
                return Ok(new { success = true, data = exam });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        // POST: /api/toeicapi/submit
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitExam([FromBody] SubmitExamInputDto input)
        {
            try
            {
                var result = await _examAppService.SubmitExam(input);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: /api/toeicapi/result/{submissionId}
        [HttpGet("result/{submissionId}")]
        public async Task<IActionResult> GetResult(int submissionId)
        {
            try
            {
                var result = await _examAppService.GetSubmissionResult(submissionId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }
    }
}
