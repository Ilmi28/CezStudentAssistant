# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /app

COPY ["src/CezStudentAssistant.API/CezStudentAssistant.API.csproj", "src/CezStudentAssistant.API/"]
COPY ["src/CezStudentAssistant.Application/CezStudentAssistant.Application/CezStudentAssistant.Application.csproj", "src/CezStudentAssistant.Application/CezStudentAssistant.Application/"]
COPY ["src/CezStudentAssistant.Domain/CezStudentAssistant.Domain.csproj", "src/CezStudentAssistant.Domain/"]
COPY ["src/CezStudentAssistant.Infrastructure/CezStudentAssistant.Infrastructure.csproj", "src/CezStudentAssistant.Infrastructure/"]
COPY ["src/ExternalServices/CezStudentAssistant.ExternalServices.Cez/CezStudentAssistant.Cez/CezStudentAssistant.Cez.csproj", "src/ExternalServices/CezStudentAssistant.ExternalServices.Cez/CezStudentAssistant.Cez/"]

RUN dotnet restore "src/CezStudentAssistant.API/CezStudentAssistant.API.csproj"

COPY src/CezStudentAssistant.API/ src/CezStudentAssistant.API/
COPY src/CezStudentAssistant.Application/ src/CezStudentAssistant.Application/
COPY src/CezStudentAssistant.Domain/ src/CezStudentAssistant.Domain/
COPY src/CezStudentAssistant.Infrastructure/ src/CezStudentAssistant.Infrastructure/
COPY src/ExternalServices/ src/ExternalServices/

WORKDIR "/app/src/CezStudentAssistant.API"
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CezStudentAssistant.API.dll"]
