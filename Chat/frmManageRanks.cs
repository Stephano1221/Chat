namespace Chat
{
    public partial class frmManageRanks : Form
    {
        List<Ranks.Rank> unchangedRanks = new List<Ranks.Rank>();
        List<Ranks.Rank> changedRanks = new List<Ranks.Rank>();
        Ranks.Rank selectedRank;
        Ranks.Changes changes;

        bool canRefreshRankNameTextbox = true;

        private delegate void OnRanksReceivedAllEventHandler(List<Ranks.Rank> ranks);
        private delegate void OnRanksUpdatedEventHandler(Ranks.Changes changes);

        public frmManageRanks()
        {
            InitializeComponent();
            xlsvRanks.Columns[0].Width = xlsvRanks.Width - 5;
            SubscribeToRankEvents();
            PopulatePermissions();
            RequestRanks();
        }

        private void SubscribeToRankEvents()
        {
            FrmHolder.processing.ranks.ranksReceivedAll += OnRanksReceivedAll;
            FrmHolder.processing.ranks.ranksUpdated += OnRanksUpdated;
        }

        private void PopulatePermissions()
        {
            xlsvPermissions.Items.Clear();
            foreach (KeyValuePair<Permissions.IndividualPermissionNumber, Permissions.Permission> permission in Permissions.permissionNames)
            {
                ListViewItem listViewItem = new ListViewItem(permission.Value.Name);
                listViewItem.SubItems.Add(permission.Value.Description);
                xlsvPermissions.Items.Add(listViewItem);
            }
            xlsvPermissions.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void SetPermissionsCheckedStates(Ranks.Rank rank)
        {
            foreach (ListViewItem listViewItem in xlsvPermissions.Items)
            {
                foreach (KeyValuePair<Permissions.IndividualPermissionNumber, Permissions.Permission> permission in Permissions.permissionNames)
                {
                    if (listViewItem.Text == permission.Value.Name)
                    {
                        if (Permissions.ContainsPermission(permission.Key, rank.PermissionNumber))
                        {
                            listViewItem.Checked = true;
                            break;
                        }
                        else
                        {
                            listViewItem.Checked = false;
                            break;
                        }
                    }
                }
            }
        }

        private Permissions.IndividualPermissionNumber GetPermissionNumberFromPermissionList(Ranks.Rank rank)
        {
            Permissions.IndividualPermissionNumber individualPermissionNumber = new Permissions.IndividualPermissionNumber();
            foreach (ListViewItem listViewItem in xlsvPermissions.Items)
            {
                if (listViewItem.Checked)
                {
                    foreach (KeyValuePair<Permissions.IndividualPermissionNumber, Permissions.Permission> permission in Permissions.permissionNames)
                    {
                        if (listViewItem.Text == permission.Value.Name)
                        {
                            individualPermissionNumber = Permissions.AddPermission(permission.Key, individualPermissionNumber);
                            break;
                        }
                    }
                }
            }
            return individualPermissionNumber;
        }

        private void RequestRanks()
        {
            if (FrmHolder.hosting)
            {
                //ReceiveRanks(FrmHolder.processing.GetRanksFromDatabase());
                ReceiveRanks(FrmHolder.processing.ranks.RankList);
            }
            else
            {
                FrmHolder.processing.BeginWrite(FrmHolder.processing.connectedClients[0], FrmHolder.processing.ComposeMessage(FrmHolder.processing.connectedClients[0], 0, Message.MessageTypes.RequestAllRanks, null, null));
            }
        }

        private void ReceiveRanks(List<Ranks.Rank> receivedRanks)
        {
            if (receivedRanks == null || receivedRanks.Count() == 0)
            {
                return;
            }
            List<Ranks.Rank> sortedRanks = SortRanksByLevel(receivedRanks);
            unchangedRanks.Clear();
            foreach (Ranks.Rank rank in sortedRanks)
            {
                unchangedRanks.Add(rank.DeepCopy());
                changedRanks.Add(rank.DeepCopy());
            }
            selectedRank = changedRanks.ElementAtOrDefault(0);
            DisplayChangedRanks(changedRanks);
        }

        private void RanksReceivedAll(List<Ranks.Rank> ranks)
        {
            ReceiveRanks(ranks);
        }

        private void RanksUpdated(Ranks.Changes changes)
        {
            RemoveRanksFromList(changes.RemovedRanks, changedRanks);
            ModifyRanksInList(unchangedRanks, changes.ModifiedRanks, changedRanks);
            AddRanksToList(changes.NewRanks, changedRanks);
            unchangedRanks = FrmHolder.processing.ranks.RankList;
        }

        private bool HasLocalLevelChange()
        {
            foreach (Ranks.Rank localChangedRank in changedRanks)
            {
                foreach (Ranks.Rank localUnchangedRank in unchangedRanks)
                {
                    if (localChangedRank.Id == localUnchangedRank.Id)
                    {
                        if (localChangedRank.Level != localUnchangedRank.Level)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void RemoveRankFromList(Ranks.Rank rankToRemove, List<Ranks.Rank> baseRanks)
        {
            if (rankToRemove == null || baseRanks == null || baseRanks.Count() == 0)
            {
                throw new ArgumentNullException();
            }
            foreach (Ranks.Rank rank in baseRanks)
            {
                if (rankToRemove.Id == rank.Id)
                {
                    baseRanks.Remove(rank);
                    break;
                }
            }
            SortRanksByLevel(baseRanks);
        }

        private void RemoveRanksFromList(List<Ranks.Rank> ranksToRemove, List<Ranks.Rank> baseRanks)
        {
            foreach (Ranks.Rank rankToRemove in ranksToRemove)
            {
                RemoveRankFromList(rankToRemove, baseRanks);
            }
        }

        private void AddRankToList(Ranks.Rank rankToAdd, List<Ranks.Rank> baseRanks)
        {
            if (rankToAdd == null || baseRanks == null || baseRanks.Count() == 0)
            {
                throw new ArgumentNullException();
            }
            bool alreadyExists = false;
            foreach (Ranks.Rank rank in baseRanks)
            {
                if (rankToAdd.Id == rank.Id)
                {
                    alreadyExists = true;
                    break;
                }
            }
            if (alreadyExists)
            {
                return;
            }
            baseRanks.Add(rankToAdd);
            SortRanksByLevel(baseRanks);
        }

        private void AddRanksToList(List<Ranks.Rank> ranksToAdd, List<Ranks.Rank> baseRanks)
        {
            foreach (Ranks.Rank rankToAdd in ranksToAdd)
            {
                AddRankToList(rankToAdd, baseRanks);
            }
        }

        private void ModifyRank(Ranks.Rank unmodifiedRank, Ranks.Rank modifiedRank, Ranks.Rank rankToModify)
        {
            if (modifiedRank == null || rankToModify == null)
            {
                throw new ArgumentNullException();
            }
            bool hasLocalNameChange = false;
            bool hasLocalColorChange = false;
            bool hasLocalPermissionsChange = false;
            if (unmodifiedRank != null)
            {
                hasLocalNameChange = unmodifiedRank.Name == modifiedRank.Name ? false : true;
                hasLocalColorChange = unmodifiedRank.Color == modifiedRank.Color ? false : true;
                hasLocalPermissionsChange = unmodifiedRank.PermissionNumber == modifiedRank.PermissionNumber ? false : true;
            }
            if (hasLocalNameChange == false && rankToModify.Name != modifiedRank.Name)
            {
                rankToModify.Name = modifiedRank.Name;
            }
            if (hasLocalColorChange == false && rankToModify.Color != modifiedRank.Color)
            {
                rankToModify.Color = modifiedRank.Color;
            }
            if (hasLocalPermissionsChange == false && rankToModify.PermissionNumber != modifiedRank.PermissionNumber)
            {
                rankToModify.PermissionNumber = modifiedRank.PermissionNumber;
            }
        }

        private void ModifyRanksInList(List<Ranks.Rank> unmodifiedRanks, List<Ranks.Rank> modifiedRanks, List<Ranks.Rank> ranksToModify)
        {
            if (modifiedRanks == null || ranksToModify == null)
            {
                throw new ArgumentNullException();
            }
            bool hasLocalLevelChange = HasLocalLevelChange();
            foreach (Ranks.Rank modifiedRank in modifiedRanks)
            {
                foreach (Ranks.Rank rankToModify in ranksToModify)
                {
                    if (modifiedRank.Id == rankToModify.Id)
                    {
                        Ranks.Rank unmodifiedRankTemp = null;
                        if (unmodifiedRanks != null)
                        {
                            foreach (Ranks.Rank unmodifiedRank in unmodifiedRanks)
                            {
                                if (unmodifiedRank.Id == rankToModify.Id)
                                {
                                    unmodifiedRankTemp = unmodifiedRank;
                                }
                            }
                        }
                        ModifyRank(unmodifiedRankTemp, modifiedRank, rankToModify);
                        if (hasLocalLevelChange == false)
                        {
                            rankToModify.Level = modifiedRank.Level;
                        }
                        break;
                    }
                }
            }
        }

        private void DisplayChangedRanks(List<Ranks.Rank> ranks)
        {
            xlsvRanks.Items.Clear();
            xlsvRanks.SelectedIndices.Clear();
            foreach (Ranks.Rank rank in ranks)
            {
                xlsvRanks.Items.Add(rank.Name);
            }
            xlsvRanks.Items[changedRanks.IndexOf(selectedRank)].Selected = true;
            SetPermissionsCheckedStates(selectedRank);
        }

        private List<Ranks.Rank> SortRanksByLevel(List<Ranks.Rank> unsortedRanks)
        {
            return Ranks.SortByLevel(unsortedRanks, false);
        }

        private void AddRank()
        {
            Ranks.Rank newRank = new Ranks.Rank(0, "New rank", Color.DarkGray, 2, 0); //TODO: Generate unique ID
            foreach (Ranks.Rank rank in changedRanks)
            {
                if (rank.Level > 1)
                {
                    rank.Level++;
                }
            }
            changedRanks.Add(newRank);
            changedRanks = Ranks.SortByLevel(changedRanks, false);
            selectedRank = newRank;
            DisplayChangedRanks(changedRanks);
        }

        private void RemoveRank()
        {
            if (selectedRank == null || selectedRank.Level <= 1)
            {
                return;
            }
            foreach (Ranks.Rank rank in changedRanks)
            {
                if (rank.Level > selectedRank.Level)
                {
                    rank.Level--;
                }
            }
            int indexOfSelectedRank = changedRanks.IndexOf(selectedRank);
            changedRanks.Remove(selectedRank);
            selectedRank = changedRanks[indexOfSelectedRank];
            DisplayChangedRanks(changedRanks);
        }

        private void PromoteRank()
        {
            if (selectedRank.Level <= 1 || selectedRank.Level >= Convert.ToUInt64(changedRanks.Count()))
            {
                return;
            }
            int levelDifference = 1;
            ChangeRankLevel(changedRanks, selectedRank, levelDifference);
            DisplayChangedRanks(changedRanks);
        }

        private void DemoteRank()
        {
            if (selectedRank.Level <= 2)
            {
                return;
            }
            int levelDifference = -1;
            ChangeRankLevel(changedRanks, selectedRank, levelDifference);
            DisplayChangedRanks(changedRanks);
        }

        private void ChangeRankLevel(List<Ranks.Rank> ranksByLevelDescending, Ranks.Rank rank, int levelDifference)
        {
            if (ranksByLevelDescending == null || ranksByLevelDescending.Count() == 0 || rank == null)
            {
                return;
            }
            int indexOfRank = ranksByLevelDescending.IndexOf(rank);
            int indexToInsertAt = indexOfRank - levelDifference;
            if (indexToInsertAt < 0)
            {
                indexToInsertAt = 0;
            }
            else if (indexToInsertAt >= ranksByLevelDescending.Count())
            {
                indexToInsertAt = ranksByLevelDescending.Count() - 1;
            }
            ranksByLevelDescending.RemoveAt(indexOfRank);
            ranksByLevelDescending.Insert(indexToInsertAt, rank);
            Ranks.SetLevelByIndex(ranksByLevelDescending);
        }

        private void Save(Ranks.Changes changes)
        {
            if (FrmHolder.hosting)
            {
                RemoveRanksFromList(changes.NewRanks, changedRanks);
                FrmHolder.processing.SaveRanksAsServer(null, changes);
            }
            else
            {
                FrmHolder.processing.SaveRanksAsClient(changes);
            }
        }

        private DialogResult AskToSave()
        {
            DialogResult dialogResult = MessageBox.Show("You have unsaved changes. Would you like to save?", "Save changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            return dialogResult;
        }

        private void CloseForm()
        {
            ChangesMade();
            if (changes != null && (changes.NewRanks.Count() > 0 || changes.ModifiedRanks.Count() > 0 || changes.RemovedRanks.Count() > 0))
            {
                DialogResult dialogResult = AskToSave();
                if (dialogResult == DialogResult.Yes)
                {
                    Save(changes);
                }
                else if (dialogResult == DialogResult.Cancel)
                {
                    return;
                }
            }
            this.Close();
        }

        private Ranks.Changes ChangesMade()
        {
            changes = new Ranks.Changes(unchangedRanks, changedRanks);
            return changes;
        }

        private void PopulateEditNameBox()
        {
            if (canRefreshRankNameTextbox == false || selectedRank == null)
            {
                return;
            }
            if (selectedRank.Level <= 1)
            {
                xtbxName.Enabled = false;
            }
            else
            {
                xtbxName.Enabled = true;
            }
            xtbxName.Text = selectedRank.Name;
        }

        private void RankNameTextChanged()
        {
            bool enableRankList = true;
            bool enablePermissionList = true;
            bool enableBackButton = true;
            if (String.IsNullOrWhiteSpace(xtbxName.Text))
            {

                enableRankList = false;
                enablePermissionList = false;
                enableBackButton = false;
            }
            else
            {
                enableRankList = true;
                enablePermissionList = true;
                enableBackButton = true;
            }
            xlsvRanks.Enabled = enableRankList;
            xlsvPermissions.Enabled = enablePermissionList;
            xbtnBack.Enabled = enableBackButton;
            if (xtbxName.Focused)
            {
                selectedRank.Name = xtbxName.Text.Trim();
                canRefreshRankNameTextbox = false;
                DisplayChangedRanks(changedRanks);
            }
            canRefreshRankNameTextbox = true;
        }

        private void HandleEmptyRankName()
        {
            if (String.IsNullOrWhiteSpace(xtbxName.Text))
            {
                MessageBox.Show("Rank name cannot be empty.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                xtbxName.Focus();
            }
        }

        private void SelectedRankChanged()
        {
            if (xlsvRanks.SelectedIndices.Count == 0)
            {
                return;
            }
            selectedRank = changedRanks.ElementAtOrDefault(xlsvRanks.SelectedIndices[0]);
            PopulateEditNameBox();
            SetPermissionsCheckedStates(selectedRank);
            bool enableAddRank = true;
            bool enableRemoveRank = true;
            bool enablePromoteRank = true;
            bool enableDemoteRank = true;
            if (selectedRank == null)
            {
                return;
            }
            if (selectedRank.Level == Convert.ToUInt64(changedRanks.Count()))
            {
                enablePromoteRank = false;
            }
            if (selectedRank.Level <= 1)
            {
                enableRemoveRank = false;
                enablePromoteRank = false;
                enableDemoteRank = false;
            }
            if (selectedRank.Level == 2)
            {
                enableDemoteRank = false;
            }
            xbtnAddRank.Enabled = enableAddRank;
            xbtnRemoveRank.Enabled = enableRemoveRank;
            xbtnRankPromote.Enabled = enablePromoteRank;
            xbtnRankDemote.Enabled = enableDemoteRank;
        }

        private void SetRankPermissions(ItemCheckEventArgs e)
        {
            if (e == null || xlsvPermissions.Focused == false)
            {
                return;
            }
            foreach (KeyValuePair<Permissions.IndividualPermissionNumber, Permissions.Permission> permission in Permissions.permissionNames)
            {
                if (xlsvPermissions.Items[e.Index].Text == permission.Value.Name)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        selectedRank.PermissionNumber = Permissions.AddPermission(permission.Key, selectedRank.PermissionNumber);
                        break;
                    }
                    else
                    {
                        selectedRank.PermissionNumber = Permissions.RemovePermission(permission.Key, selectedRank.PermissionNumber);
                        break;
                    }
                }
            }
        }

        private void OnRanksUpdated(object sender, Ranks.Changes e)
        {
            if (xlsvRanks.InvokeRequired)
            {
                xlsvRanks.BeginInvoke(new OnRanksUpdatedEventHandler(RanksUpdated), e);
            }
            else
            {
                RanksUpdated(e);
            }
        }

        private void OnRanksReceivedAll(object sender, List<Ranks.Rank> e)
        {
            if (xlsvRanks.InvokeRequired)
            {
                xlsvRanks.BeginInvoke(new OnRanksReceivedAllEventHandler(RanksReceivedAll), e);
            }
            else
            {
                RanksReceivedAll(e);
            }
        }

        private void xbtnBack_Click(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void xbtnRemoveRank_Click(object sender, EventArgs e)
        {
            RemoveRank();
        }

        private void xbtnAddRank_Click(object sender, EventArgs e)
        {
            AddRank();
        }

        private void xbtnRankPromote_Click(object sender, EventArgs e)
        {
            PromoteRank();
        }

        private void xbtnRankDemote_Click(object sender, EventArgs e)
        {
            DemoteRank();
        }

        private void xlsvRanks_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedRankChanged();
        }

        private void xtbxName_TextChanged(object sender, EventArgs e)
        {
            RankNameTextChanged();
        }

        private void xtbxName_Leave(object sender, EventArgs e)
        {
            HandleEmptyRankName();
        }

        private void xlsvPermissions_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            SetRankPermissions(e);
        }
    }
}
