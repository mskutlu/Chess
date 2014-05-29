namespace Chess
{
    partial class ChessTable
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
            this.panelBase = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblPos = new System.Windows.Forms.Label();
            this.lblTurn = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.panelBase)).BeginInit();
            this.SuspendLayout();
            // 
            // panelBase
            // 
            this.panelBase.Image = global::Chess.pieces._base;
            this.panelBase.Location = new System.Drawing.Point(12, 12);
            this.panelBase.Name = "panelBase";
            this.panelBase.Size = new System.Drawing.Size(500, 500);
            this.panelBase.TabIndex = 0;
            this.panelBase.TabStop = false;
            this.panelBase.Paint += new System.Windows.Forms.PaintEventHandler(this.panelBase_Paint);
            this.panelBase.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelBase_MouseDown);
            this.panelBase.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelBase_MouseMove);
            this.panelBase.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelBase_MouseUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(518, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Board on memory:";
            // 
            // lblPos
            // 
            this.lblPos.AutoSize = true;
            this.lblPos.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblPos.Location = new System.Drawing.Point(518, 62);
            this.lblPos.Name = "lblPos";
            this.lblPos.Size = new System.Drawing.Size(98, 17);
            this.lblPos.TabIndex = 2;
            this.lblPos.Text = "<POSITIONS>";
            // 
            // lblTurn
            // 
            this.lblTurn.AutoSize = true;
            this.lblTurn.Location = new System.Drawing.Point(518, 12);
            this.lblTurn.Name = "lblTurn";
            this.lblTurn.Size = new System.Drawing.Size(145, 26);
            this.lblTurn.TabIndex = 3;
            this.lblTurn.Text = "Turn 1\r\nWaiting for white\'s movement";
            // 
            // ChessTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(725, 523);
            this.Controls.Add(this.lblTurn);
            this.Controls.Add(this.lblPos);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panelBase);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "ChessTable";
            this.Text = "Chess Table";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChessTable_FormClosing);
            this.Load += new System.EventHandler(this.ChessTable_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ChessTable_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.panelBase)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox panelBase;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblPos;
        private System.Windows.Forms.Label lblTurn;

    }
}

