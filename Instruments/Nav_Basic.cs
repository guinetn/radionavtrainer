using ng.graphiques;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace Instruments.Navigation
{
    public enum OBSoptions : int {compas, radiale1, radiale2};
  
  public class Nav_Basic : System.Windows.Forms.UserControl
  {
        // Propriétés
        public int Heading1 = 0;                   // Cap courant (en haut au centre)
        public int Heading2 = 0;                   // Autre cap (ex: pour rmi)
        public int Heading3 = 0;                   // Autre cap (ex: pour rmi)
        public bool mouseEnabled;                  // Souris on/off
        public MoveableGraphic_ForNav emetteur;    // Position de la source du signal
        public MoveableGraphic_ForNav avion;       // Position de l'avion

        public OBSoptions obsfonction;

        // Pour dessiner le contrôle
        protected PointF centerPoint;   // Pt central du contrôle
        protected int rayonExt;         // Rayon extérieur
        protected int rayonInt;         // Rayon intérieur
        public int capsLetters_Size;    // Taille des lettres

        // Gestion des clics
        public Point clickedPt;                 

        #region Init
        // ---------------------------------------------------------------------------

        public Nav_Basic():base()
        {
          emetteur = null;
          avion = null;
        }

        public void Init()
        {
          Heading1 = Heading2 = Heading3 = 0;
        }

        public void attachBalise(ref MoveableGraphic_ForNav em)
        {
          emetteur = em;
        }

        public void attachAvion(ref MoveableGraphic_ForNav plane)
        {
          avion = plane;
        }

        private void Nav_Basic_Resize(object sender, System.EventArgs e)
        {
          centerPoint =  new Point(this.Width/2,this.Height/2);
          Invalidate();
        }

        // ---------------------------------------------------------------------------

        private void InitializeComponent()
        {
                // 
                // Nav_Basic
                // 
                this.Name = "Nav_Basic";
                this.Resize += new System.EventHandler(this.Nav_Basic_Resize);
        }
        #endregion

        #region FONCTIONS

        // ---------------------------------------------------------------------------

        public Point getRectCenter(Rectangle r)
        {
              return new Point( r.X + r.Width/2 + (r.Width%2),
                                r.Y + r.Height/2+ (r.Height%2) );
        }

        public bool isInCircle(Point pointToTest, int rayonInt, int rayonExt)
        {
          double distance = getDistanceFromCenter(pointToTest);
          return ( rayonInt <= distance &&  distance <= rayonExt ? true:false);
        }

        // ---------------------------------------------------------------------------

        public static Point getVector(Point p1, Point p2)
        {
          // Renvoi le vecteur entre 2 points
          return new Point( p2.X-p1.X , p2.Y-p1.Y);
        }

        // ---------------------------------------------------------------------------

        public double getDistanceFromCenter(Point p)
        {
          return getDistance( new Point((int)centerPoint.X - p.X, (int)centerPoint.Y - p.Y) );
        }
                                  
        // ---------------------------------------------------------------------------

        public static double getDistance(Point p)
        {
          // Pythagore
          return Math.Sqrt( Math.Pow(p.X,2) + Math.Pow(p.Y,2) );
        }

        // ---------------------------------------------------------------------------

        public static double distance(Point p1, Point p2)
        {
          // Pythagore
          return getDistance( new Point(p2.X-p1.X, p2.Y-p1.Y) );
        }

        // ---------------------------------------------------------------------------

        public static double getAngle(int radiale, Point posemetteur, Point posavion)
        {
          // Trouve l'angle entre le point emetteur par lequel passe la radiale "radiale" et le point "avion"

          Point p = getVector(posemetteur, posavion);

           Matrix m = new Matrix(1,0,0,-1, 0,0); // Symétrie d'axe ox (oy vers le haut)
           m.Rotate(-radiale);
           Point []Pts = {p};
           m.TransformPoints(Pts);

           p = Pts[0];
           double angleAvionRadiale = Math.Atan2(p.X, p.Y); // inverse X et Y!
           return RadiansToDegres( angleAvionRadiale );
        }

        // ---------------------------------------------------------------------------

        public static double DegresToRadians(double deg)
        {
          return deg*Math.PI/180.0;
        }

        // ---------------------------------------------------------------------------

        public static double RadiansToDegres(double rad)
        {
          return rad*180.0/Math.PI;
        }

        // ---------------------------------------------------------------------------

        public static int validHeading(int newCap)
        {
           if (newCap < 0)
               return 360 + newCap;
           else if (360 <= newCap )
               return newCap-360;
          return newCap;
        }

        // ---------------------------------------------------------------------------
        #endregion

        #region GRAPHIQUES PREDEFINIS

        public static GraphicsPath getArrowGraphic(int longueur)
        {
           // Renvoi les points pour dessiner un triangle
           GraphicsPath Arrow = new GraphicsPath(); // Partie gauche de l'avion seulement
           if (0<longueur)
               Arrow.AddRectangle( new Rectangle(-2,0,4,longueur) );
           Point [] Pointe = { new Point(-6, 0) ,     // x  .           x
                               new Point( 6, 0) ,     // x  .  x
                               new Point( 0, -7),     //             x  .  x
                               new Point(-6, 0)    };
           Arrow.AddLines( Pointe );
           return Arrow;
        }                     

        // ---------------------------------------------------------------------------

        public static GraphicsPath getContourAvionGraphic()
        {
           GraphicsPath avion = new GraphicsPath(); // Partie gauche de l'avion seulement
           Point [] Contour_avion1G = { new Point(0,0) , new Point(-4,7) , new Point(-6,16) , new Point(-7,28) };
           Point [] Contour_avion2G = { new Point(-33,49), new Point(-33,58), new Point(-7,49), new Point(-7,60),
                                       new Point(-16,69), new Point(-16,79), new Point(-3,69), new Point(0,73) };
           avion.AddBeziers( Contour_avion1G );
           avion.AddLines(  Contour_avion2G );

           Point [] Contour_avion2D = { new Point(0,73), new Point(3,69), new Point(16,79),
                             new Point(16,69), new Point(7,60), new Point(7,49),
                             new Point(33,58), new Point(33,49) };
           avion.AddLines(  Contour_avion2D );
           Point [] Contour_avion1D = { new Point(7,28), new Point(6,16), new Point(4,7), new Point(0,0) };
           avion.AddBeziers( Contour_avion1D );

           Matrix m = new Matrix(1,0,0,1, 33,0);
           avion.Transform(m);
           return avion;

           /*
           // ou par symétrie d'axe ox à partir de la partie gauche, on obtient la partie droite par:
           Matrix m = new Matrix(-1,0,0,1, 0,0); // Symétrie d'axe oy
           m.TransformPoints(Contour_avion2);
           Array.Reverse( Contour_avion2);
           avion.AddLines(  Contour_avion2 );

           m.TransformPoints(Contour_avion1);
           Array.Reverse( Contour_avion1);
           avion.AddBeziers( Contour_avion1 );
           */
        }
        #endregion

        #region Trace Radiale

        public static void Radiale_Draw(Graphics g, Pen p, Point src, double angle, int longueur)
        {
                 angle-=90.0;
                 Point fin = new Point(  src.X + (int)(longueur*Math.Cos( DegresToRadians(angle))),
                                         src.Y + (int)(longueur*Math.Sin( DegresToRadians(angle))) );
                 g.DrawLine(p, src, fin);
        }

        #endregion

        #region Trace Cadre DME

        public static void traceDME(Graphics g, Point origin, double distance)
        {
                g.ResetTransform();
                Rectangle r = new Rectangle(origin.X, origin.Y, 32,14);

                // 3D
                g.DrawRectangle(Pens.Black, r);
                r.X+=1; r.Y+=1;
                r.Width -= 2;
                r.Height -= 2;
                g.DrawRectangle(Pens.White, r);
                g.FillRectangle(Brushes.Black,r);

                Font textefont = new Font("Arial", 9, FontStyle.Bold);
                StringFormat format = new StringFormat();
                format.Alignment=StringAlignment.Center;
                g.DrawString( string.Format("{0:#0.0}", distance) , textefont, new SolidBrush(Color.White), 31, origin.Y, format);

                // Verrière
                Color cc = Color.FromArgb(199,208,206,255);
                LinearGradientBrush b = new LinearGradientBrush(new Rectangle(-55,-55,40,10), Color.FromArgb(40, Color.Black), cc, LinearGradientMode.ForwardDiagonal);
                b.SetBlendTriangularShape(0.55f, 0.40F);
                g.FillRectangle(b, r);
                b.Dispose();
        }

        #endregion

        #region Trace Règle

        public static void traceRegle(Graphics g, Point origin, float angle, double longueur, double echellePixParNm)
        {
                // TRACE LA REGLE
                g.ResetTransform();
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TranslateTransform(origin.X, origin.Y);

                // Définir la règle
                FontFamily family = new FontFamily("Arial");
                int fontStyle = (int)FontStyle.Regular;
                StringFormat format = new StringFormat();
                format.Alignment=StringAlignment.Center;

                GraphicsPath gp_regle = new GraphicsPath();
                gp_regle.AddRectangle( new Rectangle(0,0,(int)longueur,18) );
                for (float r=0.0f; r<longueur; )//r+=(float)echellePixParNm) // pour que les unités
                {
                  for (int s=0; s<5; s++)
                  {
                    gp_regle.AddLine( new PointF(r,0.0f), new PointF(r,(r>0?5.0f:0.0f)) );
                    gp_regle.AddLine( new PointF(r,0.0f), new PointF(r,0.0f) );
                    r += (float) echellePixParNm/5.0f;
                    if(longueur<=r)
                      break;
                  }
                }

                // Tourner
                string numeros = " Nm";
                Matrix mirror = new Matrix(1,0,0,1, 0,0);
                for (float r=(float)echellePixParNm; r<longueur; r+=(float)echellePixParNm) // cette boucle doit-être indépendante de celle ci-dessus
                {
                  numeros = (r/echellePixParNm).ToString() + numeros;
                  gp_regle.AddString(numeros, family, fontStyle, 10, new Point((int)r-1,6), format);
                  numeros = "";
                }

                mirror.Rotate(angle);
                gp_regle.Transform(mirror);
                g.FillPath(new SolidBrush(Color.FromArgb(120,Color.Ivory)), gp_regle);
                g.DrawPath( Pens.DimGray, gp_regle);
        }

        #endregion

        #region Trace Ndb

        public static void Ndb_Draw(Graphics g, MoveableGraphic_ForNav Ndb, string datas) // img_name ne sert que pour tracer des images
        {
          // Trace une NDB
          if (! Ndb.visible) return;

          g.ResetTransform();
          g.SmoothingMode = SmoothingMode.AntiAlias;

          // Stylo
          Pen p = new Pen(Ndb.couleur, 2); // Datas contient le nom de la couleur
          p.DashStyle = DashStyle.Dot;

          int []valeurs = {1,4,8,12};

          Point center = Ndb.getCenter();

          for (int i=3; 0<=i; i--)
          {
             g.DrawEllipse(p, center.X-valeurs[i], center.Y-valeurs[i], valeurs[i]*2,valeurs[i]*2);
             if (i<3)
                 p.DashStyle = DashStyle.Solid;
          }

          // Rectangle de délimitation
          // g.DrawRectangle(Pens.Red,Ndb.area.Left,Ndb.area.Top, Ndb.area.Width-1, Ndb.area.Height-1);

          // Centre
          //g.DrawEllipse(Pens.Blue, center.X, center.Y, 1,1);
        }

        #endregion

        #region Trace Vor

        public static void Vor_Draw(Graphics g, MoveableGraphic_ForNav Vor, string datas)
        {
          // Trace un vor
          if (! Vor.visible) return;
          
          Point center = Vor.getCenter();
          Pen p = new Pen( Color.Ivory, 1);

          #region showradials
          if (Vor.radialsShow)
          {
                     g.ResetTransform();
                     g.SmoothingMode = SmoothingMode.AntiAlias;

                     int Heading = 0;
                     switch (Vor.source.obsfonction)
                     {
                      case OBSoptions.compas:
                              Heading = Vor.source.Heading1;
                              break;
                      case OBSoptions.radiale1:
                              Heading = Vor.source.Heading2;
                              break;
                      case OBSoptions.radiale2:
                              Heading = Vor.source.Heading3;
                              break;
                     }

                     p.DashStyle = DashStyle.Dash;
                     Radiale_Draw(g, p, center, Heading+10, 150);
                     Radiale_Draw(g, p, center, Heading-10, 150);

                     Radiale_Draw(g, p, center, Heading+180, 150);
                     Radiale_Draw(g, p, center, Heading+190, 150);
                     Radiale_Draw(g, p, center, Heading+170, 150);

                     p.Color = Vor.couleur;
                     Radiale_Draw(g, p, center, Heading, 150);

                     // Axe TO/FROM
                     p.Color = Color.Gray;
                     p.DashStyle = DashStyle.Dot;
                     Radiale_Draw(g, p, center, Heading+90, 150);
                     Radiale_Draw(g, p, center, Heading-90, 150);
          }
          #endregion

          g.ResetTransform();
          g.SmoothingMode = SmoothingMode.AntiAlias;

          // ---------------------------------------------------------------------------

          // Rectangle de délimitation
          g.DrawRectangle(new Pen( Vor.couleur, 1), Vor.area.Left+1, Vor.area.Top+1,
                             Vor.area.Width-2, Vor.area.Height-3);

          GraphicsPath gp_vor = new GraphicsPath();
          gp_vor.AddLine(0,0,10,0);
          g.TranslateTransform(center.X-5,center.Y-Vor.area.Height/2);

                // Tourner
                for (int rt=0;rt<6 ;rt++)
                {
                  Matrix mirror = new Matrix(1,0,0,1, 0,0);
                  mirror.RotateAt(60, new Point(5,8) );
                  gp_vor.Transform(mirror);
                  g.DrawPath( new Pen( Vor.couleur, 1), gp_vor);
                }
          // ---------------------------------------------------------------------------
         /*
         VORTACAN
          g.TranslateTransform(center.X-3,center.Y-Vor.area.Height/2);
          p = new Pen(Vor.couleur,1);

          g.DrawEllipse(p, 2,9, 2,2);
          Rectangle r = new Rectangle(0,0,6,3);
          SolidBrush br = new SolidBrush( Color.Black );
          return;

          for (int f=0; f<1 ;f++)    // 1ère fois = ombre, 2e = couleur du vor
          {
              g.FillRectangle(br,r);
              g.DrawLine(p, 0,3, -2,7); // Ymin = 3;
              g.DrawLine(p, 6,3, 8,7);  // Xmax =8

              g.TranslateTransform(13,12);
              g.RotateTransform(120);
              g.FillRectangle(br,r);
              g.DrawLine(p, 0,3, -2,7);  // Xmin = -2
              g.DrawLine(p, 6,3, 8,7);

              g.TranslateTransform(13,12);
              g.RotateTransform(119);
              g.FillRectangle(br,r);
              g.DrawLine(p, 0,3, -2,7); // Ymax = 7
              g.DrawLine(p, 6,3, 8,7);
              g.TranslateTransform(1,1);
              br.Color = Vor.couleur;
          }
          */
        }

        #endregion

        #region Trace Plane

        public static void Plane_Draw(Graphics g, MoveableGraphic_ForNav Plane, string datas)
        {
                // TRACE L'AVION
                if (! Plane.visible) return;
                g.ResetTransform();
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                GraphicsPath gp_plane = getContourAvionGraphic();
                Point center = Plane.getCenter();
                Matrix mirror = new Matrix(1,0,0,1, -Plane.area.Width/2, -Plane.area.Height/2); // centrer
                mirror.Scale(0.4f,0.4f);  // Réduire la taille
                gp_plane.Transform(mirror);

                // Tourner
                mirror = new Matrix(1,0,0,1, 0,0);
                mirror.RotateAt(Plane.angle, new Point(0,0) );

                GraphicsPath gpOld = new GraphicsPath();
                gpOld = (GraphicsPath)gp_plane.Clone();
                gp_plane.Transform(mirror);

                // Couleurs
                Color bodyColor = Color.Ivory;
                Color contourColor = Color.Black;

                g.ResetTransform();
                                                       
                // Rectangle de délimitation
               //  g.DrawRectangle(Pens.Red, Plane.area.Left, Plane.area.Top,  Plane.area.Width-1, Plane.area.Height-1);

                // Point au centre
               //  g.DrawEllipse(Pens.Red, center.X, center.Y, 1, 1);
                
                // Trace l'avion
                g.TranslateTransform(center.X, center.Y);
                g.FillPath( new SolidBrush(bodyColor) , gp_plane);
                g.DrawPath( new Pen(contourColor)     , gp_plane);
        }

        #endregion

        #region Trace Image !! LENT !!

        public static void Img_Draw(Graphics g, MoveableGraphic_ForNav Img, string img_name)
        {
          // Trace une image
          Point center = Img.area.Location;
          g.ResetTransform();

          // Rectangle de délimitation
          // g.DrawRectangle(Pens.Blue,Img.area.Left,Img.area.Top, Img.area.Width-1, Img.area.Height-1);

          g.TranslateTransform(center.X,center.Y);
                                                               
          // Obtenir l'image depuis la DLL:                  
          Assembly resassembly = Assembly.GetEntryAssembly();
          if (resassembly==null) return;
          string [] RessourcesNames =  resassembly.GetManifestResourceNames();  // noms de toutes les ressources
          string TheRessource = RessourcesNames[0];
          TheRessource = TheRessource.Replace(".resources","");
          System.Resources.ResourceManager resources = new System.Resources.ResourceManager( TheRessource, resassembly);
                                              
          // Tracer l'image
          Bitmap i = (Bitmap)resources.GetObject(img_name); // ex: img_name = "vor.bmp"
          i.MakeTransparent(Color.White);
          g.DrawImage(i, 0, 0, i.Width, i.Height);
        }                  
        #endregion

  }
}






