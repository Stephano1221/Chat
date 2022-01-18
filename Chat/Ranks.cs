namespace Chat
{
    public static class Ranks
    {
        public static List<Rank> ranksInMemoryForTestingOnly = new List<Rank>() //TESTING: POPULATED FOR TESTING PURPOSES ONLY. CHECK DATABASE WHEN CHECKING RANKS.
        {
            new Rank(3, "Administrator", Color.Blue, 3, Permissions.GetPermissionsNumber(new Permissions.IndividualPermissionNumber[] { Permissions.IndividualPermissionNumber.Overseer, Permissions.IndividualPermissionNumber.Kick, Permissions.IndividualPermissionNumber.Ban, Permissions.IndividualPermissionNumber.Mute, Permissions.IndividualPermissionNumber.Deafen, Permissions.IndividualPermissionNumber.DeleteMessages, Permissions.IndividualPermissionNumber.RegulateChannels, Permissions.IndividualPermissionNumber.RegulateRanks, Permissions.IndividualPermissionNumber.PingAll, Permissions.IndividualPermissionNumber.ReadMessages, Permissions.IndividualPermissionNumber.SendMessages, Permissions.IndividualPermissionNumber.HearVoice, Permissions.IndividualPermissionNumber.SendVoice})),
            new Rank(2, "Moderator", Color.Azure, 2, Permissions.GetPermissionsNumber(new Permissions.IndividualPermissionNumber[] { Permissions.IndividualPermissionNumber.Mute, Permissions.IndividualPermissionNumber.Deafen, Permissions.IndividualPermissionNumber.DeleteMessages, Permissions.IndividualPermissionNumber.PingAll, Permissions.IndividualPermissionNumber.ReadMessages, Permissions.IndividualPermissionNumber.SendMessages, Permissions.IndividualPermissionNumber.HearVoice, Permissions.IndividualPermissionNumber.SendVoice})),
            new Rank(1, "@All", Color.Azure, 1, Permissions.GetPermissionsNumber(new Permissions.IndividualPermissionNumber[] { Permissions.IndividualPermissionNumber.Mute, Permissions.IndividualPermissionNumber.Deafen, Permissions.IndividualPermissionNumber.DeleteMessages, Permissions.IndividualPermissionNumber.PingAll, Permissions.IndividualPermissionNumber.ReadMessages, Permissions.IndividualPermissionNumber.SendMessages, Permissions.IndividualPermissionNumber.HearVoice, Permissions.IndividualPermissionNumber.SendVoice})),
            new Rank(4, "Test4", Color.Azure, 4, Permissions.GetPermissionsNumber(new Permissions.IndividualPermissionNumber[] { Permissions.IndividualPermissionNumber.Mute, Permissions.IndividualPermissionNumber.Deafen, Permissions.IndividualPermissionNumber.DeleteMessages, Permissions.IndividualPermissionNumber.PingAll, Permissions.IndividualPermissionNumber.ReadMessages, Permissions.IndividualPermissionNumber.SendMessages, Permissions.IndividualPermissionNumber.HearVoice, Permissions.IndividualPermissionNumber.SendVoice}))
        };

        public static List<Rank> GetRanksMatchingName(string name)
        {
            List<Rank> matchingRanks = new List<Rank>();
            foreach (Rank rank in ranksInMemoryForTestingOnly)
            {
                if (rank.Name == name)
                {
                    matchingRanks.Add(rank);
                }
            }
            return matchingRanks;
        }

        public static List<Rank> SortByLevel(List<Rank> ranks, bool ascending)
        {
            if (ascending)
            {
                return ranks.OrderBy(rank => rank.Level).ToList();
            }
            else
            {
                return ranks.OrderByDescending(rank => rank.Level).ToList();
            }
        }

        public class Rank
        {
            public ulong Id { get; set; }
            public string Name { get; set; }
            public Color Color { get; set; }
            public ulong Level { get; set; }
            public ulong PermissionsNumber { get; set; }

            public Rank(ulong id, string name, Color color, ulong level, ulong permissionsNumber)
            {
                this.Id = id;
                this.Name = name;
                this.Color = color;
                this.Level = level;
                this.PermissionsNumber = permissionsNumber;
            }

            public Rank DeepCopy()
            {
                Rank rankCopy = new Rank(Id, Name, Color, Level, PermissionsNumber);
                return rankCopy;
            }
        }
    }
}
