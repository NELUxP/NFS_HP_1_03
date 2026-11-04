using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2;
using libdebug;
using System.Diagnostics;

namespace NFS_HP_1_03
{
    public partial class Form1 : Form
    {
        private bool isDragging = false;
        private Point lastLocation;

        private PS4DBG PS4;
        ProcessList processList;
        private int PS4_PID = 0;

        public Form1()
        {
            InitializeComponent();
            // Initialize numeric boxes
            numericUpDown1.DecimalPlaces = 2;
            numericUpDown2.DecimalPlaces = 2;
            numericUpDown3.DecimalPlaces = 2;
            numericUpDown1.Increment = 1.0m;
            numericUpDown2.Increment = 1.0m;
            numericUpDown3.Increment = 1.0m;

            // Optional: Add adjustment buttons
        }

        // Property to check connection
        private bool IsPS4Connected => PS4 != null && PS4_PID != 0;

        void AttachProcessName(string processName)
        {
            try
            {
                if (PS4 == null)
                {
                    throw new ArgumentNullException("You are not connected to PS4/PS5!", "Warning!");
                }
                else
                {
                    if (PS4 != null)
                    {
                        string TheProcName = processName;
                        processList = PS4.GetProcessList();
                        PS4_PID = processList.FindProcess(TheProcName, false).pid;

                        MessageBox.Show("You Attached to game!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        siticoneHtmlLabel3.Text = "You are connected";
                        siticoneHtmlLabel3.ForeColor = Color.LawnGreen;

                        // Enable features after attaching
                        EnableFeatures(true);
                    }
                }
            }
            catch (Exception EX)
            {
                MessageBox.Show(EX.Message, "Упс", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Load saved IP when form starts
            LoadSavedIP();
        }

        // Load saved IP from settings
        private void LoadSavedIP()
        {
            try
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.userip))
                {
                    siticoneTextBox1.Text = Properties.Settings.Default.userip;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading saved IP: {ex.Message}");
            }
        }

        // Enable/disable feature controls based on connection state
        private void EnableFeatures(bool enable)
        {
            siticoneButton3.Enabled = enable;  // FPS unlocker
            siticoneToggleSwitch4.Enabled = enable;  // Nitrous toggle
            // Add other feature controls here as needed
        }

        // Update connection status UI
        private void UpdateConnectionStatus(bool connected)
        {
            if (connected)
            {
                siticoneButton2.BackColor = Color.Green;
                siticoneButton2.Text = "Connected";
                siticoneHtmlLabel3.Text = "Connected to PS4";
                siticoneHtmlLabel3.ForeColor = Color.LawnGreen;
            }
            else
            {
                siticoneButton2.BackColor = Color.FromArgb(64, 64, 64);
                siticoneButton2.Text = "Connect";
                siticoneHtmlLabel3.Text = "Disconnected";
                siticoneHtmlLabel3.ForeColor = Color.Red;
                EnableFeatures(false);
            }
        }

        private void siticonePanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void siticoneControlBox4_Click(object sender, EventArgs e)
        {

        }

        private void siticoneControlBox3_Click(object sender, EventArgs e)
        {

        }

        private void siticonePanel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastLocation = e.Location;
            }
        }

        private void siticonePanel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X,
                    (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void siticonePanel1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void siticoneHtmlLabel4_Click(object sender, EventArgs e)
        {

        }

        private void siticonePictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void siticoneButton3_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsPS4Connected) return;

                // Read pointer and calculate address in one line
                ulong baseAddress = PS4.ReadMemory<ulong>(PS4_PID, 0x14d9770);
                ulong FrameRateUnlocker = baseAddress + 0x118a00;

