using DocumentService.PolyglotPersistence.DBContext;
using DocumentService.PolyglotPersistence.IRepositories;
using DocumentService.PolyglotPersistence.IServices;
using DocumentService.PolyglotPersistence.Models;
using DocumentService.PolyglotPersistence.Models.ErrorModels;
using DocumentService.PolyglotPersistence.Repositories;
using DocumentService.PolyglotPersistence.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using System.Reflection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

//defined standard BSON representation for guid
#pragma warning disable CS0618 // Type or member is obsolete
BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
#pragma warning restore CS0618 // Type or member is obsolete

// configure limit for upload
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Register the Swagger generator, defining 1 or more Swagger documents
builder.Services.AddSwaggerGen(c =>
{
    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// check which database is selected and configure it (and its repository)
if (builder.Configuration.GetSection("DatabaseType").Value == "MongoDB")
{
    // define MongoDB server
    builder.Services.Configure<DatabaseSettings>(
        builder.Configuration.GetSection("DatabaseSettings"));
    builder.Services.AddSingleton<MongoDBContext>();
    builder.Services.AddScoped(typeof(IRepository), typeof(MongoRepository));
}
else if (builder.Configuration.GetSection("DatabaseType").Value == "PostgreSql")
{
    // define PostgreSql server
    builder.Services.AddDbContext<SqlContext>(options =>
        options.UseNpgsql(builder.Configuration.GetSection("DatabaseSettings").GetSection("ConnectionString").Value));
    builder.Services.AddScoped(typeof(IRepository), typeof(SqlRepository));
}
else if (builder.Configuration.GetSection("DatabaseType").Value == "MySql")
{
    // define MySql server
    builder.Services.AddDbContext<SqlContext>(options =>
        options.UseMySQL(builder.Configuration.GetSection("DatabaseSettings").GetSection("ConnectionString").Value));
    builder.Services.AddScoped(typeof(IRepository), typeof(SqlRepository));
}
else
{
    // define MicrosoftSql server
    builder.Services.AddDbContext<SqlContext>(options =>
        options.UseSqlServer(builder.Configuration.GetSection("DatabaseSettings").GetSection("ConnectionString").Value));
    builder.Services.AddScoped(typeof(IRepository), typeof(SqlRepository));
}

// controller services
builder.Services.AddScoped<IDocumentServices, DocumentServices>();
builder.Services.AddScoped<IFolderServices, FolderServices>();
builder.Services.AddScoped<IPermissionServices, PermissionServices>();

// helper services
builder.Services.AddScoped<IAzureBlobServices, AzureBlobServices>();
builder.Services.AddScoped<IElasticSearchServices, ElasticSearchServices>();

builder.Services.AddMvc();

// if using Kestrel
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

// If using IIS:
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

// configure authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
    )
    .AddJwtBearer(options =>
    {
        // base-address of your IdentityServer
        options.Authority = builder.Configuration.GetSection("IdentityServer").GetSection("Authority").Value;
        options.Audience = "documentservice";

        options.TokenValidationParameters.ValidateIssuer = false;
        options.TokenValidationParameters.ValidateLifetime = false;

        // audience is optional
        options.TokenValidationParameters.ValidateAudience = false;

        // it's recommended to check the type header to avoid "JWT confusion" attacks
        options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
    });

WebApplication app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>(); // for uncought error handling

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(builder =>
{
    builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
