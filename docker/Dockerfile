FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
ARG USER_ID=1000
ARG GROUP_ID=1000
ARG USER_NAME=wsdscan

RUN apk add --no-cache \
    ca-certificates \
    gphoto2 \
    sane-backends \
    bash \
    shadow

RUN groupadd -g ${GROUP_ID} -o ${USER_NAME} && useradd -m -u ${USER_ID} -g ${GROUP_ID} -o -s /bin/bash ${USER_NAME}

WORKDIR /app

COPY --chown=${USER_ID}:${GROUP_ID} --from=build /app/out .

VOLUME ["/app/scans"]

ENTRYPOINT ["sh", "-c", "umask 0002 && dotnet WsdScanService.Host.dll"]