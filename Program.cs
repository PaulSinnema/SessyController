using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;
using SessyController.Common;
using SessyController.Extensions;
using SessyController.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Bind Sessy configuration
builder.Configuration.AddUserSecrets<Program>();
builder.Services.Configure<SessyBatteryConfig>(builder.Configuration.GetSection("Sessy:Batteries"));

builder.Services.AddHttpClient();

builder.Services.AddTransient(typeof(LoggingService<>));
builder.Services.AddSingleton<DayAheadMarketService>();
builder.Services.AddScoped<SessyService>();
builder.Services.AddScoped<BatteriesService>();

builder.Services.AddHostedService(provider => provider.GetRequiredService<DayAheadMarketService>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        // Log de fout
        var detailedException = exception.ToDetailedString();
        Console.WriteLine(detailedException);
        context.Response.StatusCode = 500;

        if(app.Environment.IsDevelopment())
            await context.Response.WriteAsync($"Internal Server Error\n\n{detailedException}");
        else
            await context.Response.WriteAsync($"Internal Server Error");
    });
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
