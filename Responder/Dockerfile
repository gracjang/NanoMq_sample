FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Responder/Responder.csproj", "Responder/"]
RUN dotnet restore "Responder/Responder.csproj"
COPY . .
WORKDIR "/src/Responder"
RUN dotnet build "Responder.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Responder.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Responder.dll"]
