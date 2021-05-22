using System;
using System.Collections.Generic;

namespace Intf
{
    //////////
    //ELEMENTS
    //////////

    //WORLD
    public class World
    {
        public bool done = false;
        public string start;
        public Player player = new Player();
        public string inputChar = ">";

        Dictionary<string, Room> rooms = new Dictionary<string, Room>();

        List<Command> commands;
        public List<Command> Commands {get{return commands;}} 

        Dictionary<string, Func<string>> responses = new Dictionary<string, Func<string>>();
        public Dictionary<string, Func<string>> Responses {get{return responses;}}
        Dictionary<string, Func<string, string>> responsesT = new Dictionary<string, Func<string, string>>();
        public Dictionary<string, Func<string, string>> ResponsesT {get{return responsesT;}}
        Dictionary<string, Func<string, string, string>> responsesD = new Dictionary<string, Func<string, string, string>>();
        public Dictionary<string, Func<string, string, string>> ResponsesD {get{return responsesD;}}

        public Command AddCommand(string id, CommandType type, string[] preps = null, string[] dipreps = null)
        {
            Command cmd = new Command(id, type, preps, dipreps);
            if (commands == null)
            {
                commands = new List<Command>();
            }
            commands.Add(cmd);
            return cmd;
        }
        public void SetIntransitiveCommand(string id, Func<string> response)
        {
            responses.Add(id, response);
        }
        public void SetTransitiveCommand(string id, Func<string, string> responseT)
        {
            responsesT.Add(id, responseT);
        }
        public void SetDitransitiveCommand(string id, Func<string, string, string> responseD)
        {
            responsesD.Add(id, responseD);
        }

        public Room AddRoom(string roomID)
        {
            Room newRoom = new Room(roomID);
            rooms.Add(roomID, newRoom);
            return newRoom;
        }
    }

    //PLAYER
    public class Player
    {
        public Room current_room;
        List<Item> inventory = new List<Item>();
        public List<Item> Inventory {get{return inventory;}}

        public bool InInventory(string target)
        {
            foreach (Item item in inventory)
            {
                if (item.ID == target)
                {
                    return true;
                }
            }
            return false;
        }

        public bool InRoom(string target)
        {
            return current_room.GameObjects.ContainsKey(target);
        }

        public bool CanAccessObject(string target)
        {
            return InInventory(target) || InRoom(target);
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

            if (current_room.GameObjects.ContainsKey(target))
            {
                return current_room.GameObjects[target];
            }

            return null;
        }

        public void AddToInventory(string target)
        {
            if (current_room.GameObjects.ContainsKey(target))
            {
                inventory.Add((Item) current_room.GameObjects[target]);
                current_room.GameObjects.Remove(target);
            }
        }
        public void RemoveFromInventory(string target)
        {
            Item toRemove = null;
            foreach (Item item in inventory)
            {
                if (item.ID == target)
                {
                    toRemove = item;
                }
            }
            inventory.Remove(toRemove);
        }
    }

    //ROOM
    public class Room
    {
        public Room(string _id)
        {
            id = _id;
        }

        string id;
        public string ID {get{return id;}}
        public string description;
        public Dictionary<string, string> exits;
        // Dictionary<string, Item> items;
        // public Dictionary<string, Item> Items {get{return items;}}
        // Dictionary<string, Person> people;
        // public Dictionary<string, Person> People {get{return people;}}
        Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();
        public Dictionary<string, GameObject> GameObjects {get{return gameObjects;}}

        public T AddObject<T>(string objID) where T : GameObject
        {
            object[] args = new object[] {objID};
            T newObj = (T) Activator.CreateInstance(typeof(T), args);
            gameObjects.Add(objID, newObj);
            return newObj;
        }
        public void RemoveObject(string itemID)
        {
            if (gameObjects.ContainsKey(itemID))
            {
                gameObjects.Remove(itemID);
            }
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
        protected Dictionary<string, Func<string>> responsesT = new Dictionary<string, Func<string>>();
        public Dictionary<string, Func<string>> ResponsesT {get{return responsesT;}}
        protected Dictionary<string, Func<string, string>> responsesD = new Dictionary<string, Func<string, string>>();
        public Dictionary<string, Func<string, string>> ResponsesD {get{return responsesD;}}

        public Dictionary<string, bool> conditions = new Dictionary<string, bool> ();

        public string description;

        public void SetTransitiveCommand(string id, Func<string> responseT)
        {
            responsesT.Add(id, responseT);
        }
        public void SetDitransitiveCommand(string id, Func<string, string> responseD)
        {
            responsesD.Add(id, responseD);
        }
    }

    public class Item : GameObject
    {
        public Item(string _id) : base(_id) {}

        //public bool takeable = false;
        //public bool persistent = true;
        //public bool visible = true;
    }
    public class Container : Item
    {
        public Container(string _id) : base(_id) {}

        //public bool locked;
        Dictionary<string, Item> items = new Dictionary<string, Item>();
        public Dictionary<string, Item> Items {get{return items;}}

        public Item AddItem(string itemID)
        {
            Item newItem = new Item(itemID);
            items.Add(itemID, newItem);

            // if (locked)
            // {
            //     newItem.visible = false;
            // }
            return newItem;
        }
        public void RemoveItem(string itemID)
        {
            items.Remove(itemID);
        }
    }

