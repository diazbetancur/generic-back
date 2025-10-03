using CC.Domain.Dtos;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardioTVController : ControllerBase
    {
        private readonly ICardioTVService _cardioTVService;

        public CardioTVController(ICardioTVService cardioTVService)
        {
            _cardioTVService = cardioTVService;
        }

        /// <summary>
        /// GET api/cardioTV
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _cardioTVService.GetAllAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// GET api/cardioTV/c5b257e0-e73f-4f34-a30c-c0e139ad8e58
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            return Ok(await _cardioTVService.FindByIdAsync(id).ConfigureAwait(false));
        }

        /// <summary>
        /// POST api/cardioTV
        /// </summary>
        /// <param cardioTVDto="CardioTVDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(CardioTVDto cardioTVDto)
        {
            var response = await _cardioTVService.AddAsync(cardioTVDto).ConfigureAwait(false);
            return Ok(response);
        }

        /// <summary>
        /// PUT api/cardioTV
        /// </summary>
        /// <param cardioTVDto="CardioTVDto"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Put(CardioTVDto cardioTVDto)
        {
            var response = await _cardioTVService.FindByIdAsync(cardioTVDto.Id).ConfigureAwait(false);

            if (response == null)
                return BadRequest();

            cardioTVDto.DateCreated = response.DateCreated;
            await _cardioTVService.UpdateAsync(cardioTVDto).ConfigureAwait(false);
            return Ok(cardioTVDto);
        }

        /// <summary>
        /// DELETE api/CardioTV
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _cardioTVService.FindByIdAsync(id).ConfigureAwait(false);

            if (response == null)
                return BadRequest();

            await _cardioTVService.DeleteAsync(response).ConfigureAwait(false);
            return Ok(response);
        }
    }
}
