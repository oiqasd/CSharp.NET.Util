## 项目说明
1.此项目为.NetStandard2.0通用帮助项目，  
2.nuget服务器地址为 https://www.nuget.org/packages/CSharp.Net.Standard.Util/
3.nuget install CSharp.Net.Standard.Util -Version 1.0.0 -Source https://www.nuget.org/packages/CSharp.Net.Standard.Util/ 或者  Install-Package CSharp.Net.Standard.Util -Version 1.0.0 -Source https://www.nuget.org/packages/CSharp.Net.Standard.Util/
4.外部项目在引用本地包时需要在解决方案跟目录放置 NuGet.Config 文件，文件内容如下
	<?xml version="1.0" encoding="utf-8"?>
	<configuration>
		<packageSources>
			<clear />
			<add key="Local" value="http://本地服务器地址" />
			<add key="NuGet.org" value="https://api.nuget.org/v3/index.json" />
		</packageSources>
	</configuration>

#### RedisHelper   
* 主要使用框架 StackExchange.Redis ，封装了对Redis的操作   
* 不同的Redis数据库使用不同的ConnectionString进行区分    
* 打包命令：参考网站 <https://docs.microsoft.com/zh-cn/dotnet/core/tools/dotnet-pack?tabs=netcore2x>   
dotnet pack CSharp.Net.Standard.Util/CSharp.Net.Standard.Util.csproj /p:PackageVersion=1.0.0

#### 