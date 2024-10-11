using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Quiz_DataBank.Classes;
using System.Text;
using LkDataConnection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Quiz_DataBank;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });

builder.Services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });
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

    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Program>()
                      .UseKestrel();
        });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<Quiz_DataBank.Classes.Connection>();
EncryptYourConnection encryptor = new EncryptYourConnection();
encryptor.EncryptAndDisplayConnectionString();

var app = builder.Build();
app.UseHttpsRedirection();

app.UseCors("ReactConnection");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

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


app.UseStaticFiles();
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(
//        Path.Combine(Directory.GetCurrentDirectory(), "public")),
//    RequestPath = "/public"
//});


//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(
//        Path.Combine(builder.Environment.ContentRootPath, "public")),
//    RequestPath = "/public"
//});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
