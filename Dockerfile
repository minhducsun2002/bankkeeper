FROM mcr.microsoft.com/dotnet/sdk:8.0.203-alpine3.19 as build
WORKDIR /app

COPY . . 
RUN dotnet restore
RUN dotnet publish -o dist

FROM mcr.microsoft.com/dotnet/runtime:8.0.3-alpine3.19
RUN apk add --no-cache icu-libs

# Disable the invariant mode
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8
COPY --from=build /app/dist /app
WORKDIR /app
CMD dotnet ./Bankkeeper.dll
