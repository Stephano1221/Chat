namespace Chat
{
    public static class Permissions
    {
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

        public static ulong GetPermissionsNumber(IndividualPermissionNumber[] individualPermissionNumbers)
        {
            ulong permissionNumber = 0;
            foreach (IndividualPermissionNumber individualPermissionNumber in individualPermissionNumbers)
            {
                permissionNumber += Convert.ToUInt64(individualPermissionNumber);
            }
            return permissionNumber;
        }
    }
}
