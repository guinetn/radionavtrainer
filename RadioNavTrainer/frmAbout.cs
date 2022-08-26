using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Globalization; // CultureInfo

namespace RadioNavTrainer
{
	/// <summary>
	/// Description Résumé de WinForm1.
	/// </summary>
	public class frmAbout : System.Windows.Forms.Form
	{
		/// <summary>
		/// Variable requise par le concepteur.
		/// </summary>
		private System.ComponentModel.Container components = null;
                private System.Windows.Forms.Button btClose;
                private System.Windows.Forms.Panel panel1;
        private Button bt_ok;
        private System.Windows.Forms.RichTextBox richTextBox1;

		public frmAbout()
		{
			//
			// Requis pour la gestion du concepteur Windows Form
			//
		   	InitializeComponent();
                        
		}

		/// <summary>
		/// Nettoyage des ressources utilisées.
		/// </summary>
		protected override void Dispose (bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Code généré par le concepteur Windows Form
		/// <summary>
		/// Méthode requise pour la gestion du concepteur - ne pas modifier
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
                {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAbout));
            this.btClose = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.bt_ok = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btClose
            // 
            this.btClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(102)))), ((int)(((byte)(134)))));
            this.btClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btClose.ForeColor = System.Drawing.Color.White;
            this.btClose.Image = ((System.Drawing.Image)(resources.GetObject("btClose.Image")));
            this.btClose.Location = new System.Drawing.Point(258, 535);
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size(84, 37);
            this.btClose.TabIndex = 0;
            this.btClose.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btClose.UseVisualStyleBackColor = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
            this.panel1.CausesValidation = false;
            this.panel1.Controls.Add(this.bt_ok);
            this.panel1.Controls.Add(this.richTextBox1);
            this.panel1.Controls.Add(this.btClose);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(618, 585);
            this.panel1.TabIndex = 1;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseMove);
            // 
            // bt_ok
            // 
            this.bt_ok.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bt_ok.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.bt_ok.Location = new System.Drawing.Point(400, 434);
            this.bt_ok.Name = "bt_ok";
            this.bt_ok.Size = new System.Drawing.Size(75, 34);
            this.bt_ok.TabIndex = 1;
            this.bt_ok.Text = "OK";
            this.bt_ok.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.White;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Location = new System.Drawing.Point(23, 84);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBox1.Size = new System.Drawing.Size(452, 339);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // frmAbout
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            this.BackColor = System.Drawing.Color.Blue;
            this.ClientSize = new System.Drawing.Size(500, 475);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAbout";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "A Propos";
            this.TransparencyKey = System.Drawing.Color.Blue;
            this.Load += new System.EventHandler(this.frmAbout_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

                }
		#endregion

                #region DEPLACEMENT DU CADRE
             
                private Point StartPos= new Point(0,0);

                private void panel1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
                {
                   if(e.Button == MouseButtons.Left)
                   {
                      StartPos.X = e.X;
                      StartPos.Y = e.Y;
                   }

                }

                private void panel1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
                {
                    if (e.Button==MouseButtons.Left)
                    {
                       this.Left = Cursor.Position.X-StartPos.X;
                       this.Top = Cursor.Position.Y-StartPos.Y;
                    }
                }
                #endregion
                
                private void frmAbout_Load(object sender, System.EventArgs e)
                {
                   string Texte = this.richTextBox1.Text;
                   int p = Texte.IndexOf("<<ENGLISH>>"); // !! coller le texte us sans saut de ligne après <<ENGLISH>>"
                   frmMain f = (frmMain)Owner;
                   string langue = f.cultInfo.Name;

                   if ( langue=="en" && 0<p)
                     Texte = Texte.Substring(p+11);
                   else
                     Texte = Texte.Substring(0, p);

                   richTextBox1.Rtf = Texte;
                }
                
 	}
}
