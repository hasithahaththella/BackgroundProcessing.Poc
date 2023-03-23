#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["BackgroundProcessing.Api/BackgroundProcessing.Api.csproj", "BackgroundProcessing.Api/"]
RUN dotnet restore "BackgroundProcessing.Api/BackgroundProcessing.Api.csproj"
COPY . .
WORKDIR "/src/BackgroundProcessing.Api"
RUN dotnet build "BackgroundProcessing.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BackgroundProcessing.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BackgroundProcessing.Api.dll"]