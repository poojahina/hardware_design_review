using Backend.Agents;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();

app.Run();
