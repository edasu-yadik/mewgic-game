FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY tools/ClanServer/ClanServer.csproj tools/ClanServer/
RUN dotnet restore tools/ClanServer/ClanServer.csproj
COPY tools/ClanServer/ tools/ClanServer/
COPY Assets/Scripts/Core/ Assets/Scripts/Core/
COPY Assets/Resources/Levels/ Assets/Resources/Levels/
RUN dotnet publish tools/ClanServer/ClanServer.csproj -c Release --no-restore -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000
COPY --from=build /out/ .
USER $APP_UID
ENTRYPOINT ["dotnet", "ClanServer.dll"]
