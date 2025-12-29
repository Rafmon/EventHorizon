FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

ARG PLATFORM
ARG BUILD_TAG="unknown"

COPY ./output/${PLATFORM}/ .

RUN echo "$BUILD_TAG" > buildinfo.txt

ENTRYPOINT ["dotnet", "EventHorizon.dll"]
