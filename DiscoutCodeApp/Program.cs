using DiscountCodeApp.Core.Interfaces;
using DiscountCodeApp.Core.Services;
using DiscountCodeApp.Hubs;
using DiscountCodeApp.Infrastructure.Generator;
using DiscountCodeApp.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true); // Dev only
    });
});

builder.Services.AddScoped<IGenerateCodeService, GenerateCodeService>();
builder.Services.AddScoped<ICodeGenerator, CodeGenerator>();
builder.Services.AddScoped<IDiscountCodeRepository, DiscountCodeRepository>();

var app = builder.Build();

app.UseCors();

app.MapHub<DiscountHub>("/discount");

app.Run();
