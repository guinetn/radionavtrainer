using Instruments.Navigation.RoseCaps;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Instruments.Navigation.VorHsi
{
    /// <summary>
    /// Description Résumé de UserControl.
    /// </summary>
    public class Nav_VorHsi : Nav_RoseCaps                         
	{
            public int relevementVor;      // angle entre la radiale et le cap
            private bool modeHsi;          // mode HSI (true) ou VOR (false)
            public bool inSecteurFrom;
            
            // ---------------------------------------------------------------------------

            public new void SomethingHasChanged()
            {
              if (ModeHSI)
              {
                if (avion!=null)
                   Heading1 = validHeading( avion.angle);
                relevementVor = (int)getAngle(Heading2, avion.getCenter(), emetteur.getCenter());
              }
              else
                 relevementVor = (int)getAngle(Heading1, avion.getCenter(), emetteur.getCenter());

              inSecteurFrom = ! isIn_From_VorSector(relevementVor);
              distanceToPlane = getDistance( getVector( emetteur.area.Location, avion.area.Location) );
              // !! distance en pixels !

              Invalidate();
            }

            // ---------------------------------------------------------------------------

            #region Properties Implementation

            [Category("Nav_Design"), Description("Mode HSI ou VOR")]
            public bool ModeHSI
            {
               get { return modeHsi;  }
               set { modeHsi = value; }
            }

            #endregion

            #region Init

            public Nav_VorHsi() : base()
            {
                    // Cet appel est requis par le concepteur Windows.Forms.
                    InitializeComponent();

                    // Activates double buffering
                    SetStyle(ControlStyles.UserPaint, true);
                    SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                    SetStyle(ControlStyles.DoubleBuffer, true);
                    SetStyle(ControlStyles.ResizeRedraw, true);
                    SetStyle(ControlStyles.SupportsTransparentBackColor,true);
                    Init_Vor();
            }

            public void Init_Vor()
            {
               // Paramètres par défaut
               relevementVor = 0;
               inSecteurFrom = true;
               CapsIn = true;

               ParametersChanged += new OnParamsChanged( SomethingHasChanged );
            }

            private void Nav_VorHsi_Resize(object sender, System.EventArgs e)
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
                        // Nav_VorHsi
                        // 
                        this.Name = "Nav_VorHsi";
                        this.Resize += new System.EventHandler(this.Nav_VorHsi_Resize);
                        this.Paint += new System.Windows.Forms.PaintEventHandler(this.Nav_VorHsi_Paint);
                }
            #endregion

            #endregion

            // ---------------------------------------------------------------------------

            public bool isIn_From_VorSector(int relevementvor)
            {
              // true = secteur TO  ,  false = secteur FROM
              return (-90 <= relevementvor && relevementvor <= 90);
           }

           // ---------------------------------------------------------------------------

           #region Paint
           private void Nav_VorHsi_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
           {
                // Trace le VOR/HSI

                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.ResetTransform();

                StringFormat format = new StringFormat();
                format.Alignment=StringAlignment.Center;

                g.TranslateTransform(centerPoint.X , centerPoint.Y);

                if (DisplayName)
                {
                 string nomctrl = "VOR";
                 if (modeHsi)
                   nomctrl = "HSI";

                 Font textefont = new Font("Arial", 7, FontStyle.Bold);
                 g.DrawString(nomctrl , textefont, new SolidBrush(Color.White), rayonExt-10,rayonExt-10, format);
                }


                if (modeHsi)
                   g.RotateTransform(Heading2-Heading1);



                // Zone hachurée "DRAPEAU DU VOR OFF"
                double DME_distance = distanceToPlane/EchellePixParNm;
                if (  DME_distance < (EchelleNM/10.0f) )
                {
                  //g.TranslateTransform(centerPoint.X, centerPoint.Y);

                  // Rectangle 3D
                  Rectangle rect = new Rectangle(-8,-17,14,8);
                  g.DrawRectangle(Pens.Black, rect);
                  rect.X+=1; rect.Y+=1;
                  rect.Width -= 1;
                  rect.Height -= 1;
                  g.DrawRectangle(Pens.White, rect);

                  HatchBrush hb = new HatchBrush(HatchStyle.WideDownwardDiagonal, Color.Red, Color.Black);
                  g.FillRectangle(hb,rect);
                }


                // Stylo
                Pen cerclepen = new Pen(Color.Ivory,2);
                cerclepen.Width=1;
                cerclepen.Brush = Brushes.Ivory; 

                // Petits cercles de part et d'autre du centre à 2° d'intervalle:    °°°°°X°°°°°
                int pixelsParDegre = RayonExt/20;
                for (int x=1; x<6; x+=1)
                {
                    g.DrawEllipse(cerclepen,  x*pixelsParDegre*2-1, -1, 2, 2);
                    g.DrawEllipse(cerclepen, -x*pixelsParDegre*2-1, -1, 2, 2);
                }
                g.TranslateTransform(0, -RayonInt-6);
                
                // Affiche les triangles TO/FROM
                FontFamily VorFont = new FontFamily("Arial");
                int fontStyle = (int)FontStyle.Bold;
                GraphicsPath gp = new GraphicsPath();

                GraphicsPath gp_Arrow = getArrowGraphic(0);
                RectangleF rectangle = new RectangleF(-12,-10,23,20);
                if (inSecteurFrom)
                {
                  // FROM
                  g.TranslateTransform(RayonInt-25,centerPoint.Y-12);

                  gp.AddString("FR", VorFont, fontStyle, 12, rectangle, format);
                  g.TranslateTransform(-2,-1);
                  g.FillPath(Brushes.Black, gp);
                  g.TranslateTransform(2,1);
                  g.FillPath(Brushes.Ivory, gp);     

                  g.TranslateTransform(-1, 4);
                  g.RotateTransform(180);

                  g.TranslateTransform(-2,-1);
                  g.FillPath(Brushes.Black, gp_Arrow);
                  g.TranslateTransform(2,1);
                  g.FillPath(Brushes.White, gp_Arrow);
                }
                else
                {
                  // TO
                  g.TranslateTransform(RayonInt-25 , centerPoint.Y-45);

                  g.TranslateTransform(-2,-1);
                  g.FillPath(Brushes.Black, gp_Arrow);
                  g.TranslateTransform(2,1);
                  g.FillPath(Brushes.White, gp_Arrow);

                  g.TranslateTransform(1, 11);
                  gp.AddString("TO", VorFont, fontStyle, 12, rectangle, format);

                  g.TranslateTransform(-2,-1);
                  g.FillPath(Brushes.Black, gp);
                  g.TranslateTransform(2,1);
                  g.FillPath(Brushes.Ivory, gp);
                }

                // Vis centrale
                g.ResetTransform();
                Rectangle rr = this.ClientRectangle;
                LinearGradientBrush bvs = new LinearGradientBrush(rr, Color.Black,Color.Yellow, LinearGradientMode.ForwardDiagonal);
                g.TranslateTransform(centerPoint.X, centerPoint.Y);
                g.DrawEllipse(Pens.Black, -3,-3,6,6);
                g.FillEllipse(bvs, -3,-3,6,6);
                
                // ---------------------------------------------------------------------------
                // Aiguille de flanquement  |||
                
                g.ResetTransform();
                g.TranslateTransform(centerPoint.X, centerPoint.Y);
                if (modeHsi)
                    g.RotateTransform(Heading2-Heading1);

                cerclepen.Color = Color.Yellow;
                cerclepen.Width = 2;
                cerclepen.StartCap= LineCap.Triangle;
                cerclepen.EndCap= LineCap.Triangle;

                double deviation = relevementVor;
                if ( 90.0 < Math.Abs(deviation) )
                   deviation = 180.0*Math.Sign(deviation) - deviation;
//                deviation = -deviation;

                double xf = deviation * pixelsParDegre;

                if (10 < Math.Abs(deviation))
                   xf = Math.Sign(deviation)*12.0*pixelsParDegre; // Déplacement max = 12°
                int yf = (int)(RayonInt*0.50f);

                g.TranslateTransform(-2,-1);
                g.DrawLine(Pens.Black, (int)xf, (int)(-yf) , (int)xf, (int)yf);
                g.DrawLine(Pens.Black, 0,  RayonExt , 0,  (int)yf+2);
                g.DrawLine(Pens.Black, 0, -RayonExt , 0, -(int)yf-2);

                g.TranslateTransform(2,1);
                g.DrawLine(cerclepen, (int)xf, (int)(-yf) , (int)xf, (int)yf);
                g.DrawLine(cerclepen, 0,  RayonExt , 0,  (int)yf+2);
                g.DrawLine(cerclepen, 0, -RayonExt , 0, -(int)yf-2);

                // Index des caps
                g.TranslateTransform(0, -RayonInt-6);
                g.TranslateTransform(-2,-1);
                g.FillPath(Brushes.Black, gp_Arrow);
                g.TranslateTransform(2,1);
                g.FillPath(Brushes.Yellow, gp_Arrow);

                // DME
                Nav_Basic.traceDME(g, new Point(14,3), DME_distance);

                // Flanquement / | \
                //double alpha = Math.Acos( xf/(RayonInt/2.0) );
                //double yf = Math.Sin(alpha)*(RayonInt/2.0);

                // Verre
                Rectangle rrr = this.ClientRectangle;
                Color cc = Color.FromArgb(50, Color.MediumSlateBlue);
                LinearGradientBrush b = new LinearGradientBrush(rrr, Color.Transparent, cc, LinearGradientMode.ForwardDiagonal);
                b.SetBlendTriangularShape(0.59f, 0.50F);

                GraphicsPath gp_verre = new GraphicsPath();
                gp_verre.AddEllipse(centerPoint.X-RayonExt-9, centerPoint.Y-RayonExt-9, RayonExt*2+16, RayonExt*2+16 );
                g.FillPath(b, gp_verre);
           }
           #endregion Paint

           // ---------------------------------------------------------------------------
   	}
}

