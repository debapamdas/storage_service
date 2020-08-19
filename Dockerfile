FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS restore
WORKDIR /app
COPY . .
RUN dotnet restore

FROM restore as publish
WORKDIR /app
RUN dotnet publish --configuration Release --no-restore

FROM publish as test
WORKDIR /app/Nokia.Storage.Test
RUN dotnet test

FROM base AS final
WORKDIR /app
COPY --from=publish /app/Nokia.Storage/bin/Release/netcoreapp3.1/publish/ .
ENTRYPOINT ["dotnet", "Nokia.Storage.dll"]