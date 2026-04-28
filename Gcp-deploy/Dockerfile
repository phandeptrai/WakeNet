FROM node:20-alpine AS fe
WORKDIR /fe

COPY wakenetserver.client/package*.json ./
RUN npm install
COPY wakenetserver.client/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY WakeNetServer.Server/WakeNetServer.Server.csproj WakeNetServer.Server/
RUN dotnet restore WakeNetServer.Server/WakeNetServer.Server.csproj

COPY . .

# Copy FE dist -> ASP.NET Core wwwroot
RUN mkdir -p WakeNetServer.Server/wwwroot
COPY --from=fe /fe/dist/ WakeNetServer.Server/wwwroot/

RUN dotnet publish WakeNetServer.Server/WakeNetServer.Server.csproj -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out ./

# Serve FE + API on one port
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "WakeNetServer.Server.dll"]

