FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG TARGETARCH
WORKDIR /source

COPY *.props *.targets ./
COPY samples/*.props samples/*.targets ./samples/
COPY samples/ActionsToolkitSharp.Glob.Sample/ActionsToolkitSharp.Glob.Sample.csproj samples/ActionsToolkitSharp.Glob.Sample/
COPY src/*.props src/*.targets ./src/
COPY src/ActionsToolkitSharp.Core/ActionsToolkitSharp.Core.csproj src/ActionsToolkitSharp.Core/
COPY src/ActionsToolkitSharp.Octokit/ActionsToolkitSharp.Octokit.csproj src/ActionsToolkitSharp.Octokit/
COPY src/ActionsToolkitSharp.Glob/ActionsToolkitSharp.Glob.csproj src/ActionsToolkitSharp.Glob/
RUN dotnet restore "samples/ActionsToolkitSharp.Glob.Sample/ActionsToolkitSharp.Glob.Sample.csproj" -a $TARGETARCH

COPY . .
RUN dotnet publish "samples/ActionsToolkitSharp.Glob.Sample/ActionsToolkitSharp.Glob.Sample.csproj" -a $TARGETARCH --no-restore -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /app .
USER root
ENTRYPOINT ["/app/ActionsToolkitSharp.Glob.Sample"]
