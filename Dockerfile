FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["DockerSmtpTest/DockerSmtpTest.csproj", "DockerSmtpTest/"]
RUN dotnet restore "DockerSmtpTest/DockerSmtpTest.csproj"
COPY . .
WORKDIR "/src/DockerSmtpTest"
RUN dotnet build "DockerSmtpTest.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DockerSmtpTest.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DockerSmtpTest.dll"]
