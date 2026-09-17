using Backend.Agents;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddHttpClient<AzureOpenAIService>();
builder.Services.AddScoped<CircuitAgent>();
builder.Services.AddScoped<DatasheetAgent>();
builder.Services.AddScoped<ComplianceAgent>();
builder.Services.AddScoped<ReviewerAgent>();
builder.Services.AddScoped<ReviewOrchestrator>();

var app = builder.Build();

app.UseCors();
app.MapControllers();

app.Run();
