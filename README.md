# Containerizr

This repo contains a PoC for building Docker images using C#. 

## How to work with it?

The easiest way is to have a look at the Containerizr.Samples project. It has some samples that you can look at.

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