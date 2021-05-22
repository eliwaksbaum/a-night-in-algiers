using System;
using System.Collections.Generic;
using CommandData;
using CommandData.Commands;

namespace Intf
{
    //////////
    //ELEMENTS
    //////////

    //WORLD
    public class World
    {
        public bool done = false;
        public Player player = new Player();
        public string inputChar = ">";

        Dictionary<string, Room> rooms = new Dictionary<string, Room>();
        public Dictionary<string, Func<string>> responses;
        public Dictionary<string, Func<string, string>> responsesT;
        public Dictionary<string, Func<string, string, string>> responsesD;

        public Room AddRoom(string roomID)
        {
            Room newRoom = new Room(roomID);
            rooms.Add(roomID, newRoom);
            return newRoom;
        }

        public void Done()
        {
            responses = new Dictionary<string, Func<string>>()
            {
                {"look", Browse},
                {"help", Help},
                {"inv", Inv},
                {"quit", Quit}
            };
            responsesT = new Dictionary<string, Func<string, string>>()
            {
                {"examine", What},
                {"take", Take},
                {"talk", Talk},
                {"go", Go},
                {"use", UseT}
            };
            responsesD = new Dictionary<string, Func<string, string, string>>()
            {
                {"give", Give},
                {"use", UseD}
            };
        }

        public string Start()
        {
            return "this is the start";
        }

        public string Browse()
        {
            return "look at all this stuff";
        }

        public string Help()
        {
            return "heh good luck";
        }

        public string Inv()
        {  
            return "this is your inventory";
        }

        public string Quit()
        {
            done = true;
            return "way to be existential. bye";
        }

        public string What(string objID)
        {
            return "lemme see";
        }

        public string Take(string objID)
        {
            return "take";
        }

        public string Talk(string objID)
        {
            return "yatta yata";
        }

        public string Go(string objID)
        {
            return "away we go";
        }

        public string UseT(string objID)
        {
            return "use t";
        }

        public string UseD(string obj1Id, string obj2ID)
        {
            return "use d";
        }

        public string Give(string obj1ID, string obj2ID)
        {
            return "here u go";
        }
    }

    //PLAYER
    public class Player
    {
        Room current_room;
        List<Item> inventory;

        public bool CanAccessObject(string target)
        {
            bool inInv = false;
            foreach (Item item in inventory)
            {
                if (item.ID == target)
                {
                    inInv = true;
                    break;
                }
            }

            bool inRoom;
            inRoom = current_room.GameObjects.ContainsKey(target);

            return inInv || inRoom;
        }

        public GameObject GetObject(string target)
        {
            foreach (Item item in inventory)
            {
                if (item.ID == target)
                {
                    return item;
                }
            }

            return current_room.GameObjects[target];
        }
    }

    //ROOM
    public class Room
    {
        public Room(string _id)
        {
            _id = id;
        }

        string id;
        public string ID {get{return id;}}
        public string description;
        public Dictionary<string, string> exits;
        Dictionary<string, GameObject> gameObjects;
        public Dictionary<string, GameObject> GameObjects {get{return gameObjects;}}

        public Item AddItem(string itemID)
        {
            Item newItem = new Item(itemID);
            if (gameObjects == null)
            {
                gameObjects = new Dictionary<string, GameObject>();
            }
            gameObjects.Add(itemID, newItem);
            return newItem;
        }
        public Person AddPerson(string personID)
        {
            Person newPerson = new Person(personID);
            if (gameObjects == null)
            {
                gameObjects = new Dictionary<string, GameObject>();
            }
            gameObjects.Add(personID, newPerson);
            return newPerson;
        }
    }

    //GAMEOBJECTS
    public class GameObject
    {
        public GameObject(string _id)
        {
            id = _id;
        }

        string id;
        public string ID {get{return id;}}
        protected Dictionary<string, Func<string>> responsesT;
        public Dictionary<string, Func<string>> ResponsesT {get{return responsesT;}}
        protected Dictionary<string, Func<string, string>> responsesD;
        public Dictionary<string, Func<string, string>> ResponsesD {get{return responsesD;}}

        public Func<string> OnLook;
        public Func<string> OnWhat;

        public virtual void Done(){}
    }

    public class Item : GameObject
    {
        public Item(string _id) : base(_id) {}

        public bool takeable = false;
        public bool targetable = false;
        public List<string> acceptedTools;
        public bool visible = true;

        public Func<string> OnTake;
        public Func<string> OnUseT;
        public Func<string, string> OnUseDSuccess;
        public Func<string, string> OnUseDFail;

        public string Take()
        {
            if (takeable)
            {
                //Move item to inventory
            }
            return OnTake();
        }

        public string UseD(string tool)
        {
            if (acceptedTools.Contains(tool))
            {
                return OnUseDSuccess(tool);
            }
            else
            {
                return OnUseDFail(tool);
            }
        }

        public override void Done()
        {
            responsesT = new Dictionary<string, Func<string>> ()
            {
                {"what", OnWhat},
                {"take", Take},
                {"use", OnUseT}
            };
            responsesD = new Dictionary<string, Func<string, string>> ()
            {
                {"use", UseD}
            };
        }
    }
    public class Container : Item
    {
        public Container(string _id) : base(_id) {}

        public bool locked;
        Dictionary<string, Item> items;

