using Instruments.Navigation.RoseCaps; // Nav_RoseCaps
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Instruments.Navigation.Compas
{
    /// <summary>
    /// Description Résumé de UserControl.
    /// </summary>
    public class Nav_Compas : Nav_RoseCaps
	{

            #region Properties Implementation

            private bool plane3D;

            [Category("Nav_Design"), Description("Avion en 3D")]
            public bool Plane3D
            {
               get { return plane3D;  }
               set { plane3D = value; Invalidate(); }
            }

            #endregion

            #region Init

            public Nav_Compas() : base()
            {
                    // Cet appel est requis par le concepteur Windows.Forms.
                    InitializeComponent();
                                                            
                    // Activates double buffering
                    SetStyle(ControlStyles.UserPaint, true);
                    SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                    SetStyle(ControlStyles.DoubleBuffer, true);
                    SetStyle(ControlStyles.SupportsTransparentBackColor,true);
                    Init_Compas();

            }

            public new void SomethingHasChanged()
            {
              Invalidate();
            }

            public void Init_Compas()
            {
               // Paramètres par défaut
               ParametersChanged += new OnParamsChanged( SomethingHasChanged );
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
                        // Nav_Compas
                        // 
                        this.Letters_Size = 16;
                        this.Name = "Nav_Compas";
                        this.ShowButton = true;
                        this.Size = new System.Drawing.Size(182, 182);
                        this.Paint += new System.Windows.Forms.PaintEventHandler(this.Nav_Compas_Paint);
                }

            #endregion

            #endregion

            // ---------------------------------------------------------------------------

            #region Paint

            private void Nav_Compas_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
            {
                // Trace le compas

                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // TRACE L'AVION
                GraphicsPath gp_avion = getContourAvionGraphic();
                Color Couleur_BodyAvion  = Color.Gray; // Color.FromArgb(170, Color.WhiteSmoke );
                Color Couleur_BordsAvion = Color.FromArgb(200, Color.White );

                float zoom = 0.88f;
                Matrix mirror = new Matrix(1,0,0,1, 0,0);
                mirror.Scale(zoom, zoom);
                gp_avion.Transform(mirror);

                Rectangle Limits = Rectangle.Truncate( gp_avion.GetBounds() );

                g.ResetTransform();                                            // 4 = nez plus près du cap suivi
                g.TranslateTransform(centerPoint.X - Limits.Width/2, centerPoint.Y - 4 - Limits.Height/2);

                if (plane3D)
                {
                  g.TranslateTransform( -1, -2);
                  g.DrawPath( new Pen(Color.Black, 2) , gp_avion);
                }
                g.ResetTransform();
                g.TranslateTransform(centerPoint.X - Limits.Width/2,  centerPoint.Y - 4- Limits.Height/2); //-Limits.Width , centerPoint.Y - 4 - Limits.Height/2)

                g.FillPath( new SolidBrush(Couleur_BodyAvion) , gp_avion);
                g.DrawPath( new Pen(Couleur_BordsAvion)       , gp_avion);


                // Vis centrale
                g.ResetTransform();
                Rectangle rect = this.ClientRectangle;
                LinearGradientBrush bvs = new LinearGradientBrush(rect, Color.Black,Color.Yellow, LinearGradientMode.ForwardDiagonal);
                g.TranslateTransform(centerPoint.X, centerPoint.Y);
                g.DrawEllipse(Pens.Black, -3,-3,6,6);
                g.FillEllipse(bvs, -3,-3,6,6);

                // Verre
                if ( this.GetType() != typeof(Nav_Compas) )
                  return;

                g.ResetTransform();
                Rectangle rrr = this.ClientRectangle;
                Color cc = Color.FromArgb(50, Color.MediumSlateBlue);
                LinearGradientBrush b = new LinearGradientBrush(rrr, Color.Transparent, cc, LinearGradientMode.ForwardDiagonal);
                b.SetBlendTriangularShape(0.59f, 0.50F);

                GraphicsPath gp_verre = new GraphicsPath();
                gp_verre.AddEllipse(centerPoint.X-RayonExt-9, centerPoint.Y-RayonExt-9, RayonExt*2+16, RayonExt*2+16 );
                g.FillPath(b, gp_verre);
            }

            #endregion

            // ---------------------------------------------------------------------------
   	}
}

