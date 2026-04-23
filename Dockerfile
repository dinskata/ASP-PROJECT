FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ASP PROJECT.csproj", "./"]
COPY ["NuGet.Config", "./"]
RUN dotnet restore "ASP PROJECT.csproj"

COPY . .
RUN dotnet publish "ASP PROJECT.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000} dotnet \"ASP PROJECT.dll\""]
