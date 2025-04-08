using Microsoft.Azure.Cosmos;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NixersDB;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using NixersDB;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

//  For detailed logs on azure
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); 
builder.Logging.SetMinimumLevel(LogLevel.Debug); 


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddDbContext<NixersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var accountEndpoint = configuration["CosmosDb:AccountEndpoint"];
    var accountKey = configuration["CosmosDb:AccountKey"];
    return new CosmosClient(accountEndpoint, accountKey);
});


// string connectionString = builder.Configuration.GetConnectionString("AzureStorage");

// builder.Services.AddSingleton<BlobStorageService>();
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
builder.Services.AddSingleton(x =>
{
    var connectionString = builder.Configuration["AzureStorage:ConnectionString"];
    return new BlobServiceClient(connectionString);
});




builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });

    c.OperationFilter<FileUploadOperationFilter>(); 

    c.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});

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
