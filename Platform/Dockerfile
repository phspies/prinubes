#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:latest AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /src

COPY ["ComputePlatform/ComputePlatform.csproj", "ComputePlatform/"]
RUN dotnet restore "ComputePlatform/ComputePlatform.csproj"
COPY . .
WORKDIR "/src/ComputePlatform"
RUN dotnet build "ComputePlatform.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "ComputePlatform.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ["dotnet", "dev-certs", "https", "--trust"]
ENTRYPOINT ["dotnet", "ComputePlatform.dll"]
