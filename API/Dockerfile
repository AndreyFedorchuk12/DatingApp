FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-dev
WORKDIR /app
EXPOSE 8080
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-dev /app/out .
ENTRYPOINT ["dotnet", "API.dll"]