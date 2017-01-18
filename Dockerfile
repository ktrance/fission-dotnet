FROM microsoft/dotnet:runtime

WORKDIR /userfunc
COPY out .
ENTRYPOINT ["dotnet", "fission-dotnet.dll"]
