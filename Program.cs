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
            string instructions = String.Join(Environment.NewLine,
                "",
                "help, h - display this list of commands",
                "inventory, inv, i - display your inventory",
                "look, l (around) - look at your surroundings",
                "go, travel (to) - move to a new room",
                "examine, what - examine a particular object",
                "take, pick up - take an item and place it in your inventory",
                "talk (to) - talk to another character in the room",
                "use - use an item in your inventory",
                "use .. on / with - use an item in your inventory on something in the room",
                "give .. to - give an item in your inventory to another character in the room",
                "",
                ""
            );
            World world = new World();
            world.start = instructions + "You awake in your bedroom.";
            Player player = world.player;

            //COMMANDS
            Command look = world.AddIntransitiveCommand("look", CommandType.Intransitive, new string[]{"l"}, preps: new string[] {"around"});
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

            Command help = world.AddIntransitiveCommand("help", CommandType.Intransitive, new string[]{"h"});
            world.SetIntransitiveCommand("help", () => {
                return "heh gl";
            });
            
            Command inv = world.AddIntransitiveCommand("inv", CommandType.Intransitive, new string[]{"i", "inventory"});
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

            Command quit = world.AddIntransitiveCommand("quit", CommandType.Intransitive, new string[]{"q"});
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
                        return "You decide nothing is really worth doing today. You make your way to your balcony and look out onto the city, watching life pass by. It is a beautiful night in Algiers." + Environment.NewLine;
                    }
                    else if (answer == "n" || answer == "no")
                    {
                        quitDone = true;
                        return "You seem to have lost your train of thought. Were you in the middle of something?";
                    }
                    else
                    {
                        Console.WriteLine("\nAre you sure you want to give up? (yes/no)\n");
                    }
                }
                return "";
            });
            
            Command what = world.AddTransitiveCommand("examine", CommandType.Transitive, "Examine what?", new string[]{"what"});
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

            Command take = world.AddTransitiveCommand("take", CommandType.Transitive, "Take what?", new string[]{"pick"}, new string[]{"up"});
            world.SetTransitiveCommand("take", (obj) => {
                if (player.InInventory(obj))
                {
                    return "You already have the " + obj + " in your inventory.";
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

            Command talk = world.AddTransitiveCommand("talk", CommandType.Transitive, "Talk to whom", preps: new string[]{"to", "with"});
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

            Command go = world.AddTransitiveCommand("go", CommandType.Transitive, "Go where?", new string[]{"travel"}, new string[]{"to"});
            world.SetTransitiveCommand("go", (newRoom) => {
                if (!player.current_room.Exits.ContainsKey(newRoom))
                {
                    return "There's no " + newRoom + " to go to from here.";
                }
                else
                {
                    string newRoomID = player.current_room.Exits[newRoom];
                    player.current_room = world.Rooms[newRoomID];
                    return world.Responses["look"]();
                }
            });

            Command use = world.AddDitransitiveCommand("use", CommandType.Ditransitive, "Use what?", new string[]{"on", "with"});
            world.SetDitransitiveCommand("use", (tool, target) => {
                if (!player.InInventory(tool))
                {
                    string indef = (Parser.StartsWithVowel(tool))? "an " : "a ";
                    return "You don't have " + indef + tool + " in your inventory.";
                }
                else if (player.GetObject(tool).ResponsesT.ContainsKey("use"))
                {
                    return player.GetObject(tool).ResponsesT["use"]();
                }
                else if (target == "")
                {
                    return "Use " + tool + " on what?";
                }
                else if (!player.CanAccessObject(target))
                {
                    return "There is no " + target + " to use " + tool + " on here.";
                }
                else
                {
                    GameObject targetObj = player.GetObject(target);;
                    string nullHandler = "You can't use the " + tool + " with the " + target + ".";
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

            Command give = world.AddDitransitiveCommand("give", CommandType.Ditransitive, "Give what?", new string[]{"to"});
            world.SetDitransitiveCommand("give", (gift, person) => {
                if (!player.InInventory(gift))
                {
                    string indef = (Parser.StartsWithVowel(gift))? "an " : "a ";
                    return "You don't have " + indef + gift + " in you inventory.";
                }
                else if (person == "")
                {
                    return "Give " + gift + " to whom?";
                }
                else if (!player.CanAccessObject(person))
                {
                    return "There is nobody named " + person + " here to give the " + gift + " to.";
                }
                else
                {
                    GameObject personObj = player.GetObject(person);
                    string nullHandler = "You can't give the " + gift + " to the " + person + ".";
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
            //COMMANDS||


            //CHAMBRE
            Room chambre = world.AddRoom("chambre");
            chambre.description = "The bedroom has some saggy straw chairs, a tall wooden WARDROBE, a TABLE, and a brass bed. A cool breeze flows in from the BALCONY. The door to the empty ROOM hangs open.";
            chambre.AddExit("balcony", "balcony");
            chambre.AddExit("room", "antechambre");
            chambre.AddObject<GameObject>("chairs");
            chambre.AddObject<GameObject>("bed");

                //WARDROBE
                Container wardrobe = chambre.AddObject<Container>("wardrobe");
                wardrobe.conditions.Add("locked", true);
                wardrobe.conditions.Add("stuck", true);
                wardrobe.SetTransitiveCommand("what", () => {
                    if (wardrobe.conditions["locked"])
                        {return "It's an old wooden wardrobe. You think you left something in it, but it's locked.";}
                    else if (wardrobe.conditions["stuck"])
                        {return "The wardrobe is unlocked, but it's stuck shut. You think you could pry it open but don't know with what.";}
                    else if (wardrobe.GameObjects.ContainsKey("pipe"))
                        {return "Inside the wardrobe is only dust and an old PIPE.";}
                    else
                        {return "The wardrobe is empty.";}
                });
                wardrobe.SetDitransitiveCommand("use", (tool) => {
                    if (wardrobe.conditions["locked"] && tool == "key")
                        {return "The key fits in the wardrobe drawer. You turn it and hear a click.";}
                    else if(wardrobe.conditions["stuck"] && tool == "knife")
                        {return "You slip the knife between the doors and the wardrobe pops open.";}
                    else {return null;}
                });
                wardrobe.SetTransitiveCommand("take", () => {
                    return "The wardrobe is too heavy to move.";
                });
                    //PIPE
                    GameObject pipe = wardrobe.AddObject("pipe");
                    pipe.SetTransitiveCommand("what", () => {
                        return "An old pipe. You'd forgotten you had it.";
                    });
                    pipe.SetTransitiveCommand("take", () => {
                        player.AddToInventory("pipe", wardrobe);
                        return "You take the pipe. Maybe you'll have a smoke on the balcony.";
                    });
                    pipe.SetTransitiveCommand("use", () => {
                        return "You realize you're out of tobacco. You can't be bothered to go out and buy any.";
                    });
                    

                //TABLE
                GameObject table = chambre.AddObject<GameObject>("table");
                table.SetTransitiveCommand("what", () => {
                    return "A dark brown wooden table. A pile of LAUNDRY and a PAN sit on top.";
                });
                table.SetTransitiveCommand("take", () => {
                    return "The table is stuck to the floor";
                });
                
                //LAUNDRY
                GameObject laundry = chambre.AddObject<GameObject>("laundry");
                laundry.SetTransitiveCommand("what", () => {
                    return "A pile of laundry you haven't felt like washing. A white SHIRT and a dark pair of PANTS lie on top.";
                });
                laundry.SetTransitiveCommand("take", () => {
                    return "No, you'd rather not do the washing now.";
                });

                //SHIRT
                Container shirt = chambre.AddObject<Container>("shirt");
                shirt.SetTransitiveCommand("what", () => {
                    string response = "A nice what shirt. The breast pocket is stained blue.";
                    if (shirt.GameObjects.ContainsKey("pen"))
                        {response = response + " You must have left your PEN in it.";}
                    return response;
                });
                shirt.SetTransitiveCommand("take", () => {
                    return "You shouldn't go out in a stained shirt.";
                });
                    //PEN
                    GameObject pen = shirt.AddObject("pen");
                    pen.SetTransitiveCommand("what", () => {
                        return "A fine, blue pen.";
                    });
                    pen.SetTransitiveCommand("take", () => {
                        player.AddToInventory("pen", shirt);
                        return "Hopefully it doesn't leak again.";
                    });
                    
                //PANTS
                Container pants = chambre.AddObject<Container>("pants");
                pants.SetTransitiveCommand("what", () => {
                    string response = "A wrinkled pair of pants.";
                    if (pants.GameObjects.Count != 0)
                    {
                        response = response + " There seems to be something in the pocket. You reach inside and find ";
                        if (pants.GameObjects.ContainsKey("armband") && pants.GameObjects.ContainsKey("ticket"))
                        {
                            response = response + "a movie TICKET and your black ARMBAND.";
                        }
                        else if (pants.GameObjects.ContainsKey("armband"))
                        {
                            response = response + "your black ARMBAND.";
                        }
                        else if (pants.GameObjects.ContainsKey("ticket"))
                        {
                            response = response + "a movie TICKET";
                        }
                    }
                    return response;
                });
                    //ARMBAND
                    GameObject armband = pants.AddObject("armband");
                    armband.SetTransitiveCommand("what", () => {
                        return "The black arm band from Maman's funeral.";
                    });
                    armband.SetTransitiveCommand("take", () => {
                        player.AddToInventory("armband", pants);
                        return "You think about putting it on, but stuff it in your pocket instead. You don't feel like mourning any more.";
                    });
                    armband.SetTransitiveCommand("use", () => {
                        return "You'd rather not wear the armband. You don't feel like mourning anymore.";
                    });
                    //TICKET
                    GameObject ticket = pants.AddObject("ticket");
                    ticket.SetTransitiveCommand("what", () => {
                        return "The ticket from the movie you took Marie to last night. The Fernandel one. It was funny, but too stupid.";
                    });
                    ticket.SetTransitiveCommand("take", () => {
                        player.AddToInventory("ticket", pants);
                        return "You take the ticket. It's not worth keeping around.";
                    });            

                //PAN
                GameObject pan = chambre.AddObject<GameObject>("pan");
                pan.SetTransitiveCommand("what", () => {
                    return "A cheap cooking pan. Looking at it makes you hungry, but you're all out of anything you could fry.";
                });
                pan.SetTransitiveCommand("take", () => {
                    return "You should leave the pan in your room";
                });
                pan.SetDitransitiveCommand("take", (tool) => {
                    if (tool == "eggs")
                        {return "On second thought, you can't be bothered to cook anything.";}
                    else
                        {return null;}
                });
            
            //BALCONY
            Room balcony = world.AddRoom("balcony");
            balcony.description = "You gaze out over the city. Behind you is the BEDROOM.";
            balcony.AddExit("bedroom", "chambre");
                
                //EMMANUEL
                GameObject emmanuel = balcony.AddObject<GameObject>("emmanuel");
                emmanuel.SetTransitiveCommand("look", () => {
                    return "Below you, you see EMMANUEL waving up at you.";
                });
                emmanuel.SetTransitiveCommand("talk", () => {
                    return "Hey, pal. Come outside. I want to talk to you.";
                });

            //ANTECHAMBRE

            player.current_room = chambre;
            return world;
        }
    }
}
