#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DocumentService.PolyglotPersistence.csproj", "."]
RUN dotnet restore "./DocumentService.PolyglotPersistence.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "DocumentService.PolyglotPersistence.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DocumentService.PolyglotPersistence.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DocumentService.PolyglotPersistence.dll"]