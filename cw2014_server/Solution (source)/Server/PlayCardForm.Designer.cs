namespace Windwardopolis2
{
	partial class PlayCardForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.comboBoxCards = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.comboBoxPlayedBy = new System.Windows.Forms.ComboBox();
			this.labelCompanyPassengerPlayer = new System.Windows.Forms.Label();
			this.comboBoxCompanyPassengerPlayer = new System.Windows.Forms.ComboBox();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(32, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Card:";
			// 
			// comboBoxCards
			// 
			this.comboBoxCards.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCards.FormattingEnabled = true;
			this.comboBoxCards.Location = new System.Drawing.Point(12, 25);
			this.comboBoxCards.Name = "comboBoxCards";
			this.comboBoxCards.Size = new System.Drawing.Size(295, 21);
			this.comboBoxCards.TabIndex = 3;
			this.comboBoxCards.SelectedIndexChanged += new System.EventHandler(this.comboBoxCards_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 59);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(57, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Played By:";
			// 
			// comboBoxPlayedBy
			// 
			this.comboBoxPlayedBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxPlayedBy.FormattingEnabled = true;
			this.comboBoxPlayedBy.Location = new System.Drawing.Point(12, 75);
			this.comboBoxPlayedBy.Name = "comboBoxPlayedBy";
			this.comboBoxPlayedBy.Size = new System.Drawing.Size(295, 21);
			this.comboBoxPlayedBy.TabIndex = 1;
			this.comboBoxPlayedBy.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
			// 
			// labelCompanyPassengerPlayer
			// 
			this.labelCompanyPassengerPlayer.AutoSize = true;
			this.labelCompanyPassengerPlayer.Location = new System.Drawing.Point(12, 109);
			this.labelCompanyPassengerPlayer.Name = "labelCompanyPassengerPlayer";
			this.labelCompanyPassengerPlayer.Size = new System.Drawing.Size(133, 13);
			this.labelCompanyPassengerPlayer.TabIndex = 6;
			this.labelCompanyPassengerPlayer.Text = "CompanyPassengerPlayer:";
			// 
			// comboBoxCompanyPassengerPlayer
			// 
			this.comboBoxCompanyPassengerPlayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCompanyPassengerPlayer.FormattingEnabled = true;
			this.comboBoxCompanyPassengerPlayer.Location = new System.Drawing.Point(12, 125);
			this.comboBoxCompanyPassengerPlayer.Name = "comboBoxCompanyPassengerPlayer";
			this.comboBoxCompanyPassengerPlayer.Size = new System.Drawing.Size(295, 21);
			this.comboBoxCompanyPassengerPlayer.TabIndex = 7;
			this.comboBoxCompanyPassengerPlayer.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
			// 
			// btnOk
			// 
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Location = new System.Drawing.Point(151, 162);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 8;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(232, 162);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 9;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// PlayCardForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(319, 198);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.comboBoxCompanyPassengerPlayer);
			this.Controls.Add(this.labelCompanyPassengerPlayer);
			this.Controls.Add(this.comboBoxPlayedBy);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.comboBoxCards);
			this.Controls.Add(this.label1);
			this.Name = "PlayCardForm";
			this.Text = "Play Card";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboBoxCards;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox comboBoxPlayedBy;
		private System.Windows.Forms.Label labelCompanyPassengerPlayer;
		private System.Windows.Forms.ComboBox comboBoxCompanyPassengerPlayer;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
	}
}