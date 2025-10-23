using System;
using Polly;
using System.Net.Http;
using Polly.CircuitBreaker;

namespace Poc_Refit_CircuitBreaker
{
    public static class circuitBreaker
    {
        public static AsyncCircuitBreakerPolicy<HttpResponseMessage> CreatePolicy()
        {
            return Policy
                .HandleResult<HttpResponseMessage>(r => 
                    (int)r.StatusCode >= 500 || 
                    r.StatusCode == System.Net.HttpStatusCode.RequestTimeout) 
                .Or<Exception>() 
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5, 
                    durationOfBreak: TimeSpan.FromSeconds(3),
                    onBreak: (result, duration) =>
                    {
                        ShowCircuitState("Open (onBreak)", ConsoleColor.Red);
                        Console.WriteLine($"Circuit vai ficar aberto por {duration.TotalSeconds}s");
                    },                            
                    onReset: () =>
                    {
                        ShowCircuitState("Closed (onReset)", ConsoleColor.Green);
                    },
                    onHalfOpen: () =>
                    {
                        ShowCircuitState("Half Open (onHalfOpen)", ConsoleColor.Yellow);
                    });
        }

        private static void ShowCircuitState(
            string descStatus, ConsoleColor backgroundColor)
        {
            var previousBackgroundColor = Console.BackgroundColor;
            var previousForegroundColor = Console.ForegroundColor;
            
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = ConsoleColor.Black;
            
            Console.Out.WriteLine($" ***** Estado do Circuito: {descStatus} **** ");
            
            Console.BackgroundColor = previousBackgroundColor;
            Console.ForegroundColor = previousForegroundColor;
        }
    }
}