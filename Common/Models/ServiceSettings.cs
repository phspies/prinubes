using MySqlConnector;

namespace Prinubes.Common.Models
{
    public class ServiceSettings
    {
        public MySqlConnectionStringBuilder GetMysqlConnection()
        {
            return new MySqlConnectionStringBuilder()
            {
                Server = MYSQL_SERVER,
                Port = MYSQL_PORT ?? 0,
                Database = MYSQL_DATABASE,
                UserID = MYSQL_USER,
                Password = MYSQL_PASSWORD,
            };
        }
        public ServiceSettings(string? _KAFKA_CONSUMER_GROUP_ID = null, string? _MYSQL_DATABASE = null)
        {
            if (Environment.GetEnvironmentVariable("MYSQL_DATABASE") == null)
            {
                JWT_SECRET = "EBOHltOtlvsBorhGvfOxq27k5X334nYU";
                JWT_EXPIRE_TIME = 100;
                MYSQL_SERVER = "10.0.1.23";
                MYSQL_PORT = 3306;
                MYSQL_DATABASE = _MYSQL_DATABASE ?? "prinubes";
                MYSQL_USER = "root";
                MYSQL_PASSWORD = "1qazxsw2";
                REDIS_CACHE_HOST = "10.0.1.28";
                REDIS_CACHE_PORT = 6379;
                REDIS_CACHE_PASSWORD = "";
                REDIS_TTL = 30;
                KAFKA_BOOTSTRAP = "10.0.1.24";
                KAFKA_IDEMPOTENCE = true;
                KAFKA_RETRIES = 20;
                KAFKA_CONSUMER_GROUP_ID = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                KAFKA_ENABLE_AUTO_COMMIT = false;
            }
            else
            {
                foreach (var prop in typeof(ServiceSettings).GetProperties())
                {
                    if (prop.Name == "KAFKA_CONSUMER_GROUP_ID")
                    {
                        prop.SetValue(this, Convert.ToString(_KAFKA_CONSUMER_GROUP_ID ?? System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name));
                    }
                    else
                    {
                        if (Environment.GetEnvironmentVariable(prop.Name) != null)
                        {
                            if (prop.PropertyType == typeof(double))
                            {
                                prop.SetValue(this, Convert.ToDouble(Environment.GetEnvironmentVariable(prop.Name)));
                            }
                            else if (prop.PropertyType == typeof(int))
                            {
                                prop.SetValue(this, Convert.ToInt16(Environment.GetEnvironmentVariable(prop.Name)));
                            }
                            else if (prop.PropertyType == typeof(uint))
                            {
                                prop.SetValue(this, Convert.ToUInt16(Environment.GetEnvironmentVariable(prop.Name)));
                            }
                            else if (prop.PropertyType == typeof(bool))
                            {
                                prop.SetValue(this, Convert.ToBoolean(Environment.GetEnvironmentVariable(prop.Name)));
                            }
                            else if (prop.PropertyType == typeof(string))
                            {
                                prop.SetValue(this, Convert.ToString(Environment.GetEnvironmentVariable(prop.Name)));
                            }
                        }
                        else
                        {
                            throw new Exception($"Environment variable {prop.Name} does not exist");
                        }
                    }
                }
            }
        }
        public string? JWT_SECRET { get; set; }
        public double? JWT_EXPIRE_TIME { get; set; }
        public int? KAFKA_RETRIES { get; set; }
        public string? MYSQL_SERVER { get; set; }
        public uint? MYSQL_PORT { get; set; }
        public string? MYSQL_DATABASE { get; set; }
        public string? MYSQL_USER { get; set; }
        public string? MYSQL_PASSWORD { get; set; }
        public string? REDIS_CACHE_HOST { get; set; }
        public int? REDIS_CACHE_PORT { get; set; }
        public string? REDIS_CACHE_PASSWORD { get; set; }
        public int? REDIS_TTL { get; set; }
        public string? KAFKA_BOOTSTRAP { get; set; }
        public bool? KAFKA_IDEMPOTENCE { get; set; }
        public string? KAFKA_CONSUMER_GROUP_ID { get { return MYSQL_DATABASE; } set { } }
        public bool? KAFKA_ENABLE_AUTO_COMMIT { get; set; }
    }
}
