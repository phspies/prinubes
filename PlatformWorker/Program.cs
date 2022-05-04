using AutoMapper;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Prinubes.Common.Helpers;
using Prinubes.Common.Kafka;
using Prinubes.Common.Kafka.Consumer;
using Prinubes.Common.Kafka.Producer;
using Prinubes.Common.Models;
using Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware;
using Prinubes.PlatformWorker.Datamodels;

var builder = WebApplication.CreateBuilder(args);


builder.WebHost.UseKestrel(options =>
{
    //options.Limits.MaxConcurrentConnections = 100;
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
});

//Build ServiceSettings object from environment variables
ServiceSettings serviceSettings = new ServiceSettings(_MYSQL_DATABASE: "prinubes_platformworker");
builder.Services.AddSingleton<ServiceSettings>(serviceSettings);
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AutoMapperProfile());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddLogging(StartupFactory.LoggingBuilder());
ILogger<Program> logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
using ILoggerFactory loggerFactory = LoggerFactory.Create(StartupFactory.LoggingBuilder());



if (!args.Any(x => x.ToLower().Contains("testing")))
{
    logger.LogInformation("MYSQL Details: {0}@{1}:{2}/{3}", serviceSettings.MYSQL_USER, serviceSettings.MYSQL_SERVER, serviceSettings.MYSQL_PORT, serviceSettings.MYSQL_DATABASE);

    //setup database connection and logging
    builder.Services.AddDbContextPool<PrinubesPlatformWorkerDBContext>((serviceProvider, optionsBuilder) =>
    {
        optionsBuilder.UseLoggerFactory(LoggerFactory.Create(StartupFactory.LoggingBuilder()));
        optionsBuilder.UseMySql(serviceSettings.GetMysqlConnection().ConnectionString, ServerVersion.AutoDetect(serviceSettings.GetMysqlConnection().ConnectionString));
    });

    //perform migrations
    builder.Services.BuildServiceProvider().GetRequiredService<PrinubesPlatformWorkerDBContext>().MigrateIfRequired(); ;

}


//Kafka producer
var kafkaProducerConfig = new ProducerConfig() { BootstrapServers = serviceSettings.KAFKA_BOOTSTRAP, EnableIdempotence = serviceSettings.KAFKA_IDEMPOTENCE, MessageSendMaxRetries = serviceSettings.KAFKA_RETRIES };
builder.Services.AddSingleton<ProducerConfig>(kafkaProducerConfig);
builder.Configuration.Bind("producer", kafkaProducerConfig);
builder.Services.AddKafkaProducer();

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

//redis caching
builder.Services.CachingBuilder(serviceSettings);

var app = builder.Build();


app.Run();

