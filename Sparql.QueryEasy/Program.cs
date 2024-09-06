using Microsoft.AspNetCore.Hosting.Builder;
using Sparql.QueryEasy.Repositories;
using Sparql.QueryEasy.Services;
using Sparql.QueryEasy.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

builder.Services.AddKeyedScoped<IQueryExecutor, RemoteQueryExecutor>("Remote");
builder.Services.AddKeyedScoped<IQueryExecutor, LocalQueryExecutor>("Local");
builder.Services.AddHttpClient<IEndpointService, EndpointService>();

var app = builder.Build();

app.MapHealthChecks("/health");

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
