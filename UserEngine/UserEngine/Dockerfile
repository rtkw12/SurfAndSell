#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["UserEngine/UserEngine/UserEngine.csproj", "UserEngine/UserEngine/"]
COPY ["Common/Common.Caching/Common.Caching.csproj", "Common/Common.Caching/"]
COPY ["Common/Common.Util/Common.Util.csproj", "Common/Common.Util/"]
COPY ["Common/Common.MongoDb/Common.MongoDb.csproj", "Common/Common.MongoDb/"]
COPY ["Common/Common.Messaging/Common.Messaging.csproj", "Common/Common.Messaging/"]
RUN dotnet restore "UserEngine/UserEngine/UserEngine.csproj"
COPY . .
WORKDIR "/src/UserEngine/UserEngine"
RUN dotnet build "UserEngine.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserEngine.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 7922/tcp

ENTRYPOINT ["dotnet", "UserEngine.dll"]