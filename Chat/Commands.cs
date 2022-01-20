namespace Chat
{
    public static class Commands
    {
        public static Command[] defaultCommandsInfo = new Command[]
        {
            new Command(new string[] { "help" }, "Provides a list of available commands, or information on a specified command", null, new Command.Parameter[] { new Command.Parameter("Command", "The command to query") }),
            new Command(new string[] { "kick" }, "Kicks a user from the server", new Command.Parameter[] { new Command.Parameter("User", "The user to kick") }, new Command.Parameter[] { new Command.Parameter("Reason", "The reason for the kick") }),
            new Command(new string[] { "manageranks" }, "Opens the rank management screen", null, null),
            new Command(new string[] { "giverank" }, "Gives a user the specified rank", new Command.Parameter[] { new Command.Parameter("User", "The user to give the rank to"), new Command.Parameter("Rank", "The rank to give") }, null),
            new Command(new string[] { "takerank" }, "Takes a user from the specified rank", new Command.Parameter[] { new Command.Parameter("User", "The user to take the rank from"), new Command.Parameter("Rank", "The rank to take") }, null),
            new Command(new string[] { "ranks" }, "Lists all ranks, or the ranks of a specified user", null, new Command.Parameter[] { new Command.Parameter("User", "The user to list the ranks of") })
        };

        public class Command
        {
            public string[] Names { get; set; }
            public string Description { get; set; }
            public Parameter[] RequiredParameters { get; set; }
            public Parameter[] OptionalParameters { get; set; }

            public Command(string[] names, string description, Parameter[] requiredParameters, Parameter[] optionalparameters)
            {
                this.Names = names;
                this.Description = description;
                this.RequiredParameters = requiredParameters;
                this.OptionalParameters = optionalparameters;
            }

            public class Parameter
            {
                public string Name { get; set; }
                public string Description { get; set; }

                public Parameter(string name, string description)
                {
                    this.Name = name;
                    this.Description = description;
                }
            }
        }
    }
}
