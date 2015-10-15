using Checkers.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Checkers
{
    public partial class MainWindow
    {


        private void lblAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (AboutBox1 about_window = new AboutBox1())
                about_window.ShowDialog();
        }


        public void AddSystemLog(string msg)
        {
            this.internalAddLog(msg, true);
        }

        public void AddPlayerLog(string msg)
        {
            this.internalAddLog(msg, false);
        }

        private void internalAddLog(string s, bool system_message)
        {
            if (system_message)
            {
                this.richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
                this.richTextBox1.SelectionColor = Color.Red;
                this.richTextBox1.SelectionFont = new Font(this.richTextBox1.SelectionFont, FontStyle.Bold);
            }
            else
            {
                if (this.current_turn == PawnColor.Black)
                    this.richTextBox1.SelectionAlignment = HorizontalAlignment.Right;
                if (this.current_turn == PawnColor.White)
                    this.richTextBox1.SelectionAlignment = HorizontalAlignment.Left;

                this.richTextBox1.SelectionColor = Color.Black;
                this.richTextBox1.SelectionFont = new Font(this.richTextBox1.SelectionFont, FontStyle.Regular);
            }

            this.richTextBox1.AppendText(s + "\n");
            this.richTextBox1.ScrollToCaret();
        }
    }
}
