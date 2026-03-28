using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using StreamingPlatform.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace StreamingPlatform
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Încarcă datele (filme, seriale, documentare)
            ApiEndpoints.LoadData();

            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     STREAMZONE — PLATFORMĂ DE STREAMING                     ║");
            Console.WriteLine("║     Backend API + Frontend UI                                ║");
            Console.WriteLine("║     Student: Nichita | Grupa: TI-233 | UTM                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // Creăm web server-ul
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            // Servește fișierele statice din folderul UI/
            var uiPath = Path.Combine(Directory.GetCurrentDirectory(), "UI");
            if (Directory.Exists(uiPath))
            {
                 // Servește index.html ca pagină principală
                app.UseDefaultFiles(new DefaultFilesOptions
                {
                    FileProvider = new PhysicalFileProvider(uiPath),
                    DefaultFileNames = new List<string> { "index.html" }
                });

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(uiPath),
                    RequestPath = ""
                });

            }

            // Înregistrează endpoint-urile API
            ApiEndpoints.MapEndpoints(app);

            // Redirecționează "/" către index.html
            app.MapGet("/", (HttpContext context) =>
            {
                context.Response.Redirect("/index.html");
                return Task.CompletedTask;
            });

            Console.WriteLine("  ✅ API pornit cu succes!");
            Console.WriteLine("  ┌─────────────────────────────────────────┐");
            Console.WriteLine("  │  Deschide în browser:                   │");
            Console.WriteLine("  │  http://localhost:5000                   │");
            Console.WriteLine("  │                                         │");
            Console.WriteLine("  │  API Endpoints:                         │");
            Console.WriteLine("  │  http://localhost:5000/api/movies        │");
            Console.WriteLine("  │  http://localhost:5000/api/series        │");
            Console.WriteLine("  │  http://localhost:5000/api/documentaries │");
            Console.WriteLine("  │  http://localhost:5000/api/content       │");
            Console.WriteLine("  │  http://localhost:5000/api/genres        │");
            Console.WriteLine("  │  http://localhost:5000/api/stats         │");
            Console.WriteLine("  │  http://localhost:5000/api/search?q=dark │");
            Console.WriteLine("  │  http://localhost:5000/api/ratings/Inception │");
            Console.WriteLine("  │                                         │");
            Console.WriteLine("  │  Apasă Ctrl+C pentru a opri serverul    │");
            Console.WriteLine("  └─────────────────────────────────────────┘");
            Console.WriteLine();

            // Deschide browser-ul automat
System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
{
    FileName = "http://localhost:5000",
    UseShellExecute = true
});

app.Run("http://localhost:5000");

        }
    }
}