    public class Person : GameObject
    {
        public Person(string _id) : base(_id) {}

        //public List<string> acceptedGifts;
    }

    ////////
    //PARSER
    ////////

    public enum CommandType {Intransitive, Transitive, Ditransitive, Multi}
    public class Command
    {
        public Command(string _id, CommandType _type, string[] _preps = null, string[] _dipreps = null)
        {
            id = _id;
            type = _type;
            preps = _preps;
            dipreps = _dipreps;
        }
        
        CommandType type;
        public CommandType Type {get{return type;}}
        string id;
        public string ID {get{return id;}}
        string[] preps;
        public string[] Preps {get{return preps;}}
        string[] dipreps;
        public string[] Dipreps {get{return dipreps;}}

        public string [] aliases;
        public string MissingTargetError;
        public string[] MissingTarget2Error;
    }
    public struct IntransitiveCmd
    {
        public IntransitiveCmd(Command _command, Func<string> _response)
        {
            command = _command;
            response = _response;
        }
        public Command command;
        public Func<string> response;
    }
    public struct TransitiveCmd
    {
        public TransitiveCmd(Command _command, Func<string, string> _response)
        {
            command = _command;
            response = _response;
        }
        public Command command;
        public Func<string, string> response;
    }
    public struct DitransitiveCmd
    {
        public DitransitiveCmd(Command _command, Func<string, string, string> _response)
        {
            command = _command;
            response = _response;
        }
        public Command command;
        public Func<string, string, string> response;
    }

    public class Parser
    {
        
        static public string Parse(string input, World world)
        {
            input = input.ToLower();
            string[] words = (input.Contains(" "))?
                input.Split(" ", StringSplitOptions.RemoveEmptyEntries) : new string[]{input};

            Command cmd = FindCmd(words[0], world.Commands, input);
            if (cmd == null)
            {
                return words[0] + " is not a valid command.";
            }           
            List<string> remainder = GetRemainderList(cmd, words);

            return HandleType(cmd, remainder, world);
        }

        static Command FindCmd(string word0, List<Command> commands, string input)
        {
            int matchCount = 0;
            Command match = null;

            foreach (Command cmd in commands)
            {
                if (cmd.ID == word0)
                {
                    matchCount ++;
                    match = cmd;
                }
                
                if (cmd.aliases != null)
                {
                    foreach (string nickname in cmd.aliases)
                    {
                        if (nickname == word0)
                        {
                            matchCount ++;
                            match = cmd;
                        }
                    }
                }
            }
            
            if (matchCount > 1)
            {
                return FindType(input, commands);
            }
            return match;
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

        static Command FindType(string input, List<Command> commands)
        {
            //determine what type it is
            //Command oneType = new Command(cmd.ID, type, cmd.Preps);
            //return oneType;

            return null;
        }

        static string HandleType(Command cmd, List<string> remainder, World world)
        {
            switch (cmd.Type)
            {
                case CommandType.Intransitive:
                    return HandleIntransitive(cmd, remainder, world);
                case CommandType.Transitive:
                    return HandleTransitive(cmd, remainder, world);
                case CommandType.Ditransitive:
                    return HandleDitransitive(cmd, remainder, world);
                default:
                    return "bad type";
            }
        }

        static string HandleIntransitive(Command cmd, List<string> remainder, World world)
        {
            if (remainder.Count > 0)
            {
                return cmd.ID + " shouldn't have any words after it.";
            }
            else
            {
                return world.Responses[cmd.ID]();
            }
        }

        static string HandleTransitive(Command cmd, List<string> remainder, World world)
        {
            if (remainder.Count > 1)
            {
                return "Only one word should come after " + cmd.ID + ".";
            }
            else if (remainder.Count < 1)
            {
                return cmd.MissingTargetError;
            }
            else
            {
                string objID = remainder[0];
                return world.ResponsesT[cmd.ID](objID);
            }
        }

        static string HandleDitransitive(Command cmd, List<string> remainder, World world)
        {
            //Make sure we have an object1
            if (remainder.Count < 1)
            {
                return cmd.MissingTargetError;
            }
            else
            {
                //Get obj1
                string obj1ID = remainder[0];
                remainder.RemoveAt(0);

                if (remainder.Count == 0)
                {
                    string[] responses = cmd.MissingTarget2Error;
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
                    return cmd.ID + " .. " + remainder[0] + " is not a valid command. Try " + cmd.ID + " .. " + cmd.Dipreps[0] + " instead.";
                }
                else
                {
                    remainder.RemoveAt(0);
                    //Make sure we have an object2
                    if (remainder.Count < 1)
                    {
                        string[] responses = cmd.MissingTarget2Error;
                        return responses[0] + obj1ID + responses[1];
                    }
                    else
                    {
                        string obj2ID = remainder[0];
                        return world.ResponsesD[cmd.ID](obj1ID, obj2ID);
                    }
                    
                }
            }
        }

        public static bool StartsWithVowel(string str)
        {
            switch (str.Substring(0, 1))
            {
                case "a":
                    return true;
                case "e":
                    return true;
                case "i":
                    return true;
                case "o":
                    return true;
                case "u":
                    return true;
                default:
                    return false;
            }
        }
    }
}