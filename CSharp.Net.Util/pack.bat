dotnet restore
dotnet build -c Release
dotnet pack  -c Release .\CSharp.Net.Util.csproj -o .\bin /p:Version=7.2.5-beta.4  --include-symbols

pause