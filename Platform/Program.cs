using AutoMapper;
using Confluent.Kafka;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prinubes.Common.Helpers;
using Prinubes.Common.Kafka;
using Prinubes.Common.Kafka.Consumer;
using Prinubes.Common.Kafka.Producer;
using Prinubes.Common.Models;
using Prinubes.Platforms.Datamodels;
using Prinubes.Platforms.Datamodels.Domain;
using System.Text;

  
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(options =>
{
    //options.Limits.MaxConcurrentConnections = 100;
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
});

//Build ServiceSettings object from environment variables
ServiceSettings serviceSettings = new(_MYSQL_DATABASE: "prinubes_platform");
builder.Services.AddSingleton<ServiceSettings>(serviceSettings);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentityMicroservice", Version = "v1" });
});
builder.Services.AddSwaggerGenNewtonsoftSupport();

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AutoMapperProfile());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddControllers().AddNewtonsoftJson(StartupFactory.MvcNewtonsoftJsonOptionsBuilder());

builder.Services.AddLogging(StartupFactory.LoggingBuilder());
ILogger<Program> logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
using ILoggerFactory loggerFactory = LoggerFactory.Create(StartupFactory.LoggingBuilder());




if (!args.Any(x => x.ToLower().Contains("testing")))
{
    logger.LogInformation("MYSQL Details: {0}@{1}:{2}/{3}", serviceSettings.MYSQL_USER, serviceSettings.MYSQL_SERVER, serviceSettings.MYSQL_PORT, serviceSettings.MYSQL_DATABASE);

    //setup database connection and logging
    builder.Services.AddDbContextPool<PrinubesPlatformDBContext>((serviceProvider, optionsBuilder) =>
    {
        optionsBuilder.UseLoggerFactory(LoggerFactory.Create(StartupFactory.LoggingBuilder()));
        optionsBuilder.UseMySql(serviceSettings.GetMysqlConnection().ConnectionString, ServerVersion.AutoDetect(serviceSettings.GetMysqlConnection().ConnectionString),
            mysqlOptions => mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null)
            );
    });

    //perform migrations
    builder.Services.BuildServiceProvider().GetRequiredService<PrinubesPlatformDBContext>().MigrateIfRequired(); ;

}

//redis caching
builder.Services.CachingBuilder(serviceSettings);


//Kafka producer
logger.LogInformation("KAFKA Details: {0}@{1}", serviceSettings.KAFKA_BOOTSTRAP, serviceSettings.KAFKA_CONSUMER_GROUP_ID);
builder.Services.AddKafkaProducer();
var kafkaProducerConfig = new ProducerConfig() { BootstrapServers = serviceSettings.KAFKA_BOOTSTRAP, EnableIdempotence = serviceSettings.KAFKA_IDEMPOTENCE, MessageSendMaxRetries = serviceSettings.KAFKA_RETRIES };
builder.Services.AddSingleton<ProducerConfig>(kafkaProducerConfig);
builder.Configuration.Bind("producer", kafkaProducerConfig);

//start kafka consumers
var consumerConfig = new ConsumerConfig()
{
    BootstrapServers = serviceSettings.KAFKA_BOOTSTRAP,
    GroupId = serviceSettings.KAFKA_CONSUMER_GROUP_ID,
    EnableAutoCommit = serviceSettings.KAFKA_ENABLE_AUTO_COMMIT,
    AllowAutoCreateTopics = true
};

builder.Services.AddSingleton<ConsumerConfig>(consumerConfig);
builder.Configuration.Bind("consumer", consumerConfig);
builder.Services.AddHostedService<KafkaWorker>();


builder.Services.AddKafkaConsumer(typeof(Program));
builder.Services.AddKafkaProducer();

// configure jwt authentication
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<JwtBearer<PrinubesPlatformDBContext>>();
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.EventsType = typeof(JwtBearer<PrinubesPlatformDBContext>);
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(serviceSettings.JWT_SECRET)),
        ValidateIssuer = false,
        ValidateAudience = false,
        LifetimeValidator = Prinubes.Common.Helpers.LifetimeValidatorHelper.LifetimeValidator
    };
}).AddCookie(options => options.LoginPath = "/identity/users/authenticate");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.SetIsOriginAllowed(origin => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
        });
});

var app = builder.Build();

//update routemap information for this service and publish to service bus
using (var scope = app.Services.CreateScope())
{
    var routePathDOA = new RoutePathDOA(scope.ServiceProvider);
    await routePathDOA.SyncronizeAsync(builder.Services.BuildServiceProvider().GetService<IActionDescriptorCollectionProvider>().ActionDescriptors.Items);
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



app.Run();