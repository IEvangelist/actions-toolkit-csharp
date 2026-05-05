FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG TARGETARCH
WORKDIR /source

COPY *.props *.targets ./
COPY samples/*.props samples/*.targets ./samples/
COPY samples/ActionsToolkitSharp.Artifact.Sample/ActionsToolkitSharp.Artifact.Sample.csproj samples/ActionsToolkitSharp.Artifact.Sample/
COPY src/*.props src/*.targets ./src/
COPY src/ActionsToolkitSharp.Core/ActionsToolkitSharp.Core.csproj src/ActionsToolkitSharp.Core/
COPY src/ActionsToolkitSharp.HttpClient/ActionsToolkitSharp.HttpClient.csproj src/ActionsToolkitSharp.HttpClient/
COPY src/ActionsToolkitSharp.Artifact/ActionsToolkitSharp.Artifact.csproj src/ActionsToolkitSharp.Artifact/
RUN dotnet restore "samples/ActionsToolkitSharp.Artifact.Sample/ActionsToolkitSharp.Artifact.Sample.csproj" -a $TARGETARCH

COPY . .
RUN dotnet publish "samples/ActionsToolkitSharp.Artifact.Sample/ActionsToolkitSharp.Artifact.Sample.csproj" -a $TARGETARCH --no-restore -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /app .
USER root
ENTRYPOINT ["/app/ActionsToolkitSharp.Artifact.Sample"]
