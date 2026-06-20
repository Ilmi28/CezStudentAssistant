# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /app

COPY ["src/CezStudentAssistant.Worker/CezStudentAssistant.Worker.csproj", "src/CezStudentAssistant.Worker/"]
COPY ["src/CezStudentAssistant.Application/CezStudentAssistant.Application.csproj", "src/CezStudentAssistant.Application/"]
COPY ["src/CezStudentAssistant.Domain/CezStudentAssistant.Domain.csproj", "src/CezStudentAssistant.Domain/"]
COPY ["src/CezStudentAssistant.Infrastructure/CezStudentAssistant.Infrastructure.csproj", "src/CezStudentAssistant.Infrastructure/"]
COPY ["src/ExternalServices/CezStudentAssistant.Cez/CezStudentAssistant.Cez.csproj", "src/ExternalServices/CezStudentAssistant.Cez/"]

RUN dotnet restore "src/CezStudentAssistant.Worker/CezStudentAssistant.Worker.csproj"

COPY src/CezStudentAssistant.Worker/ src/CezStudentAssistant.Worker/
COPY src/CezStudentAssistant.Application/ src/CezStudentAssistant.Application/
COPY src/CezStudentAssistant.Domain/ src/CezStudentAssistant.Domain/
COPY src/CezStudentAssistant.Infrastructure/ src/CezStudentAssistant.Infrastructure/
COPY src/ExternalServices/ src/ExternalServices/

EXPOSE 8082

WORKDIR "/app/src/CezStudentAssistant.Worker"
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CezStudentAssistant.Worker.dll"]
