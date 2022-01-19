using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat
{
    public partial class frmManageRanks : Form
    {
        List<Ranks.Rank> unchangedRanks = new List<Ranks.Rank>();
        List<Ranks.Rank> changedRanks = new List<Ranks.Rank>();

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
            xclbPermissions.Items.Clear();
            xclbPermissions.Items.Add("Overseer");
            xclbPermissions.Items.Add("Kick");
            xclbPermissions.Items.Add("Ban");
            xclbPermissions.Items.Add("Mute");
            xclbPermissions.Items.Add("Deafen");
            xclbPermissions.Items.Add("Delete Messages");
            xclbPermissions.Items.Add("Regulate Channels");
            xclbPermissions.Items.Add("Regulate Ranks");
            xclbPermissions.Items.Add("Ping @All");
            xclbPermissions.Items.Add("Read Messages");
            xclbPermissions.Items.Add("Send Messages");
            xclbPermissions.Items.Add("Listen");
            xclbPermissions.Items.Add("Speak");
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
            }
            changedRanks = unchangedRanks;
            DisplayChangedRanks(changedRanks, 0);
        }

        private void DisplayChangedRanks(List<Ranks.Rank> ranks, int selectedRankIndex)
        {
            xlsvRanks.Items.Clear();
            foreach (Ranks.Rank rank in ranks)
            {
                xlsvRanks.Items.Add(rank.Name);
            }
            xlsvRanks.SelectedIndices.Clear();
            xlsvRanks.Items[selectedRankIndex].Selected = true;
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
            DisplayChangedRanks(changedRanks, changedRanks.Count() - 2);
        }

        private void RemoveRank()
        {
            Ranks.Rank selectedRank = GetSelectedRank(changedRanks);
            if (selectedRank == null || selectedRank.Level <= 1)
            {
                return;
            }
            changedRanks.Remove(selectedRank);
            DisplayChangedRanks(changedRanks, xlsvRanks.SelectedIndices[0]);
        }

        private void PromoteRank()
        {
            Ranks.Rank selectedRank = GetSelectedRank(changedRanks);
            if (selectedRank.Level <= 1 || selectedRank.Level >= Convert.ToUInt64(changedRanks.Count()))
            {
                return;
            }
            int levelIncreaseAmount = 1;
            int selectedRankIndex = xlsvRanks.SelectedIndices[0];
            int newSelectedRankIndex = selectedRankIndex - levelIncreaseAmount;
            IncreaseRankLevel(changedRanks, selectedRank, levelIncreaseAmount);
            changedRanks = SortRanksByLevel(changedRanks);
            DisplayChangedRanks(changedRanks, newSelectedRankIndex);
        }

        private void DemoteRank()
        {
            Ranks.Rank selectedRank = GetSelectedRank(changedRanks);
            if (selectedRank.Level <= 2)
            {
                return;
            }
            int levelDecreaseAmount = 1;
            int selectedRankIndex = xlsvRanks.SelectedIndices[0];
            int newSelectedRankIndex = selectedRankIndex + levelDecreaseAmount;
            DecreaseRankLevel(changedRanks, selectedRank, levelDecreaseAmount);
            changedRanks = SortRanksByLevel(changedRanks);
            DisplayChangedRanks(changedRanks, newSelectedRankIndex);
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

        private Ranks.Rank GetSelectedRank(List<Ranks.Rank> ranks)
        {
            if (xlsvRanks == null || xlsvRanks.SelectedIndices.Count == 0)
            {
                return null;
            }
            int selectedRankIndex = xlsvRanks.SelectedIndices[0];
            return ranks.ElementAt(selectedRankIndex);
        }

        private void Save()
        {
            foreach (Ranks.Rank rank in changedRanks)
            {
                rank.Name = rank.Name.Trim();
            }
        }

        private DialogResult AskToSave()
        {
            DialogResult dialogResult = MessageBox.Show("You have unsaved changes. Would you like to save?", "Save changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            return dialogResult;
        }

        private void CloseForm()
        {
            List<Ranks.Rank> changes = ChangesMade();
            if (changes != null && changes.Count() > 0)
            {
                DialogResult dialogResult = AskToSave();
                if (dialogResult == DialogResult.Yes)
                {
                    Save();
                }
                else if (dialogResult == DialogResult.Cancel)
                {
                    return;
                }
            }
            this.Close();
        }

        private List<Ranks.Rank> ChangesMade()
        {
            return null;
            throw new NotImplementedException();
        }

        private void PopulateEditNameBox()
        {
            if (canRefreshRankNameTextbox == false)
            {
                return;
            }
            if (xlsvRanks == null || xlsvRanks.SelectedIndices.Count == 0)
            {
                return;
            }
            Ranks.Rank selectedRank = GetSelectedRank(changedRanks);
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
            if (String.IsNullOrWhiteSpace(xtbxName.Text))
            {
                xlsvRanks.Enabled = false;
            }
            else
            {
                xlsvRanks.Enabled = true;
            }
            if (xtbxName.Focused)
            {
                Ranks.Rank selectedRank = GetSelectedRank(changedRanks);
                selectedRank.Name = xtbxName.Text.Trim();
                canRefreshRankNameTextbox = false;
                DisplayChangedRanks(changedRanks, xlsvRanks.SelectedIndices[0]);
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
            PopulateEditNameBox();
            Ranks.Rank selectedRank = GetSelectedRank(changedRanks);
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

        private void RecordUnsavedChange()
        {

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
    }
}
