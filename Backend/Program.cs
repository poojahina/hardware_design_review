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
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hardware Reviewer API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

app.Run();
