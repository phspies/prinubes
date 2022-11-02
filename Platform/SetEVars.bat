set MYSQL_SERVER=192.168.0.103
set MYSQL_PORT=3306
set MYSQL_DATABASE=prinubes_platform
set MYSQL_USER=remote
set MYSQL_PASSWORD=VMware1!
set REDIS_CACHE_USE=true
set REDIS_CACHE_HOST=192.168.0.103
set REDIS_CACHE_PORT=6379
set REDIS_CACHE_PASSWORD=VMware1!
set KAFKA_BOOTSTRAP=192.168.0.103
set KAFKA_IDEMPOTENCE=true
set JWT_SECRET=EBOHltOtlvsBorhGvfOxq27k5X334nYU
set JWT_EXPIRE_TIME=100
set KAFKA_RETRIES=20
set REDIS_TTL=30
set KAFKA_ENABLE_AUTO_COMMIT=true
set BACKGROUND_WORKER_INTERVAL=10

dotnet ef migrations add PlatformTypeChange
rem dotnet ef database update
rem dotnet ef database update 0
rem dotnet ef migrations remove