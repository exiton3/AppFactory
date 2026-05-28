FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["samples/AspNetCore.UserService/AspNetCore.UserService.csproj", "samples/AspNetCore.UserService/"]
COPY ["src/", "src/"]
RUN dotnet restore "samples/AspNetCore.UserService/AspNetCore.UserService.csproj"
COPY . .
WORKDIR "/src/samples/AspNetCore.UserService"
RUN dotnet build "AspNetCore.UserService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AspNetCore.UserService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspNetCore.UserService.dll"]
