FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore
COPY ConstantLearning.csproj ./
RUN dotnet restore

# copy the rest of the source
COPY . .

# build
RUN dotnet build -c Release -o /app/build

# publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8888

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConstantLearning.dll"]
