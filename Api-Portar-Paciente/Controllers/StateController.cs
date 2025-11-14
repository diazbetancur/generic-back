using Api_Portar_Paciente.Controllers.Base;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para gestión de estados
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StateController : ControllerBase<State, StateDto>
    {
        public StateController(IStateService service) : base(service)
        {
        }
    }
}