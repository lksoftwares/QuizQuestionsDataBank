using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Quiz_DataBank.Classes;
using System.Text;
using LkDataConnection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactConnection", policy =>
    {
        policy.WithOrigins("*")  
        .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowedToAllowWildcardSubdomains();  
    });
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<Quiz_DataBank.Classes.Connection>();



var app = builder.Build();

app.UseCors("ReactConnection");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Use(async (httpContext, next) =>
{
    try
    {
        httpContext.Request.EnableBuffering();
        string requestBody = await new StreamReader(httpContext.Request.Body, Encoding.UTF8).ReadToEndAsync();
        httpContext.Request.Body.Position = 0;
        Console.WriteLine($"Request body: {requestBody}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception reading request: {ex.Message}");
    }

    Stream originalBody = httpContext.Response.Body;
    try
    {
        using var memStream = new MemoryStream();
        httpContext.Response.Body = memStream;

      
        await next(httpContext);

        memStream.Position = 0;
        string responseBody = new StreamReader(memStream).ReadToEnd();

        memStream.Position = 0;
        await memStream.CopyToAsync(originalBody);
        Console.WriteLine(responseBody);
    }
    finally
    {
        httpContext.Response.Body = originalBody;
    }
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
