using CC.Domain.Dtos;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocTypeController : ControllerBase
    {
        private readonly IDocTypeService _docTypeService;

        public DocTypeController(IDocTypeService docTypeService)
        {
            _docTypeService = docTypeService;
        }

        /// <summary>
        /// GET api/docType
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _docTypeService.GetAllAsync(x => x.IsActive).ConfigureAwait(false));
        }

        /// <summary>
        /// GET api/docType/c5b257e0-e73f-4f34-a30c-c0e139ad8e58
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            return Ok(await _docTypeService.FindByIdAsync(id).ConfigureAwait(false));
        }

        /// <summary>
        /// POST api/docType
        /// </summary>
        /// <param DocTypeDto="docType"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(DocTypeDto docType)
        {
            var checkCode = await _docTypeService.GetAllAsync(x => x.Code.Trim().ToLower() == docType.Code.Trim().ToLower()).ConfigureAwait(false);

            if (checkCode.Any())
            {
                if (checkCode.First().IsActive)
                    return BadRequest("Codigo ya existe.");
                else
                    checkCode.First().IsActive = true;

                await _docTypeService.UpdateAsync(checkCode.First()).ConfigureAwait(false);
                return Ok(checkCode.First());
            }

            var response = await _docTypeService.AddAsync(docType).ConfigureAwait(false);
            return Ok(response);
        }

        /// <summary>
        /// PUT api/docType
        /// </summary>
        /// <param DocTypeDto="docType"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Put(DocTypeDto docType)
        {
            var checkCode = await _docTypeService
                .GetAllAsync(x => x.Code.Trim().ToLower() == docType.Code.Trim().ToLower()
                && x.Id != docType.Id).ConfigureAwait(false);

            if (checkCode.Any())
            {
                if (checkCode.First().IsActive)
                    return BadRequest("Codigo ya existe.");
                else
                    checkCode.First().IsActive = true;

                await _docTypeService.UpdateAsync(checkCode.First()).ConfigureAwait(false);
                docType.IsActive = false;
                await _docTypeService.UpdateAsync(docType).ConfigureAwait(false);
                return Ok(checkCode.First());
            }

            await _docTypeService.UpdateAsync(docType).ConfigureAwait(false);
            return Ok(docType);
        }

        /// <summary>
        /// DELETE api/docType
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _docTypeService.FindByIdAsync(id).ConfigureAwait(false);

            if (response == null)
                return BadRequest();

            response.IsActive = false;
            await _docTypeService.UpdateAsync(response).ConfigureAwait(false);
            return Ok(response);
        }
    }
}