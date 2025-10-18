using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Services;
using Api_Portar_Paciente.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrequentQuestionsController : ControllerBase<FrequentQuestions, FrequentQuestionsDto>
    {
        public FrequentQuestionsController(IFrequentQuestionsService service) : base(service)
        {
        }
    }
}