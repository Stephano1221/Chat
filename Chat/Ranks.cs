using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chat
{
    public class Ranks
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

        public static List<Rank> SetLevelByIndex(List<Rank> ranks)
        {
            ulong nextSettableLevel = Convert.ToUInt64(ranks.Count());
            foreach (Rank rank in ranks)
            {
                if (rank.Level == nextSettableLevel)
                {
                    continue;
                }
                rank.Level = nextSettableLevel--;
            }
            return ranks;
        }

        public string SerializeToJson()
        {
            string json = JsonSerializer.Serialize(this);
            return json;
        }

        public static Ranks DeserializeFromJson(string json)
        {
            Ranks ranks = JsonSerializer.Deserialize<Ranks>(json);
            return ranks;
        }

        public class Rank
        {
            public ulong Id { get; }
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
            public List<Rank> newRanks { get; set; } = new List<Rank>();
            public List<Rank> modifiedRanks { get; set; } = new List<Rank>();
            public List<Rank> unmodifiedRanks { get; set; } = new List<Rank>();
            public List<Rank> removedRanks { get; set; } = new List<Rank>();

            public Changes(List<Rank> baseRanks, List<Rank> targetRanks)
            {
                GetChanges(baseRanks, targetRanks);
            }

            public void GetChanges(List<Rank> baseRanks, List<Rank> targetRanks)
            {
                newRanks = new List<Rank>();
                modifiedRanks = new List<Rank>();
                unmodifiedRanks = new List<Rank>();
                removedRanks = new List<Rank>();
                foreach (Rank baseRank in baseRanks)
                {
                    bool baseRankFound = false;
                    foreach (Rank targetRank in targetRanks)
                    {
                        if (baseRank.Id == targetRank.Id)
                        {
                            baseRankFound = true;
                            if (ChangesFound(baseRank, targetRank))
                            {
                                modifiedRanks.Add(targetRank);
                                break;
                            }
                            else
                            {
                                unmodifiedRanks.Add(targetRank);
                                break;
                            }
                        }
                    }
                    if (baseRankFound == false)
                    {
                        removedRanks.Add(baseRank);
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

            public static bool ChangesFound(Rank baseRank, Rank targetRank)
            {
                if (baseRank.Id != targetRank.Id) { return true; }
                if (baseRank.Name != targetRank.Name) { return true; }
                if (baseRank.Color != targetRank.Color) { return true; }
                if (baseRank.Level != targetRank.Level) { return true; }
                if (baseRank.PermissionNumber != targetRank.PermissionNumber) { return true; }
                return false;
            }

            public List<Rank> MergeChanges(List<Rank> baseRanks)
            {
                if (removedRanks != null && removedRanks.Count() > 0)
                {
                    foreach (Rank deletedRank in removedRanks)
                    {
                        foreach (Rank rank in baseRanks)
                        {
                            if (deletedRank.Id == rank.Id)
                            {
                                baseRanks.Remove(rank);
                                break;
                            }
                        }
                    }
                }
                if (modifiedRanks != null && modifiedRanks.Count() > 0)
                {
                    foreach (Rank editedRank in modifiedRanks)
                    {
                        for (int i = 0; i < baseRanks.Count(); i++)
                        {
                            if (editedRank.Id == baseRanks[i].Id)
                            {
                                baseRanks[i] = editedRank.DeepCopy();
                                break;
                            }
                        }
                    }
                }
                if (newRanks != null && newRanks.Count() > 0)
                {
                    foreach (Rank addedRank in newRanks)
                    {
                        baseRanks.Add(addedRank.DeepCopy());
                    }
                }
                return baseRanks;
            }

            public string SerializeToJson()
            {
                string json = JsonSerializer.Serialize(this);
                return json;
            }
        }
    }
}
