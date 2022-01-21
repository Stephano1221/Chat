namespace Chat
{
    public static class Permissions
    {
        [Flags]
        public enum IndividualPermissionNumber : ulong
        {
            None = 0,
            Overseer = 1,
            Kick = 2,
            Ban = 4,
            Mute = 8,
            Deafen = 16,
            DeleteMessages = 32,
            RegulateChannels = 64,
            RegulateRanks = 128,
            PingAll = 256,
            ReadMessages = 512,
            SendMessages = 1024,
            HearVoice = 2048,
            SendVoice = 4096
        }

        public static Dictionary<IndividualPermissionNumber, Permission> permissionNames = new Dictionary<IndividualPermissionNumber, Permission>()
        {
            //{ IndividualPermissionNumber.None, new Permission("None", "No permissions") },
            { IndividualPermissionNumber.Overseer, new Permission("Overseer", "Caution: grants all permissions") },
            { IndividualPermissionNumber.Kick, new Permission("Kick users", "Allows users to kick other users from the server. They may rejoin later.") },
            { IndividualPermissionNumber.Ban, new Permission("Ban users", "Allows users to ban other users from the server. They cannot rejoin unless they are unbanned.") },
            { IndividualPermissionNumber.Mute, new Permission("Mute users", "Allows users to prevent others from speaking in voice channels.") },
            { IndividualPermissionNumber.Deafen, new Permission("Deafen users", "Allows users to prevent others from listening in voice channels. This also prevents them from speaking.") },
            { IndividualPermissionNumber.DeleteMessages, new Permission("Delete messages", "Allows users to delete messages sent by other users.") },
            { IndividualPermissionNumber.RegulateChannels, new Permission("Regulate channels", "Allows users to create, edit and delete channels which they have access to.") },
            { IndividualPermissionNumber.RegulateRanks, new Permission("Regulate ranks", "Allows users to create, edit and delete ranks below their highest rank, both server-wide and per-channel.") },
            { IndividualPermissionNumber.PingAll, new Permission("Ping @all", "Allows users to ping @all users.") },
            { IndividualPermissionNumber.ReadMessages, new Permission("Read messages", "Allows users to read messages in text channels.") },
            { IndividualPermissionNumber.SendMessages, new Permission("Send messages", "Allows users to send messages in text channels.") },
            { IndividualPermissionNumber.HearVoice, new Permission("Listen", "Allows users to connect and listen in voie channels.") },
            { IndividualPermissionNumber.SendVoice, new Permission("Speak", "Allows users to speak in voice channels.") }
        };

        public static ulong GetPermissionsNumberAsUlong(IndividualPermissionNumber permissionNumber)
        {
            return Convert.ToUInt64(permissionNumber);
        }

        public static bool ContainsPermission(IndividualPermissionNumber basePermissionNumber, IndividualPermissionNumber targetPermissionNumber)
        {
            if ((targetPermissionNumber & basePermissionNumber) == basePermissionNumber)
            {
                return true;
            }
            return false;
        }

        public static IndividualPermissionNumber AddPermission(IndividualPermissionNumber basePermissionNumber, IndividualPermissionNumber targetPermissionNumber)
        {
            return targetPermissionNumber |= basePermissionNumber;
        }

        public static IndividualPermissionNumber RemovePermission(IndividualPermissionNumber basePermissionNumber, IndividualPermissionNumber targetPermissionNumber)
        {
            return targetPermissionNumber &= ~basePermissionNumber;
        }


        public class Permission
        {
            public string Name { get; set; }
            public string Description { get; set; }

            public Permission(string name, string description)
            {
                this.Name = name;
                this.Description = description;
            }
        }
    }
}
