dotnet restore
dotnet build -c Release
dotnet pack  -c Release .\CSharp.Net.Tools.csproj -o .\bin /p:Version=0.0.1-beta.1  --include-symbols
:: -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg

pause