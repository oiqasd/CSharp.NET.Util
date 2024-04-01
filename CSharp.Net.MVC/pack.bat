dotnet restore
dotnet build -c Release
dotnet pack  -c Release .\CSharp.Net.Mvc.csproj -o .\bin /p:Version=1.1.0-beta.1

pause