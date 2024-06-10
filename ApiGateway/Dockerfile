FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["ApiGateway.csproj", "."]
RUN dotnet restore "./ApiGateway.csproj"

COPY . .
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "ApiGateway.dll"]