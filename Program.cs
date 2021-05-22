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
            Player player = world.player;

            //COMMANDS
            Command look = world.AddCommand("look", CommandType.Intransitive, new string[] {"around"});
            look.aliases = new string[]{"l"};
            world.SetIntransitiveCommand("look", () => {
                string output = player.current_room.description;
                foreach (GameObject gameObject in player.current_room.GameObjects.Values)
                {
                    if (gameObject.ResponsesT.ContainsKey("look"))
                    {
                        output = output + " " + gameObject.ResponsesT["look"]();
                    }
                }
                return output;
            });

            Command help = world.AddCommand("help", CommandType.Intransitive);
            help.aliases = new string[]{"h"};
            world.SetIntransitiveCommand("help", () => {
                return "heh gl";
            });
            
            Command inv = world.AddCommand("inv", CommandType.Intransitive);
            inv.aliases = new string[]{"i", "inventory"};
            world.SetIntransitiveCommand("inv", () => {
                if (player.Inventory.Count == 0)
                {
                    return "Your inventory is empty.";
                }
                else
                {
                    string output = "";
                    foreach (string item in player.Inventory.Keys)
                    {
                        string first = item.Substring(0, 1).ToUpper();
                        string rest = item.Substring(1);
                        output = output + first + rest + ", ";
                    }
                    return output.Remove(output.Length - 2);
                }
            });

            Command quit = world.AddCommand("quit", CommandType.Intransitive);
            quit.aliases = new string[]{"q"};
            world.SetIntransitiveCommand("quit", () => {
                bool quitDone = false;
                Console.WriteLine("\nAre you sure you want to give up?\n");
                while (!quitDone)
                {
                    string answer = Console.ReadLine();
                    if (answer == "y" || answer == "yes")
                    {
                        quitDone = true;
                        world.done = true;
                        return "nice. way to be absurdist\n";
                    }
                    else if (answer == "n" || answer == "no")
                    {
                        quitDone = true;
                        return "way to soldier on. gl";
                    }
                    else
                    {
                        Console.WriteLine("\nAre you sure you want to give up? (yes/no)\n");
                    }
                }
                return "";
            });
            
            Command what = world.AddCommand("examine", CommandType.Transitive);
            what.aliases = new string[]{"what"};
            what.MissingTargetError = "Examine what?";
            world.SetTransitiveCommand("examine", (obj) => {
                if (!player.CanAccessObject(obj))
                {
                    return "There is no " + obj + " to examine here.";
                }
                else
                {
                    GameObject obObj = player.GetObject(obj);
                    if (!obObj.ResponsesT.ContainsKey("what"))
                    {
                        return "You can't examine the " + obj + ".";
                    }
                    else
                    {
                        return obObj.ResponsesT["what"]();
                    }
                }
            });

            Command take = world.AddCommand("take", CommandType.Transitive, new string[]{"up"});
            take.aliases = new string[]{"pick"};
            take.MissingTargetError = "Take what?";
            world.SetTransitiveCommand("take", (obj) => {
                if (player.InInventory(obj))
                {
                    return "You already have the " + obj + " in your inventory";
                }
                else if (!player.InRoom(obj))
                {
                    return "There is no " + obj + " to take here.";
                }
                else
                {
                    GameObject obObj = player.GetObject(obj);
                    if (!obObj.ResponsesT.ContainsKey("take"))
                    {
                        return "You can't take the " + obj + ".";
                    }
                    else
                    {
                        return obObj.ResponsesT["take"]();
                    }
                }
            });

            Command talk = world.AddCommand("talk", CommandType.Transitive, new string[]{"to", "with"});
            talk.MissingTargetError = "Talk to whom?";
            world.SetTransitiveCommand("talk", (obj) => {
                if (!player.InRoom(obj))
                {
                    return "There is nobody named " + obj + " to talk to here.";
                }
                else
                {
                    GameObject obObj = player.GetObject(obj);
                    if (!obObj.ResponsesT.ContainsKey("talk"))
                    {
                        return "You can't talk to the " + obj + ".";
                    }
                    else
                    {
                        return obObj.ResponsesT["talk"]();
                    }
                }
            });

            Command go = world.AddCommand("go", CommandType.Transitive, new string[]{"to"});
            go.aliases = new string[]{"travel"};
            go.MissingTargetError = "Go where?";
            world.SetTransitiveCommand("go", (obj) => {
                return "zoom zoom";
            });

            Command useT = world.AddCommand("use", CommandType.Transitive);
            useT.MissingTargetError = "Use what?";
            world.SetTransitiveCommand("use", (obj) => {
                if (!player.InInventory(obj))
                {
                    string indef = (Parser.StartsWithVowel(obj))? "an " : "a ";
                    return "You don't have " + indef + obj + " in you inventory";
                }
                else
                {
                    GameObject obObj = player.GetObject(obj);
                    if (!obObj.ResponsesT.ContainsKey("use"))
                    {
                        return "You can't use the " + obj + ".";
                    }
                    else
                    {
                        return obObj.ResponsesT["use"]();
                    }
                }
            });

            Command useD = world.AddCommand("use", CommandType.Ditransitive, dipreps: new string[]{"on", "with"});
            useD.MissingTargetError = "Use what?";
            useD.MissingTarget2Error = new string[] {"Use ", " on what?"};
            world.SetDitransitiveCommand("use", (tool, target) => {
                if (!player.InInventory(tool))
                {
                    string indef = (Parser.StartsWithVowel(tool))? "an " : "a ";
                    return "You don't have " + indef + tool + " in you inventory.";
                }
                else if (!player.CanAccessObject(target))
                {
                    return "There is no " + target + " to use " + tool + " on here.";
                }
                else
                {
                    GameObject targetObj = player.GetObject(target);;
                    string nullHandler = "You can't use the " + tool + "with the " + target + ".";
                    if (!targetObj.ResponsesD.ContainsKey("use"))
                    {
                        return nullHandler;
                    }
                    else
                    {
                        string response = targetObj.ResponsesD["use"](tool);
                        return (response == null)? nullHandler : response;
                    }
                }
            });

            Command give = world.AddCommand("give", CommandType.Ditransitive, dipreps: new string[]{"to"});
            give.MissingTargetError = "Give what?";
            give.MissingTarget2Error = new string[] {"Give ", " to whom?"};
            world.SetDitransitiveCommand("give", (gift, person) => {
                if (!player.InInventory(gift))
                {
                    string indef = (Parser.StartsWithVowel(gift))? "an " : "a ";
                    return "You don't have " + indef + gift + " in you inventory.";
                }
                else if (!player.CanAccessObject(person))
                {
                    return "There is nobody named " + person + " here to give the " + gift + " to.";
                }
                else
                {
                    GameObject personObj = player.GetObject(person);
                    string nullHandler = "You can't give the " + gift + "to the " + person + ".";
                    if (!personObj.ResponsesD.ContainsKey("give"))
                    {
                        return nullHandler;
                    }
                    else
                    {
                        string response = personObj.ResponsesD["give"](gift);
                        return (response == null)? nullHandler : response;
                    }
                }
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
            GameObject chair = chambre.AddObject<GameObject>("chair");
            chair.conditions.Add("marked", false);
            chair.SetTransitiveCommand("look", () => {
                if (!chair.conditions["marked"])
                {
                    return "There's a chair in the corner.";
                }
                else
                {
                    return "There's a chair in the corner, covered in scribbles.";
                }
            });
            chair.SetTransitiveCommand("what", () => {
                if (!chair.conditions["marked"])
                {
                    return "It's an old, yellowing wicker chair.";
                }
                else
                {
                    return "It's an old, yellowing wicker chair. It's covered in black streaks.";
                }
            });
            chair.SetTransitiveCommand("take", () => {
                return "The chair is too heavy to pick up.";
            });
            chair.SetDitransitiveCommand("give", (gift) => {
                if (gift == "marker")
                {
                    chair.conditions["marked"] = true;
                    player.RemoveFromInventory("marker");
                    return "You scrawl all over the chair with the marker.";
                }
                else {return null;}
            });

            //MARKER
            GameObject marker = chambre.AddObject<GameObject>("marker");
            marker.SetTransitiveCommand("look", () => {
                return "A marker lies discarded on the floor.";
            });
            marker.SetTransitiveCommand("what", () => {
                return "It's one of those big-ass sharpies. Black.";
            });
            marker.SetTransitiveCommand("take", () => {
                player.AddToInventory("marker");
                return "You slip the marker into your pocket.";
            });

            //Coin
            GameObject coin = chambre.AddObject<GameObject>("coin");
            coin.SetTransitiveCommand("look", () => {
                return "A coin is stuck between the floorboards.";
            });
            coin.SetTransitiveCommand("what", () => {
                return "It's a greenish penny.";
            });
            coin.SetTransitiveCommand("take", () => {
                player.AddToInventory("coin");
                return "You slip the penny into your pocket.";
            });

            player.current_room = chambre;
            return world;
        }
    }
}
