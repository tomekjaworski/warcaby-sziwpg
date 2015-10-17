namespace Checkers
{
    partial class DebugWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DebugWindow));
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkDontCheckStopConditions = new System.Windows.Forms.CheckBox();
            this.chkAllowMovingBotPawnsByMouse = new System.Windows.Forms.CheckBox();
            this.chkAllowNormalPawnMoveBack = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(16, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(441, 59);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(469, 85);
            this.panel1.TabIndex = 1;
            // 
            // chkDontCheckStopConditions
            // 
            this.chkDontCheckStopConditions.AutoSize = true;
            this.chkDontCheckStopConditions.Checked = global::Checkers.Properties.Settings.Default.debug_dont_check_stop_conds;
            this.chkDontCheckStopConditions.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Checkers.Properties.Settings.Default, "debug_dont_check_stop_conds", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.chkDontCheckStopConditions.Location = new System.Drawing.Point(19, 149);
            this.chkDontCheckStopConditions.Name = "chkDontCheckStopConditions";
            this.chkDontCheckStopConditions.Size = new System.Drawing.Size(268, 17);
            this.chkDontCheckStopConditions.TabIndex = 101;
            this.chkDontCheckStopConditions.Text = "DEBUG: Nie sprawdzaj warunków zakończenia gry";
            this.chkDontCheckStopConditions.UseVisualStyleBackColor = true;
            // 
            // chkAllowMovingBotPawnsByMouse
            // 
            this.chkAllowMovingBotPawnsByMouse.AutoSize = true;
            this.chkAllowMovingBotPawnsByMouse.Checked = global::Checkers.Properties.Settings.Default.debug_play_as_bot;
            this.chkAllowMovingBotPawnsByMouse.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Checkers.Properties.Settings.Default, "debug_play_as_bot", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.chkAllowMovingBotPawnsByMouse.Location = new System.Drawing.Point(19, 126);
            this.chkAllowMovingBotPawnsByMouse.Name = "chkAllowMovingBotPawnsByMouse";
            this.chkAllowMovingBotPawnsByMouse.Size = new System.Drawing.Size(228, 17);
            this.chkAllowMovingBotPawnsByMouse.TabIndex = 100;
            this.chkAllowMovingBotPawnsByMouse.Text = "DEBUG: Pozwól ruszać piony bota myszką";
            this.chkAllowMovingBotPawnsByMouse.UseVisualStyleBackColor = true;
            // 
            // chkAllowNormalPawnMoveBack
            // 
            this.chkAllowNormalPawnMoveBack.AutoSize = true;
            this.chkAllowNormalPawnMoveBack.Checked = global::Checkers.Properties.Settings.Default.debug_allow_pawn_move_back;
            this.chkAllowNormalPawnMoveBack.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Checkers.Properties.Settings.Default, "debug_allow_pawn_move_back", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.chkAllowNormalPawnMoveBack.Location = new System.Drawing.Point(19, 172);
            this.chkAllowNormalPawnMoveBack.Name = "chkAllowNormalPawnMoveBack";
            this.chkAllowNormalPawnMoveBack.Size = new System.Drawing.Size(264, 17);
            this.chkAllowNormalPawnMoveBack.TabIndex = 102;
            this.chkAllowNormalPawnMoveBack.Text = "DEBUG: Pozwól na ruch zwykłych pionów do tyłu";
            this.chkAllowNormalPawnMoveBack.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(382, 205);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 103;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // DebugWindow
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(469, 240);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.chkAllowNormalPawnMoveBack);
            this.Controls.Add(this.chkDontCheckStopConditions);
            this.Controls.Add(this.chkAllowMovingBotPawnsByMouse);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "DebugWindow";
            this.Text = "Parametry uruchomieniowe";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DebugWindow_FormClosing);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnOk;
        public System.Windows.Forms.CheckBox chkDontCheckStopConditions;
        public System.Windows.Forms.CheckBox chkAllowMovingBotPawnsByMouse;
        public System.Windows.Forms.CheckBox chkAllowNormalPawnMoveBack;
    }
}