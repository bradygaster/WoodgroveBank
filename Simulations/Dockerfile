#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 11111
EXPOSE 30000

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Simulations/Simulations.csproj", "Simulations/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "Simulations/Simulations.csproj"
COPY . .
WORKDIR "/src/Simulations"
RUN dotnet build "Simulations.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Simulations.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Simulations.dll"]