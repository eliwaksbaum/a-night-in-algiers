using Intf;
using System;

public class AlgiersWorld
{
    public static World SetWorld()
    {
        string instructions = String.Join(Environment.NewLine,
            "help, h - display this list of commands",
            "inventory, inv, i - display your inventory",
            "look, l (around) - look at your surroundings",
            "go, travel (to) - move to a new room",
            "examine, what - examine a an object in the room or in your inventory",
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

        //LOOK
        world.AddIntransitiveCommand("look", CommandType.Intransitive, new string[]{"l"}, preps: new string[] {"around"});
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

        //HELP
        world.AddIntransitiveCommand("help", CommandType.Intransitive, new string[]{"h"});
        world.SetIntransitiveCommand("help", () => {
            return instructions;
        });
        
        //INV
        world.AddIntransitiveCommand("inv", CommandType.Intransitive, new string[]{"i", "inventory"});
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

        //QUIT
        world.AddIntransitiveCommand("quit", CommandType.Intransitive, new string[]{"q"});
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
        
        //WHAT
        world.AddTransitiveCommand("examine", CommandType.Transitive, "Examine what?", new string[]{"what"});
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

        //TAKE
        world.AddTransitiveCommand("take", CommandType.Transitive, "Take what?", new string[]{"pick"}, new string[]{"up"});
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

        //TALK
        world.AddTransitiveCommand("talk", CommandType.Transitive, "Talk to whom?", preps: new string[]{"to", "with"});
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

        //GO
        world.AddTransitiveCommand("go", CommandType.Transitive, "Go where?", new string[]{"travel"}, new string[]{"to"});
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

        //USE
        world.AddDitransitiveCommand("use", CommandType.Ditransitive, "Use what?", new string[]{"on", "with"});
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

        //GIVE
        world.AddDitransitiveCommand("give", CommandType.Ditransitive, "Give what?", new string[]{"to"});
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
        chambre.description = "The bedroom has some saggy straw chairs, a WARDROBE whose mirror has gone yellow, a dressing TABLE, and a brass bed. A cool breeze flows in from the BALCONY. The door to the empty ROOM hangs open.";
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
                {
                    wardrobe.conditions["locked"] = false;
                    return "You turn the key in the lock, but the wardrobe stays shut. You think you need to pry it open somehow.";
                }
                else if(wardrobe.conditions["stuck"] && tool == "knife")
                {
                    wardrobe.conditions["stuck"] = false;
                    return "You slip the knife between the doors and the wardrobe pops open.";
                }
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
                        {response = response + "a movie TICKET and your black ARMBAND.";}
                    else if (pants.GameObjects.ContainsKey("armband"))
                        {response = response + "your black ARMBAND.";}
                    else if (pants.GameObjects.ContainsKey("ticket"))
                        {response = response + "a movie TICKET";}
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
            pan.SetDitransitiveCommand("use", (tool) => {
                if (tool == "eggs")
                    {return "You realize you don't have any bread left and you don't feel like going downstairs to buy some. It doesn't seem worth eating, anyhow.";}
                else
                    {return null;}
            });
        
        //BALCONY
        Room balcony = world.AddRoom("balcony");
        balcony.description = "You gaze out over the city. Behind you is the BEDROOM.";
        balcony.AddExit("bedroom", "chambre");
            
            //EMMANUEL
            GameObject emmanuel_bal = balcony.AddObject<GameObject>("emmanuel");
            emmanuel_bal.SetTransitiveCommand("look", () => {
                return "Below you, you see EMMANUEL waving up at you.";
            });
            emmanuel_bal.SetTransitiveCommand("talk", () => {
                return "Hey, pal. Come outside. I want to talk to you.";
            });

        //ANTECHAMBRE
        Room antechambre = world.AddRoom("antechambre");
        antechambre.description = "The room is empty. Since Maman went to the home, you moved everything to your BEDROOM. Everything is easier that way. The door to the landing is on your LEFT.";
        antechambre.AddExit("bedroom", "chambre");
        antechambre.AddExit("left", "landing");

        //LANDING
        Room landing = world.AddRoom("landing");
        landing.description = "RAYMOND is leaning against the wall, smoking a cigarette. The door to your APARTMENT hangs ajar. The doors to the other apartments are all closed. A staircase leads OUTSIDE.";
        landing.AddExit("apartment", "antechambre");
        landing.AddExit("outside", "street");

            //KEY
            GameObject key = landing.AddObject<GameObject>("key");
            key.SetTransitiveCommand("look", () => {
                return "On the top step lies a familiar looking KEY.";
            });
            key.SetTransitiveCommand("what", () => {
                return "The key to your wardrobe. You must have dropped it.";
            });
            key.SetTransitiveCommand("take", () => {
                player.AddToInventory("key", landing);
                return "You pick the key up off the staircase.";
            });

            //RAYMOND
            Person raymond = landing.AddObject<Person>("raymond");
            raymond.conditions.Add("firstTalk", false);
            raymond.SetTransitiveCommand("talk", () => {
                if (raymond.Gifts.Count == 0 && !raymond.conditions["firstTalk"])
                    {
                        raymond.conditions["firstTalk"] = true;
                        return "'Monsieur Meursault, would you mind doing me a favor? There's this girl, see, and I need to write her a letter. And the only pen I have is the entirely wrong kind of pen for this letter. Could I borrow a pen of yours, a fine pen, Monsieur?'";
                    }
                else if (raymond.Gifts.Count == 0 && raymond.conditions["firstTalk"])
                    {return "'Do you have a fine pen I could borrow, Monsieur?'";}
                else if (raymond.Gifts.Count == 1)
                    {return "'Have you found a nice seashell for my letter, Monsieur?'";}
                else if (raymond.Gifts.Count == 2)
                    {return "'Thanks again, Meursault. You're a real pal.'";}
                else
                    {return null;}
            });
            raymond.SetDitransitiveCommand("give", (gift) => {
                if (raymond.Gifts.Count == 0 && gift == "pen")
                {
                    player.RemoveFromInventory("pen");
                    raymond.AddGift("pen");
                    return "'Thank you, Monsieur. Might I ask you another favor? This girl, see, she loves the ocean. I think if I put in a little seashell with this letter it might go over better with her. Do you think you could find me a nice little seashell?'";
                }
                else if (raymond.Gifts.Count == 1 && gift == "seashell")
                {
                    player.RemoveFromInventory("seashell");
                    raymond.AddGift("seashell");
                    return "'I think this will do the trick. Now you're a pal, Meursault. I don't know what this girl has gotten so upset over, anyway. It was nothing, really.'";
                }
                else
                {
                    string indef = Parser.StartsWithVowel(gift)? " an " : " a ";
                    string epy = (raymond.Gifts.Count < 2)? " Monsieur, " : " Meursault, ";
                    return "'Why," + epy + "what would I do with" + indef + gift + "?'";
                }
            });

        //STREET
        Room street = world.AddRoom("street");
        street.description = "You stand on a busy street corner. Celeste's RESTAURANT is across the way, next to the streetcar stop for the line to the BEACH.";
        street.AddExit("restaurant", "restaurant");
        street.AddExit("beach", "beach");
        street.AddExit("building", "landing");

            //KNIFE
            GameObject knife = street.AddObject<GameObject>("knife");
            knife.SetTransitiveCommand("look", () => {
                return "The sun glints off of a small KNIFE lying discarded on the pavement.";
            });
            knife.SetTransitiveCommand("what", () => {
                return "A short, rusted knife.";
            });
            knife.SetTransitiveCommand("take", () => {
                player.AddToInventory("knife", street);
                return "You squint from the glare as you pick the knife up off the street. You never know when a knife might be useful.";
            });

            //EMMANUEL
            Person emmanuel_st = street.AddObject<Person>("emmanuel");
            emmanuel_st.conditions.Add("firstTalk", false);
            emmanuel_st.SetTransitiveCommand("look", () => {
                return "EMMANUEL stands outside the BUILDING. He waves at you.";
            });
            emmanuel_st.SetTransitiveCommand("talk", () => {
                if (!emmanuel_st.conditions["firstTalk"] && emmanuel_st.Gifts.Count == 0)
                {
                    emmanuel_st.conditions["firstTalk"] = true;
                    return "'Hey, pal. Do you remember the name of that movie you took Marie to the other day? I'd like to take this girl to see it tonight.'" + Environment.NewLine + "You can't remember what the movie was called. You tell Emmanuel you'll just give him the ticket.";
                }
                else if (emmanuel_st.Gifts.Count == 0)
                {
                    return "'Did you find that movie ticket?'";
                }
                else
                {
                    return "Thanks again, sport.";
                }
            });
            emmanuel_st.SetDitransitiveCommand("give", (gift) => {
                if (gift == "ticket")
                {
                    player.RemoveFromInventory("ticket");
                    emmanuel_st.AddGift("ticket");
                    balcony.RemoveObject("emmanuel");
                    return "'Ah, the Fernandel one. Sounds fun. Thanks, sport.'";
                }
                else
                {
                    string ind = Parser.StartsWithVowel(gift)? " an " : " a ";
                    return "'Well, I'm not sure what I'd do with" + ind + gift + ", pal.'";
                }
            });
        
        //BEACH
        Room beach = world.AddRoom("beach");
        beach.description = "The sun beats down on the white sand. The stone stairs lead back up to where you can catch a streetcar back to your BLOCK.";
        beach.AddExit("block", "street");

            //SEASHELL
            GameObject seashell = beach.AddObject<GameObject>("seashell");
            seashell.SetTransitiveCommand("look", () => {
                return "You notice a SEASHELL in the sand.";
            });
            seashell.SetTransitiveCommand("what", () => {
                return "A small, pretty seashell.";
            });
            seashell.SetTransitiveCommand("take", () => {
                player.AddToInventory("seashell", beach);
                return "You pick up the seashell. It is surprisingly smooth.";
            });

            //MARIE
            Person marie = beach.AddObject<Person>("marie");
            marie.conditions.Add("swimming", true);
            marie.conditions.Add("first", false);
            marie.conditions.Add("second", false);
            marie.SetTransitiveCommand("look", () => {
                if (marie.conditions["swimming"])
                    {return "MARIE is swimming in the water. She looks so good.";}
                else
                    {return "MARIE is sitting on the sand. You want her so bad.";}
            });
            marie.SetTransitiveCommand("talk", () => {
                if (marie.conditions["swimming"])
                {
                    marie.conditions["swimming"] = false;
                    return "MARIE wades out of the water. Her tan makes her face look like a flower. She wants to know what you've been doing today; you tell her nothing, really.";
                }
                else if (!marie.conditions["first"])
                {
                    marie.conditions["first"] = true;
                    return "'Do you love me?'" + Environment.NewLine + "You tell her it doesn't mean anything but that you don't think so.";
                }
                else if (!marie.conditions["second"])
                {
                    marie.conditions["second"] = true;
                    return "'Do you want to marry me?'" + Environment.NewLine + "You tell her it doesn't make any difference to you and that you could if she wanted to.";
                }
                else
                    {return "'You're peculiar. That's probably why I love you.'";}
            });
            marie.SetDitransitiveCommand("give", (gift) => {
                if (gift == "necklace")
                {
                    string response = "You hand Marie the necklace. She seems happy with it. She smiles at you and laughs in such a way that you kiss her.";
                    if (marie.conditions["swimming"])
                    {
                        marie.conditions["swimming"] = false;
                        response = "MARIE wades out of the water. Her tan makes her face look like a flower. " + response;
                    }
                    return response;
                }
                else
                {
                    string ind = Parser.StartsWithVowel(gift)? " an " : " a ";
                    return "You don't think Marie would like" + ind + gift + " very much.";
                }
            });

        //RESTAURANT
        Room restaurant = world.AddRoom("restaurant");
        restaurant.description = "The door back to the STREET creaks when you open it. The restaurant is quiet. CELESTE stands behind the register. The chrome finish on the BAR catches the sunlight.";
        restaurant.AddExit("street", "street");

            //CELESTE
            Container celeste = restaurant.AddObject<Container>("celeste");
            celeste.SetTransitiveCommand("talk", () => {
                string response = "I'm sorry for your loss, Monsieur. You only have one mother.";
                if (celeste.GameObjects.ContainsKey("eggs"))
                {
                    response = response + " I've a few extra EGGS today, if you want any.";
                }
                return response;
            });
            celeste.SetDitransitiveCommand("give", (gift) => {
                return "What's this? Don't worry about your bill; those are just details between us.";
            });
                //EGGS
                GameObject eggs = celeste.AddObject("eggs");
                eggs.SetTransitiveCommand("what", () => {
                    return "A few extra eggs Celeste is letting you have.";
                });
                eggs.SetTransitiveCommand("take", () => {
                    player.AddToInventory("eggs", celeste);
                    return "You decide to take two eggs. You wonder if you ought to have taken three";
                });

            //BAR
            Container bar = restaurant.AddObject<Container>("bar");
            bar.SetTransitiveCommand("what", () => {
                string response = "It hurts to look at the bar; you can feel the sunlight reflecting off of it.";
                if (bar.GameObjects.ContainsKey("necklace"))
                {
                    response = response + " Somone left a NECKLACE draping off the edge.";
                }
                return response;
            });
                //NECKLACE
                GameObject necklace = bar.AddObject("necklace");
                necklace.SetTransitiveCommand("what", () => {
                    return "A simple, gold necklace. You think it would look nice on Marie.";
                });
                necklace.SetTransitiveCommand("take", () => {
                    player.AddToInventory("necklace", bar);
                    return"There's no way to find out whose it is. Might as well take it.";
                });

        player.current_room = chambre;
        return world;
    }
}