using Api_Portar_Paciente.Controllers.Base;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para gestión de tipos de solicitud
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestTypeController : ControllerBase<RequestType, RequestTypeDto>
    {
        public RequestTypeController(IRequestTypeService service) : base(service)
        {
        }
    }
}