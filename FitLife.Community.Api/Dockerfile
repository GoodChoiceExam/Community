FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FitLife.Community.slnx .
COPY FitLife.Community.Api/FitLife.Community.Api.csproj FitLife.Community.Api/
COPY FitLife.Community.Tests/FitLife.Community.Tests.csproj FitLife.Community.Tests/
RUN dotnet restore FitLife.Community.slnx

COPY . .
RUN dotnet test FitLife.Community.Tests/FitLife.Community.Tests.csproj --no-restore
RUN dotnet publish FitLife.Community.Api/FitLife.Community.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FitLife.Community.Api.dll"]
