FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Fourteen.API/Fourteen.API.csproj", "Fourteen.API/"]
COPY ["Fourteen.Application/Fourteen.Application.csproj", "Fourteen.Application/"]
COPY ["Fourteen.Infrastructure/Fourteen.Infrastructure.csproj", "Fourteen.Infrastructure/"]
COPY ["Fourteen.Domain/Fourteen.Domain.csproj", "Fourteen.Domain/"]
RUN dotnet restore "Fourteen.API/Fourteen.API.csproj"
COPY . .
WORKDIR "/src/Fourteen.API"
RUN dotnet build "Fourteen.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fourteen.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fourteen.API.dll"]