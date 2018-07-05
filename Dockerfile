FROM microsoft/dotnet:2.1-sdk

WORKDIR /src
COPY . .
RUN dotnet publish -o out

EXPOSE 5000

ENTRYPOINT [ "dotnet", "out/ClusterDemo.dll" ]

HEALTHCHECK --interval=3s --timeout=3s CMD curl --fail http://localhost:5000/health/status || exit 1