# Roslyn Analyzers #

[.NET Compiler Platform (*Roslyn*) Analyzers](https://docs.microsoft.com/visualstudio/code-quality/roslyn-analyzers-overview) analyse your code for issues as you type. Scaffold comes with a [*rule set*](../CodeAnalysis.ruleset) file which you can use to help you configure any Roslyn analyzer you may use in any of your projects. The rule set file is applied to your project via the [*Directory.Build.props*](../Directory.Build.props) file.

## Analyzers ##

Scaffold uses the following analyzers in some or all of the projects in the solution. For an analyzer to work for a given project, it must be installed for that project.

### [Microsoft.AspNetCore.Mvc.Api.Analyzers](https://docs.microsoft.com/aspnet/core/web-api/advanced/analyzers) ###

Web API analyzers is only used in the [Scaffold.WebApi](../Sources/Scaffold.WebApi) project. One of the analyzers in the package inspects controllers annotated with ApiControllerAttribute and identifies actions that don't entirely document their responses.

### [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) ###

StyleCop analyzers help enforce style and consistency when writing C# code. These analyzers should be installed in every project in the Scaffold solution.

### [xUnit.Analyzers](https://github.com/xunit/xunit.analyzers) ###

If your project has a reference to xUnit, it will have xUnit analyzers installed. It should only be present in test projects. xUnit analyzers are intended to help you write better tests.