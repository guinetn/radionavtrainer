using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;                            
using System.Drawing.Drawing2D;       
using System.Data;
using System.Windows.Forms;
using Instruments.Navigation; // Nav_Basic

namespace Instruments.Navigation.RoseCaps
{
	/// <summary>
	/// Description Résumé de UserControl.
	/// </summary>
	public class Nav_RoseCaps : Nav_Basic
	{
            /// <summary>
            /// Variable requise par le concepteur.
            /// </summary>
            private System.ComponentModel.IContainer components;

            private int lineWidth; // Epasisseur des traits des caps
            private bool showButton;
            private bool capsIn;       // Caps à l'interieur ou à l'exterieur
            private bool mouseInCenterActivated; // Clic dans le cercle central permet la rotation ou pas

            private Color marqueurColor;
            private bool  marqueurShow;

            // Pour le clic long
            private int Cap_MouseDown_Delta;    // variation du cap par mouse_down
            private int Attente_MouseDown = 70; // en ms

            // MousePos
            private double angle_Precedent; // Angle précédent (pour calcul vitesse rotation)
            private System.Windows.Forms.Timer timer_Cap; // indicateur si "MouseMove" dans la rose des cap  OU  dans les vents

            // Bouton
            private Point Heading_BtPos = new Point(145,15);
            private int Heading_BtSize = 19;
            private int Heading_BtAngle = 0;
            private Rectangle HeadingRect;

            // Echelles (viennent de la fenêtre principale).
            // Pour calculs de distances par rapport aux autres objets
            public double EchellePixParNm=1;
            public int EchelleNM=1;
            public double distanceToPlane=1;   // Distance de l'avion au VOR (en pixels)

            private bool displayName = true;
            // vis
            public string vis="1234"; // 1 +/ou 2 +/ou 3 +/ou 4 dans le sens horaire. début à 10h

            // Evénements
            public delegate void OnParamsChanged();
            public OnParamsChanged ParametersChanged;

            // Fonctions communes à tous les composants
            public void SomethingHasChanged()
            {
              if ( OnChanges != null)  // Evénement utilisateur
                   OnChanges();
              Invalidate();
            }

            // ---------------------------------------------------------------------------

            private void Nav_RoseCaps_Load(object sender, System.EventArgs e)
            {
              ParametersChanged += new OnParamsChanged( SomethingHasChanged );
            }

            // ---------------------------------------------------------------------------

            #region Properties Implementation

            [Category("Nav_Design"), Description("1 +/ou 2 +/ou 3 +/ou 4 dans le sens horaire. début à 10h")]
            public string Vis
            {
               get { return vis;  }
               set { vis = value; Invalidate(); }
            }

            [Category("Nav_Design"), Description("Affiche le nom de l\'élément")]
            public bool DisplayName
            {
               get { return displayName;  }
               set { displayName = value; Invalidate(); }
            }


            [Category("Nav_Design"), Description("Action associée au bouton")]
            public OBSoptions ObsFonction
            {
               get { return obsfonction;  }
               set { obsfonction = value; }
            }

            [Category("Nav_Design"), Description("Clic dans le centre actif")]
            public bool MouseInCenterActivated
            {
               get { return mouseInCenterActivated;  }
               set { mouseInCenterActivated = value; }
            }

            [Category("Nav_Design"), Description("Souris interactive")]
            public bool MouseEnabled
            {
               get { return mouseEnabled;  }
               set { mouseEnabled = value; }
            }

            [Category("Nav_Design"), Description("Couleur du marqueur")]
            public Color MarqueurColor
            {
               get { return marqueurColor;  }
               set { marqueurColor = value; Invalidate(); }
            }

            [Category("Nav_Design"), Description("Marqueur actif")]
            public bool MarqueurShow
            {
               get { return marqueurShow;  }
               set { marqueurShow = value; Invalidate(); }
            }

            [Category("Nav_Design"), Description("Diametre Exterieur")]
            public int RayonExt
            {
               get { return rayonExt;  }
               set { rayonExt = value; Invalidate(); }
            }

