FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG TARGETARCH
WORKDIR /source

COPY *.props *.targets ./
COPY samples/*.props samples/*.targets ./samples/
COPY samples/ActionsToolkit.Core.Sample/ActionsToolkit.Core.Sample.csproj samples/ActionsToolkit.Core.Sample/
COPY src/*.props src/*.targets ./src/
COPY src/ActionsToolkit.Core/ActionsToolkit.Core.csproj src/ActionsToolkit.Core/
COPY src/ActionsToolkit.Octokit/ActionsToolkit.Octokit.csproj src/ActionsToolkit.Octokit/
RUN dotnet restore "samples/ActionsToolkit.Core.Sample/ActionsToolkit.Core.Sample.csproj" -a $TARGETARCH

COPY . .
RUN dotnet publish "samples/ActionsToolkit.Core.Sample/ActionsToolkit.Core.Sample.csproj" -a $TARGETARCH --no-restore -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /app .
USER root
ENTRYPOINT ["/app/ActionsToolkit.Core.Sample"]
