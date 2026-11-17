FROM mcr.microsoft.com/dotnet/sdk:11.0.100@sha256:65f3c7c6a25a239ab3e6719f2896147e7b68242ff2d8818dd70498bde6ef8bea AS build
WORKDIR /source
COPY launcher/ ./
RUN ["dotnet", "publish", "ActionsToolkit.Runner.csproj", "--configuration", "Release", "--output", "/launcher", "--no-self-contained", "-p:UseAppHost=false"]

# Arbitrary file-based apps need the SDK at execution time, not just the runtime.
FROM mcr.microsoft.com/dotnet/sdk:11.0.100@sha256:65f3c7c6a25a239ab3e6719f2896147e7b68242ff2d8818dd70498bde6ef8bea
ENV DOTNET_NOLOGO=true \
    DOTNET_CLI_TELEMETRY_OPTOUT=true \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
COPY --from=build /launcher/ /opt/actions-toolkit/
WORKDIR /github/workspace
ENTRYPOINT ["dotnet", "/opt/actions-toolkit/ActionsToolkit.Runner.dll"]
