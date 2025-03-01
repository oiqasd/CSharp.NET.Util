dotnet restore
dotnet build -c Release
dotnet pack  -c Release /p:Version=7.2.3
pause