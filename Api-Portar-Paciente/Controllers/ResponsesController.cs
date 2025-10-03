using CC.Aplication.Services;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponsesController : ControllerBase
    {
        private readonly IResponseQuestionService _responseQuestionService;
        public ResponsesController(IResponseQuestionService responseQuestionService)
        {
            _responseQuestionService = responseQuestionService;
        }

        /// <summary>
        /// GET api/ResponseQuestion
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync(Guid idQuestion)
        {
            return Ok(await _responseQuestionService.GetAllAsync(x=> x.QuestionId == idQuestion).ConfigureAwait(false));
        }

        /// <summary>
        /// POST api/ResponseQuestion
        /// </summary>
        /// <param ResponseQuestionDto="response"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(ResponseQuestionDto response)
        {
            var responseService = await _responseQuestionService.AddAsync(response).ConfigureAwait(false);
            return Ok(response);
        }

        /// <summary>
        /// POST api/ResponseQuestion
        /// </summary>
        /// <param ResponseQuestionDto="response"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Put(ResponseQuestionDto response)
        {
            var responseService = await _responseQuestionService.FindByIdAsync(response.Id).ConfigureAwait(false);

            if (response == null)
                return BadRequest();

            response.DateCreated = response.DateCreated;
            await _responseQuestionService.UpdateAsync(response).ConfigureAwait(false);
            return Ok(response);
        }

        /// <summary>
        /// DELETE api/FrecuentQuestions
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _responseQuestionService.FindByIdAsync(id).ConfigureAwait(false);

            if (response == null)
                return BadRequest();

            await _responseQuestionService.DeleteAsync(response).ConfigureAwait(false);
            return Ok(response);
        }
    }
}
