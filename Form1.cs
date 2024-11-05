using System.Management;
using System.Security.Principal;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace ShadowCopyManager
{
    public partial class Form1 : Form
    {

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool RevertToSelf();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!IsAdministrator())
            {
                RestartAsAdministrator();
            }
            else
            {
                PopulateDiskComboBox();
            }
        }

        private bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void RestartAsAdministrator()
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Application.ExecutablePath,
                Verb = "runas"
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("This application requires administrator privileges to run.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Application.Exit();
        }

        private void PopulateDiskComboBox()
        {
            diskComboBox.Items.Clear();
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed))
            {
                diskComboBox.Items.Add($"{drive.Name} ({drive.VolumeLabel})");
            }

            if (diskComboBox.Items.Count > 0)
            {
                diskComboBox.SelectedIndex = 0;
            }
        }

        private async void CreateButton_Click(object sender, EventArgs e)
        {
            if (diskComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a disk.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string selectedDisk = diskComboBox.SelectedItem.ToString().Split(' ')[0];

            try
            {
                var scope = new ManagementScope(@"\\.\root\cimv2");
                await Task.Run(() => scope.Connect());

                using var shadowCopyClass = new ManagementClass(scope, new ManagementPath("Win32_ShadowCopy"), null);
                using var parameters = shadowCopyClass.GetMethodParameters("Create");
                parameters["Volume"] = selectedDisk;

                using var result = await Task.Run(() => shadowCopyClass.InvokeMethod("Create", parameters, null));

                if ((uint)result["ReturnValue"] == 0)
                {
                    //MessageBox.Show("Shadow copy created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshShadowCopiesList();
                }
                else
                {
                    MessageBox.Show("Failed to create shadow copy.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            await UpdateCurrentMaxSize();
        }

        private async Task<string> GetVolumeIdFromDriveLetter(string driveLetter)
        {
            driveLetter = driveLetter.TrimEnd('\\');

            var scope = new ManagementScope(@"\\.\root\cimv2");
            await Task.Run(() => scope.Connect());

            var query = new ObjectQuery($"SELECT DeviceID FROM Win32_Volume WHERE DriveLetter = '{driveLetter}'");
            using var searcher = new ManagementObjectSearcher(scope, query);
            var volumes = await Task.Run(() => searcher.Get());

            if (volumes.Count == 0)
            {
                throw new Exception("Volume not found");
            }

            return volumes.Cast<ManagementObject>().First()["DeviceID"].ToString();
        }

        private async Task<ManagementObject> GetShadowStorage(string volumeId)
        {
            var scope = new ManagementScope(@"\\.\root\cimv2");
            await Task.Run(() => scope.Connect());

            // Construct the correct Volume reference string
            string volumeRef = $"Win32_Volume.DeviceID=\"{volumeId.Replace("\\", "\\\\\\\\")}\"";

            var query = new ObjectQuery($"SELECT * FROM Win32_ShadowStorage WHERE Volume = '{volumeRef}'");
            using var searcher = new ManagementObjectSearcher(scope, query);

            var shadowStorages = await Task.Run(() => searcher.Get());
            return shadowStorages.Cast<ManagementObject>().FirstOrDefault();
        }

        private async Task<List<ManagementObject>> GetShadowCopies(string volumeId)
        {
            var scope = new ManagementScope(@"\\.\root\cimv2");
            await Task.Run(() => scope.Connect());

            string escapedVolumeId = volumeId.Replace("\\", "\\\\");

            var query = new ObjectQuery($"SELECT * FROM Win32_ShadowCopy WHERE VolumeName = '{escapedVolumeId}'");
            using var searcher = new ManagementObjectSearcher(scope, query);

            return await Task.Run(() => searcher.Get().Cast<ManagementObject>().ToList());
        }

        private async void RefreshShadowCopiesList()
        {
            if (diskComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a disk.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string selectedDisk = diskComboBox.SelectedItem.ToString().Split(' ')[0];

            try
            {
                string volumeId = await GetVolumeIdFromDriveLetter(selectedDisk);
                var shadowCopies = await GetShadowCopies(volumeId);

                var dataSource = shadowCopies.Select(sc => new
                {
                    ID = sc["ID"].ToString(),
                    CreationTime = ManagementDateTimeConverter.ToDateTime(sc["InstallDate"].ToString()),
                    OriginatingMachine = sc["OriginatingMachine"].ToString(),
                    VolumeName = sc["VolumeName"].ToString()
                }).ToList();

                dataSource.Reverse();

                shadowCopiesDataGridView.DataSource = dataSource;

                // Configure columns
                shadowCopiesDataGridView.Columns["ID"].Visible = false;
                shadowCopiesDataGridView.Columns["OriginatingMachine"].Visible = false;
                shadowCopiesDataGridView.Columns["VolumeName"].Visible = false;

                // Check if the "Delete" column already exists
                if (shadowCopiesDataGridView.Columns["Delete"] == null)
                {
                    DataGridViewButtonColumn deleteButtonColumn = new DataGridViewButtonColumn();
                    deleteButtonColumn.Name = "Delete";
                    deleteButtonColumn.Text = "Delete";
                    deleteButtonColumn.UseColumnTextForButtonValue = true;
                    shadowCopiesDataGridView.Columns.Insert(0, deleteButtonColumn);
                }
                else
                {
                    // If it exists, ensure it's in the first position
                    shadowCopiesDataGridView.Columns["Delete"].DisplayIndex = 0;
                }

                // Add "Open" button column
                if (shadowCopiesDataGridView.Columns["Open"] == null)
                {
                    DataGridViewButtonColumn openButtonColumn = new DataGridViewButtonColumn();
                    openButtonColumn.Name = "Open";
                    openButtonColumn.Text = "Open";
                    openButtonColumn.UseColumnTextForButtonValue = true;
                    shadowCopiesDataGridView.Columns.Insert(1, openButtonColumn);
                }
                else
                {
                    // If it exists, ensure it's in the second position
                    shadowCopiesDataGridView.Columns["Open"].DisplayIndex = 1;
                }

                // Adjust column widths
                shadowCopiesDataGridView.Columns["CreationTime"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                shadowCopiesDataGridView.Columns["Delete"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                shadowCopiesDataGridView.Columns["Open"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                // Set column headers
                shadowCopiesDataGridView.Columns["CreationTime"].HeaderText = "Creation Time";

                // Refresh the DataGridView
                shadowCopiesDataGridView.Refresh();

                // Update the current max size
                await UpdateCurrentMaxSize();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void ShadowCopiesDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var shadowCopyId = shadowCopiesDataGridView.Rows[e.RowIndex].Cells["ID"].Value.ToString();
                var creationTime = (DateTime)shadowCopiesDataGridView.Rows[e.RowIndex].Cells["CreationTime"].Value;
                var originatingMachine = shadowCopiesDataGridView.Rows[e.RowIndex].Cells["OriginatingMachine"].Value.ToString();
                var volumeName = shadowCopiesDataGridView.Rows[e.RowIndex].Cells["VolumeName"].Value.ToString();

                if (e.ColumnIndex == shadowCopiesDataGridView.Columns["Delete"].Index)
                {
                    if (MessageBox.Show($"Are you sure you want to delete the shadow copy created at {creationTime}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        try
                        {
                            await DeleteShadowCopy(shadowCopyId);
                            //MessageBox.Show("Shadow copy deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshShadowCopiesList();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting shadow copy: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else if (e.ColumnIndex == shadowCopiesDataGridView.Columns["Open"].Index)
                {
                    try
                    {
                        OpenShadowCopy(originatingMachine, creationTime);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening shadow copy: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async Task DeleteShadowCopy(string shadowCopyId)
        {
            var scope = new ManagementScope(@"\\.\root\cimv2");
            await Task.Run(() => scope.Connect());

            var query = new ObjectQuery($"SELECT * FROM Win32_ShadowCopy WHERE ID = '{shadowCopyId}'");
            using var searcher = new ManagementObjectSearcher(scope, query);
            var shadowCopies = await Task.Run(() => searcher.Get());

            foreach (ManagementObject shadowCopy in shadowCopies)
            {
                await Task.Run(() => shadowCopy.Delete());
            }
        }

        private void OpenShadowCopy(string originatingMachine, DateTime creationTime)
        {
            if (diskComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a disk.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string selectedDisk = diskComboBox.SelectedItem.ToString().Split(' ')[0].TrimEnd('\\');
            string driveLetter = selectedDisk.Substring(0, 1);
            string gmtTime = creationTime.ToUniversalTime().ToString("yyyy.MM.dd-HH.mm.ss");
            string path = $@"\\{originatingMachine}\{driveLetter}$\@GMT-{gmtTime}";

            try
            {
                LaunchExplorerAsUser(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening shadow copy: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LaunchExplorerAsUser(string path)
        {
            IntPtr ppToken = IntPtr.Zero;
            try
            {
                // Get the access token of the logged-in user
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                IntPtr tokenHandle = identity.Token;

                // Impersonate the user
                if (!ImpersonateLoggedOnUser(tokenHandle))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                // Launch Explorer as the logged-in user
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = path,
                    UseShellExecute = true
                });

                // Revert the impersonation
                RevertToSelf();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching Explorer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Ensure we always revert to the original security context
                RevertToSelf();
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            RefreshShadowCopiesList();
        }

        private async void DiskComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateCurrentMaxSize();
            RefreshShadowCopiesList();
        }

        private async Task UpdateCurrentMaxSize()
        {
            if (diskComboBox.SelectedItem == null) return;

            string selectedDisk = diskComboBox.SelectedItem.ToString().Split(' ')[0];

            try
            {
                string volumeId = await GetVolumeIdFromDriveLetter(selectedDisk);
                var shadowStorage = await GetShadowStorage(volumeId);

                if (shadowStorage != null)
                {
                    ulong maxSpace = Convert.ToUInt64(shadowStorage["MaxSpace"]);
                    ulong usedSpace = Convert.ToUInt64(shadowStorage["UsedSpace"]);

                    string maxSpaceStr = FormatSize(maxSpace);
                    string usedSpaceStr = FormatSize(usedSpace);

                    currentMaxSizeLabel.Text = $"Max Size: {maxSpaceStr}";
                    usedSpaceLabel.Text = $"Used Space: {usedSpaceStr}";

                    // Update progress bar
                    if (maxSpace > 0)
                    {
                        int percentage = (int)((double)usedSpace / maxSpace * 100);
                        usageProgressBar.Value = Math.Min(percentage, 100);
                    }
                    else
                    {
                        usageProgressBar.Value = 0;
                    }
                }
                else
                {
                    currentMaxSizeLabel.Text = "No shadow storage found for this volume";
                    usedSpaceLabel.Text = "Used Space: N/A";
                    usageProgressBar.Value = 0;
                }
            }
            catch (Exception ex)
            {
                currentMaxSizeLabel.Text = $"Error: {ex.Message}";
                usedSpaceLabel.Text = "Used Space: N/A";
                usageProgressBar.Value = 0;
            }
        }

        private string FormatSize(ulong bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        private ulong ParseHumanReadableSize(string input)
        {
            var match = Regex.Match(input.Trim(), @"^(\d+(?:\.\d+)?)\s*([KMGT]?B?)$", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                throw new ArgumentException("Invalid size format. Use formats like '10M', '1.5G', etc.");
            }

            double value = double.Parse(match.Groups[1].Value);
            string unit = match.Groups[2].Value.ToUpper();

            switch (unit)
            {
                case "KB":
                case "K": return (ulong)(value * 1024);
                case "MB":
                case "M": return (ulong)(value * 1024 * 1024);
                case "GB":
                case "G": return (ulong)(value * 1024 * 1024 * 1024);
                case "TB":
                case "T": return (ulong)(value * 1024 * 1024 * 1024 * 1024);
                default: return (ulong)value;
            }
        }

        private async Task<ManagementObject> GetSelectedShadowStorage()
        {
            if (diskComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a disk.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            string selectedDisk = diskComboBox.SelectedItem.ToString().Split(' ')[0].TrimEnd('\\');
            var scope = new ManagementScope(@"\\.\root\cimv2");
            await Task.Run(() => scope.Connect());

            // First, get the DeviceID for the selected volume
            var volumeQuery = new ObjectQuery($"SELECT DeviceID FROM Win32_Volume WHERE DriveLetter = '{selectedDisk}'");
            using var volumeSearcher = new ManagementObjectSearcher(scope, volumeQuery);
            var volumes = await Task.Run(() => volumeSearcher.Get());

            if (volumes.Count == 0)
            {
                currentMaxSizeLabel.Text = "Current Max Size: Volume not found";
                return null;
            }

            string deviceId = volumes.Cast<ManagementObject>().First()["DeviceID"].ToString();

            return await GetShadowStorage(deviceId);
        }

        private async void SetMaxSizeButton_Click(object sender, EventArgs e)
        {

            ulong maxSizeBytes;
            try
            {
                maxSizeBytes = ParseHumanReadableSize(maxSizeTextBox.Text);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var targetStorage = await GetSelectedShadowStorage();

                if (targetStorage == null)
                {

                    MessageBox.Show("Can't resize storage before it's created; create any Shadow to initialize a Shadow Storage.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    targetStorage["MaxSpace"] = maxSizeBytes;
                    targetStorage.Put();

                    await UpdateCurrentMaxSize();
                    RefreshShadowCopiesList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting max size: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}