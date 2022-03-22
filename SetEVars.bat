set MYSQL_SERVER=10.0.0.103
set MYSQL_PORT=3306
set MYSQL_DATABASE=prinubes_identity
set MYSQL_USER=root
set MYSQL_PASSWORD=VMware1!
set REDIS_CACHE_USE=true
set REDIS_CACHE_HOST=10.0.0.103
set REDIS_CACHE_PORT=6379
set REDIS_CACHE_PASSWORD=VMware1!
set KAFKA_BOOTSTRAP=10.0.0.103
set KAFKA_IDEMPOTENCE=true

#dotnet ef migrations add InitialCreate
dotnet ef database update