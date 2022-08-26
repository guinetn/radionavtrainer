using Instruments.Navigation.Compas;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Instruments.Navigation.AdfRmi
{
    /// <summary>
    /// Description Résumé de UserControl.
    /// </summary>
    public class Nav_AdfRmi : Nav_Compas
	{
            public int relevement_Adf;
            private bool modeRmi; // mode RMI (true) ou ADF (false)

            // ---------------------------------------------------------------------------

            public new void SomethingHasChanged()
            {
              // Variation du cap, recalculer le cap adf
              if (modeRmi && avion!=null)
                Heading1 = validHeading( avion.angle );

              double angle = getAngle(0, emetteur.getCenter(), avion.getCenter());
              relevement_Adf = validHeading( 180-(int)angle + avion.angle);

              distanceToPlane = getDistance( getVector( emetteur.area.Location, avion.area.Location) );
              // !! distance en pixels !

              Invalidate();
            }

            // ---------------------------------------------------------------------------

            #region Properties Implementation

            [Category("Nav_Design"), Description("Mode ADF ou RMI")]
            public bool ModeRmi
            {
               get { return modeRmi;  }
               set { modeRmi = value; }
            }

            #endregion

            #region Init

            public Nav_AdfRmi() : base()
            {
                    // Cet appel est requis par le concepteur Windows.Forms.
                    InitializeComponent();

                    // Activates double buffering
                    SetStyle(ControlStyles.UserPaint, true);
                    SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                    SetStyle(ControlStyles.DoubleBuffer, true);
                    SetStyle(ControlStyles.ResizeRedraw, true);
                    SetStyle(ControlStyles.SupportsTransparentBackColor,true);
                    Init_Adf();
            }

            public void Init_Adf()
            {
               // Paramètres par défaut
               relevement_Adf = validHeading(Heading1 - 90);
               ParametersChanged += new OnParamsChanged( SomethingHasChanged );
            }

            private void Nav_AdfRmi_Resize(object sender, System.EventArgs e)
            {
               centerPoint =  new Point(this.Width/2,this.Height/2);
            }

            /// <summary>
            /// Nettoyage des ressources utilisées.
            /// </summary>
            protected override void Dispose(bool disposing)
            {
                    base.Dispose(disposing);
            }

            #region Code généré par le concepteur de composant
            /// <summary>
            /// Méthode requise pour la gestion du concepteur  - ne pas modifier 
            /// le contenu de cette méthode avec l'éditeur de code.
            /// </summary>
            private void InitializeComponent()
                {
                        // 
                        // Nav_AdfRmi
                        // 
                        this.Name = "Nav_AdfRmi";
                        this.Resize += new System.EventHandler(this.Nav_AdfRmi_Resize);
                        this.Paint += new System.Windows.Forms.PaintEventHandler(this.Nav_AdfRmi_Paint);
                }

            #endregion

            #endregion

            // ---------------------------------------------------------------------------

            public void Set_CapAdf(int newCapadf)
            {
              relevement_Adf = validHeading(newCapadf);
              if (ParametersChanged != null)
                ParametersChanged();
            }

            // ---------------------------------------------------------------------------

            #region Paint

            private void Nav_AdfRmi_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
            {
                // Trace l'adf
                                                          
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                if (DisplayName)
                {
                 string nomctrl = "ADF";
                 if (modeRmi)
                   nomctrl = "RMI";

                 Font textefont = new Font("Arial", 7, FontStyle.Bold);
                 StringFormat format = new StringFormat();
                 format.Alignment=StringAlignment.Center;
                 g.DrawString(nomctrl , textefont, new SolidBrush(Color.White), rayonExt-10,rayonExt-10, format);
                }

                // Flêche ADF
                GraphicsPath gp_Arrow = getArrowGraphic((RayonInt-16)*2);
                Rectangle r = Rectangle.Truncate( gp_Arrow.GetBounds() );

                g.ResetTransform();
                g.TranslateTransform(centerPoint.X, centerPoint.Y);
                g.RotateTransform(-relevement_Adf);
                g.TranslateTransform(0, + 6 - (int)(r.Height/2.0f)); // +7 = hauteur de la pointe

                // ombre de la flêche
                g.TranslateTransform(-2,-1);
                g.DrawPath( new Pen(Color.Black) , gp_Arrow);
                g.FillPath( new SolidBrush(Color.Black), gp_Arrow);

                // Flêche
                g.TranslateTransform(2,1);
                g.DrawPath( new Pen(Color.Black) , gp_Arrow);
                g.FillPath( new SolidBrush(Color.Yellow), gp_Arrow);

                // DME
                double DME_distance = distanceToPlane/EchellePixParNm;
                Nav_Basic.traceDME(g, new Point(14,3), DME_distance);

                // Vis centrale
                /*g.ResetTransform();
                g.TranslateTransform(centerPoint.X, centerPoint.Y);
                g.FillEllipse(Brushes.Black, -2,-2,4,4);*/

                // Vis centrale
                g.ResetTransform();
                Rectangle rect = this.ClientRectangle;
                LinearGradientBrush bvs = new LinearGradientBrush(rect, Color.Black,Color.Yellow, LinearGradientMode.ForwardDiagonal);
                g.TranslateTransform(centerPoint.X, centerPoint.Y);
                g.DrawEllipse(Pens.Black, -3,-3,6,6);
                g.FillEllipse(bvs, -3,-3,6,6);

                // Verre

                g.ResetTransform();
                Rectangle rrr = this.ClientRectangle;
                Color cc = Color.FromArgb(50, Color.MediumSlateBlue);
                LinearGradientBrush b = new LinearGradientBrush(rrr, Color.Transparent, cc, LinearGradientMode.ForwardDiagonal);
                b.SetBlendTriangularShape(0.59f, 0.50F);

                GraphicsPath gp_verre = new GraphicsPath();
                gp_verre.AddEllipse(centerPoint.X-RayonExt-9, centerPoint.Y-RayonExt-9, RayonExt*2+16, RayonExt*2+16 );
                g.FillPath(b, gp_verre);
            }

            #endregion  Paint

            // ---------------------------------------------------------------------------
   	}
}

