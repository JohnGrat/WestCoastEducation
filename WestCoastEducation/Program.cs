using Business.Dtos.Books;
using Business.Dtos.Comments;
using Business.Repositories.Default;
using Business.Repositories;
using Google.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Text;
using WestCoastEducation.Auth;
using WestCoastEducation.Config;
using WestCoastEducation.Endpoints;
using WestCoastEducation.EndPoints;
using WestCoastEducation.Helpers;
using DataAccess.Data;
using Google.Cloud.Firestore;
using System.Reflection;
using AutoMapper;
using System.Text.Json.Serialization;
using WestCoastEducation.Hubs;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

//Add JwtConfig
JwtConfig jwtConfig = configuration.GetSection(nameof(JwtConfig)).Get<JwtConfig>();
builder.Services.AddSingleton(jwtConfig);


//AutoMapper
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile(new Business.AutoMapperProfile());
});
var mapper = config.CreateMapper();
builder.Services.AddSingleton(mapper);

// For Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("IdentityDatabase")));
builder.Services.AddDbContext<BookstoreContext>(options => options.UseSqlServer(configuration.GetConnectionString("BookstoreDatabase")));


//Lowercase urls
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

//Dependency Inject Services
builder.Services.AddScoped<IJwtUtils, JwtUtils>();
builder.Services.AddScoped<IRepository<BookDto, BookBriefDto>, BookRepository>();
builder.Services.AddScoped<IRepository<CommentDto, CommentBriefDto>, CommentRepository>();

// For Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


//Avoid object cycle
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});



// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidAudience = jwtConfig.Audience,
        ValidIssuer = jwtConfig.Issuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
        ClockSkew = TimeSpan.FromSeconds(1),
        RequireExpirationTime = true,
        ValidateLifetime = true,
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs/Commenthub")))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();


//Add Firestore
var fireStore = new FirestoreDbBuilder
{
    ProjectId = configuration["Firestore:ProjectId"],
    JsonCredentials = fireStoreCred()
}.Build();
builder.Services.AddSingleton(fireStore);


//Add SignalR for the commentsHub
builder.Services.AddSignalR();


//AddSpa
builder.Services.AddSpaStaticFiles(configuration => {
    configuration.RootPath = "ClientApp/dist";
});

// Create the IServiceProvider instance
IServiceProvider serviceProvider = builder.Services.BuildServiceProvider();

// Initialize the ServiceLocator
ServiceLocator.Initialize(serviceProvider);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

//Add SignalR for the commentsHub
app.MapHub<CommentHub>("/hubs/Commenthub");

//Mapping endpoints
app.MapAuthEndpoints();
app.MapBookEndpoints();



var spaPath = "/app";
app.Map(new PathString(spaPath), client =>
{
    client.UseSpaStaticFiles();
    client.UseSpa(spa => {
        spa.Options.SourcePath = "ClientApp";
        spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                ResponseHeaders headers = ctx.Context.Response.GetTypedHeaders();
                headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    NoStore = true,
                    MustRevalidate = true
                };
            }
        };
    });
});

app.UseAuthentication();
app.UseAuthorization();



app.Run();


static string fireStoreCred()
{
    using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WestCoastEducation.firestore.json");
    using StreamReader reader = new(stream);
    return reader.ReadToEnd();
}