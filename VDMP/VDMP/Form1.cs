using NHotkey;
using NHotkey.WindowsForms; // Required Library
using System;
using System.Diagnostics;
using System.Reflection;
using WindowsDesktop; // Required Library


namespace VDMP
{
    public partial class Form1 : Form
    {
        int previousVisitedDesktopHashCode = 0;

        public Form1()
        {
            InitializeComponent();


            displayDesktopButtons();

            VirtualDesktop.CurrentChanged += (_, args) =>
            {
                previousVisitedDesktopHashCode = args.OldDesktop.GetHashCode();
                highlightCurrentDesktop();
            };

            //refresh button list when new virtual desktop is created or removed
            VirtualDesktop.Created += (_, args) =>
            {
                desktop1Btn.Invoke(new Action(() =>
                {
                    displayDesktopButtons();
                }));

            };

            VirtualDesktop.Destroyed += (_, args) =>
            {
                desktop1Btn.Invoke(new Action(() =>
                {
                    displayDesktopButtons();
                }));
            };


            //default grid size
            desktopButtonsPanel.MaximumSize = new Size(desktop1Btn.Width * 6, 0);


            ToolTip myToolTip = new ToolTip();
            myToolTip.SetToolTip(backToPreviousDesktop, "Back to previous desktop: Ctrl + Win + B");

            ToolTip myToolTip2 = new ToolTip();
            myToolTip2.SetToolTip(minimalToggleBtn, "Minimal View - Toggle");

            ToolTip myToolTip3 = new ToolTip();
            myToolTip2.SetToolTip(leftDesktopSwitch, "Left");

            ToolTip myToolTip4 = new ToolTip();
            myToolTip2.SetToolTip(rightDesktopSwitch, "Right");

            HotkeyManager.Current.AddOrReplace("BackToPreviousDesktop", ModKeys.Control | ModKeys.Windows | Keys.B, OnBackHotkeyPress);

            
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Give the OS 100-200ms to register the window view
            await Task.Delay(200);
            VirtualDesktop.PinWindow(this.Handle);
        }

        private void OnBackHotkeyPress(object sender, HotkeyEventArgs e)
        {
            SwitchDesktopByHashCode(previousVisitedDesktopHashCode);
        }

        void displayDesktopButtons()
        {
            var desktops = VirtualDesktop.GetDesktops();

            //Only buttons for available virtual desktops; hide remaining desktop buttons
            for (int i = 1; i <= 50; i++)
            {
                var desktopBtnReference = this.Controls.Find("desktop" + i.ToString() + "Btn", true);
                Button desktopBtn = (Button)desktopBtnReference[0];

                if (i <= desktops.Length)
                {
                    desktopBtn.Visible = true;
                }
                else
                {
                    desktopBtn.Visible = false;
                }
            }
        }

        private void SwitchDesktop(int index)
        {
            try
            {
                var desktops = VirtualDesktop.GetDesktops();
                if (index < desktops.Length)
                {
                    desktops[index].Switch();

                }
                else
                {
                    MessageBox.Show($"Virtual Desktop {index + 1} does not exist.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error switching desktop: " + ex.Message);
            }
        }

        void SwitchDesktopByHashCode(int hashCode)
        {
            try
            {
                var desktops = VirtualDesktop.GetDesktops();
                for (int i = 0; i < desktops.Length; i++)
                {
                    if (desktops[i].GetHashCode() == hashCode)
                    {
                        desktops[i].Switch();
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error switching desktop: " + ex.Message);
            }
        }


        private void minimalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleMinimalView();
        }

        private void minimalToggleBtn_Click(object sender, EventArgs e)
        {
            toggleMinimalView();
        }

        void toggleMinimalView()
        {
            if (FormBorderStyle != FormBorderStyle.FixedSingle)
            {
                FormBorderStyle = FormBorderStyle.FixedSingle;
                menuStrip1.Visible = true;
                desktopButtonsPanel.Location = new Point(0, 25);
            }
            else
            {
                FormBorderStyle = FormBorderStyle.None;
                menuStrip1.Visible = false;
                desktopButtonsPanel.Location = new Point(0, 0);
            }
        }



        private void desktopBtn_Click(object sender, EventArgs e)
        {
            string desktopNumberText = ((Button)sender).Text;
            int goToDesktopNum = Convert.ToInt16(desktopNumberText) - 1;  //virtual desktop index starts at 0 in the library
            SwitchDesktop(goToDesktopNum);

        }



        private void backToPreviousDesktop_Click(object sender, EventArgs e)
        {
            SwitchDesktopByHashCode(previousVisitedDesktopHashCode);
        }

        void changeDisplayGridSize(int size)
        {
            desktopButtonsPanel.MaximumSize = new Size(desktop1Btn.Width * size, 0);
        }


        private void grid1x1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeDisplayGridSize(2);
        }

        private void grid4x4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeDisplayGridSize(4);
        }

        void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string gridNumberText = ((ToolStripMenuItem)sender).Text;
            int newGridNumber = Convert.ToInt16(gridNumberText);
            changeDisplayGridSize(newGridNumber);
        }



        private void addDesktop_Click(object sender, EventArgs e)
        {
            var desktops = VirtualDesktop.GetDesktops();
            if (desktops.Length < 50)
            {
                VirtualDesktop.Create();
            }
        }

        private void removeDesktop_Click(object sender, EventArgs e)
        {
            var desktops = VirtualDesktop.GetDesktops();
            if (desktops.Length > 2)
            {
                desktops[desktops.Length - 1].Remove();
            }
        }

        private void leftDesktopSwitch_Click(object sender, EventArgs e)
        {
            var desktop = VirtualDesktop.Current;
            var left = desktop.GetLeft();
            if (left != null)
                left.Switch();
        }

        private void rightDesktopSwitch_Click(object sender, EventArgs e)
        {
            var desktop = VirtualDesktop.Current;
            var right = desktop.GetRight();
            if (right != null)
                right.Switch();
        }

        void highlightCurrentDesktop()
        {
            var desktops = VirtualDesktop.GetDesktops();
            var currentDesktop = VirtualDesktop.Current;
            for (int i = 0; i < desktops.Length; i++)
            {
                var desktopBtnReference = this.Controls.Find("desktop" + (i + 1).ToString() + "Btn", true);
                Button desktopBtn = (Button)desktopBtnReference[0];

                if (currentDesktop.GetHashCode() == desktops[i].GetHashCode())
                {
                    desktopBtn.BackColor = Color.DarkGray;
                    desktopBtn.ForeColor = Color.White;
                }
                else
                {
                    desktopBtn.BackColor = Color.White;
                    desktopBtn.ForeColor = Color.Black;
                }
            }
        }

        void changeOpacity(int newOpacity)
        {
            this.Opacity = (double)newOpacity / 100;
        }

        private void opacitySelectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string opacityText = ((ToolStripMenuItem)sender).Text;
            int newOpacity = Convert.ToInt16(opacityText);
            changeOpacity(newOpacity);
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(alwaysOnTopToolStripMenuItem.Checked)
            {
                TopMost = true;
            }
            else
            {
                TopMost = false;
            }
            
        }
    }
}
