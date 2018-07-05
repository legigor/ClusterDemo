using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClusterDemo
{
    class Program
    {
        public static bool Initialized;
        static readonly AutoResetEvent _closing = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            StartWebApi();
            
            Initialize();
            Start();

            _closing.WaitOne();
        }

        private static void StartWebApi()
        {
            var builder = new WebHostBuilder().UseStartup<Program>();
            builder.UseKestrel(options => 
            {
                options.ListenAnyIP(5000);
            });
            var host = builder.Build();
            host.RunAsync();
        }

        public void ConfigureServices(IServiceCollection services) => services
            .AddMvcCore()
            .AddJsonFormatters();

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            // loggerFactory
            //     .AddConsole()
            //     .AddDebug();

            app
                .UseDeveloperExceptionPage()
                .UseMvc();
        }

        static void Start()
        {
            Task.Factory.StartNew(() =>
            {
                while (Initialized)
                {
                    // Log("PING");
                    Thread.Sleep(1000);
                }
            });
        }

        static void Initialize()
        {
            Log("Initializing . . . .");

            // docker stop <cid>
            AssemblyLoadContext.Default.Unloading += ctx => 
            {
                Initialized = false;
                Log("Unloading . . .");
                Thread.Sleep(10_000);                
                Log("Unloaded.");

                _closing.Set();
            };

            // Ctrl+C
            Console.CancelKeyPress += (s, e) => 
            {
                Initialized = false;
                Log("Canceling . . .");
                Thread.Sleep(5000);
                Log("Canceled.");

                _closing.Set();
            };

            // ...

            Thread.Sleep(5000);

            Initialized = true;
            Log("Initialized");
        }

        static void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now.ToString()} {message}");
        }
    }

    [ApiController]
    [Route("/health")]
    public class HealthApi : ControllerBase
    {
        [HttpGet]
        [Route("status")]
        public IActionResult Status()
        {
            if(Program.Initialized)
                return Ok("OK");
            
            return new ObjectResult("ERROR")
            {
                StatusCode = 500
            };
        }
    }
}
