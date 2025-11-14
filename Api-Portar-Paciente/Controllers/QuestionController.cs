using CC.Aplication.Services;
using CC.Domain.Dtos;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly IResponseQuestionService _responseQuestionService;

        public QuestionController(IResponseQuestionService responseQuestionService, IQuestionService questionService)
        {
            _responseQuestionService = responseQuestionService;
            _questionService = questionService;
        }

        /// <summary>
        /// GET api/Questions
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _questionService.GetAllAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// GET api/Questions/c5b257e0-e73f-4f34-a30c-c0e139ad8e58
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            return Ok(await _questionService.FindByIdAsync(id).ConfigureAwait(false));
        }

        /// <summary>
        /// POST api/Questions
        /// </summary>
        /// <param QuestionDto="question"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(QuestionDto question)
        {
            var response = await _questionService.AddAsync(question).ConfigureAwait(false);
            return Ok(response);
        }

        /// <summary>
        /// PUT api/FrecuentQuestions/c5b257e0-e73f-4f34-a30c-c0e139ad8e58
        /// </summary>
        /// <param QuestionDto="question"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Put(QuestionDto question)
        {
            var response = await _questionService.FindByIdAsync(question.Id).ConfigureAwait(false);

            if (response == null)
                return BadRequest();

            question.DateCreated = response.DateCreated;
            await _questionService.UpdateAsync(question).ConfigureAwait(false);

            return Ok(question);
        }

        /// <summary>
        /// DELETE api/FrecuentQuestions
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _questionService.FindByIdAsync(id).ConfigureAwait(false);

            if (response == null)
                return BadRequest();

            var responsesToDelete = await _responseQuestionService.GetAllAsync(x => x.QuestionId == id).ConfigureAwait(false);

            if (responsesToDelete.Any())
                await _responseQuestionService.DeleteRangeAsync(responsesToDelete).ConfigureAwait(false);

            await _questionService.DeleteAsync(response).ConfigureAwait(false);
            return Ok(response);
        }
    }
}