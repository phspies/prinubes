#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
#force webhook`ff df 

FROM mcr.microsoft.com/dotnet/sdk:latest AS base
WORKDIR /app
EXPOSE 443
#
FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /src


COPY ["Identity/Identity.csproj", "Identity/"]
RUN dotnet restore "Identity/Identity.csproj"
COPY . .
WORKDIR "/src/Identity"
RUN dotnet build "Identity.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "Identity.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ["dotnet", "dev-certs", "https", "--trust"]
ENTRYPOINT ["dotnet run", "Identity.dll"]

