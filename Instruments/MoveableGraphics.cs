using Instruments.Navigation; // Nav_Basic

namespace ng.graphiques
{
    #region ngPanel
    public class ngPanel: Panel
    {
        // Composant Panel avec un antiflickering
        private System.ComponentModel.Container components = null;

        public ngPanel() : base()
        {
            InitializeComponent();
            // Reduce flicker
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
        }

        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                    if( components != null )
                            components.Dispose();
            }
            base.Dispose( disposing );
        }

        private void InitializeComponent()
        {

        }
    };
    #endregion

    #region MoveableGraphic

    public class MoveableGraphic
    {
          public Color couleur;    // Nom de la couleur Ex: Red
          public int angle;         // Angle quelconque (cap avion, radiale de balise...)
          public Rectangle area;    // Zone du dessin
                                    // Coin supérieur gauche = centre du dessin

          public Nav_Basic source; //
          public bool visible = true;

          public static MoveableGraphic selectedElement; // Element à déplacer (1 seul à la fois)

          // ---------------------------------------------------------------------------
          public MoveableGraphic()
          {
              area = new Rectangle(0,0,20,20);
              source = null;
              couleur = Color.Yellow;  // par défaut
          }

          // ---------------------------------------------------------------------------

          public MoveableGraphic(Point p, Size s, string couleurGraphic, Nav_Basic src)
          {
            area = new Rectangle(p,s);
            couleur = Color.FromName(couleurGraphic);
            source = src;
          }

          // ---------------------------------------------------------------------------

          public bool isSelected(Point clickedPt)
          { return area.Contains(clickedPt); }

          // ---------------------------------------------------------------------------

          public void move(Point clickedPt)
          {
            // Détermine le coin superieur gauche pour que le "clickedPt" soit au centre du dessin
            area.X = clickedPt.X - area.Width/2;
            area.Y = clickedPt.Y - area.Height/2;
            if (OnChanged != null)
               OnChanged();
          }

          // ---------------------------------------------------------------------------

          public void setAngle(int newAngle)
          {
            angle = newAngle;
            if (OnChanged != null)
               OnChanged();
          }

          // ---------------------------------------------------------------------------

          public Point getCenter()
          {
           return centerOfObject(area);
          }

          public static Point centerOfObject(Rectangle r)
          {
              return new Point( r.X + r.Width/2 + (r.Width%2),
                                r.Y + r.Height/2+ (r.Height%2) );
          }

          // ---------------------------------------------------------------------------
          // Evenements

          public delegate void ChangeEventtDlg();
          public ChangeEventtDlg OnChanged;

          // ---------------------------------------------------------------------------
          // Fonctions
          public delegate void Dessiner(Graphics g, MoveableGraphic_ForNav obj, string datas);
          public Dessiner Draw;
    };
    #endregion

    #region MoveableGraphic_ForNav

    public class MoveableGraphic_ForNav: MoveableGraphic
    {
       public bool radialsShow;

       public MoveableGraphic_ForNav(Point p, Size s, string couleur, Nav_Basic src):base(p, s, couleur, src)
       {
         radialsShow = false;
       }

       public MoveableGraphic_ForNav(Point p, Size s, string couleur, Nav_Basic src, bool radialsshow):base(p, s, couleur, src)
       {
         radialsShow = radialsshow;
       }
    };
    #endregion
}






