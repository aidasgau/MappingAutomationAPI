using MappingAutomationAPI.Models;
using MappingAutomationAPI.Services;
using Microsoft.EntityFrameworkCore;
using MappingAutomationAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAI"));
// builder.Services.Configure<TestSettings>(builder.Configuration.GetSection("TestSettings"));

builder.Services.AddDbContext<VectorDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PgVectorDb"),
        o => o.UseVector()
    )
);

builder.Services.AddScoped<OpenAIService>();
builder.Services.AddScoped<VectorDbService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
