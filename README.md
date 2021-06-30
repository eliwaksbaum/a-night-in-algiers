# Algiers
A bare-bones text adventure tool written in C#, used to write a creative thesis on Albert Camus' *The Stranger* in text adventure form. Intf.cs holds the namespace with all the classes and methods required to build a game. AlgiersWorld.cs is an example of how to use those classes and methods to build a simple text-adventure game, in this case a lesson in absurdism set in the world of *The Stranger*. If you wanted to make your own game this way (though really you should be using a real engine like Inform7 or TADS), read below for build instructions.

## Build
You need to download and install the dotnet SDK. I built to two platforms: Console and Web. Console is a stand-alone app that runs in the terminal on Windows or Linux. Web is a [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) app that compiles to Web Assembly (essentially the same output you get from building a Unity project for WebGL, if you're familiar). This is dotnet, though, so you could probably figure out how to publish it in other ways, too.
### Console
- run `dotnet new console` in an empty directory
- place Intf.cs in the home directory
- using the Intf namespace, create a public class with a public `SetWorld`method that creates all the objects and rooms in your game and returns a `World` object
- The `Main` method in Program.cs is where the game logic goes. Use  `Console.WriteLine()` `Console.ReadLine()` and `Parser.Parse()`, to collect input, generate responses, and update the display. See the Console folder for an example.
- To publish, run `dotnet  publish -c Release`
- The build will appear in bin/Release/net**/publish
### Web
- run `dotnet new blazorwasm` in an empty directory
- You can clear all the files out of Pages and Shared except for Index.razor and MainLayout.razor
- place Intf.cs in the home directory
- using the Intf namespace, create a public class with a public `SetWorld`method that creates all the objects and rooms in your game and returns a `World` object
- The Index.razor file is where the game logic goes. Use an html `input` element to collect user input, run `Parser.Parse` on it, and use some `@bind` statements to update the displays. See the BlazorWasm folder for an example.
- Pay attention to the `<base>` tag in the index.html in wwwroot. That needs to be wherever your game is going to end up relative to your website's home directory.
- To publish, run `dotnet  publish -c Release`
- The build will appear in bin/Release/net**/publish/wwwroot 