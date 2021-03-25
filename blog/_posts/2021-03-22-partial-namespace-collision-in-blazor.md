---
title: Workaround partial namespace collision in ASP.NET Core Blazor
category: "asp-net-core"
tags: [ASP.NET Core, Blazor]
redirect_from:
  - /asp-net-core-blazor/partial-namespace-collision-in-blazor/
---

Errors like these are very annoying:

> The type or namespace name 'AspNetCore' does not exist in the namespace 'Example.Microsoft'

This happens due to the fact that the razor generator is not using `global::` when referencing to types.

For more informations regarding this bug take a look at:
1. [The use of namespaces that partially share names with those in Blazor project will fail build of project #28012](https://github.com/dotnet/aspnetcore/issues/28012)
2. [Update Razor compiler to use global:: more liberally #18757](https://github.com/dotnet/aspnetcore/issues/18757)

In case you are the luckily author of a typical `.razor` component that resides in the namespace `<name>.AspNetCore[.<name>]` then there are three workarounds.

## First workaround (worst)

Use another namespace overall.

**Consequence**

You cannot use your desired namespace. 

## Seocond workaround

Make use of `@namespace` in `<name>.razor` and if `<name>.razor.cs` exists, update the namespace in `<name>.razor.cs` accordingly.

**Consequence**

The namespace of components and other classes differ. 

## Third workaround (best)

Steps:

1\. Move `@code{}` from `<name>.razor` to `<name>.cs` (not `<name>.razor.cs`) à la

{% highlight csharp linenos %}
using Microsoft.AspNetCore.Components;

namespace <name>.AspNetCore.<name>
{
    public class <name>: ComponentBase
    {
        ...
    }
}
{% endhighlight %}

2\. Imitate render logic from `<name>.razor` in {% highlight csharp %}protected override void BuildRenderTree(RenderTreeBuilder builder){% endhighlight %}
<br/>3\. Remove `<name>.razor`

**Consequence**

You have to know or learn how to implement render logic.