            [Category("Nav_Design"), Description("Diametre Interieur")]
            public int RayonInt
            {
               get { return rayonInt;  }
               set { rayonInt = value; Invalidate(); }
            }

            [Category("Nav_Design"), Description("Taille des lettres")]
            public int Letters_Size
            {
               get { return capsLetters_Size;  }
               set { capsLetters_Size = value; Invalidate(); }
            }

            [Category("Nav_Design"), Description("Taille des traits")]
            public int LineWidth
            {
               get { return lineWidth;  }
               set { lineWidth = value; Invalidate(); }
            }

            [Category("Nav_Design"), Description("Affiche le bouton de sélection de cap")]
            public bool ShowButton
            {
               get { return showButton;  }
               set { showButton = value; Invalidate(); }
            }

            [Category("Nav_Design"), Description("Caps extérieurs ou intérieurs")]
            public bool CapsIn
            {
               get { return capsIn;  }
               set { capsIn = value; Invalidate(); }
            }

            #endregion

            #region Events Implementation

            public delegate void ChangedParam_Delegate(); // Pour avoir des événements

            [Category("Nav_Events"), Description("Evénement à déclencher lors du changement d'un paramètre")]
            public virtual event ChangedParam_Delegate OnChanges;

            #endregion

            #region Init

            public Nav_RoseCaps() : base()
            {
                    // Cet appel est requis par le concepteur Windows.Forms.
                    InitializeComponent();
                           
                    // Activates double buffering
                    SetStyle(ControlStyles.UserPaint, true);
                    SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                    SetStyle(ControlStyles.DoubleBuffer, true);
                    SetStyle(ControlStyles.ResizeRedraw, true);
                    SetStyle(ControlStyles.SupportsTransparentBackColor,true);
                    Init_RoseCaps();
            }

            public void Init_RoseCaps()
            {
               // Paramètres par défaut
               centerPoint =  new Point(this.Width/2,this.Height/2);
               Letters_Size = 12;
               Heading1 = 0;
               rayonExt = 68;
               rayonInt = 50;
               LineWidth = 2;
               HeadingRect = new Rectangle(Heading_BtPos.X, Heading_BtPos.Y ,Heading_BtSize,Heading_BtSize);
            }

            private void Nav_RoseCaps_Resize(object sender, System.EventArgs e)
            {
               centerPoint =  new Point(this.Width/2,this.Height/2);
            }

            /// <summary>
            /// Nettoyage des ressources utilisées.
            /// </summary>
            protected override void Dispose(bool disposing)
            {
                    if (disposing)
                    {
                            if (components != null)
                                    components.Dispose();
                    }
                    base.Dispose(disposing);
            }

