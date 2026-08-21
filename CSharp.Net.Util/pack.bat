dotnet restore
dotnet build -c Release
dotnet pack  -c Release .\CSharp.Net.Util.csproj -o .\bin /p:Version=7.4.0
:: -beta.1  --include-symbols
:: -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg

pause