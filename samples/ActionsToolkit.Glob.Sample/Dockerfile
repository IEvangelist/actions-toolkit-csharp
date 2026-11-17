FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:11.0.100 AS build
ARG TARGETARCH
WORKDIR /source

COPY *.props *.targets ./
COPY samples/*.props samples/*.targets ./samples/
COPY samples/ActionsToolkit.Glob.Sample/ActionsToolkit.Glob.Sample.csproj samples/ActionsToolkit.Glob.Sample/
COPY src/*.props src/*.targets ./src/
COPY src/ActionsToolkit.Core/ActionsToolkit.Core.csproj src/ActionsToolkit.Core/
COPY src/ActionsToolkit.Octokit/ActionsToolkit.Octokit.csproj src/ActionsToolkit.Octokit/
COPY src/ActionsToolkit.Glob/ActionsToolkit.Glob.csproj src/ActionsToolkit.Glob/
RUN dotnet restore "samples/ActionsToolkit.Glob.Sample/ActionsToolkit.Glob.Sample.csproj" -a $TARGETARCH

COPY . .
RUN dotnet publish "samples/ActionsToolkit.Glob.Sample/ActionsToolkit.Glob.Sample.csproj" -a $TARGETARCH --no-restore -o /app

FROM mcr.microsoft.com/dotnet/runtime:11.0
WORKDIR /app
COPY --from=build /app .
USER root
ENTRYPOINT ["/app/ActionsToolkit.Glob.Sample"]
