namespace Chat
{
    public static class Ranks
    {
        public static List<Rank> ranksInMemoryForTestingOnly = new List<Rank>() //TESTING: POPULATED FOR TESTING PURPOSES ONLY. CHECK DATABASE WHEN CHECKING RANKS.
        {
            new Rank(3, "Administrator", Color.Blue, 3, Permissions.IndividualPermissionNumber.Overseer | Permissions.IndividualPermissionNumber.Kick | Permissions.IndividualPermissionNumber.Ban | Permissions.IndividualPermissionNumber.Mute | Permissions.IndividualPermissionNumber.Deafen | Permissions.IndividualPermissionNumber.DeleteMessages | Permissions.IndividualPermissionNumber.RegulateChannels | Permissions.IndividualPermissionNumber.RegulateRanks | Permissions.IndividualPermissionNumber.PingAll | Permissions.IndividualPermissionNumber.ReadMessages | Permissions.IndividualPermissionNumber.SendMessages | Permissions.IndividualPermissionNumber.HearVoice | Permissions.IndividualPermissionNumber.SendVoice),
            new Rank(2, "Moderator", Color.Azure, 2, Permissions.IndividualPermissionNumber.Mute | Permissions.IndividualPermissionNumber.Deafen | Permissions.IndividualPermissionNumber.DeleteMessages | Permissions.IndividualPermissionNumber.PingAll | Permissions.IndividualPermissionNumber.ReadMessages | Permissions.IndividualPermissionNumber.SendMessages | Permissions.IndividualPermissionNumber.HearVoice | Permissions.IndividualPermissionNumber.SendVoice),
            new Rank(1, "@All", Color.Azure, 1, Permissions.IndividualPermissionNumber.Mute | Permissions.IndividualPermissionNumber.Deafen | Permissions.IndividualPermissionNumber.DeleteMessages | Permissions.IndividualPermissionNumber.PingAll | Permissions.IndividualPermissionNumber.ReadMessages | Permissions.IndividualPermissionNumber.SendMessages | Permissions.IndividualPermissionNumber.HearVoice | Permissions.IndividualPermissionNumber.SendVoice),
            new Rank(4, "Test4", Color.Azure, 4, Permissions.IndividualPermissionNumber.Mute | Permissions.IndividualPermissionNumber.Deafen | Permissions.IndividualPermissionNumber.DeleteMessages | Permissions.IndividualPermissionNumber.PingAll | Permissions.IndividualPermissionNumber.ReadMessages | Permissions.IndividualPermissionNumber.SendMessages | Permissions.IndividualPermissionNumber.HearVoice | Permissions.IndividualPermissionNumber.SendVoice)
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
            public Permissions.IndividualPermissionNumber PermissionNumber { get; set; }

            public Rank(ulong id, string name, Color color, ulong level, Permissions.IndividualPermissionNumber permissionsNumber)
            {
                this.Id = id;
                this.Name = name;
                this.Color = color;
                this.Level = level;
                this.PermissionNumber = permissionsNumber;
            }

            public Rank DeepCopy()
            {
                Rank rankCopy = new Rank(Id, Name, Color, Level, PermissionNumber);
                return rankCopy;
            }
        }

        public class Changes
        {
            public List<Rank> newRanks { get; set;} = new List<Rank>();
            public List<Rank> changedRanks { get; set; } = new List<Rank>();
            public List<Rank> deletedRanks { get; set; } = new List<Rank>();

            public Changes(List<Rank> baseRanks, List<Rank> targetRanks)
            {
                GetChanges(baseRanks, targetRanks);
            }

            public Changes GetChanges(List<Rank> baseRanks, List<Rank> targetRanks)
            {
                foreach(Rank baseRank in baseRanks)
                {
                    bool baseRankFound = false;
                    foreach(Rank targetRank in targetRanks)
                    {
                        if (baseRank.Id == targetRank.Id)
                        {
                            baseRankFound = true;
                            if (ChangesFound(baseRank, targetRank))
                            {
                                changedRanks.Add(targetRank);
                                break;
                            }
                        }
                    }
                    if (baseRankFound == false)
                    {
                        deletedRanks.Add(baseRank);
                    }
                }
                foreach (Rank targetRank in targetRanks)
                {
                    bool targetRankFound = false;
                    foreach (Rank baseRank in baseRanks)
                    {
                        if (targetRank.Id == baseRank.Id)
                        {
                            targetRankFound = true;
                        }
                    }
                    if (targetRankFound == false)
                    {
                        newRanks.Add(targetRank);
                    }
                }
                return this;
            }

            public bool ChangesFound(Rank baseRank, Rank targetRank)
            {
                if (baseRank.Id != targetRank.Id) { return true; }
                if (baseRank.Name != targetRank.Name) { return true; }
                if (baseRank.Color != targetRank.Color) { return true; }
                if (baseRank.Level != targetRank.Level) { return true; }
                if (baseRank.PermissionNumber != targetRank.PermissionNumber) { return true; }
                return false;
            }

            public void SaveChanges()
            {
                if (deletedRanks != null && deletedRanks.Count() > 0)
                {
                    foreach (Rank deletedRank in deletedRanks)
                    {
                        foreach (Rank rank in ranksInMemoryForTestingOnly)
                        {
                            if (deletedRank.Id == rank.Id)
                            {
                                ranksInMemoryForTestingOnly.Remove(rank);
                                break;
                            }
                        }
                    }
                }
                if (changedRanks != null && changedRanks.Count() > 0)
                {
                    foreach (Rank changedRank in changedRanks)
                    {
                        for (int i = 0; i < ranksInMemoryForTestingOnly.Count(); i++)
                        {
                            if (changedRank.Id == ranksInMemoryForTestingOnly[i].Id)
                            {
                                ranksInMemoryForTestingOnly[i] = changedRank.DeepCopy();
                                break;
                            }
                        }
                    }
                }
                if (newRanks != null && newRanks.Count() > 0)
                {
                    foreach (Rank newRank in newRanks)
                    {
                        ranksInMemoryForTestingOnly.Add(newRank.DeepCopy());
                    }
                }
            }
        }
    }
}
