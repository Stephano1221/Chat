namespace Chat
{
    public partial class frmManageRanks : Form
    {
        List<Ranks.Rank> unchangedRanks = new List<Ranks.Rank>();
        List<Ranks.Rank> changedRanks = new List<Ranks.Rank>();
        Ranks.Rank selectedRank;

        bool canRefreshRankNameTextbox = true;

        public frmManageRanks()
        {
            InitializeComponent();
            xlsvRanks.Columns[0].Width = xlsvRanks.Width - 5;
            PopulatePermissions();
            RequestRanks();
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
            unchangedRanks.Clear();
            if (FrmHolder.hosting)
            {
                //ReceiveRanks(FrmHolder.processing.GetRanksFromDatabase());
                ReceiveRanks(Ranks.ranksInMemoryForTestingOnly);
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
            foreach (Ranks.Rank rank in sortedRanks)
            {
                unchangedRanks.Add(rank.DeepCopy());
                changedRanks.Add(rank.DeepCopy());
            }
            selectedRank = changedRanks.ElementAtOrDefault(0);
            DisplayChangedRanks(changedRanks);
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
            Ranks.Rank newRank = new Ranks.Rank(9999, "New rank", Color.DarkGray, 2, 0); //TODO: Generate unique ID
            foreach (Ranks.Rank rank in changedRanks)
            {
                if (rank.Level > 1)
                {
                    rank.Level++;
                }
            }
            changedRanks.Insert(changedRanks.Count() - 1, newRank);
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
            int levelIncreaseAmount = 1;
            IncreaseRankLevel(changedRanks, selectedRank, levelIncreaseAmount);
            changedRanks = SortRanksByLevel(changedRanks);
            DisplayChangedRanks(changedRanks);
        }

        private void DemoteRank()
        {
            if (selectedRank.Level <= 2)
            {
                return;
            }
            int levelDecreaseAmount = 1;
            DecreaseRankLevel(changedRanks, selectedRank, levelDecreaseAmount);
            changedRanks = SortRanksByLevel(changedRanks);
            DisplayChangedRanks(changedRanks);
        }

        private void IncreaseRankLevel(List<Ranks.Rank> ranksByLevelDescending, Ranks.Rank rankToPromote, int levelIncreaseAmount)
        {
            int indexOfRankToPromote = ranksByLevelDescending.IndexOf(rankToPromote);
            for (int i = indexOfRankToPromote; i > indexOfRankToPromote - levelIncreaseAmount; i--)
            {
                ranksByLevelDescending.ElementAt(i - 1).Level--;
            }
            rankToPromote.Level += Convert.ToUInt64(levelIncreaseAmount);
        }

        private void DecreaseRankLevel(List<Ranks.Rank> ranksByLevelDescending, Ranks.Rank rankToDemote, int levelDecreaseAmount)
        {
            int indexOfRankToDemote = ranksByLevelDescending.IndexOf(rankToDemote);
            for (int i = indexOfRankToDemote; i < indexOfRankToDemote + levelDecreaseAmount; i++)
            {
                ranksByLevelDescending.ElementAt(i + 1).Level++;
            }
            rankToDemote.Level -= Convert.ToUInt64(levelDecreaseAmount);
        }

        private void Save(Ranks.Changes changes)
        {
            changes.SaveChanges();
        }

        private DialogResult AskToSave()
        {
            DialogResult dialogResult = MessageBox.Show("You have unsaved changes. Would you like to save?", "Save changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            return dialogResult;
        }

        private void CloseForm()
        {
            Ranks.Changes changes = ChangesMade();
            if (changes != null && (changes.newRanks.Count() > 0 || changes.modifiedRanks.Count() > 0 || changes.removedRanks.Count() > 0))
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
            Ranks.Changes changes = new Ranks.Changes(unchangedRanks, changedRanks);
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
