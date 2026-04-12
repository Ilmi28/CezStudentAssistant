# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /app

COPY ["src/CezStudentAssistant.API/CezStudentAssistant.API.csproj", "src/CezStudentAssistant.API/"]
COPY ["src/CezStudentAssistant.Domain/CezStudentAssistant.Domain.csproj", "src/CezStudentAssistant.Domain/"]
COPY ["src/CezStudentAssistant.Infrastructure/CezStudentAssistant.Infrastructure.csproj", "src/CezStudentAssistant.Infrastructure/"]

RUN dotnet restore "src/CezStudentAssistant.API/CezStudentAssistant.API.csproj"

COPY src/CezStudentAssistant.API/ src/CezStudentAssistant.API/
COPY src/CezStudentAssistant.Domain/ src/CezStudentAssistant.Domain/
COPY src/CezStudentAssistant.Infrastructure/ src/CezStudentAssistant.Infrastructure/

WORKDIR "/app/src/CezStudentAssistant.API"
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CezStudentAssistant.API.dll"]