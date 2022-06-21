# DocumentService
Microservice polyglot persistence on an example of a document service

![]([https://github.com/Your_Repository_Name/Your_GIF_Name.gif](https://github.com/adnan-selimovic/DocumentService/blob/master/Download.gif))

# Docker support
Docker support was enabled during project creation in Visual Studio
Note about "Docker Desktop WSL 2 backend"
https://docs.docker.com/desktop/windows/wsl/

# The story about the .NET analyzers
https://docs.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#enablenetanalyzers
https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview#latest-updates

.NET anlyzers are target-framework agnostic, which is cool. We can develop using the .NET 5+ SDK and still target earlier versions of .NET, such as netcoreapp3.1 and net472, like I did on this project. We have also option not to move to the .NET 5+ SDK, for our older projects, and to have non-SDK-style .NET Framework project as well (https://docs.microsoft.com/en-us/nuget/resources/check-project-format). I installed the Microsoft.CodeAnalysis.NetAnalyzers NuGet package to decouple rule updates from .NET SDK updates, automatically turning off the built-in SDK analyzer. In this configuration, which I think provides maximum flexibility with the use of SDKs and target frameworks, adding the EnableNETAnalyzers property to the MSBuild project file (.csproj) (or a Directory.Build.props file) and setting it to true, will generate a build warrning.

Next EnforceCodeStyleInBuild
https://docs.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#enablenetanalyzers

EnableNETAnalyzers property to either your project file or a Directory.Build.props file
to decouple rule updates from .NET SDK updates and to be able 
If we are developing using the .NET 5+ SDK, but are targeting .NET 3.1
Unlike for projects that target .NET 5+ SDKs, we must explicitly enable .NET code analysis for projects targeting .NET 3.1 by setting the EnableNETAnalyzers property to true, in the MSBuild project file (.csproj):
<PropertyGroup>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
</PropertyGroup>
    

Analyzers that are installed from a NuGet package, in our case
PM> Install-Package Microsoft.CodeAnalysis.NetAnalyzers -Version 5.0.3

