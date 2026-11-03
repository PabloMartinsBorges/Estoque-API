# Fase Base para Execução
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# Fase de Compilação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Copia todos os .csproj para resolver as dependências.
COPY ["Estoque.Api/Estoque.Api.csproj", "Estoque.Api/"]
COPY ["Estoque.Application/Estoque.Application.csproj", "Estoque.Application/"]
COPY ["Estoque.Domain/Estoque.Domain.csproj", "Estoque.Domain/"]
COPY ["Estoque.Infra.Data/Estoque.Infra.Data.csproj", "Estoque.Infra.Data/"]
COPY ["Estoque.Infra.IoC/Estoque.Infra.Ioc.csproj", "Estoque.Infra.Ioc/"]

# 2. Restaura as dependências com base nos projetos
RUN dotnet restore "Estoque.Api/Estoque.Api.csproj"

# 3. Copia o restante do código-fonte de toda a solução
COPY . .

# 4. Compila o projeto da API
WORKDIR "/src/Estoque.Api"
RUN dotnet build "Estoque.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Fase de Publicação
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Estoque.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Imagem Final enxuta
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Estoque.Api.dll"]