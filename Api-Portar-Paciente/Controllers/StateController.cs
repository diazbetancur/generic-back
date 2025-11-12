using Api_Portar_Paciente.Controllers.Base;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para gestión de estados
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class StateController : ControllerBase<State, StateDto>
    {
        public StateController(IStateService service) : base(service)
        {
        }
    }
}
