FROM mcr.microsoft.com/dotnet/sdk:3.1.412 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj /app
RUN dotnet restore

# Copy everything else and build
COPY . /app
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:3.1
ENV ASPNETCORE_URLS=http://*:5001
ENV ASPNETCORE_ENVIRONMENT="production"

EXPOSE 5000
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "eVoucher.dll"]