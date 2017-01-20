FROM microsoft/dotnet:1.1.0-runtime

WORKDIR /fissiontest
COPY out .
EXPOSE 8888

ENTRYPOINT ["dotnet"]

CMD ["fissiondotnet.dll"]