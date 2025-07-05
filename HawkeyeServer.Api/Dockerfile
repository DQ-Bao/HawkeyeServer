FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY HawkeyeServer.Api.csproj .
RUN dotnet restore "HawkeyeServer.Api.csproj"
COPY . .
RUN dotnet build "HawkeyeServer.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HawkeyeServer.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HawkeyeServer.Api.dll"]
