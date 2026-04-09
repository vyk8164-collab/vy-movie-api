# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file solution và project để restore trước cho nhanh
COPY ["ASP.slnx", "./"]
COPY ["ASP/ASP.csproj", "ASP/"]
RUN dotnet restore "ASP/ASP.csproj"

# Copy toàn bộ code còn lại vào
COPY . .
WORKDIR "/src/ASP"
RUN dotnet build "ASP.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "ASP.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final (Chạy app)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ASP.dll"]