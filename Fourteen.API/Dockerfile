FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Genderize.API/Genderize.API.csproj", "Genderize.API/"]
COPY ["Genderize.Application/Genderize.Application.csproj", "Genderize.Application/"]
COPY ["Genderize.Infrastructure/Genderize.Infrastructure.csproj", "Genderize.Infrastructure/"]
COPY ["Genderize.Domain/Genderize.Domain.csproj", "Genderize.Domain/"]
RUN dotnet restore "Genderize.API/Genderize.API.csproj"
COPY . .
WORKDIR "/src/Genderize.API"
RUN dotnet build "Genderize.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Genderize.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genderize.API.dll"]