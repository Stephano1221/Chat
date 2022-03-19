﻿using System.Text.Json;
using System.Text.Json.Serialization;
using System.Numerics;

namespace Chat
{
    public class Ranks
    {
        public event EventHandler<List<Rank>> ranksReceivedAll;
        public event EventHandler<Changes> ranksUpdated;
        private List<Rank> _rankList = new List<Rank>();
        /// <summary>
        /// Represents a list of ranks on a server.
        /// Use <see cref="UpdateRanksList(Changes)"/> or <see cref="UpdateRanksList(List{Rank})"/> to set.
        /// </summary>
        public List<Rank> RankList
        {
            get { return _rankList; }
            private set { _rankList = value; }
        }

        public void UpdateRanksList(List<Rank> ranks)
        {
            RankList = ranks;
            InvokeRanksReceivedAll(this, ranks);
        }

        public void UpdateRanksList(Changes changes)
        {
            RankList = changes.MergeChanges(RankList);
            InvokeRanksUpdated(this, changes);
        }

        public List<Rank> GetRanksMatchingName(string name)
        {
            List<Rank> matchingRanks = new List<Rank>();
            foreach (Rank rank in RankList)
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
            List<Rank> sortedRanks;
            if (ascending)
            {
                sortedRanks = ranks.OrderBy(rank => rank.Level).ToList();
            }
            else
            {
                sortedRanks = ranks.OrderByDescending(rank => rank.Level).ToList();
            }
            List<Rank> sortedRanksWithUniqueLevels = SetLevelByIndex(sortedRanks);
            return sortedRanksWithUniqueLevels;
        }

        public static List<Rank> SetLevelByIndex(List<Rank> ranks)
        {
            ulong nextSettableLevel = Convert.ToUInt64(ranks.Count()) + 1;
            foreach (Rank rank in ranks)
            {
                nextSettableLevel--;
                if (rank.Level == nextSettableLevel)
                {
                    continue;
                }
                rank.Level = nextSettableLevel;
            }
            return ranks;
        }

        public static bool IsValidRank(Rank rank)
        {
            if (string.IsNullOrWhiteSpace(rank.Name))
            {
                return false;
            }
            return true;
        }

        public void AddFirstRank()
        {
            Permissions.IndividualPermissionNumber permissionNumber = Permissions.IndividualPermissionNumber.None;
            permissionNumber = Permissions.AddPermission(permissionNumber, Permissions.IndividualPermissionNumber.ReadMessages);
            permissionNumber = Permissions.AddPermission(permissionNumber, Permissions.IndividualPermissionNumber.SendMessages);
            permissionNumber = Permissions.AddPermission(permissionNumber, Permissions.IndividualPermissionNumber.HearVoice);
            permissionNumber = Permissions.AddPermission(permissionNumber, Permissions.IndividualPermissionNumber.SendVoice);
            Rank firstRank = new Rank(1, "All", Color.Empty, 1, permissionNumber);
            RankList.Insert(0, firstRank);
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

        private void InvokeRanksReceivedAll(object sender, List<Rank> ranksList)
        {
            if (ranksReceivedAll != null)
            {
                ranksReceivedAll.Invoke(this, ranksList);
            }
        }

        private void InvokeRanksUpdated(object sender, Changes mostRecentChange)
        {
            if (ranksUpdated != null)
            {
                ranksUpdated.Invoke(sender, mostRecentChange);
            }
        }

        public class Rank
        {
            [JsonConverter(typeof(BigIntegerJsonConverter))]
            public BigInteger Id { get; }
            public string Name { get; set; }
            public Color Color { get; set; }
            public ulong Level { get; set; }
            public Permissions.IndividualPermissionNumber PermissionNumber { get; set; }

            public Rank(BigInteger id, string name, Color color, ulong level, Permissions.IndividualPermissionNumber permissionNumber)
            {
                this.Id = id;
                this.Name = name;
                this.Color = color;
                this.Level = level;
                this.PermissionNumber = permissionNumber;
            }

            [JsonConstructor]
            public Rank()
            {

            }

            public Rank DeepCopy()
            {
                Rank rankCopy = new Rank(Id, Name, Color, Level, PermissionNumber);
                return rankCopy;
            }
        }

        public class Changes
        {
            public List<Rank> NewRanks { get; set; } = new List<Rank>();
            public List<Rank> ModifiedRanks { get; set; } = new List<Rank>();
            public List<Rank> UnmodifiedRanks { get; set; } = new List<Rank>();
            public List<Rank> RemovedRanks { get; set; } = new List<Rank>();

            public Changes(List<Rank> baseRanks, List<Rank> targetRanks)
            {
                GetChanges(baseRanks, targetRanks);
            }

            [JsonConstructor]
            public Changes()
            {

            }

            public void GetChanges(List<Rank> baseRanks, List<Rank> targetRanks)
            {
                NewRanks = new List<Rank>();
                ModifiedRanks = new List<Rank>();
                UnmodifiedRanks = new List<Rank>();
                RemovedRanks = new List<Rank>();
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
                                ModifiedRanks.Add(targetRank);
                                break;
                            }
                            else
                            {
                                UnmodifiedRanks.Add(targetRank);
                                break;
                            }
                        }
                    }
                    if (baseRankFound == false)
                    {
                        RemovedRanks.Add(baseRank);
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
                        NewRanks.Add(targetRank);
                    }
                }
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
                if (RemovedRanks != null && RemovedRanks.Count() > 0)
                {
                    foreach (Rank deletedRank in RemovedRanks)
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
                if (ModifiedRanks != null && ModifiedRanks.Count() > 0)
                {
                    foreach (Rank editedRank in ModifiedRanks)
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
                if (NewRanks != null && NewRanks.Count() > 0)
                {
                    foreach (Rank addedRank in NewRanks)
                    {
                        baseRanks.Add(addedRank.DeepCopy());
                    }
                }
                baseRanks = SortByLevel(baseRanks, false);
                return baseRanks;
            }

            public string SerializeToJson()
            {
                string json = JsonSerializer.Serialize(this);
                return json;
            }

            public static Changes DeserializeFromJson(string json)
            {
                Changes changes = JsonSerializer.Deserialize<Changes>(json);
                return changes;
            }
        }
    }
}
