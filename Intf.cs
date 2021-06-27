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
        public string state;
        public string start;
        public Player player = new Player();
        public string inputChar = ">";

        Dictionary<string, Room> rooms = new Dictionary<string, Room>();
        public Dictionary<string, Room> Rooms {get{return rooms;}}

        List<Command> commands = new List<Command>();
        public List<Command> Commands {get{return commands;}} 

        Dictionary<string, Func<string>> responses = new Dictionary<string, Func<string>>();
        public Dictionary<string, Func<string>> Responses {get{return responses;}}
        Dictionary<string, Func<string, string>> responsesT = new Dictionary<string, Func<string, string>>();
        public Dictionary<string, Func<string, string>> ResponsesT {get{return responsesT;}}
        Dictionary<string, Func<string, string, string>> responsesD = new Dictionary<string, Func<string, string, string>>();
        public Dictionary<string, Func<string, string, string>> ResponsesD {get{return responsesD;}}

        public Command AddIntransitiveCommand(string id, CommandType type, string[] aliases = null, string[] preps = null)
        {
            Command cmd = new Command(id, type, _aliases: aliases, _preps: preps);
            commands.Add(cmd);
            return cmd;
        }
        public Command AddTransitiveCommand(string id, CommandType type, string missingTargetError, string[] aliases = null, string[] preps = null)
        {
            Command cmd = new Command(id, type, missingTargetError, _aliases: aliases, _preps: preps);
            commands.Add(cmd);
            return cmd;
        }
        public Command AddDitransitiveCommand(string id, CommandType type, string missingTargetError, string[] dipreps, string[] aliases = null)
        {
            Command cmd = new Command(id, type, missingTargetError, dipreps, aliases);
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
        Dictionary<string, GameObject> inventory = new Dictionary<string, GameObject>();
        public Dictionary<string, GameObject> Inventory {get{return inventory;}}
        List<string> waypoints = new List<string>();
        public List<string> Waypoints {get{return waypoints;}}
        Dictionary<string, int> counters = new Dictionary<string, int>();
        public Dictionary<string, int> Counters {get{return counters;}}

        public void AddWaypoint(string newPoint)
        {
            if (!waypoints.Contains(newPoint))
            {
                waypoints.Add(newPoint);
            }
        }

        public void AddCounter(string counter)
        {
            counters.Add(counter, 0);
        }

        public void IncrementCounter(string counter)
        {
            if (counters.ContainsKey(counter))
            {
                counters[counter] += 1;
            }
        }

        public bool InInventory(string target)
        {
            return inventory.ContainsKey(target);
        }

        public bool InRoom(string target)
        {
            return current_room.InRoom(target);
        }

        public bool CanAccessObject(string target)
        {
            return InInventory(target) || InRoom(target);
        }

        public GameObject GetObject(string target)
        {
            if (InInventory(target))
            {
                return inventory[target];
            }

            if (InRoom(target))
            {
                return current_room.GetObject(target);
            }

            return null;
        }

        public void AddToInventory(string target, IOrigin origin)
        {
            if (origin.GameObjects.ContainsKey(target))
            {
                inventory.Add(target, origin.GameObjects[target]);
                origin.RemoveObject(target);
            }
        }
        public void RemoveFromInventory(string target)
        {
            inventory.Remove(target);
        }
    }

    //ORIGIN
    public interface IOrigin
    {
        void RemoveObject(string target);
        Dictionary<string, GameObject> GameObjects {get;}
    }

    //ROOM
    public class Room : IOrigin
    {
        public Room(string _id)
        {
            id = _id;
        }

        string id;
        public string ID {get{return id;}}
        public string description;
        public Action OnEnter = () => {};
        public Action OnExit = () => {};
        Dictionary<string, string> exits = new Dictionary<string, string>();
        public Dictionary<string, string> Exits {get{return exits;}}
        Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();
        public Dictionary<string, GameObject> GameObjects {get{return gameObjects;}}

        Dictionary<string, Container> containers = new Dictionary<string, Container>();

        public T AddObject<T>(string objID) where T : GameObject
        {
            GameObject newObj;

            if (typeof(T) == typeof(Container))
            {
                newObj = new Container(objID);
                containers.Add(objID, (Container) newObj);
            }
            else
            {
                Object[] args = new Object[] {objID};
                newObj = (T) Activator.CreateInstance(typeof(T), args);
            }

            gameObjects.Add(objID, newObj);
            return (T) newObj;
        }
        public void RemoveObject(string itemID)
        {
            if (gameObjects.ContainsKey(itemID))
            {
                gameObjects.Remove(itemID);
            }
        }

        public void AddExit(string goWord, string roomID)
        {
            exits.Add(goWord, roomID);
        }

        public bool InRoom(string target)
        {
            bool inBase = gameObjects.ContainsKey(target);
            foreach (Container container in containers.Values)
            {
                if (container.GameObjects.ContainsKey(target))
                {
                    return true;
                }
            }
            return inBase;
        }

        public GameObject GetObject(string target)
        {
            if (gameObjects.ContainsKey(target))
            {
                return gameObjects[target];
            }
            else
            {
                foreach (Container container in containers.Values)
                {
                    if (container.GameObjects.ContainsKey(target))
                    {
                        return container.GameObjects[target];
                    }
                }
            }
            return null;
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

    public class Container : GameObject, IOrigin
    {
        public Container(string _id) : base(_id) {}

        Dictionary<string, GameObject> items = new Dictionary<string, GameObject>();
        public Dictionary<string, GameObject> GameObjects {get{return items;}}

        public GameObject AddObject(string itemID)
        {
            GameObject newItem = new GameObject(itemID);
            items.Add(itemID, newItem);

            return newItem;
        }
        public void RemoveObject(string itemID)
        {
            items.Remove(itemID);
        }
    }

    public class Person : GameObject
    {
        public Person(string _id) : base(_id) {}

        List<string> gifts = new List<string>();
        public List<string> Gifts {get{return gifts;}}

        public void AddGift(string giftID)
        {
            gifts.Add(giftID);
        }
    }

    ////////
    //PARSER
    ////////

    public enum CommandType {Intransitive, Transitive, Ditransitive, Multi}
    public class Command
    {
        public Command(string _id, CommandType _type, string _missingTargetError = null,
        string[] _dipreps = null, string[] _aliases = null, string[] _preps = null)
        {
            id = _id;
            type = _type;
            missingTargetError = _missingTargetError;
            dipreps = _dipreps;
            aliases = _aliases;
            preps = _preps;
        }
        
        CommandType type;
        public CommandType Type {get{return type;}}
        string id;
        public string ID {get{return id;}}
        string[] preps;
        public string[] Preps {get{return preps;}}
        string[] dipreps;
        public string[] Dipreps {get{return dipreps;}}
        string [] aliases;
        public string [] Aliases {get{return aliases;}}
        string missingTargetError;
        public string MissingTargetError {get{return missingTargetError;}}
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
        static public string Clear = Environment.NewLine + Environment.NewLine;
        
        static public string Parse(string input, World world)
        {
            input = input.ToLower();

            if (String.IsNullOrWhiteSpace(input))
            {
                return "Please type a command.";
            }

            string[] words = (input.Contains(" "))?
                input.Split(" ", StringSplitOptions.RemoveEmptyEntries) : new string[]{input};

            Command cmd = FindCmd(words[0], world.Commands);
            if (cmd == null)
            {
                return words[0] + " is not a valid command.";
            }           
            List<string> remainder = GetRemainderList(cmd, words);

            return HandleType(cmd, remainder, world);
        }

        static Command FindCmd(string word0, List<Command> commands)
        {
            foreach (Command cmd in commands)
            {
                if (cmd.ID == word0)
                {
                    return cmd;
                }
                
                if (cmd.Aliases != null)
                {
                    foreach (string nickname in cmd.Aliases)
                    {
                        if (nickname == word0)
                        {
                            return cmd;
                        }
                    }
                }
            }
            return null;
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
                    return world.ResponsesD[cmd.ID](obj1ID, "");
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
                    string obj2ID = (remainder.Count < 1)? "" : remainder[0];
                    return world.ResponsesD[cmd.ID](obj1ID, obj2ID);
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