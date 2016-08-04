FROM microsoft/dotnet:core-deps
ADD . /app
WORKDIR /app
RUN dotnet restore

CMD [ "dotnet", "test" ]