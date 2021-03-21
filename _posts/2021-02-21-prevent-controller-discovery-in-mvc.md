---
title: How to prevent controller auto-discovery in ASP.NET Core
author: Teroneko
category: ASP.NET Core MVC
tags: [ASP.NET Core, MVC]
---

When you are a library author of controllers and the user has the ability to use their desired controllers from your library, then your customer will somewhat wonder that not only the desired but also the unsdesired controllers are showing up in the API documentation of Swagger/OpenAPI provided by Swashbuckle or NSwag. So, what's the reason?

The above conclusion is drawn based on the fact that many people are adding controllers from a library by simply adding an assembly via `ApplicationPart` to the `ApplicationPartManager`:

{% highlight csharp linenos %}
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddControllers()
        .AddApplicationPart(typeof(LibraryAuthoredController).Assembly);
}
{% endhighlight %}

This will announce all controllers as available what we don't want. These controllers are also used for the "API exploration" in [ApiExplorer][ApiExplorer]. The result is unwanted controllers in the API documentation of Swagger/OpenAPI.

> **Info:** In the scope of this post I won't elaborate how `ApplicationPart`s works. Therefore I can refer to an excellent post from Andrew Lock which you can read here: [When ASP.NET Core can't find your controller: debugging application parts][Andrew Lock].

## Solution

The solution sounds trivial but effective:

1. Make the controller generic
2. Register the controller manually via ApplicationPart

Let's do it step by step. Let's assume we have a `BearerSignInController` that is normally not generic.

{% highlight csharp linenos %}
[ApiController]
public class BearerSignInController : Controller
{ }
{% endhighlight %}

Make the controller now generic. In this case I use a very restrictive constraint where `TNotUseful` has to implement `ISingleton` and it's implementation `Singleton` is sealed:

{% highlight csharp linenos %}
/// <summary>
/// This interface has no purpose. It just serves
/// as helper interface being usable as constraint.
/// </summary>
public interface ISingleton
{ }

public sealed class Singleton : ISingleton
{
    public static readonly Singleton Default = new Singleton();

    private Singleton() { }
}

[ApiController]
public class BearerSignInController<TNotUseful> : Controller
        where TNotUseful : ISingleton
{ }
{% endhighlight %}

Volià, our controller is now generic and no one should miss its intention.

Now we need to register the controller manually. Therefore I will use my class [TypesProvidingApplicationPart][TypesProvidingApplicationPart Code].  It is part of the NuGet package [Teronis.AspNetCore.Mvc][TypesProvidingApplicationPart Package].

{% highlight csharp linenos %}
/// <summary>
/// Adds <see cref="BearerSignInController"/> to <see cref="IMvcBuilder"/>.
/// </summary>
/// <param name="mvcBuilder"></param>
/// <param name="applicationPartName">Sets <see cref="ApplicationPart.Name". If null the name is <see cref="TypesProvidingApplicationPart"/>.</param>
/// <returns></returns>
public static IMvcBuilder AddBearerSignInControllers(this IMvcBuilder mvcBuilder, string? applicationPartName)
{
    mvcBuilder.ConfigureApplicationPartManager(setup => {
        var controllerType = typeof(BearerSignInController<Singleton>).GetTypeInfo();
        var typesProvider = TypesProvidingApplicationPart.Create(applicationPartName, controllerType);
        setup.ApplicationParts.Add(typesProvider);
    });

    return mvcBuilder;
}
{% endhighlight %}

[ApiExplorer]: https://github.com/aspnet/AspNetWebStack/blob/main/src/System.Web.Http/Description/ApiExplorer.cs
[Andrew Lock]: https://andrewlock.net/when-asp-net-core-cant-find-your-controller-debugging-application-parts/
[TypesProvidingApplicationPart Code]: https://teroneko.de/docs/Teronis.DotNet/Teronis.Mvc.TypesProvidingApplicationPart.html
[TypesProvidingApplicationPart Package]: https://www.nuget.org/packages/Teronis.AspNetCore.Mvc