#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 11111
EXPOSE 30000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ATM/ATM.csproj", "ATM/"]
RUN dotnet restore "ATM/ATM.csproj"
COPY . .
WORKDIR "/src/ATM"
RUN dotnet build "ATM.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ATM.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ATM.dll"]