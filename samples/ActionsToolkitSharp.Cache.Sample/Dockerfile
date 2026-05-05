FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG TARGETARCH
WORKDIR /source

COPY *.props *.targets ./
COPY samples/*.props samples/*.targets ./samples/
COPY samples/ActionsToolkitSharp.Cache.Sample/ActionsToolkitSharp.Cache.Sample.csproj samples/ActionsToolkitSharp.Cache.Sample/
COPY src/*.props src/*.targets ./src/
COPY src/ActionsToolkitSharp.Core/ActionsToolkitSharp.Core.csproj src/ActionsToolkitSharp.Core/
COPY src/ActionsToolkitSharp.HttpClient/ActionsToolkitSharp.HttpClient.csproj src/ActionsToolkitSharp.HttpClient/
COPY src/ActionsToolkitSharp.Cache/ActionsToolkitSharp.Cache.csproj src/ActionsToolkitSharp.Cache/
RUN dotnet restore "samples/ActionsToolkitSharp.Cache.Sample/ActionsToolkitSharp.Cache.Sample.csproj" -a $TARGETARCH

COPY . .
RUN dotnet publish "samples/ActionsToolkitSharp.Cache.Sample/ActionsToolkitSharp.Cache.Sample.csproj" -a $TARGETARCH --no-restore -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /app .
USER root
ENTRYPOINT ["/app/ActionsToolkitSharp.Cache.Sample"]
