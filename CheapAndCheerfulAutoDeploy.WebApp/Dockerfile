FROM mcr.microsoft.com/dotnet/aspnet:6.0-nanoserver-ltsc2022 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CheapAndCheerfulAutoDeploy.csproj", "./"]
RUN dotnet restore "CheapAndCheerfulAutoDeploy.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "CheapAndCheerfulAutoDeploy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CheapAndCheerfulAutoDeploy.csproj" -c Release -o /app/publish

FROM base AS final

USER ContainerAdministrator

ENV ASPNETCORE_URLS="https://+;http://+"
ENV ASPNETCORE_HTTPS_PORT="8001"

ENV ASPNETCORE_Kestrel__Certificates__Default__Password="ChangeMe"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/https/change/me.pfx"

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CheapAndCheerfulAutoDeploy.dll"]