        public Item AddItem(string itemID)
        {
            Item newItem = new Item(itemID);
            if (items == null)
            {
                items = new Dictionary<string, Item>();
            }
            items.Add(itemID, newItem);

            if (locked)
            {
                newItem.visible = false;
            }
            return newItem;
        }
    }

    public class Person : GameObject
    {
        public Person(string _id) : base(_id) {}

        public List<string> acceptedGifts;

        public Func<string> OnTalk;
        public Func<string, string> OnAcceptGift;
        public Func<string, string> OnDeclineGift;

        public string Give(string gift)
        {
            if (acceptedGifts.Contains(gift))
            {
                return OnAcceptGift(gift);
            }
            else
            {
                return OnDeclineGift(gift);
            }
        }

        public override void Done()
        {
            responsesT = new Dictionary<string, Func<string>> ()
            {
                {"what", OnWhat},
                {"talk", OnTalk}
            };
            responsesD = new Dictionary<string, Func<string, string>> ()
            {
                {"give", Give}
            };
        }
    }

    ////////
    //PARSER
    ////////

    public class Parser
    {
        

        static public string Parse(string input, World world)
        {
            input = input.ToLower();
            string[] words = (input.Contains(" "))?
                input.Split(" ", StringSplitOptions.RemoveEmptyEntries) : new string[]{input};

            Command cmd = FindCmd(words[0]);
            if (cmd.ID == "")
            {
                return words[0] + " is not a valid command.";
            }           
            List<string> remainder = GetRemainderList(cmd, words);

            return HandleType(cmd, remainder, world);
        }

        static Command FindCmd(string word0)
        {
            foreach (Command cmd in CommandData.Data.commands)
            {
                if (cmd.ID == word0)
                {
                    return cmd;
                }
                else if (CommandData.Data.aliases.ContainsKey(word0))
                {
                    if (cmd.ID == CommandData.Data.aliases[word0])
                    {
                        return cmd;
                    }
                }
            }
            return new Command();
        }

        static List<string> GetRemainderList(Command cmd, string[] words)
        {
            List<string> remainder = new List<string>();
            foreach (string word in words)
            {
                remainder.Add(word);
            }

            remainder.RemoveAt(0);
            if (cmd.Preps != null && remainder.Count > 0)
            {
                foreach (string prep in cmd.Preps)
                {
                    if (remainder[0] == prep)
                    {
                        remainder.RemoveAt(0);
                        break;
                    }
                }
            }
            return remainder;
        }

        static Command FindType(Command cmd, List<string> remainder)
        {
            //determine what type it is
            //Command oneType = new Command(cmd.ID, type, cmd.Preps);
            //return oneType;

            return new Command();
        }

        static string HandleType(Command cmd, List<string> remainder, World world)
        {
            switch (cmd.Type)
            {
                case CommandType.Intransitive:
                    return HandleIntransitive(cmd.ID, remainder, world);
                case CommandType.Transitive:
                    return HandleTransitive(cmd.ID, remainder, world);
                case CommandType.Ditransitive:
                    return HandleDitransitive(cmd, remainder, world);
                case CommandType.Multi:
                    return HandleType(FindType(cmd, remainder), remainder, world);
                default:
                    return "bad type";
            }
        }

        static string HandleIntransitive(string cmd, List<string> remainder, World world)
        {
            if (remainder.Count > 0)
            {
                return cmd + " is an intransitive command, and can't accept an object.";
            }
            else
            {
                return world.responses[cmd]();
            }
        }

        static string HandleTransitive(string cmd, List<string> remainder, World world)
        {
            if (remainder.Count > 1)
            {
                return "Only one word should come after " + cmd + ".";
            }
            else if (remainder.Count < 1)
            {
                return CommandData.Data.MissingTargetErrors[cmd];
            }
            else
            {
                string objID = remainder[0];
                return world.responsesT[cmd](objID);
            }
        }

        static string HandleDitransitive(Command cmd, List<string> remainder, World world)
        {
            //Make sure we have an object1
            if (remainder.Count < 1)
            {
                return CommandData.Data.MissingTargetErrors[cmd.ID];
            }
            else
            {
                //Get obj1
                string obj1ID = remainder[0];
                remainder.RemoveAt(0);

                if (remainder.Count == 0)
                {
                    string[] responses = CommandData.Data.MissingTarget2Errors[cmd.ID];
                    return responses[0] + obj1ID + responses[1];
                }

                //Deal with diprep
                bool goodDiprep = false;
                foreach (string diprep in cmd.Dipreps)
                {
                    if (remainder[0] == diprep)
                    {
                        goodDiprep = true;
                        break;
                    }
                }
                if (!goodDiprep)
                {
                    Console.WriteLine(cmd.Dipreps);
                    return cmd.ID + " .. " + remainder[0] + " is not a valid command. Try " + cmd.ID + " .. " + cmd.Dipreps[0] + " instead.";
                }
                else
                {
                    remainder.RemoveAt(0);
                    //Make sure we have an object2
                    if (remainder.Count < 1)
                    {
                        string[] responses = CommandData.Data.MissingTarget2Errors[cmd.ID];
                        return responses[0] + obj1ID + responses[1];
                    }
                    else
                    {
                        string obj2ID = remainder[0];
                        return world.responsesD[cmd.ID](obj1ID, obj2ID);
                    }
                    
                }
            }
        }
    }
}