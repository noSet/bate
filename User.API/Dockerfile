FROM microsoft/aspnetcore-build AS build-env
WORKDIR /code
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o out

FROM microsoft/aspnetcore
WORKDIR /app
COPY --from=build-env /code/out .

EXPOSE 80
ENTRYPOINT ["dotnet", "User.API.dll"]




#FROM microsoft/aspnetcore:2.0 AS base
#WORKDIR /app
#EXPOSE 80
#
#FROM microsoft/aspnetcore-build:2.0 AS build
#WORKDIR /src
#COPY *.sln ./
#COPY User.API/User.API.csproj User.API/
#RUN dotnet restore
#COPY . .
#WORKDIR /src/User.API
#RUN dotnet build -c Release -o /app
#
#FROM build AS publish
#RUN dotnet publish -c Release -o /app
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app .
#ENTRYPOINT ["dotnet", "User.API.dll"]
#