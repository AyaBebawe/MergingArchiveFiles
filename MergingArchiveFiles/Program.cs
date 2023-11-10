using MergingArchiveFiles.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Configure logging to use Serilog
builder.Host.ConfigureLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog(new LoggerConfiguration()
        .MinimumLevel.Error()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
        .Enrich.FromLogContext()
        .WriteTo.File("Logs/mylog.txt", rollingInterval: RollingInterval.Day,retainedFileCountLimit: null,
    rollOnFileSizeLimit: true,restrictedToMinimumLevel: LogEventLevel.Verbose)
        .CreateLogger());
   


});
builder.Services.Configure<FormOptions>(o => o.ValueCountLimit = int.MaxValue);
builder.Services.Configure<FormOptions>(o => o.ValueLengthLimit = int.MaxValue);

builder.Services.Configure<FormOptions>(o => o.MemoryBufferThreshold = int.MaxValue);
builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 3L * 1024 * 1024 * 1024);


builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 3L * 1024 * 1024 * 1024; // Allow larger request body size
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 3L * 1024 * 1024 * 1024;
});
builder.Services.AddScoped<IMergeArchiveFilesService, MergeArchiveFilesService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
