dotnet clean
dotnet restore
dotnet build -c Release
dotnet pack  -c Release /p:Version=7.3.0
pause