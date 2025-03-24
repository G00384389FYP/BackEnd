using Microsoft.Azure.Cosmos;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NixersDB;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using NixersDB;

var builder = WebApplication.CreateBuilder(args);

//  For detailed logs on azure
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); 
builder.Logging.SetMinimumLevel(LogLevel.Debug); 


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddDbContext<NixersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

Console.WriteLine(" starting cosmos"); // debug for /users issue
builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var accountEndpoint = configuration["CosmosDb:AccountEndpoint"];
    var accountKey = configuration["CosmosDb:AccountKey"];
    return new CosmosClient(accountEndpoint, accountKey);
});

// Console.WriteLine(" starting blob"); // debug for /users issue
// string connectionString = builder.Configuration.GetConnectionString("AzureStorage");
// var blobServiceClient = new BlobServiceClient(connectionString);
// builder.Services.AddSingleton(blobServiceClient);
// builder.Services.AddSingleton<BlobStorageService>();
// Console.WriteLine("Blob Complte "); // debug for /users issue



builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAllOrigins");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


Console.WriteLine("App startup completed successfully"); // debug for /users issue


app.Run();
