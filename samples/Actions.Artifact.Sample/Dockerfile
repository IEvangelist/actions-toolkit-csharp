#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["samples/Actions.Artifact.Sample/Actions.Artifact.Sample.csproj", "samples/Actions.Artifact.Sample/"]
COPY ["src/Actions.Core/Actions.Core.csproj", "src/Actions.Core/"]
COPY ["src/Actions.Octokit/Actions.Octokit.csproj", "src/Actions.Octokit/"]
COPY ["src/Actions.Artifact/Actions.Artifact.csproj", "src/Actions.Artifact/"]
RUN dotnet restore "samples/Actions.Artifact.Sample/Actions.Artifact.Sample.csproj"
COPY . .
WORKDIR "/src/samples/Actions.Artifact.Sample"
RUN dotnet build "Actions.Artifact.Sample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Actions.Artifact.Sample.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Actions.Artifact.Sample.dll"]