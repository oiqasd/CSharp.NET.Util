dotnet restore
dotnet build -c Release
dotnet pack  -c Release /p:Version=7.0.7-beta.1
pause