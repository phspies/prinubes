#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:latest AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /src

COPY ["Platform/Platform.csproj", "Platform/"]
RUN dotnet restore "Platform/Platform.csproj"
COPY . .
WORKDIR "/src/Platform"
RUN dotnet build "Platform.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "Platform.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ["dotnet", "dev-certs", "https", "--trust"]
ENTRYPOINT ["dotnet", "Platform.dll"]
