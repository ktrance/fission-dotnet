FROM microsoft/dotnet:1.1.0-runtime

WORKDIR /userfunc
COPY out .
EXPOSE 8888

CMD ["dotnet" "/userfunc/fissiondotnet.dll"]