using System.Collections.Generic;

namespace CommandData
{
    namespace Commands
    {
        public enum CommandType {Intransitive, Transitive, Ditransitive, Multi}
        public struct Command
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
        }
    }

    public class Data
    {
        public static Commands.Command[] commands = new Commands.Command[]
        {
            new Commands.Command("look", Commands.CommandType.Intransitive, new string[] {"around"}),
            new Commands.Command("help", Commands.CommandType.Intransitive),
            new Commands.Command("inv", Commands.CommandType.Intransitive),
            new Commands.Command("quit", Commands.CommandType.Intransitive),
            new Commands.Command("examine", Commands.CommandType.Transitive),
            new Commands.Command("take", Commands.CommandType.Transitive, new string[]{"up"}),
            new Commands.Command("talk", Commands.CommandType.Transitive, new string[]{"to", "with"}),
            new Commands.Command("go", Commands.CommandType.Transitive, new string[]{"to"}),
            new Commands.Command("give", Commands.CommandType.Ditransitive, _dipreps: new string[]{"to"}),
            new Commands.Command("use", Commands.CommandType.Multi, _dipreps: new string[]{"on", "with"})
        };
        public static Dictionary<string, string> aliases = new Dictionary<string, string>()
        {
            {"l", "look"},
            {"h", "help"},
            {"i", "inv"},
            {"inventory", "inv"},
            {"q", "quit"},
            {"what", "examine"},
            {"travel", "go"},
            {"pick", "take"}
        };
        public static Dictionary<string, string> MissingTargetErrors = new Dictionary<string, string>()
        {
            {"examine", "Examine what?"},
            {"take", "Take what?"},
            {"talk", "Talk to whom?"},
            {"go", "Go where?"},
            {"use", "Use what?"},
            {"give", "Give what?"}
        };
        public static Dictionary<string, string[]> InnaccessableTargetErrors = new Dictionary<string, string[]>()
        {
            ["examine"] = new string[] {"There is no ", " to examine here."},
            ["take"] = new string[] {"There is no ", " to take here."},
            ["talk"] = new string[] {"There is nobody named ", " to talk to here."},
            ["go"] = new string[] {"There is no ", " to go to from here"},
            ["use"] = new string[] {"You don't have ", " in your inventory."},
            ["give"] = new string[] {"You don't have ", " in your inventory."}
        };
        public static Dictionary<string, string[]> NullHandlerErrors = new Dictionary<string, string[]>()
        {
            ["take"] = new string[] {"You can't take ", " ."},
            ["talk"] = new string[] {"There is nobody named ", " to talk to here."},

            ["use"] = new string[] {"You can't use ", " on "},
            ["give"] = new string[] {"You can't give ", " to "}
        };
        public static Dictionary<string, string[]> MissingTarget2Errors = new Dictionary<string, string[]>()
        {
            ["give"] = new string[] {"Give ", " to whom?"},
            ["use"] = new string[] {"Use ", " on what?"}
        };
        public static Dictionary<string, string[]> InaccessableTarget2Errors = new Dictionary<string, string[]>()
        {
            ["give"] = new string[] {"There is nobody named ", " to give ", " to here."},
            ["use"] = new string[] {"There is no ", " to use ", " on here."}
        };
    }
}