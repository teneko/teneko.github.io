using System;
using Teronis.Diagnostics;

partial class Build
    {
    public static void RunSimpleProcess(string name, string args, bool echoCommand = false) =>
        SimpleProcess.Run(name, args, echoCommand: echoCommand, outputReceived: Console.WriteLine, errorReceived: Console.Error.WriteLine);

    public static string Arguments(params string[] args) =>
        string.Join(' ', args);

    public static void RunDocFx(string args) =>
        RunSimpleProcess(DotNetExecutableName, Arguments("tool run docfx -- ", args), echoCommand: true);
    }