            #region Code généré par le concepteur de composant
            /// <summary>
            /// Méthode requise pour la gestion du concepteur  - ne pas modifier 
            /// le contenu de cette méthode avec l'éditeur de code.
            /// </summary>
            private void InitializeComponent()
                {
                        this.components = new System.ComponentModel.Container();
                        this.timer_Cap = new System.Windows.Forms.Timer(this.components);
                        // 
                        // timer_Cap
                        // 
                        this.timer_Cap.Interval = 300;
                        this.timer_Cap.Tick += new System.EventHandler(this.timer_Cap_Tick);
                        // 
                        // Nav_RoseCaps
                        // 
                        this.BackColor = System.Drawing.SystemColors.Control;
                        this.Name = "Nav_RoseCaps";
                        this.Size = new System.Drawing.Size(182, 181);
                        this.Resize += new System.EventHandler(this.Nav_RoseCaps_Resize);
                        this.Load += new System.EventHandler(this.Nav_RoseCaps_Load);
                        this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Nav_RoseCaps_MouseUp);
                        this.Paint += new System.Windows.Forms.PaintEventHandler(this.Nav_RoseCaps_Paint);
                        this.DoubleClick += new System.EventHandler(this.Nav_RoseCaps_DoubleClick);
                        this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Nav_RoseCaps_MouseMove);
                        this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Nav_RoseCaps_MouseDown);
                }
            #endregion

            #endregion

            #region Paint
            private void Nav_RoseCaps_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
            {
                // Trace une rose des caps

                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Fond
                Rectangle rect = this.ClientRectangle;
                SolidBrush sb = new SolidBrush(Color.FromArgb(66,67,75)); // Fond gris
                g.FillRectangle(sb,rect);

                // Fond3D
                LinearGradientBrush b = new LinearGradientBrush(rect, Color.Black,Color.Ivory, LinearGradientMode.ForwardDiagonal);
                g.FillEllipse(b, centerPoint.X-rayonExt-9, centerPoint.Y-rayonExt-9, rayonExt*2+16, rayonExt*2+16);
                // Fond du compas
                g.FillEllipse(sb, centerPoint.X-rayonExt-5, centerPoint.Y-rayonExt-5, rayonExt*2+10, rayonExt*2+10);

                g.DrawEllipse(new Pen(Color.Black,3), centerPoint.X-rayonExt-5, centerPoint.Y-rayonExt-5, rayonExt*2+10, rayonExt*2+10);

                // Verrière
                Color cc = Color.FromArgb(208,206,255);
                b = new LinearGradientBrush(rect, Color.Black, cc, LinearGradientMode.ForwardDiagonal);
                b.SetBlendTriangularShape(0.55f, 0.40F);
                g.FillEllipse(b, centerPoint.X-rayonExt-5, centerPoint.Y-rayonExt-5, rayonExt*2+10, rayonExt*2+10);
                b.Dispose();
               
                 // Vis
                SolidBrush br = new SolidBrush(Color.Gray);

                if (0<vis.Length)
                {
                    Pen pb = new Pen(Color.Black, 2);
                    if ( 0<=vis.IndexOf("1") )
                    {
                     g.DrawEllipse(Pens.White, 3,6, 9,9);
                     g.FillEllipse(br, 2,5, 9,9);
                     g.DrawEllipse(Pens.Black, 2,5, 9,9);
                     g.DrawLine(pb, 10,8,3,12);
                    }

                    if ( 0<=vis.IndexOf("2") )
                    {
                     int w = Width-14;

                     g.DrawEllipse(Pens.White, w+1,6, 9,9);
                     g.FillEllipse(br, w,5, 9,9);
                     g.DrawEllipse(Pens.Black, w,5, 9,9);
                     g.DrawLine(pb, w+2,7,w+7,12);
                    }

                    if ( 0<=vis.IndexOf("3") )
                    {
                     int w = Width-14;
                     int h = Height-14;

                     g.DrawEllipse(Pens.White, w+1,h+1, 9,9);
                     g.FillEllipse(br, w,h, 9,9);
                     g.DrawEllipse(Pens.Black, w,h, 9,9);
                     g.DrawLine(pb, w+2,h+2,w+7,h+7);
                    }

                    if ( 0<=vis.IndexOf("4") )
                    {
                     int h = Height-14;

                     g.DrawEllipse(Pens.White, 3,h+1, 9,9);
                     g.FillEllipse(br, 2,h, 9,9);
                     g.DrawEllipse(Pens.Black, 2,h, 9,9);
                     g.DrawLine(pb, 4,h+2,9,h+7);
                    }
                }

                // Stylos et Fonte
                Pen styloCaps = new Pen(Color.White, LineWidth);

                FontFamily family = new FontFamily("Arial");
                int fontStyle = (int)FontStyle.Bold;
                StringFormat format = new StringFormat();
                format.Alignment=StringAlignment.Center;

                int capToDraw;
                string strCap;
                string []caps = {"N","W","S","E"};
                int CapsInOut = (capsIn?-20:0);
                GraphicsPath gp = new GraphicsPath();
                Rectangle MyRect;

                // Parcourir les angles à tracer
                for (int angle = Heading1; angle <= 350+Heading1; angle += 10)
                {
                    g.ResetTransform();
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TranslateTransform(centerPoint.X, centerPoint.Y);
                    g.RotateTransform(360-angle);
                    if ((angle-Heading1)%30==0)
                      g.DrawLine(styloCaps, 0, -rayonInt-2*(capsIn?0:1)+CapsInOut, 0,11-rayonInt+CapsInOut); // tirets longs de 30° en 30°
                    else if ((angle-Heading1)%10==0)
                      g.DrawLine(styloCaps, 0, -rayonInt+CapsInOut, 0,11-rayonInt+CapsInOut); // tirets moyens de 10° en 10°

                    if ((angle-Heading1)%30==0)
                    {
                      // Ecrire le cap tous les 30°
                      if ((-angle+Heading1)%90==0)
                      {
                        strCap = caps[(angle-Heading1)/90]; //  Cap multiple de 90°: Ecrire la lettre NWSE

                         MyRect = new Rectangle( new Point(-20, -RayonExt-3*(capsIn?3:1)-CapsInOut), new Size(40,RayonExt+12) );
                         gp.Reset();
                         gp.AddString(strCap, family, fontStyle, Letters_Size, MyRect, format);
                         g.FillPath(Brushes.Ivory, gp);
                       //g.DrawString(strCap,capsFont,Brushes.Ivory,0, -RayonExt-3*(capsIn?3:1)-CapsInOut, format);
                      }
                      else
                      {
                         // Cap multiple de 30° en chiffres
                         capToDraw = (int)Math.Round((360-angle+(double)Heading1)/10,0);
                         capToDraw = validHeading(capToDraw);
                         strCap = capToDraw.ToString();

                         MyRect = new Rectangle( new Point(-20, -RayonExt-1*(capsIn?10:1)-CapsInOut), new Size(40,RayonExt+12) );
                         gp.Reset();
                         gp.AddString(strCap, family, fontStyle, Letters_Size-2, MyRect, format);
                         g.FillPath(Brushes.Ivory, gp);
                       // g.DrawString(strCap, anglesFont, Brushes.Ivory, 0, -RayonExt-1*(capsIn?10:1)-CapsInOut, format);
                      }
                    }

                    g.RotateTransform(-5);  // tirets courts de 5° en 5°
                    g.DrawLine(styloCaps, 0,-rayonInt+CapsInOut,0,6-rayonInt+CapsInOut);
                }

                if (ShowButton)
                {
                    // Bouton à tourner
                    g.ResetTransform();
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TranslateTransform( Heading_BtPos.X, Heading_BtPos.Y);

                    SolidBrush  bfr = new SolidBrush(Color.White);
                    g.FillEllipse(bfr, 1,1, Heading_BtSize-1, Heading_BtSize-1);
                    bfr = new SolidBrush(Color.Black);
                    g.FillEllipse(bfr, 0,0, Heading_BtSize, Heading_BtSize);

                    Pen p = new Pen(Color.Gray, 1);
                    g.DrawEllipse(p, 2,2, Heading_BtSize-6, Heading_BtSize-6);

                    p = new Pen(Color.Gray, 2);

                    g.TranslateTransform( (Heading_BtSize-1)/2-1, (Heading_BtSize/2)-1);
                    g.RotateTransform( (float)Heading_BtAngle);

                    p.DashStyle = DashStyle.Dot;
                    g.DrawEllipse(p, -(Heading_BtSize-1)/2+1, -(Heading_BtSize-1)/2+1, Heading_BtSize-3, Heading_BtSize-3);

                    // Texte dans le bouton
                    //anglesFont = new Font("arial black", 5, FontStyle.Regular);
                    //g.DrawString("OBS", anglesFont, Brushes.Yellow, 1,-4, format);
                }


                // Marqueur de couleur (pour faire correspondre le contrôle à une balise)
                // Marqueur de couleur (pour faire correspondre le contrôle à une balise)

                if (marqueurShow)
                {
                  g.ResetTransform();
                  g.SmoothingMode = SmoothingMode.AntiAlias;
                  Point ct = new Point(HeadingRect.Left-10,HeadingRect.Top-2);
                  rect = new Rectangle(39,9,7,7);
                  LinearGradientBrush brm = new LinearGradientBrush(rect, Color.Black,Color.Ivory, LinearGradientMode.ForwardDiagonal);
                  g.FillEllipse(brm, ct.X,ct.Y, 7,7);
                  // Led du marqueur
                  SolidBrush bf = new SolidBrush(marqueurColor);
                  g.FillEllipse(bf, ct.X+1,ct.Y+1, 5,5);
                }
            }
            #endregion Paint

            #region MOUSE

            private void Nav_RoseCaps_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
            {
               clickedPt.X = e.X;
               clickedPt.Y = e.Y;

                if ( HeadingRect.Contains(clickedPt) )
                {
                   timer_Cap.Enabled = false;
                   timer_Cap.Interval= Attente_MouseDown;
                   Cap_MouseDown_Delta = 1*(clickedPt.X<Heading_BtPos.X+Heading_BtSize/2?-1:1);
                   timer_Cap.Enabled   = true;
               }
            }

            // ---------------------------------------------------------------------------

            private void Nav_RoseCaps_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
            {
              timer_Cap.Interval = Attente_MouseDown;
              timer_Cap.Enabled = false;
            }

            // ---------------------------------------------------------------------------

            private void Nav_RoseCaps_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
            {
                  // rotation des caps avec la souris
                  if (e.Button != MouseButtons.Left || !mouseEnabled) return;
                  Point pc = new Point(e.X,e.Y);

                  if (isInCircle(pc, (mouseInCenterActivated?1:RayonInt), rayonExt) )
                  {
                      double dx = pc.X - centerPoint.X;
                      double dy = pc.Y - centerPoint.Y;
                      double angle;
                      try
                      {
                        angle = RadiansToDegres( Math.Atan2(dx,dy) );
                        angle +=180;
                        if (360<=angle) angle -=360;
                      }
                      catch (DivideByZeroException exc)
                      {
                        angle = 0;
                      }

                      Cap_MouseDown_Delta = Math.Sign(angle - angle_Precedent);
                      angle_Precedent = angle;
                      addToCap( Cap_MouseDown_Delta );
                  }
            }

            private void Nav_RoseCaps_DoubleClick(object sender, System.EventArgs e)
            {
                 if (! isInCircle(clickedPt, 1, rayonExt) || !mouseEnabled)
                     return;

                  double dx = clickedPt.X - centerPoint.X;
                  double dy = -clickedPt.Y + centerPoint.Y;
                  double angle;
                  try
                  {
                    angle = Math.Atan2(dx,dy);
                  }
                  catch (DivideByZeroException o)
                  {
                    if (dy<0) angle = 3.0*Math.PI/2;
                    else      angle = Math.PI/2;
                  }
                  angle *= (180.0/Math.PI);
                  if (angle<0) angle += 360;
                  angle += Heading1;
                  if (360<=angle) angle -= 360;
                  addToCap( -Heading1+(int)angle );
            }

            #endregion

            #region Change Cap

            public void addToCap(int deltaCap)
            {
              setCap( Heading1 + deltaCap );
            }

            public void setCap(int newcap)
            {
              Heading1 = validHeading( newcap );
              if (ParametersChanged != null)
                ParametersChanged();
            }

            private void timer_Cap_Tick(object sender, System.EventArgs e)
            {
                // Timer gérant l'affichage quand on appui sur le bouton +/-
                Heading_BtAngle+=Cap_MouseDown_Delta;
                switch (obsfonction)
                {
                 case OBSoptions.compas:
                         addToCap( Cap_MouseDown_Delta );
                         break;
                 case OBSoptions.radiale1:
                         Heading2 = validHeading(Heading2+Cap_MouseDown_Delta);
                         if (ParametersChanged != null)
                             ParametersChanged();
                         break;
                 case OBSoptions.radiale2:
                         Heading3 = validHeading(Heading3 + Cap_MouseDown_Delta);
                         if (ParametersChanged != null)
                             ParametersChanged();
                         break;
                }

                if ( 30<=timer_Cap.Interval )
                  timer_Cap.Interval-=2;
            }

            #endregion

 	}
}

