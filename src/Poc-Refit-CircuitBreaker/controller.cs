using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Polly.CircuitBreaker;
using System;

namespace Poc_Refit_CircuitBreaker.Controllers
{
    [ApiController]
    [Route("api/poc")]
    public class controller : ControllerBase
    {
        private readonly IPocRefit _refit;
        private readonly IPocRefitResiliente _refitResiliente;
        private readonly ILogger<controller> _logger;

        public controller(IPocRefit refit,
            IPocRefitResiliente refitResiliente,
            ILogger<controller> logger
        )
        {
            _refitResiliente = refitResiliente;
            _refit = refit;
            _logger = logger;
        }

        [HttpGet("resiliencia")]
        public async Task<IActionResult> Circuit()
        {
            try
            {
                var result = await _refitResiliente.External(
                    new ExternalRequest
                    {
                        Anything = "Teste com circuit"
                    });

                if (result != null)
                    return Ok(result);

                return BadRequest("Resultado vazio ou inválido.");
            }
            catch (BrokenCircuitException)
            {
                string msg = "CIRCUIT BREAKER ABERTO - Fallback";
                _logger.LogWarning(msg);
                return StatusCode(200, new
                {
                    mensagem = "200 fallback",
                    detalhes = "Circuit breaker está aberto e não há cache disponível"
                });
            }
            catch (Refit.ApiException apiEx)
            {
                return StatusCode((int)apiEx.StatusCode, $"Erro externo: {apiEx.Content}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet("simples")]
        public async Task<IActionResult> Simples()
        {
            try
            {
                var result = await _refit.External(new ExternalRequest { Anything = "Teste" });

                if (result != null)
                    return Ok(result);

                return BadRequest("Resultado vazio ou inválido.");
            }
            catch (Refit.ApiException apiEx)
            {
                return StatusCode((int)apiEx.StatusCode, $"Erro externo: {apiEx.Content}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}