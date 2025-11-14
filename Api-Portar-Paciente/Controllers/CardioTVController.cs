using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Services;
using Api_Portar_Paciente.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api_Portar_Paciente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CardioTVController : ControllerBase<CardioTV, CardioTVDto>
    {
        public CardioTVController(ICardioTVService service) : base(service)
        {
        }
    }
}