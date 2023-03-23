
using BackgroundProcessing.Data;
using BackgroundProcessing.Service;
using Microsoft.EntityFrameworkCore;

namespace BackgroundProcessing.Api
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //if (app.Environment.IsDevelopment())
            //{
            //   
            //}

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            // Add Consomose Db support
            builder.Services.AddDbContextFactory<BackgroundProcessingContext>(optionsBuilder =>
                optionsBuilder.UseCosmos(
                    connectionString: config["CosmosConnectionStrings"],
                    databaseName: "BackgroundProcessingDb",
                    cosmosOptionsAction: options =>
                    {
                        options.ConnectionMode(Microsoft.Azure.Cosmos.ConnectionMode.Direct);
                        options.MaxRequestsPerTcpConnection(20);
                    })
            );

            // Register services
            builder.Services.AddTransient<BackgroundProcessingDataService>();


            // Registering a console logger for PoC
            builder.Services.AddSingleton<BackgroundProcessing.Service.WriteLine>((text, highlight, isException) =>
            {
                if (isException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (highlight)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                Console.WriteLine();
                Console.WriteLine(text);
                Console.ResetColor();
            });

            var app = builder.Build();


            app.UseSwagger();
            app.UseSwaggerUI();


            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run(); 
        }
    }
}