dotnet restore
dotnet build -c Release
dotnet pack  -c Release /p:Version=7.2.0-beta.1
pause