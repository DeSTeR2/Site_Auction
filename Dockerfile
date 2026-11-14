# auction-app image: multi-stage build of the API (dotnet publish) on top of the ASP.NET runtime.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/Auction.Api/Auction.Api.csproj src/Auction.Api/
COPY src/Auction.Core/Auction.Core.csproj src/Auction.Core/
COPY src/Auction.Infrastructure/Auction.Infrastructure.csproj src/Auction.Infrastructure/
RUN dotnet restore src/Auction.Api/Auction.Api.csproj

COPY src/Auction.Api/ src/Auction.Api/
COPY src/Auction.Core/ src/Auction.Core/
COPY src/Auction.Infrastructure/ src/Auction.Infrastructure/
RUN dotnet publish src/Auction.Api/Auction.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Auction.Api.dll"]
