# Containerizr

This repo contains a PoC for building Docker images using C#. 

## Basics

To create an image, you create an instance of an ContainerImage subclass. And since it implements IDisposable, it is hightly recommended to using `using`.

```csharp
using (var image = DebianContainerImage.Create(DotNetCoreImageVersions.SDK_7_0))
  await image.SetEntryPoint("dotnet run");
  await image.CreateImage("my_img", "lates");
}
```

The constructor also accepts a boolean that defines whether or not you want to run in "interactive mode". In this mode, each command that is run will be interactively run in a temporary container on your machine. This allows you to step-by-step debug the the set-up during development. 

__Note:__ The code defaults to interactive mode when building in debug mode

The easiest way to understand how it works, is to have a look at the Containerizr.Samples project.

## Known issues

This is a ___very___ early version that is just here to get feedback. There is a _lot_ still to be done!

The current samples work, but other than that, I guarantee nothing!

There is also some weirdness in the code to enable outputting the generated Dockerfile content in the samples that I would like to remove. But for now, it is there as it gives decent output in the console.

## Why?

Building images using C# has a few advantages. First of all, it is a lot easier to read and understand than a DOCKERFILE. It also adds IntelliSense and help when building images.

It also allows people to bundle configuration/set up into extension methods, to simplify more complex set ups. They could also be distributed using NuGet. 

## Feedback

This repo is here because I want feedback! Please help me with that part! What is good? What is weird? What would you like to see? 

One way is to add issues here on GitHub, which gives other people insights to your comments. But otherwise, you can ping me on Twitter [@ZeroKoll](https://twitter.com/ZeroKoll)
