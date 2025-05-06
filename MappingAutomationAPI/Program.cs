using MappingAutomationAPI.Models;
using MappingAutomationAPI.Services;
using Microsoft.EntityFrameworkCore;
using MappingAutomationAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// 1) Add configuration sections if you want to bind them, e.g.:
// builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAI"));
// builder.Services.Configure<TestSettings>(builder.Configuration.GetSection("TestSettings"));

// 2) Register your DbContext (must go before builder.Build())
builder.Services.AddDbContext<VectorDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PgVectorDb"),
        o => o.UseVector()  // enables pgvector extension
    )
);

// 3) Register your application services
builder.Services.AddScoped<OpenAIService>();
builder.Services.AddScoped<VectorDbService>();

// 4) Add framework services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5) Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// 6) Map controllers
app.MapControllers();

// 7) Run!
app.Run();
