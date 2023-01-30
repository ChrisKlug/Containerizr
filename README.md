# Containerizr

This repo contains a PoC for building Docker images using C#. It is currently just a proof of concept, and would be very happy if you would like to provide any form of feedback. Is it worth exploring further? Are there any features that would be required before this was usable? Or is it just a waste of time, and I should just take it out back and...well...order a cab to that farm with the big beautiful green fields where it can run around and play with all the other failed open source projects?

The easiest way to give some feedback is obviously by adding a GitHub issue right here on this repo. The other way is to simple ping me on [Twitter at @ZeroKoll](https://twitter.com/ZeroKoll)!

## Why?

Building images using C# has a few advantages. First of all, it is a lot easier to read and understand than a DOCKERFILE. It also adds IntelliSense, and help when building images.

This is in itself should make Docker more approachable for people who might not feel 100% comfortable building Docker images. And with the ability to package up "Docker set up modules" as extension methods, it should make it a lot easier to do more complex tasks even for people who are comfortable with Dockerfiles

## Basics

The general idea is to be able to create a Dockerfile using C# instead of a Dockerfile. Sure, for people who are really comfortable building Dockerfiles, it might be a complete waste of time, even if there are some helpful features for them as well.

For a very basic image, all you need to do is to create an instance of a `ContainerImage` subclass, and start adding your stuff to it by calling the required methods on it. 

__Note:__ As it does create some temporary files and a temporary container, it is highly recommended to use a `using` statement.

```csharp
using (var image = DebianContainerImage.Create(DotNetCoreImageVersions.SDK_7_0))
  await image.EnsureDirectoryExists("/DemoApi");
  await image.AddDirectory("./Resources/DemoApi", "/DemoApi", "DemoApi");
  await image.SetWorkingDirectory("/DemoApi");
  await image.SetEnvironmentVariable("DOTNET_URLS", "http://+:5000");
  await image.SetEntryPoint("dotnet run");
  await image.CreateImage("my_img", "latest");
}
```

## Requirements

The project has very few requirements. The only thing it really requires, except for your average C# development environment, is that you have Docker on your machine. 

__Comment:__ All functionality has so far been tested on Docker Desktop using a Windows machine with WSL2.

## Features

The PoC is obviously far from feature complete in any way. But it does include a few features that I wanted to explore.

### "Interactive" mode

The constructor on the `ContainerImage` base class accepts a boolean parameter called `interactive`. If this is set to true, the image creation is done in "interactive mode".

__Note:__ The `interactive` parameter defaults to `true` if you build in Debug configuration.

In "interactive mode", the `ContainerImage` class will create a temporary "interactive" container on your machine, and all commands you issue towards the `ContainerImage` instance are also executed inside the temporary container.

This allows you to examine the standard- and error output from each command that is executed, and determine if something has gone wrong. It also allows you to do step-by-step debugging while at the same time being attached to the actual container to verify what is going on inside it.

This feature should allow you to debug problems a lot quicker than you normally would be able to using a Dockerfile.

### Packaging and modularization of Docker image set up

As the project is based on .NET, it is possible to package and distribute image set up as NuGet packages. All the package needs to do, it to create an extension method that targets one of the `ContainerImage` classes, and use it to package up everything that is needed to set up a feature inside the image.

The `ContainerImage` base class currently has one subclass called `LinuxContainerImage`. This class can be targeted if you want to add Linux specific images.

__Note:__ The goal is obviously to have a `WindowsContainerImage` as well, but I have been focused on Linux-based images initially.

The `LinuxContainerImage` has 2 subclasses called `CentOSContainerImage` and `DebianContainerImage`, allowing you to target images based on their distro.

And once again, the goal is obviously to add whatever distros are needed...

In the repo, you can see this feature being used in the `Containerizr.AspNet` and `Containerizr.Packages` projects.

The `Containerizr.AspNet` project contains a helper function called `AddAspNetAppToImage()`. It allows you to easily add an ASP.NET application to an SDK-based image.

And the `Containerizr.Packages` has extension methods that allow you to install packages through either `apt-get` or `yum` depending on what distro you are using. And in the future, it should support `winget` or `Chocolatey` for Windows-based images.

### Multistage builds

The project also supports multi-stage builds by using a method called `CopyFileFromImage()`. By creation two `ContainerImage` instances, and using `CopyFileFromImage()` to copy a file from one image to the other, you are in fact creating a multistage build.

This feature can be seen in the `MultiStageBuildSample` class in the `Containerizr.Samples` project.

### Copy files from any directory

By default, the Dockerfile approach requires any file that is to be used in the image to be located in, or under, a specific directory. Using Containerizr, this is not a requirement anymore. Or rather, it works around this limitation... When you create an image by calling the `CreateImage()` method, any file that is to be copied to the image is copied to a temporary directory. A Dockerfile is then generated inside that directory, and used in a call to `docker build`. The directory is then removed.

By using a temporary directory like this, all layer-based caching functionality is kept intact, and requires no weird workarounds. The only downside is the need to copy all files to a temporary directory, but this should hopefully not be a dealbreaker.

## Architectural comments

To cover the overall architecture of the project here seems like a bit of an overkill. However, it might be worth mentioning that most methods that are called on the `ContainerImage` and its subclasses, are extension methods. These extension methods add something called `DockerDirective`s to the image. Each directive is responsible for 2 things. 

It is responsible for executing the required functionality inside the temporary container when building in "interactive mode". Allowing users to debug what happens during image creation.

But it is also responsible for creating the required entries in the temporary Dockerfile that is used when building the actual image.

An example is the `RunCommandDirective`. It looks like this

```csharp
public class RunCommandDirective : DockerDirective
{
    private readonly string command;

    public RunCommandDirective(string command)
    {
        this.command = command;
    }

    public override Task<CommandExecutionResponse> ExecuteInteractive(ExecutionContext context)
        => context.Image.InteractiveContainer.ExecuteCommand(command);

    public override Task GenerateDockerFileContent(DockerfileContext context)
    {
        context.AddDirective($"RUN {this.command}");
        return Task.CompletedTask;
    }
}
```

As you can see, it has an `ExecuteInteractive()` method that is called when running in "interactive mode". This is responsible for executing things inside the temporary container. In this case, the string-based command that is passed into the constructor.

And then it has a `GenerateDockerFileContent()` method, which is responsible for adding the required entry to the Dockerfile. In this case a `RUN` entry.

When a `CommandDirective` is added to a `ContainerImage`, `ExecuteInteractive()` method is called straight away. The directive is then added to a list of directives. And when a call to `CreateImage()` is made, it iterates over the list of directives and uses the `GenerateDockerFileContent()` methods to build up the temporary Dockerfile that is passed to `docker build`.

## Samples

To see the included features in action, have a look at the `Containerizr.Samples` project. This is a simple console application that allows you to see the different features in action. You can easily define whether you want to build you images in interactive mode, or not. All you need to do is update the following line of code in the `Program.cs` file.

```csharp
await samples[input].Execute(true);
```

Passing in true will build using interactive mode, and false will not.

## Known issues

This is a ___very___ early version that is just here to get feedback. There is a _lot_ still to be done! Windows image support, error handling, more features etc.

The current samples work, but other than that, I guarantee nothing! And once again, there is almost no error handling... it is a PoC...

I would not recommend using it for anything but testing. But if you like the idea, let me know, and it might be explored further! 

## Feedback

Once again! This repo is here because I want feedback! Please help me with that part! What is good? What is weird? What features would you like to see? Should I just drop it?

One way is to add issues here on GitHub, which gives other people insights to your comments. But otherwise, you can always ping me on [Twitter at @ZeroKoll](https://twitter.com/ZeroKoll)
