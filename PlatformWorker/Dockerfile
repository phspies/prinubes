#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:latest AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /src

COPY ["PlatformWorker/PlatformWorker.csproj", "PlatformWorker/"]
RUN dotnet restore "PlatformWorker/PlatformWorker.csproj"
COPY . .
WORKDIR "/src/PlatformWorker"
RUN dotnet build "PlatformWorker.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "PlatformWorker.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ["dotnet", "dev-certs", "https", "--trust"]
ENTRYPOINT ["dotnet", "PlatformWorker.dll"]
