using System;
using System.Collections.Generic;
using Intf;

namespace Algiers
{
    class Program
    {
        static void Main(string[] args)
        {
            World world = SetWorld();
            Console.WriteLine(world.start);
            while (!world.done)
            {
                Console.WriteLine("");
                string response = Parser.Parse(Console.ReadLine(), world);
                Console.WriteLine("");
                Console.WriteLine(response);
            }
        }

        static World SetWorld()
        {
            World world = new World();
            world.start = "this is the start";

            //COMMANDS
            Command look = world.AddCommand("look", CommandType.Intransitive, new string[] {"around"});
            look.aliases = new string[]{"l"};
            world.SetIntransitiveCommand("look", () => {
                return "look at this";
            });

            Command help = world.AddCommand("help", CommandType.Intransitive);
            help.aliases = new string[]{"h"};
            world.SetIntransitiveCommand("help", () => {
                return "heh gl";
            });
            
            Command inv = world.AddCommand("inv", CommandType.Intransitive);
            inv.aliases = new string[]{"i", "inventory"};
            world.SetIntransitiveCommand("inv", () => {
                return "inventory";
            });

            Command quit = world.AddCommand("quit", CommandType.Intransitive);
            quit.aliases = new string[]{"quit"};
            world.SetIntransitiveCommand("quit", () => {
                world.done = true;
                return "nice. way to be absurdist";
            });
            
            Command what = world.AddCommand("examine", CommandType.Transitive);
            what.aliases = new string[]{"what"};
            what.MissingTargetError = "Examine what?";
            what.InaccessibleTargetError = new string[] {"There is no ", " to examine here."};
            what.NullHandlerError = new string[] {"You can't examine ", " ."};
            world.SetTransitiveCommand("examine", (obj) => {
                return "wow check it out";
            });

            Command take = world.AddCommand("take", CommandType.Transitive, new string[]{"up"});
            take.aliases = new string[]{"pick"};
            take.MissingTargetError = "Take what?";
            take.InaccessibleTargetError = new string[] {"There is no ", " to take here."};
            take.NullHandlerError = new string[] {"You can't take ", " ."};
            world.SetTransitiveCommand("take", (obj) => {
                return "thief!";
            });

            Command talk = world.AddCommand("talk", CommandType.Transitive, new string[]{"to", "with"});
            talk.MissingTargetError = "Talk to whom?";
            talk.InaccessibleTargetError = new string[] {"There is nobody named ", " to talk to here."};
            talk.NullHandlerError = new string[] {"You can't talk to ", " ."};
            world.SetTransitiveCommand("talk", (obj) => {
                return "yatta yatta";
            });

            Command go = world.AddCommand("go", CommandType.Transitive, new string[]{"to"});
            go.aliases = new string[]{"travel"};
            go.MissingTargetError = "Go where?";
            go.InaccessibleTargetError = new string[] {"There is no ", " to go to from here"};
            go.NullHandlerError = new string[] {"You can't go to ", " ."};
            world.SetTransitiveCommand("go", (obj) => {
                return "zoom zoom";
            });

            Command useT = world.AddCommand("use", CommandType.Transitive);
            useT.MissingTargetError = "Use what?";
            useT.InaccessibleTargetError= new string[] {"You don't have ", " in your inventory."};
            useT.NullHandlerError = new string[] {"You can't use ", " ."};
            world.SetTransitiveCommand("use", (obj) => {
                return "beep boop";
            });

            Command useD = world.AddCommand("use", CommandType.Ditransitive, dipreps: new string[]{"on", "with"});
            useD.MissingTargetError = "Use what?";
            useD.InaccessibleTargetError = new string[] {"You don't have ", " in your inventory."};
            useD.NullHandlerError = new string[] {"You can't use ", " on "};
            useD.MissingTarget2Error = new string[] {"Use ", " on what?"};
            useD.InaccessibleTarget2Error = new string[] {"There is no ", " to use ", " on here."};
            world.SetDitransitiveCommand("use", (obj1, obj2) => {
                return "beep boop the bop";
            });

            Command give = world.AddCommand("give", CommandType.Ditransitive, dipreps: new string[]{"to"});
            give.MissingTargetError = "Give what?";
            give.InaccessibleTargetError = new string[] {"You don't have ", " in your inventory."};
            give.NullHandlerError = new string[] {"You can't give ", " to "};
            give.MissingTarget2Error = new string[] {"Give ", " to whom?"};
            give.InaccessibleTarget2Error = new string[] {"There is nobody named ", " to give ", " to here."};
            world.SetDitransitiveCommand("give", (obj1, obj2) => {
                return "here ya go";
            });

            //CHAMBRE
            Room chambre = world.AddRoom("chambre");
            chambre.description = "This is the bedroom.";
            chambre.exits = new Dictionary<string, string>
            {
                {"left", "balcony"},
                {"right", "antechambre"}
            };

            //CHAIR
            Item chair = chambre.AddItem("chair");

            return world;
        }
    }
}
