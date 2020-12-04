FROM mcr.microsoft.com/azure-functions/dotnet:3.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Sample.Queue.Metrics.AzureFunction/Sample.Queue.Metrics.AzureFunction.csproj", "Sample.Queue.Metrics.AzureFunction/"]
COPY ["Sample.Core/Sample.Core.csproj", "Sample.Core/"]
RUN dotnet restore "Sample.Queue.Metrics.AzureFunction/Sample.Queue.Metrics.AzureFunction.csproj"
COPY . .
WORKDIR "/src/Sample.Queue.Metrics.AzureFunction"
RUN dotnet build "Sample.Queue.Metrics.AzureFunction.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sample.Queue.Metrics.AzureFunction.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true