                PS4.WriteMemory(PS4_PID, FrameRateUnlocker, new byte[] { 0x01 });
                PS4.Notify(222, "Unlocked 60 FPS!");
            }
            catch (Exception sx)
            {
                MessageBox.Show(sx.Message, "Упс", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void siticonePanel3_Paint(object sender, PaintEventArgs e)
        {
            bool isChecked = siticoneToggleSwitch1.Checked;

            // Save the state immediately when toggled
            Properties.Settings.Default.savecheck = isChecked;
            Properties.Settings.Default.Save();

            // Disable or enable the controls within the panel
            foreach (Control control in siticonePanel3.Controls)
            {
                if (control != siticoneToggleSwitch1)
                {
                    control.Enabled = isChecked;
                }
            }
        }

        // Paint event for panel 2
        private void siticonePanel2_Paint(object sender, PaintEventArgs e)
        {
            bool isChecked = siticoneToggleSwitch2.Checked;

            // Save the state immediately when toggled
            Properties.Settings.Default.savecheck = isChecked;
            Properties.Settings.Default.Save();

            // Disable or enable the controls within the panel
            foreach (Control control in siticonePanel2.Controls)
            {
                if (control != siticoneToggleSwitch2)
                {
                    control.Enabled = isChecked;
                }
            }
        }

        private void siticoneToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            // Trigger paint event to update controls
            siticonePanel3.Invalidate();
        }

        private void siticoneToggleSwitch2_CheckedChanged(object sender, EventArgs e)
        {
            // Trigger paint event to update controls
            siticonePanel2.Invalidate();
        }

        private void siticoneTextBox1_TextChanged(object sender, EventArgs e)
        {
            // Remove any non-digit characters except periods
            string cleanedInput = Regex.Replace(siticoneTextBox1.Text, @"[^0-9.]", "");

            // Split the input into octets using periods as a separator
            string[] octets = cleanedInput.Split('.');

            string maskedIP = string.Empty;

            for (int i = 0; i < octets.Length; i++)
            {
                // Limit the octet length to 3 digits
                if (octets[i].Length > 3)
                {
                    octets[i] = octets[i].Substring(0, 3);
                }

                // Append the octet to the masked IP address
                maskedIP += octets[i];

                if (i < octets.Length - 1)
                {
                    // Add a period separator if it's not the last octet
                    maskedIP += ".";
                }
            }

            // Update the TextBox with the masked IP address
            siticoneTextBox1.Text = maskedIP;
            siticoneTextBox1.SelectionStart = maskedIP.Length;
        }

        private void siticoneButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(siticoneTextBox1.Text))
                {
                    MessageBox.Show("Please enter IPv4 PS4/PS5", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Save IP to settings
                Properties.Settings.Default.userip = siticoneTextBox1.Text;
                Properties.Settings.Default.Save();

                // Connect to PS4
                PS4 = new PS4DBG(siticoneTextBox1.Text);
                PS4.Connect();

                // Update connection status
                UpdateConnectionStatus(true);

                PS4.Notify(222, "You Connected To PS4/PS5!");
                MessageBox.Show("You connected to PS4/PS5", "Successful connection!",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception EX)
            {
                UpdateConnectionStatus(false);
                MessageBox.Show(EX.Message, "Oops", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void siticoneButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (PS4 == null)
                {
                    throw new ArgumentNullException("You are not connected to PS4/PS5!", "Warning!");
                }
                else
                {
                    AttachProcessName("eboot.bin");
                    PS4.Notify(222, "You Attached To The Game!");
                }
            }
            catch (Exception EX)
            {
                MessageBox.Show(EX.Message, "Oops", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void siticoneHtmlLabel3_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Save settings when closing
                Properties.Settings.Default.Save();
            }
            catch
            {
                // Ignore errors on closing
            }
        }

        private void siticoneToggleSwitch4_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (!IsPS4Connected) return;

                // Read pointer and calculate address in one line
                ulong baseAddress = PS4.ReadMemory<ulong>(PS4_PID, 0x014dcf30);
                ulong InfNitrous = baseAddress + 0x43a84;

                PS4.WriteMemory(PS4_PID, InfNitrous,
                    siticoneToggleSwitch4.Checked ? new byte[] { 0x01 } : new byte[] { 0x00 });

                PS4.Notify(222, siticoneToggleSwitch4.Checked ?
                    "Unlimited Nitrous ON" : "Unlimited Nitrous OFF");
            }
            catch (Exception sx)
            {
                MessageBox.Show(sx.Message, "Упс", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Revert toggle on error
                siticoneToggleSwitch4.Checked = !siticoneToggleSwitch4.Checked;
            }
        }

        private void siticoneButton4_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsPS4Connected) return;

                PS4.WriteMemory(PS4_PID, 0xe288ec, new byte[] { 0x35, 0xfa, 0x8e, 0x3c });
            }
            catch (Exception sx)
            {
                MessageBox.Show(sx.Message, "Упс", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Revert toggle on error
                siticoneToggleSwitch4.Checked = !siticoneToggleSwitch4.Checked;
            }
        }

        private void siticoneTrackBar1_Scroll(object sender, ScrollEventArgs e)
        {
            try
            {
                if (!IsPS4Connected) return;

                float value = (siticoneTrackBar1.Value) * 0.0003f;
                // Write the float value to memory
                byte[] floatBytes = BitConverter.GetBytes(value);
                PS4.WriteMemory(PS4_PID, 0xe288ec, floatBytes);
            }
            catch (Exception sx)
            {
                MessageBox.Show(sx.Message, "Упс", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Revert toggle on error
                siticoneToggleSwitch4.Checked = !siticoneToggleSwitch4.Checked;
            }
        }

        private void siticoneToggleSwitch5_CheckedChanged(object sender, EventArgs e)
        {

            try
            {
                if (!IsPS4Connected) return;
                if (siticoneToggleSwitch4.Checked)
                {

                    PS4.Notify(222, "Nitros Boost ON");
                    PS4.WriteMemory(PS4_PID, 0xe4745c, new byte[] { 0x00, 0x00, 0x00, 0x7f });

                }
                else
                {
                    PS4.Notify(222, "Nitros Boost ON OFF");
                    PS4.WriteMemory(PS4_PID, 0xe4745c, new byte[] { 0x00, 0x00, 0x80, 0x3f });
                }
            }
            catch (Exception sx)
            {
                MessageBox.Show(sx.Message, "Упс", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Revert toggle on error
                siticoneToggleSwitch4.Checked = !siticoneToggleSwitch4.Checked;
            }
        }

        private void siticoneButton5_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsPS4Connected) return;

                // Read pointer and calculate address in one line
                ulong baseAddress = PS4.ReadMemory<ulong>(PS4_PID, 0x14d9770);
                ulong FrameRateUnlocker = baseAddress + 0x118a00;

                PS4.WriteMemory(PS4_PID, FrameRateUnlocker, new byte[] { 0x00 });
                PS4.Notify(222, "Locked 30 FPS!");
            }
            catch (Exception sx)
            {
                MessageBox.Show(sx.Message, "Упс", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }
        private byte[] savedCarPosition1 = null;
        private float[] savedCarPositionFloats1 = null;

        // For Button8/Button9 (new buttons)
        private byte[] savedCarPosition2 = null;
        private float[] savedCarPositionFloats2 = null;
        private void siticoneButton6_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsPS4Connected) return;

                // Read pointer and calculate address in one line
                ulong baseAddressptr = PS4.ReadMemory<ulong>(PS4_PID, 0x14dcf28);
                ulong xyzcarpostion = baseAddressptr + 0x40;

                // Read the 12 bytes (3 floats) for position
                savedCarPosition1 = PS4.ReadMemory(PS4_PID, xyzcarpostion, 12); // 3 floats * 4 bytes = 12

                // OPTIONAL: Also save as float array for display/processing
                savedCarPositionFloats1 = new float[3];
                Buffer.BlockCopy(savedCarPosition1, 0, savedCarPositionFloats1, 0, 12);

                // Show position in UI (optional)
                float x = BitConverter.ToSingle(savedCarPosition1, 0);
                float y = BitConverter.ToSingle(savedCarPosition1, 4);
                float z = BitConverter.ToSingle(savedCarPosition1, 8);

                PS4.Notify(222, $"Position Saved!\nX: {x:F2}\nY: {y:F2}\nZ: {z:F2}");

                // For debugging - show bytes in hex
                string hexBytes = BitConverter.ToString(savedCarPosition1).Replace("-", " ");
                Console.WriteLine($"Saved bytes: {hexBytes}");
            }
            catch (Exception sx)
            {
                MessageBox.Show(sx.Message, "Упс", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void siticoneButton7_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsPS4Connected) return;

                // Check if we have a saved position
                if (savedCarPosition1 == null || savedCarPosition1.Length != 12)
                {
                    PS4.Notify(222, "No position saved!");
                    return;
                }

                // Read pointer and calculate addresses
                ulong baseAddress = PS4.ReadMemory<ulong>(PS4_PID, 0x14DCF30);
                ulong CanSpawn = baseAddress + 0x319e8;
                ulong Spawnatxyz = baseAddress + 0x31a20;
                ulong spawnatspeed = baseAddress + 0x31a40;

                // Write spawn flag and position
                PS4.WriteMemory(PS4_PID, CanSpawn, new byte[] { 0x01 });
                PS4.WriteMemory(PS4_PID, Spawnatxyz, savedCarPosition1);

                // Optional: Also update current car position if needed
                ulong baseAddressptr = PS4.ReadMemory<ulong>(PS4_PID, 0x14dcf28);
                ulong currentCarPosition = baseAddressptr + 0x40;
                PS4.WriteMemory(PS4_PID, currentCarPosition, savedCarPosition1);
                PS4.WriteMemory(PS4_PID, spawnatspeed, new byte[] { 0x00, 0x00, 0x00, 0x00 });

                PS4.Notify(222, "Teleported to saved position!");

                // Show teleported position (optional)
                float x = BitConverter.ToSingle(savedCarPosition1, 0);
                float y = BitConverter.ToSingle(savedCarPosition1, 4);
                float z = BitConverter.ToSingle(savedCarPosition1, 8);
                Console.WriteLine($"Teleported to X: {x:F2}, Y: {y:F2}, Z: {z:F2}");
            }
            catch (Exception sx)
            {
                MessageBox.Show(sx.Message, "Упс", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void siticoneButton8_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsPS4Connected) return;

                // Read pointer and calculate address in one line
                ulong baseAddressptr = PS4.ReadMemory<ulong>(PS4_PID, 0x14dcf28);
                ulong xyzcarpostion = baseAddressptr + 0x40;

                // Read the 12 bytes (3 floats) for position
                savedCarPosition2 = PS4.ReadMemory(PS4_PID, xyzcarpostion, 12); // 3 floats * 4 bytes = 12

                // OPTIONAL: Also save as float array for display/processing
                savedCarPositionFloats2 = new float[3];
                Buffer.BlockCopy(savedCarPosition2, 0, savedCarPositionFloats2, 0, 12);

                // Show position in UI (optional)
                float x = BitConverter.ToSingle(savedCarPosition2, 0);
                float y = BitConverter.ToSingle(savedCarPosition2, 4);
                float z = BitConverter.ToSingle(savedCarPosition2, 8);

                PS4.Notify(222, $"Position 2 Saved!\nX: {x:F2}\nY: {y:F2}\nZ: {z:F2}");

                // For debugging - show bytes in hex
                string hexBytes = BitConverter.ToString(savedCarPosition2).Replace("-", " ");
                Console.WriteLine($"Saved bytes (Slot 2): {hexBytes}");
            }
            catch (Exception sx)
            {
                MessageBox.Show(sx.Message, "Упс", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void siticoneButton9_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsPS4Connected) return;

                // Check if we have a saved position in slot 2
                if (savedCarPosition2 == null || savedCarPosition2.Length != 12)
                {
                    PS4.Notify(222, "No position saved in Slot 2!");
                    return;
                }

                // Read pointer and calculate addresses
                ulong baseAddress = PS4.ReadMemory<ulong>(PS4_PID, 0x14DCF30);
                ulong CanSpawn = baseAddress + 0x319e8;
                ulong Spawnatxyz = baseAddress + 0x31a20;
                ulong spawnatspeed = baseAddress + 0x31a40;

                // Write spawn flag and position
                PS4.WriteMemory(PS4_PID, CanSpawn, new byte[] { 0x01 });
                PS4.WriteMemory(PS4_PID, Spawnatxyz, savedCarPosition2);

                // Optional: Also update current car position if needed
                ulong baseAddressptr = PS4.ReadMemory<ulong>(PS4_PID, 0x14dcf28);
                ulong currentCarPosition = baseAddressptr + 0x40;
                PS4.WriteMemory(PS4_PID, currentCarPosition, savedCarPosition2);
                PS4.WriteMemory(PS4_PID, spawnatspeed, new byte[] { 0x00, 0x00, 0x00, 0x00 });

                PS4.Notify(222, "Teleported to Position 2!");

                // Show teleported position (optional)
                float x = BitConverter.ToSingle(savedCarPosition2, 0);
                float y = BitConverter.ToSingle(savedCarPosition2, 4);
                float z = BitConverter.ToSingle(savedCarPosition2, 8);
                Console.WriteLine($"Teleported to Position 2 - X: {x:F2}, Y: {y:F2}, Z: {z:F2}");
            }
            catch (Exception sx)
            {
                MessageBox.Show(sx.Message, "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void siticoneButton10_Click(object sender, EventArgs e)
        {
            
        }


        private async void siticoneButton11_Click(object sender, EventArgs e)
        {
            
        }


        private void siticoneTrackBar2_Scroll(object sender, ScrollEventArgs e)
        {
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {

        }

        private byte[] currentCoordinates = new byte[12]; // Store current coordinates
        private float[] currentFloats = new float[3]; // Store as floats for easy editing

        private void siticoneButton12_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsPS4Connected) return;

                // Set loading flag to prevent auto-teleport
                isLoadingCoordinates = true;

                // Read current car position
                ulong baseAddressptr = PS4.ReadMemory<ulong>(PS4_PID, 0x14dcf28);
                ulong xyzcarpostion = baseAddressptr + 0x40;

                currentCoordinates = PS4.ReadMemory(PS4_PID, xyzcarpostion, 12);

                // Convert to floats
                currentFloats[0] = BitConverter.ToSingle(currentCoordinates, 0);  // X
                currentFloats[1] = BitConverter.ToSingle(currentCoordinates, 4);  // Y
                currentFloats[2] = BitConverter.ToSingle(currentCoordinates, 8);  // Z


                // Display in numeric boxes
                numericUpDown1.Value = (decimal)currentFloats[0];
                numericUpDown2.Value = (decimal)currentFloats[1];
                numericUpDown3.Value = (decimal)currentFloats[2];


                PS4.Notify(222, "Coordinates loaded to editor!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reset loading flag
                isLoadingCoordinates = false;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            UpdateCoordinateAndTeleport(0, numericUpDown1.Value);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            UpdateCoordinateAndTeleport(1, numericUpDown2.Value);
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            UpdateCoordinateAndTeleport(2, numericUpDown3.Value);
        }
        private bool isLoadingCoordinates = false; // Add this flag
        private void UpdateCoordinateAndTeleport(int axisIndex, decimal newValue)
        {
            try
            {
                // Skip teleportation if we're just loading coordinates
                if (isLoadingCoordinates) return;

                if (!IsPS4Connected) return;

                // Update the float value
                currentFloats[axisIndex] = (float)newValue;

                // Convert back to bytes
                Buffer.BlockCopy(currentFloats, 0, currentCoordinates, 0, 12);

                // Teleport to new position
                TeleportToCoordinates(currentCoordinates);

                // Show updated value
                Console.WriteLine($"Axis {axisIndex} updated to: {newValue:F2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update error: {ex.Message}");
            }
        }

        private void TeleportToCoordinates(byte[] coordinates)
        {
            try
            {
                if (!IsPS4Connected) return;

                // Read pointer and calculate addresses for SPAWN
                ulong baseAddress = PS4.ReadMemory<ulong>(PS4_PID, 0x14DCF30);
                ulong CanSpawn = baseAddress + 0x319e8;
                ulong Spawnatxyz = baseAddress + 0x31a20;
                ulong spawnatspeed = baseAddress + 0x31a40;

                // Write spawn flag and position
                PS4.WriteMemory(PS4_PID, CanSpawn, new byte[] { 0x01 });
                PS4.WriteMemory(PS4_PID, Spawnatxyz, coordinates);
                PS4.WriteMemory(PS4_PID, spawnatspeed, new byte[] { 0x00, 0x00, 0x00, 0x00 });

                // **CRITICAL FIX: Also update CURRENT car position**
                ulong baseAddressptr = PS4.ReadMemory<ulong>(PS4_PID, 0x14dcf28);
                ulong currentCarPosition = baseAddressptr + 0x40;
                PS4.WriteMemory(PS4_PID, currentCarPosition, coordinates);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Teleport error: {ex.Message}");
            }
        }

        // Optional: Add keyboard shortcuts for fine control
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (numericUpDown1.Focused || numericUpDown2.Focused || numericUpDown3.Focused)
            {
                switch (keyData)
                {
                    case Keys.Control | Keys.Up:
                        // Ctrl+Up: Add 0.1
                        AdjustFocusedCoordinate(0.1f);
                        return true;

                    case Keys.Control | Keys.Down:
                        // Ctrl+Down: Subtract 0.1
                        AdjustFocusedCoordinate(-0.1f);
                        return true;

                    case Keys.Shift | Keys.Up:
                        // Shift+Up: Add 0.01
                        AdjustFocusedCoordinate(0.01f);
                        return true;

                    case Keys.Shift | Keys.Down:
                        // Shift+Down: Subtract 0.01
                        AdjustFocusedCoordinate(-0.01f);
                        return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void AdjustFocusedCoordinate(float adjustment)
        {
            NumericUpDown focusedControl = null;

            // Find which control is focused
            if (numericUpDown1.Focused)
                focusedControl = numericUpDown1;
            else if (numericUpDown2.Focused)
                focusedControl = numericUpDown2;
            else if (numericUpDown3.Focused)
                focusedControl = numericUpDown3;

            if (focusedControl != null)
            {
                decimal newValue = focusedControl.Value + (decimal)adjustment;
                focusedControl.Value = newValue;
            }
        }

        private void siticonePanel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {

            string url = "https://discord.gg/QFe5nQWu4t";
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open link: " + ex.Message);
            }
        }

        private void siticoneButton13_Click(object sender, EventArgs e)
        {
            try
            {
                // Read pointer and calculate addresses
                ulong baseAddress = PS4.ReadMemory<ulong>(PS4_PID, 0x14DCF30);
                ulong CanSpawn = baseAddress + 0x319e8;
                ulong spawnatspeed = baseAddress + 0x31a40;

                // Write spawn flag and position
                PS4.WriteMemory(PS4_PID, CanSpawn, new byte[] { 0x01 });

                PS4.WriteMemory(PS4_PID, spawnatspeed, new byte[] { 0x00, 0x00, 0x44, 0x44 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Teleport error: {ex.Message}");
            }
        }

        private void siticoneTrackBar2_Scroll_1(object sender, ScrollEventArgs e)
        {
        }
    }
}