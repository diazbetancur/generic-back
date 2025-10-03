using CC.Domain.Dtos;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrecuentQuestionsController : ControllerBase
    {
        private readonly IFrecuentQuestionsService _frecuentQuestionsService;

        public FrecuentQuestionsController(IFrecuentQuestionsService frecuentQuestionsService)
        {
            _frecuentQuestionsService = frecuentQuestionsService;
        }

        /// <summary>
        /// GET api/FrecuentQuestions
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _frecuentQuestionsService.GetAllAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// GET api/FrecuentQuestions/c5b257e0-e73f-4f34-a30c-c0e139ad8e58
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            return Ok(await _frecuentQuestionsService.FindByIdAsync(id).ConfigureAwait(false));
        }

        /// <summary>
        /// POST api/FrecuentQuestions
        /// </summary>
        /// <param frecuentQuestions="FrecuentQuestionsDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(FrecuentQuestionsDto frecuentQuestions)
        {
            var response = await _frecuentQuestionsService.AddAsync(frecuentQuestions).ConfigureAwait(false);
            return Ok(response);
        }

        /// <summary>
        /// PUT api/FrecuentQuestions/c5b257e0-e73f-4f34-a30c-c0e139ad8e58
        /// </summary>
        /// <param frecuentQuestions="FrecuentQuestionsDto"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Put(FrecuentQuestionsDto frecuentQuestions)
        {
            var response = await _frecuentQuestionsService.FindByIdAsync(frecuentQuestions.Id).ConfigureAwait(false);

            if (response == null)
                return BadRequest();

            frecuentQuestions.DateCreated = response.DateCreated;
            await _frecuentQuestionsService.UpdateAsync(frecuentQuestions).ConfigureAwait(false);
            return Ok(frecuentQuestions);
        }

        /// <summary>
        /// DELETE api/FrecuentQuestions
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _frecuentQuestionsService.FindByIdAsync(id).ConfigureAwait(false);

            if (response == null)
                return BadRequest();

            await _frecuentQuestionsService.DeleteAsync(response).ConfigureAwait(false);
            return Ok(response);
        }
    }
}