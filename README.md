# DocumentService
Microservice polyglot persistence on an example of a document service

# Docker support
Docker support was enabled during project creation in Visual Studio
Note about "Docker Desktop WSL 2 backend"
https://docs.docker.com/desktop/windows/wsl/

Unlike for the .NET 5+ SDKs, we must explicitly enable .NET code analysis for projects targeting .NET 3.1 by setting the EnableNETAnalyzers property to true, in the MSBuild project file (.csproj):
<PropertyGroup>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
</PropertyGroup>
    

Analyzers that are installed from a NuGet package, in our case
PM> Install-Package Microsoft.CodeAnalysis.NetAnalyzers -Version 5.0.3

