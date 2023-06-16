ARG aspnet_ver=6.0
ARG netsdk_ver=6.0

FROM mcr.microsoft.com/dotnet/sdk:${netsdk_ver} AS build
WORKDIR /src
COPY NuGet.config ./
COPY *.sln ./
COPY src/**/**/*.csproj ./
RUN dotnet sln list | grep ".csproj" \
    | while read -r line; do \ 
        mkdir -p $(dirname $line); \
        mv $(basename $line) $(dirname $line); \
      done;
RUN dotnet restore
COPY . .
RUN dotnet build -c Release --no-restore

FROM build AS publish
RUN dotnet publish -c Release -o /publish --no-build

FROM mcr.microsoft.com/dotnet/aspnet:${aspnet_ver} AS runtime
WORKDIR /publish
COPY --from=publish /publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Api.dll"]