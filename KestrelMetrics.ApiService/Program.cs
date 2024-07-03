using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxConcurrentConnections = 128;
    o.Limits.MaxRequestBodySize = 1024 * 1024;
});

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.Logging.SetMinimumLevel(LogLevel.Trace);

// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/hang", async context =>
{
    var delay = TimeSpan.FromSeconds(Convert.ToInt32(context.Request.Query["delay"]));
    await Task.Delay(delay).WaitAsync(context.RequestAborted);
});

app.MapPost("/login", async context =>
{
    var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("Login");

    logger.LogInformation("Login starting.");

    System.IO.Pipelines.ReadResult result = default;
    try
    {
        result = await context.Request.BodyReader.ReadAtLeastAsync(1024);

        logger.LogInformation($"Read data. IsCanceled: {result.IsCanceled}, IsCompleted: {result.IsCompleted}");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error reading data.");
        throw;
    }
    finally
    {
        context.Request.BodyReader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
    }
});

app.MapGet("/streaming", async context =>
{
    var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("Streaming");

    logger.LogInformation("Streaming starting.");

    var s = "";
    for (int i = 0; i < 100; i++)
    {
        s += "The quick brown fox jumps over the lazy dog.";
        s += Environment.NewLine;
    }

    for (int i = 0; i < 1000; i++)
    {
        logger.LogInformation("Writing data.");
        await context.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(s));
    }
});

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
