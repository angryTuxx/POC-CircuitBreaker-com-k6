using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Poc_Refit_CircuitBreaker_Receiver
{
    [ApiController]
    [Route("api/poc")]
    public class controller : ControllerBase
    {
        private readonly ILogger<controller> _logger;
        
        private Random _random = new();
        private static bool _instabilidade, _indisponibilidade;
        private static readonly object Result = new
        {
            StatusCode = 200,
            Message = "Qualquer mensagem",
            Success = false,
        };

        public controller(ILogger<controller> logger)
        {
            _logger = logger;
        }

        [HttpPost("external")]
        public async Task<IActionResult> External([FromBody] ExternalRequest request)
        {
            await Task.Delay(_random.Next(50, 100));
            if (_instabilidade)
            {
                await Task.Delay(_random.Next(150, 300));
                return Ok();
            }

            if (_indisponibilidade)
            {
                return StatusCode(500, "Erro Simulado");
            }
            
            return Ok(Result);
        }
        
        [HttpGet("instable")]
        public IActionResult Instabilidade()
        {
            _instabilidade = !_instabilidade;
            _logger.LogInformation("Iniciando fluxo com instabilidade.");
            return Ok();
        }
        
        [HttpGet("unavailable")]
        public IActionResult Indisponibilidade()
        {
            _indisponibilidade = !_indisponibilidade;
            _logger.LogInformation("Iniciando fluxo com indisponibilidade.");
            return Ok();
        }
        
        [HttpGet("clear")]
        public IActionResult Clear()
        {
            _indisponibilidade = false;
            _instabilidade = false;
            _logger.LogInformation("Instabilidade e Indisponibilidade resetados.");
            return Ok();
        }
    }
}