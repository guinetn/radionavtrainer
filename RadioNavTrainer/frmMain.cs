using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using ng.graphiques;
using Instruments.Navigation;
using System.Threading;

using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;

namespace RadioNavTrainer
{
    public partial class frmMain : Form
    {

        /// <summary>
        /// Variable requise par le concepteur.
        /// </summary>
        
        static ResourceManager rm = new ResourceManager("RadioNavTrainer.Resources.RadioNavTrainer", Assembly.GetExecutingAssembly());

        public CultureInfo cultInfo = new CultureInfo("");

        // Eléments déplaçables
        MoveableGraphic_ForNav Plane, Ndb1, Ndb2, Vor1, Vor2;
        private Rectangle Espace;  // Limites de déplacement des objets

        //    MoveableGraphic_ForNav Img;

        // Objets graphiques
        private Instruments.Navigation.Compas.Nav_Compas nav_Compas1;
        private Instruments.Navigation.VorHsi.Nav_VorHsi nav_VorHsi2;
        private Instruments.Navigation.VorHsi.Nav_VorHsi nav_VorHsi1;
        private Instruments.Navigation.AdfRmi.Nav_AdfRmi Nav_AdfRmi1;
        private Instruments.Navigation.AdfRmi.Nav_AdfRmi nav_AdfRmi2;

        private ng.graphiques.ngPanel panel_EspaceAerien;

        private System.Windows.Forms.Label labelInfos;
        private System.Windows.Forms.Label labelWind;
        private System.Windows.Forms.Label labelRule;
        private System.Windows.Forms.Label labelChrono;
        private System.Windows.Forms.Label labelTrace;
        private System.Windows.Forms.Label lblcmd_x;
        private System.Windows.Forms.Label lblcmd_p;
        private System.Windows.Forms.Label lblcmd_i;
        private System.Windows.Forms.Label lblcmd_espace;
        private System.Windows.Forms.Label lblcmd_droitgo;
        private System.Windows.Forms.Label lblcmd_obas;
        private System.Windows.Forms.Label lblcmd_v;
        private System.Windows.Forms.Label lblcmd_r;
        private System.Windows.Forms.Label lblcmd_t;
        private System.Windows.Forms.Label lblcmd_c;
        private System.Windows.Forms.Label lblcmd_e;
        private System.Windows.Forms.Label lblcmd_b;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label lblmove;
        private System.Windows.Forms.Label lblcmd_l;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label lblcmd_h;
        private System.Windows.Forms.Label lblcmd_clavier;
        private System.Windows.Forms.Label lblcmd_mouse;
        private System.Windows.Forms.TextBox label_AllowKeyPress;

        Color couleurVent = Color.Azure;

        // Animation:
        private bool traceon = false;

        private double TAS = 0;
        private double TASInNmSec = 0;
        private ArrayList traceSol;
        private double deltaX = 0, deltaY = 0; // pour le déplacement de l'avion

        private double EchellePixParNm;
        private double EchellePixParNmForTimer;

        private int turnRate = 0;        // Lacet en °/sec
        private double turnRate_temp = 0.0f;

        private int[] Speeds = { 0, 50, 100, 150, 200, 250, 300 };
        private int niveauVitesse = 0;   // 0 (Arrêt) à 7 (300 kT)
        private int niveauEchelle = 0;   // Echelle hauteur graphique = 2 à 20 Nm

        private int chronoState = 2; // 0=start 1=stop 2=reset
        private int chronoValue = 0;

        private bool paused = false; // touche [P]

        private bool rulesMode = false;
        private Point ruleStart = new Point(0, 0);
        private Point ruleEnd = new Point(0, 0);
        private double ruleLength = 0;

        // Vent
        public enum windStatus { noWind, settingWindMode, windDefinedWindMode };
        private windStatus windMode = windStatus.noWind; // 0 = not in win mode    1 = wind mode (define wind)   2 = no wind
        private Point windStart = new Point(0, 0);
        private Point windEnd = new Point(0, 0);
        private int windFrom = 0;
        private double windSpeed = 0;
        private double windSpeedInNmSec = 0;

        // Divers
        private System.Windows.Forms.Timer timer_Anim;
        private int timer_ms = 0;

        public enum vorStatus
        {
            displayNDB_VORWithRadiales = 0, displayNDB_VORWithoutRadiales,
            displayNDB, displayNothing,
            displayVORWithoutRadiales, displayVORWithRadiales
        };
        private vorStatus vorDisplay = vorStatus.displayNDB_VORWithRadiales;

        #region INIT

        public void setEspaceAerienLimits()
        {
            // utilisé par WinForm et ReSize()
            Espace = panel_EspaceAerien.Bounds;
            Espace.Width-=10;
            Espace.Height-=10;
        }

        private void WinForm_Resize(object sender, System.EventArgs e)
        {
            setEspaceAerienLimits();
            panel_EspaceAerien.Invalidate();
        }

        public frmMain()
        {
            InitializeComponent();
            setEspaceAerienLimits();

            traceSol = new ArrayList();

            Plane = new MoveableGraphic_ForNav(new Point(145, 150), new Size(27, 32), "", null);
            Plane.Draw = new MoveableGraphic_ForNav.Dessiner(Nav_Basic.Plane_Draw);
            Plane.OnChanged = new MoveableGraphic_ForNav.ChangeEventtDlg(SomethingHasChanged);

            Ndb1 = new MoveableGraphic_ForNav(new Point(50, 70), new Size(20, 20), "red", (Nav_Basic)Nav_AdfRmi1, true);
            Ndb1.Draw = new MoveableGraphic_ForNav.Dessiner(Nav_Basic.Ndb_Draw);
            Ndb1.OnChanged = new MoveableGraphic_ForNav.ChangeEventtDlg(SomethingHasChanged);
            Nav_AdfRmi1.attachBalise(ref Ndb1);
            Nav_AdfRmi1.attachAvion(ref Plane);

            Ndb2 = new MoveableGraphic_ForNav(new Point(80, 50), new Size(20, 20), "Gold", (Nav_Basic)nav_AdfRmi2, true);
            Ndb2.Draw = new MoveableGraphic_ForNav.Dessiner(Nav_Basic.Ndb_Draw);
            Ndb2.OnChanged = new MoveableGraphic_ForNav.ChangeEventtDlg(SomethingHasChanged);
            nav_AdfRmi2.attachBalise(ref Ndb2);
            nav_AdfRmi2.attachAvion(ref Plane);

            Vor1 = new MoveableGraphic_ForNav(new Point(100, 70), new Size(20, 19), "red", (Nav_Basic)nav_VorHsi1, ((int)vorDisplay<2 ? true : false));
            Vor1.Draw = new MoveableGraphic_ForNav.Dessiner(Nav_Basic.Vor_Draw);
            Vor1.OnChanged = new MoveableGraphic_ForNav.ChangeEventtDlg(SomethingHasChanged);
            nav_VorHsi1.attachBalise(ref Vor1);
            nav_VorHsi1.attachAvion(ref Plane);

            Vor2 = new MoveableGraphic_ForNav(new Point(690, 30), new Size(20, 19), "Gold", (Nav_Basic)nav_VorHsi2, ((int)vorDisplay<2 ? true : false));
            Vor2.Draw = new MoveableGraphic_ForNav.Dessiner(Nav_Basic.Vor_Draw);
            Vor2.OnChanged = new MoveableGraphic_ForNav.ChangeEventtDlg(SomethingHasChanged);
            nav_VorHsi2.attachBalise(ref Vor2);
            nav_VorHsi2.attachAvion(ref Plane);
            /*
               Img = new MoveableGraphic_ForNav( new Point(399,70) , new Size(20,19), "vor.bmp", null, true );
               Img.Draw = new MoveableGraphic_ForNav.Dessiner(Nav_Basic.Img_Draw);
               Img.OnChanged = new MoveableGraphic_ForNav.ChangeEventtDlg( SomethingHasChanged );
               */
        }

        // ---------------------------------------------------------------------------

        private void Init(bool examen)
        {
            Plane.area.Location = MoveableGraphic.centerOfObject(panel_EspaceAerien.Bounds);
            Plane.visible = true;

            if (examen)
            {
                Random rand = new Random();
                Ndb1.area.Location = new Point(rand.Next(Espace.Width), rand.Next(Espace.Height));
                Ndb2.area.Location = new Point(rand.Next(Espace.Width), rand.Next(Espace.Height));
                Vor1.area.Location = new Point(rand.Next(Espace.Width), rand.Next(Espace.Height));
                Vor2.area.Location = new Point(rand.Next(Espace.Width), rand.Next(Espace.Height));
            }
            else
            {
                Ndb1.area.Location = new Point(10, 10);
                Ndb2.area.Location = new Point(Espace.Width-100, 10);
                Vor1.area.Location = new Point(50, 30);
                Vor2.area.Location = new Point(Espace.Width-50, 30);
            }

            // radiales 0°
            nav_VorHsi1.Init();
            nav_VorHsi2.Init();

            // Stop avion
            Vitesse(-niveauVitesse);
            turnRate = 0;

            chronoState=0;
            chronoValue = 0;
            labelChrono.ForeColor= (chronoState<1 ? Color.Yellow : Color.Ivory);

            Ndb1.visible = !examen;
            Ndb2.visible = !examen;
            Vor1.visible = !examen;
            Vor2.visible = !examen;

            labelWind.ForeColor = Color.White;
            traceSol.Clear();
            nav_Compas1.setCap(0);
            niveauEchelle = 4;
            calculEchelle();
        }

        // ---------------------------------------------------------------------------

        private void calculEchelle()
        {
            EchellePixParNm = Espace.Height/niveauEchelle;
            EchellePixParNmForTimer = EchellePixParNm;
            EchellePixParNmForTimer /= (1000.0/timer_Anim.Interval);

            nav_VorHsi1.EchellePixParNm = EchellePixParNm;
            nav_VorHsi1.EchelleNM = niveauEchelle;

            nav_VorHsi2.EchellePixParNm = EchellePixParNm;
            nav_VorHsi2.EchelleNM = niveauEchelle;

            Nav_AdfRmi1.EchellePixParNm = EchellePixParNm;
            Nav_AdfRmi1.EchelleNM = niveauEchelle;
            nav_AdfRmi2.EchellePixParNm = EchellePixParNm;
            nav_AdfRmi2.EchelleNM = niveauEchelle;

            SomethingHasChanged();
        }

        // ---------------------------------------------------------------------------


        #region Code généré par le concepteur Windows Form
        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.nav_VorHsi2 = new Instruments.Navigation.VorHsi.Nav_VorHsi();
            this.nav_VorHsi1 = new Instruments.Navigation.VorHsi.Nav_VorHsi();
            this.Nav_AdfRmi1 = new Instruments.Navigation.AdfRmi.Nav_AdfRmi();
            this.nav_Compas1 = new Instruments.Navigation.Compas.Nav_Compas();
            this.panel_EspaceAerien = new ng.graphiques.ngPanel();
            this.nav_AdfRmi2 = new Instruments.Navigation.AdfRmi.Nav_AdfRmi();
            this.timer_Anim = new System.Windows.Forms.Timer(this.components);
            this.labelInfos = new System.Windows.Forms.Label();
            this.labelWind = new System.Windows.Forms.Label();
            this.labelRule = new System.Windows.Forms.Label();
            this.labelChrono = new System.Windows.Forms.Label();
            this.labelTrace = new System.Windows.Forms.Label();
            this.lblcmd_x = new System.Windows.Forms.Label();
            this.lblcmd_p = new System.Windows.Forms.Label();
            this.lblcmd_i = new System.Windows.Forms.Label();
            this.lblcmd_espace = new System.Windows.Forms.Label();
            this.lblcmd_droitgo = new System.Windows.Forms.Label();
            this.lblcmd_obas = new System.Windows.Forms.Label();
            this.lblcmd_v = new System.Windows.Forms.Label();
            this.lblcmd_r = new System.Windows.Forms.Label();
            this.lblcmd_t = new System.Windows.Forms.Label();
            this.lblcmd_c = new System.Windows.Forms.Label();
            this.lblcmd_e = new System.Windows.Forms.Label();
            this.lblcmd_b = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.lblmove = new System.Windows.Forms.Label();
            this.lblcmd_l = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.lblcmd_h = new System.Windows.Forms.Label();
            this.lblcmd_clavier = new System.Windows.Forms.Label();
            this.lblcmd_mouse = new System.Windows.Forms.Label();
            this.label_AllowKeyPress = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // nav_VorHsi2
            // 
            this.nav_VorHsi2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nav_VorHsi2.BackColor = System.Drawing.SystemColors.Control;
            this.nav_VorHsi2.CapsIn = true;
            this.nav_VorHsi2.DisplayName = true;
            this.nav_VorHsi2.Letters_Size = 16;
            this.nav_VorHsi2.LineWidth = 2;
            this.nav_VorHsi2.Location = new System.Drawing.Point(860, 131);
            this.nav_VorHsi2.MarqueurColor = System.Drawing.Color.Gold;
            this.nav_VorHsi2.MarqueurShow = true;
            this.nav_VorHsi2.ModeHSI = true;
            this.nav_VorHsi2.MouseEnabled = false;
            this.nav_VorHsi2.MouseInCenterActivated = false;
            this.nav_VorHsi2.Name = "nav_VorHsi2";
            this.nav_VorHsi2.ObsFonction = Instruments.Navigation.OBSoptions.radiale1;
            this.nav_VorHsi2.RayonExt = 68;
            this.nav_VorHsi2.RayonInt = 50;
            this.nav_VorHsi2.ShowButton = true;
            this.nav_VorHsi2.Size = new System.Drawing.Size(209, 209);
            this.nav_VorHsi2.TabIndex = 14;
            this.nav_VorHsi2.Vis = "23";
            this.nav_VorHsi2.OnChanges += new Instruments.Navigation.RoseCaps.Nav_RoseCaps.ChangedParam_Delegate(this.SomethingHasChanged);
            // 
            // nav_VorHsi1
            // 
            this.nav_VorHsi1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nav_VorHsi1.BackColor = System.Drawing.SystemColors.Control;
            this.nav_VorHsi1.CapsIn = true;
            this.nav_VorHsi1.DisplayName = true;
            this.nav_VorHsi1.Letters_Size = 16;
            this.nav_VorHsi1.LineWidth = 2;
            this.nav_VorHsi1.Location = new System.Drawing.Point(654, 131);
            this.nav_VorHsi1.MarqueurColor = System.Drawing.Color.Red;
            this.nav_VorHsi1.MarqueurShow = true;
            this.nav_VorHsi1.ModeHSI = false;
            this.nav_VorHsi1.MouseEnabled = true;
            this.nav_VorHsi1.MouseInCenterActivated = true;
            this.nav_VorHsi1.Name = "nav_VorHsi1";
            this.nav_VorHsi1.ObsFonction = Instruments.Navigation.OBSoptions.compas;
            this.nav_VorHsi1.RayonExt = 68;
            this.nav_VorHsi1.RayonInt = 50;
            this.nav_VorHsi1.ShowButton = true;
            this.nav_VorHsi1.Size = new System.Drawing.Size(209, 209);
            this.nav_VorHsi1.TabIndex = 13;
            this.nav_VorHsi1.Vis = "14";
            this.nav_VorHsi1.OnChanges += new Instruments.Navigation.RoseCaps.Nav_RoseCaps.ChangedParam_Delegate(this.SomethingHasChanged);
            // 
            // Nav_AdfRmi1
            // 
            this.Nav_AdfRmi1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Nav_AdfRmi1.BackColor = System.Drawing.SystemColors.Control;
            this.Nav_AdfRmi1.CapsIn = false;
            this.Nav_AdfRmi1.DisplayName = true;
            this.Nav_AdfRmi1.Letters_Size = 16;
            this.Nav_AdfRmi1.LineWidth = 2;
            this.Nav_AdfRmi1.Location = new System.Drawing.Point(222, 131);
            this.Nav_AdfRmi1.MarqueurColor = System.Drawing.Color.Red;
            this.Nav_AdfRmi1.MarqueurShow = true;
            this.Nav_AdfRmi1.ModeRmi = false;
            this.Nav_AdfRmi1.MouseEnabled = true;
            this.Nav_AdfRmi1.MouseInCenterActivated = true;
            this.Nav_AdfRmi1.Name = "Nav_AdfRmi1";
            this.Nav_AdfRmi1.ObsFonction = Instruments.Navigation.OBSoptions.compas;
            this.Nav_AdfRmi1.Plane3D = true;
            this.Nav_AdfRmi1.RayonExt = 68;
            this.Nav_AdfRmi1.RayonInt = 50;
            this.Nav_AdfRmi1.ShowButton = true;
            this.Nav_AdfRmi1.Size = new System.Drawing.Size(218, 209);
            this.Nav_AdfRmi1.TabIndex = 16;
            this.Nav_AdfRmi1.Vis = "";
            // 
            // nav_Compas1
            // 
            this.nav_Compas1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nav_Compas1.BackColor = System.Drawing.SystemColors.Control;
            this.nav_Compas1.CapsIn = false;
            this.nav_Compas1.DisplayName = true;
            this.nav_Compas1.Letters_Size = 16;
            this.nav_Compas1.LineWidth = 2;
            this.nav_Compas1.Location = new System.Drawing.Point(5, 131);
            this.nav_Compas1.MarqueurColor = System.Drawing.Color.Empty;
            this.nav_Compas1.MarqueurShow = false;
            this.nav_Compas1.MouseEnabled = true;
            this.nav_Compas1.MouseInCenterActivated = true;
            this.nav_Compas1.Name = "nav_Compas1";
            this.nav_Compas1.ObsFonction = Instruments.Navigation.OBSoptions.compas;
            this.nav_Compas1.Plane3D = true;
            this.nav_Compas1.RayonExt = 68;
            this.nav_Compas1.RayonInt = 50;
            this.nav_Compas1.ShowButton = true;
            this.nav_Compas1.Size = new System.Drawing.Size(218, 209);
            this.nav_Compas1.TabIndex = 17;
            this.nav_Compas1.Vis = "14";
            this.nav_Compas1.OnChanges += new Instruments.Navigation.RoseCaps.Nav_RoseCaps.ChangedParam_Delegate(this.nav_Compas1_OnChanges);
            // 
            // panel_EspaceAerien
            // 
            this.panel_EspaceAerien.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_EspaceAerien.BackColor = System.Drawing.Color.Silver;
            this.panel_EspaceAerien.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel_EspaceAerien.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_EspaceAerien.Location = new System.Drawing.Point(10, 11);
            this.panel_EspaceAerien.Name = "panel_EspaceAerien";
            this.panel_EspaceAerien.Size = new System.Drawing.Size(882, 88);
            this.panel_EspaceAerien.TabIndex = 0;
            this.panel_EspaceAerien.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_EspaceAerien_Paint);
            this.panel_EspaceAerien.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel_EspaceAerien_MouseDown);
            this.panel_EspaceAerien.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel_EspaceAerien_MouseMove);
            // 
            // nav_AdfRmi2
            // 
            this.nav_AdfRmi2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nav_AdfRmi2.BackColor = System.Drawing.SystemColors.Control;
            this.nav_AdfRmi2.CapsIn = false;
            this.nav_AdfRmi2.DisplayName = true;
            this.nav_AdfRmi2.Letters_Size = 16;
            this.nav_AdfRmi2.LineWidth = 2;
            this.nav_AdfRmi2.Location = new System.Drawing.Point(439, 131);
            this.nav_AdfRmi2.MarqueurColor = System.Drawing.Color.Gold;
            this.nav_AdfRmi2.MarqueurShow = true;
            this.nav_AdfRmi2.ModeRmi = true;
            this.nav_AdfRmi2.MouseEnabled = false;
            this.nav_AdfRmi2.MouseInCenterActivated = true;
            this.nav_AdfRmi2.Name = "nav_AdfRmi2";
            this.nav_AdfRmi2.ObsFonction = Instruments.Navigation.OBSoptions.compas;
            this.nav_AdfRmi2.Plane3D = true;
            this.nav_AdfRmi2.RayonExt = 68;
            this.nav_AdfRmi2.RayonInt = 50;
            this.nav_AdfRmi2.ShowButton = false;
            this.nav_AdfRmi2.Size = new System.Drawing.Size(219, 209);
            this.nav_AdfRmi2.TabIndex = 18;
            this.nav_AdfRmi2.Vis = "23";
            // 
            // timer_Anim
            // 
            this.timer_Anim.Interval = 250;
            this.timer_Anim.Tick += new System.EventHandler(this.timer_Anim_Tick);
            // 
            // labelInfos
            // 
            this.labelInfos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInfos.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelInfos.ForeColor = System.Drawing.Color.White;
            this.labelInfos.Location = new System.Drawing.Point(13, 104);
            this.labelInfos.Name = "labelInfos";
            this.labelInfos.Size = new System.Drawing.Size(423, 21);
            this.labelInfos.TabIndex = 28;
            this.labelInfos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelWind
            // 
            this.labelWind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelWind.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelWind.ForeColor = System.Drawing.Color.White;
            this.labelWind.Location = new System.Drawing.Point(439, 104);
            this.labelWind.Name = "labelWind";
            this.labelWind.Size = new System.Drawing.Size(328, 21);
            this.labelWind.TabIndex = 29;
            this.labelWind.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelRule
            // 
            this.labelRule.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelRule.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelRule.ForeColor = System.Drawing.Color.White;
            this.labelRule.Location = new System.Drawing.Point(858, 104);
            this.labelRule.Name = "labelRule";
            this.labelRule.Size = new System.Drawing.Size(127, 21);
            this.labelRule.TabIndex = 30;
            this.labelRule.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelChrono
            // 
            this.labelChrono.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelChrono.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelChrono.ForeColor = System.Drawing.Color.White;
            this.labelChrono.Location = new System.Drawing.Point(780, 104);
            this.labelChrono.Name = "labelChrono";
            this.labelChrono.Size = new System.Drawing.Size(68, 21);
            this.labelChrono.TabIndex = 32;
            this.labelChrono.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTrace
            // 
            this.labelTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTrace.Enabled = false;
            this.labelTrace.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelTrace.ForeColor = System.Drawing.Color.Yellow;
            this.labelTrace.Location = new System.Drawing.Point(996, 104);
            this.labelTrace.Name = "labelTrace";
            this.labelTrace.Size = new System.Drawing.Size(49, 21);
            this.labelTrace.TabIndex = 38;
            this.labelTrace.Text = "Trace";
            this.labelTrace.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblcmd_x
            // 
            this.lblcmd_x.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_x.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_x.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_x.ForeColor = System.Drawing.Color.White;
            this.lblcmd_x.Location = new System.Drawing.Point(6, 453);
            this.lblcmd_x.Name = "lblcmd_x";
            this.lblcmd_x.Size = new System.Drawing.Size(396, 19);
            this.lblcmd_x.TabIndex = 39;
            this.lblcmd_x.Text = "[X]   Exercice (trouvez les balises)";
            // 
            // lblcmd_p
            // 
            this.lblcmd_p.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_p.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_p.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_p.ForeColor = System.Drawing.Color.White;
            this.lblcmd_p.Location = new System.Drawing.Point(6, 417);
            this.lblcmd_p.Name = "lblcmd_p";
            this.lblcmd_p.Size = new System.Drawing.Size(396, 18);
            this.lblcmd_p.TabIndex = 40;
            this.lblcmd_p.Text = "[P]   Pause";
            // 
            // lblcmd_i
            // 
            this.lblcmd_i.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_i.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_i.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_i.ForeColor = System.Drawing.Color.White;
            this.lblcmd_i.Location = new System.Drawing.Point(6, 435);
            this.lblcmd_i.Name = "lblcmd_i";
            this.lblcmd_i.Size = new System.Drawing.Size(396, 18);
            this.lblcmd_i.TabIndex = 41;
            this.lblcmd_i.Text = "[I]    Initialiser";
            // 
            // lblcmd_espace
            // 
            this.lblcmd_espace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_espace.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_espace.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_espace.ForeColor = System.Drawing.Color.White;
            this.lblcmd_espace.Location = new System.Drawing.Point(6, 398);
            this.lblcmd_espace.Name = "lblcmd_espace";
            this.lblcmd_espace.Size = new System.Drawing.Size(396, 19);
            this.lblcmd_espace.TabIndex = 44;
            this.lblcmd_espace.Text = "[Espace]   Tout droit (inclinaison nulle)";
            // 
            // lblcmd_droitgo
            // 
            this.lblcmd_droitgo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_droitgo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_droitgo.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_droitgo.ForeColor = System.Drawing.Color.White;
            this.lblcmd_droitgo.Location = new System.Drawing.Point(6, 380);
            this.lblcmd_droitgo.Name = "lblcmd_droitgo";
            this.lblcmd_droitgo.Size = new System.Drawing.Size(396, 18);
            this.lblcmd_droitgo.TabIndex = 43;
            this.lblcmd_droitgo.Text = "[Gauche][Droite]   Augmente/réduit le taux de virage  (°/sec)";
            // 
            // lblcmd_obas
            // 
            this.lblcmd_obas.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_obas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_obas.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_obas.ForeColor = System.Drawing.Color.White;
            this.lblcmd_obas.Location = new System.Drawing.Point(6, 361);
            this.lblcmd_obas.Name = "lblcmd_obas";
            this.lblcmd_obas.Size = new System.Drawing.Size(396, 19);
            this.lblcmd_obas.TabIndex = 42;
            this.lblcmd_obas.Text = "[Haut][Bas]  Accélérer/Ralentir [0-300kT]";
            // 
            // lblcmd_v
            // 
            this.lblcmd_v.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_v.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_v.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_v.ForeColor = System.Drawing.Color.White;
            this.lblcmd_v.Location = new System.Drawing.Point(409, 398);
            this.lblcmd_v.Name = "lblcmd_v";
            this.lblcmd_v.Size = new System.Drawing.Size(275, 19);
            this.lblcmd_v.TabIndex = 50;
            this.lblcmd_v.Text = "[V]  Mode vent (définir,activer,off)";
            // 
            // lblcmd_r
            // 
            this.lblcmd_r.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_r.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_r.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_r.ForeColor = System.Drawing.Color.White;
            this.lblcmd_r.Location = new System.Drawing.Point(409, 380);
            this.lblcmd_r.Name = "lblcmd_r";
            this.lblcmd_r.Size = new System.Drawing.Size(275, 18);
            this.lblcmd_r.TabIndex = 49;
            this.lblcmd_r.Text = "[R]   Mode règle (mesure de distance)";
            // 
            // lblcmd_t
            // 
            this.lblcmd_t.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_t.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_t.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_t.ForeColor = System.Drawing.Color.White;
            this.lblcmd_t.Location = new System.Drawing.Point(409, 361);
            this.lblcmd_t.Name = "lblcmd_t";
            this.lblcmd_t.Size = new System.Drawing.Size(275, 19);
            this.lblcmd_t.TabIndex = 48;
            this.lblcmd_t.Text = "[T]  Tracer la trajectoire ";
            // 
            // lblcmd_c
            // 
            this.lblcmd_c.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_c.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_c.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_c.ForeColor = System.Drawing.Color.White;
            this.lblcmd_c.Location = new System.Drawing.Point(409, 435);
            this.lblcmd_c.Name = "lblcmd_c";
            this.lblcmd_c.Size = new System.Drawing.Size(275, 18);
            this.lblcmd_c.TabIndex = 47;
            this.lblcmd_c.Text = "[C]   Chronomètre (ready,off,reset) ";
            // 
            // lblcmd_e
            // 
            this.lblcmd_e.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_e.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_e.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_e.ForeColor = System.Drawing.Color.White;
            this.lblcmd_e.Location = new System.Drawing.Point(409, 417);
            this.lblcmd_e.Name = "lblcmd_e";
            this.lblcmd_e.Size = new System.Drawing.Size(275, 18);
            this.lblcmd_e.TabIndex = 46;
            this.lblcmd_e.Text = "[E]  Modifier l\'echelle ";
            // 
            // lblcmd_b
            // 
            this.lblcmd_b.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_b.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_b.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_b.ForeColor = System.Drawing.Color.White;
            this.lblcmd_b.Location = new System.Drawing.Point(409, 453);
            this.lblcmd_b.Name = "lblcmd_b";
            this.lblcmd_b.Size = new System.Drawing.Size(275, 19);
            this.lblcmd_b.TabIndex = 45;
            this.lblcmd_b.Text = "[B]  Affichage des balises/radiales";
            // 
            // label19
            // 
            this.label19.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label19.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label19.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label19.ForeColor = System.Drawing.Color.White;
            this.label19.Location = new System.Drawing.Point(694, 398);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(316, 19);
            this.label19.TabIndex = 56;
            // 
            // label20
            // 
            this.label20.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label20.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label20.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label20.ForeColor = System.Drawing.Color.White;
            this.label20.Location = new System.Drawing.Point(694, 380);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(316, 18);
            this.label20.TabIndex = 55;
            // 
            // lblmove
            // 
            this.lblmove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblmove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblmove.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblmove.ForeColor = System.Drawing.Color.White;
            this.lblmove.Location = new System.Drawing.Point(694, 361);
            this.lblmove.Name = "lblmove";
            this.lblmove.Size = new System.Drawing.Size(316, 19);
            this.lblmove.TabIndex = 54;
            this.lblmove.Text = "Déplacer les objets: cliquez, déplacez, relachez";
            // 
            // lblcmd_l
            // 
            this.lblcmd_l.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_l.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_l.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_l.ForeColor = System.Drawing.Color.White;
            this.lblcmd_l.Location = new System.Drawing.Point(694, 435);
            this.lblcmd_l.Name = "lblcmd_l";
            this.lblcmd_l.Size = new System.Drawing.Size(321, 18);
            this.lblcmd_l.TabIndex = 53;
            this.lblcmd_l.Text = "[L]  Langue (FR/US)";
            // 
            // label23
            // 
            this.label23.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label23.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label23.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label23.ForeColor = System.Drawing.Color.White;
            this.label23.Location = new System.Drawing.Point(694, 417);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(316, 18);
            this.label23.TabIndex = 52;
            // 
            // lblcmd_h
            // 
            this.lblcmd_h.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_h.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_h.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_h.ForeColor = System.Drawing.Color.White;
            this.lblcmd_h.Location = new System.Drawing.Point(694, 453);
            this.lblcmd_h.Name = "lblcmd_h";
            this.lblcmd_h.Size = new System.Drawing.Size(321, 19);
            this.lblcmd_h.TabIndex = 51;
            this.lblcmd_h.Text = "[H]  Help";
            // 
            // lblcmd_clavier
            // 
            this.lblcmd_clavier.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_clavier.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_clavier.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_clavier.ForeColor = System.Drawing.Color.Yellow;
            this.lblcmd_clavier.Location = new System.Drawing.Point(6, 341);
            this.lblcmd_clavier.Name = "lblcmd_clavier";
            this.lblcmd_clavier.Size = new System.Drawing.Size(161, 19);
            this.lblcmd_clavier.TabIndex = 57;
            this.lblcmd_clavier.Text = "Commandes clavier:";
            // 
            // lblcmd_mouse
            // 
            this.lblcmd_mouse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblcmd_mouse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblcmd_mouse.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblcmd_mouse.ForeColor = System.Drawing.Color.Yellow;
            this.lblcmd_mouse.Location = new System.Drawing.Point(694, 341);
            this.lblcmd_mouse.Name = "lblcmd_mouse";
            this.lblcmd_mouse.Size = new System.Drawing.Size(160, 19);
            this.lblcmd_mouse.TabIndex = 58;
            this.lblcmd_mouse.Text = "Commandes souris:";
            // 
            // label_AllowKeyPress
            // 
            this.label_AllowKeyPress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label_AllowKeyPress.Location = new System.Drawing.Point(756, 415);
            this.label_AllowKeyPress.Name = "label_AllowKeyPress";
            this.label_AllowKeyPress.Size = new System.Drawing.Size(127, 23);
            this.label_AllowKeyPress.TabIndex = 31;
            this.label_AllowKeyPress.Text = "label_AllowKeyPress";
            // 
            // frmMain
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(899, 473);
            this.Controls.Add(this.label_AllowKeyPress);
            this.Controls.Add(this.lblcmd_mouse);
            this.Controls.Add(this.lblcmd_clavier);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.lblmove);
            this.Controls.Add(this.lblcmd_l);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.lblcmd_h);
            this.Controls.Add(this.lblcmd_v);
            this.Controls.Add(this.lblcmd_r);
            this.Controls.Add(this.lblcmd_t);
            this.Controls.Add(this.lblcmd_c);
            this.Controls.Add(this.lblcmd_e);
            this.Controls.Add(this.lblcmd_b);
            this.Controls.Add(this.lblcmd_espace);
            this.Controls.Add(this.lblcmd_droitgo);
            this.Controls.Add(this.lblcmd_obas);
            this.Controls.Add(this.lblcmd_i);
            this.Controls.Add(this.lblcmd_p);
            this.Controls.Add(this.lblcmd_x);
            this.Controls.Add(this.labelTrace);
            this.Controls.Add(this.labelChrono);
            this.Controls.Add(this.labelRule);
            this.Controls.Add(this.labelWind);
            this.Controls.Add(this.labelInfos);
            this.Controls.Add(this.panel_EspaceAerien);
            this.Controls.Add(this.nav_Compas1);
            this.Controls.Add(this.Nav_AdfRmi1);
            this.Controls.Add(this.nav_AdfRmi2);
            this.Controls.Add(this.nav_VorHsi1);
            this.Controls.Add(this.nav_VorHsi2);
            this.DoubleBuffered = true;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(600, 492);
            this.Name = "frmMain";
            this.Text = "RadioNavTrainer   V1.0      nguinet.pro@gmail.com";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.frmMain_Closing);
            this.Load += new System.EventHandler(this.WinForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.WinForm_KeyDown);
            this.Resize += new System.EventHandler(this.WinForm_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        [STAThread]
        static void Main()
        {
            Application.Run(new frmMain());
        }

        private void WinForm_Load(object sender, System.EventArgs e)
        {
            try
            {
                Left   = (int)Application.UserAppDataRegistry.GetValue("Left", 0);
                Top    = (int)Application.UserAppDataRegistry.GetValue("Top", 0);
                Height = (int)Application.UserAppDataRegistry.GetValue("Height", 619);
                Width  = (int)Application.UserAppDataRegistry.GetValue("Width", 907);
                cultInfo = new CultureInfo((string)Application.UserAppDataRegistry.GetValue("Culture", ""));
            }
            catch
            {
            }

            Init(false);
            calculEchelle();

            label_AllowKeyPress.Left = this.Width+40; // Masquer l'intercepteur de KeyPress


            // Vent nul au centre
            Point p = MoveableGraphic.centerOfObject(panel_EspaceAerien.Bounds);
            windStart = p;
            windEnd = p;

            Update_Langue();
        }

        private void frmMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save Datas in Registry
            Application.UserAppDataRegistry.SetValue("Left", Left);
            Application.UserAppDataRegistry.SetValue("Top", Top);
            Application.UserAppDataRegistry.SetValue("Height", Height);
            Application.UserAppDataRegistry.SetValue("Width", Width);
            Application.UserAppDataRegistry.SetValue("Culture", cultInfo.Name);
        }


        #endregion

        // ---------------------------------------------------------------------------

        #region OnChanges

        private void nav_Compas1_OnChanges()
        {
            Plane.setAngle(nav_Compas1.Heading1);
        }

        // ---------------------------------------------------------------------------

        public void SomethingHasChanged()
        {
            nav_Compas1.SomethingHasChanged();
            Nav_AdfRmi1.SomethingHasChanged();
            nav_AdfRmi2.SomethingHasChanged();
            nav_VorHsi1.SomethingHasChanged();
            nav_VorHsi2.SomethingHasChanged();

            panel_EspaceAerien.Invalidate();
        }

        #endregion

        // ---------------------------------------------------------------------------

        #region PAINT

        private void panel_EspaceAerien_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Rectangle rz = this.ClientRectangle;
            LinearGradientBrush gbr = new LinearGradientBrush(rz, Color.Blue, Color.Ivory, LinearGradientMode.BackwardDiagonal);
            g.FillRectangle(gbr, rz);

            // Trace les objets                  
            if (Plane.Draw!=null)
                Plane.Draw(g, Plane, "");


            if (Ndb1.Draw!=null)
                Ndb1.Draw(g, Ndb1, "");
            if (Ndb2.Draw!=null)
                Ndb2.Draw(g, Ndb2, "");

            if (Vor1.Draw!=null)
                Vor1.Draw(g, Vor1, "");
            if (Vor2.Draw!=null)
                Vor2.Draw(g, Vor2, "");

            //if (Img.Draw!=null)  // TROP LENT !!
            //  Img.Draw(g, Img, "vor.bmp");   // l'image doit être dans le .res de la feuille

            // Dessine la trace au sol
            if (traceon)
            {
                g.ResetTransform();
                foreach (Point p in traceSol)
                    g.DrawEllipse(Pens.Black, p.X, p.Y, 1, 1);
            }


            // Trace le vent
            if (windMode == windStatus.settingWindMode || windMode==windStatus.windDefinedWindMode)
            {
                g.ResetTransform();
                Pen ventpen = new Pen(couleurVent, 3);
                ventpen.StartCap = LineCap.RoundAnchor;
                ventpen.EndCap   = LineCap.Triangle;
                g.DrawLine(ventpen, windStart, windEnd);

                // Flêche vent
                GraphicsPath gp_Arrow = Nav_Basic.getArrowGraphic(1);
                Rectangle r = Rectangle.Truncate(gp_Arrow.GetBounds());

                g.TranslateTransform(windEnd.X, windEnd.Y);
                g.RotateTransform(windFrom+180);
                g.TranslateTransform(0, +6 - (int)(r.Height/2.0f)); // +7 = hauteur de la pointe

                g.DrawPath(new Pen(couleurVent), gp_Arrow);
                g.FillPath(new SolidBrush(couleurVent), gp_Arrow);
            }

            // Règles de mesure
            if (rulesMode)
            {
                g.ResetTransform();
                double angle = Nav_Basic.getAngle(90, ruleStart, ruleEnd);
                double L = Nav_Basic.distance(ruleStart, ruleEnd);
                Nav_Basic.traceRegle(g, ruleStart, (float)angle, L, EchellePixParNm);
                /* Simple trait:
                Pen rulePen = new Pen(Color.Yellow,3);
                rulePen.StartCap = LineCap.RoundAnchor;
                rulePen.EndCap   = LineCap.Triangle;
                g.DrawLine(rulePen, ruleStart, ruleEnd);*/
            }

            // Echelle
            g.ResetTransform();
            Pen echPen = new Pen(Color.Ivory, 2);
            echPen.StartCap = LineCap.DiamondAnchor;
            echPen.EndCap   = LineCap.DiamondAnchor;
            echPen.Width = 1;
            g.DrawLine(echPen, panel_EspaceAerien.Width-125, panel_EspaceAerien.Height-8, panel_EspaceAerien.Width-10, panel_EspaceAerien.Height-8);

            double dist = 115.0f/EchellePixParNm;
            Font textefont = new Font("Arial", 9, FontStyle.Bold);
            StringFormat format = new StringFormat();
            format.Alignment=StringAlignment.Center;
            if (niveauEchelle==20) // Dernier niveau echelle
                g.DrawString(string.Format("MAX: {0:#0.0} Nm", dist), textefont, new SolidBrush(Color.Yellow), panel_EspaceAerien.Width-62, panel_EspaceAerien.Height-25, format);
            else
                g.DrawString(string.Format("{0:#0.0} Nm", dist), textefont, new SolidBrush(Color.Ivory), panel_EspaceAerien.Width-62, panel_EspaceAerien.Height-25, format);
            
            displayInfos();
        }
        #endregion

        #region MOUSE

        private void panel_EspaceAerien_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Sélectionne un objet          
            Point ptClicked = new Point(e.X, e.Y);
            MoveableGraphic_ForNav.selectedElement = null;

            if (windMode==windStatus.settingWindMode)
            {
                windStart = ptClicked;
                panel_EspaceAerien.Invalidate();
                return;
            }

            if (rulesMode)
            {
                ruleStart = ptClicked;
                return;
            }

            if (Plane.isSelected(ptClicked))
                MoveableGraphic_ForNav.selectedElement = Plane;
            else if (Ndb1.isSelected(ptClicked))
                MoveableGraphic_ForNav.selectedElement = Ndb1;
            else if (Ndb2.isSelected(ptClicked))
                MoveableGraphic_ForNav.selectedElement = Ndb2;
            else if (Vor1.isSelected(ptClicked))
                MoveableGraphic_ForNav.selectedElement = Vor1;
            else if (Vor2.isSelected(ptClicked))
                MoveableGraphic_ForNav.selectedElement = Vor2;
            //else if (Img.isSelected(ptClicked) )
            //  MoveableGraphic_ForNav.selectedElement = Img;
        }

        // ---------------------------------------------------------------------------

        private void panel_EspaceAerien_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Déplace un objet
            if (e.Button!= MouseButtons.Left)
                return;

            if (windMode==windStatus.settingWindMode)
            {
                windEnd = new Point(e.X, e.Y);

                double v = Nav_Basic.getAngle(0, windStart, windEnd);
                windFrom = Nav_Basic.validHeading((int)v-180);

                windSpeed = Nav_Basic.getDistance(Nav_Basic.getVector(windStart, windEnd));
                windSpeed /= EchellePixParNmForTimer;
                windSpeedInNmSec = windSpeed/3600;

                panel_EspaceAerien.Invalidate();
            }

            if (rulesMode)
            {
                ruleEnd = new Point(e.X, e.Y);
                ruleLength = Nav_Basic.distance(ruleStart, ruleEnd);
                panel_EspaceAerien.Invalidate();
            }

            if (MoveableGraphic_ForNav.selectedElement != null && Espace.Contains(e.X+10, e.Y+10))
                MoveableGraphic_ForNav.selectedElement.move(new Point(e.X, e.Y));  // Pt cliqué = centre du dessin
        }

        #endregion

        #region Animation

        // Timer pour déplacer l'avion

        private void timer_Anim_Tick(object sender, System.EventArgs e)
        {
            traceSol.Add(Plane.getCenter());

            int t = (int)1000.0/timer_Anim.Interval;

            turnRate_temp += (double)turnRate/t;
            int ent = Convert.ToInt32(turnRate_temp);
            if (1 <= Math.Abs(ent))
            {
                turnRate_temp -= (double)ent;
                nav_Compas1.addToCap(ent);     // Lacet
            }

            timer_ms++;
            if (t <= timer_ms)
            {
                timer_ms = 0;
                if (chronoState==0)
                    chronoValue++;
            }

            deltaX += EchellePixParNmForTimer*(double)TASInNmSec*Math.Sin(Nav_Basic.DegresToRadians(Plane.angle));
            // +ventX:
            deltaX -= EchellePixParNmForTimer*(double)windSpeedInNmSec*Math.Sin(Nav_Basic.DegresToRadians(windFrom));

            int entier = (int)deltaX;
            if (1<=Math.Abs(entier))
            {
                Plane.area.X += entier;
                deltaX -= entier;
            }

            deltaY += EchellePixParNmForTimer*(double)TASInNmSec*Math.Cos(Nav_Basic.DegresToRadians(Plane.angle));
            // + ventY:
            deltaY -= EchellePixParNmForTimer*(double)windSpeedInNmSec*Math.Cos(Nav_Basic.DegresToRadians(windFrom));

            entier = (int)deltaY;
            if (1<=Math.Abs(entier))
            {
                Plane.area.Y -= entier;
                deltaY -= entier;
            }

            // ---------------------------------------------------------------------------
            // EMPECHE LES SORTIES LIMITES ECRAN
            // ---------------------------------------------------------------------------

            // Limite latérales
            Point ppl = Plane.getCenter();

            if (ppl.X < 0)
                Plane.area.X = panel_EspaceAerien.Width - Plane.area.Width/2;
            else if (panel_EspaceAerien.Width < ppl.X)
                Plane.area.X = -Plane.area.Width/2;

            // Limites verticales
            if (ppl.Y < 0)
                Plane.area.Y = panel_EspaceAerien.Height - Plane.area.Height/2;
            else if (panel_EspaceAerien.Height < ppl.Y)
                Plane.area.Y = -Plane.area.Height/2;

            SomethingHasChanged();
        }

        // ---------------------------------------------------------------------------

        private void displayInfos()
        {
            // avion                                            
            string Infos = "";
            int capAvion = nav_Compas1.Heading1;

            // Effets du vent
            double crossWind = 0.0, headWind = 0.0;
            if (windSpeed==0)
                labelWind.Text = rm.GetString("nt_pasvent", cultInfo);
            else
            {
                labelWind.Text = rm.GetString("nt_vent", cultInfo) + ": " + windFrom.ToString() + "°/" + ((int)windSpeed).ToString() + " kT";

                // CrossWind et HeadWind
                getWindEffects(capAvion, windSpeed, windFrom, out crossWind, out headWind);
                labelWind.Text += "      " + String.Format(rm.GetString("nt_crosswind", cultInfo) + "={0:##0.0 kT}", crossWind);
                labelWind.Text += String.Format("  " + rm.GetString("nt_headwind", cultInfo) + "={0:##0.0 kT}", headWind);
            }

            // Infos de base
            Infos = rm.GetString("nt_cap", cultInfo) +"=" + capAvion.ToString();
            Infos += "°    " + rm.GetString("nt_vpropre", cultInfo) + "=" + TAS.ToString() + " kT";
            Infos += "    " + rm.GetString("nt_vsol", cultInfo) + "=" + String.Format("{0:##0.0}", TAS-headWind) + " kT";
            Infos += "  " + rm.GetString("nt_tauxvirage", cultInfo) + "=" + turnRate.ToString() + "°/sec";
            labelInfos.Text = Infos;


            // Traçage du vent
            if (windMode == windStatus.settingWindMode)
            {
                labelWind.BorderStyle = BorderStyle.FixedSingle;
                labelWind.BackColor = Color.Yellow;
                labelWind.ForeColor = Color.Red;
                if (windSpeed==0)
                    labelWind.Text = rm.GetString("nt_tracezvent", cultInfo); // "Tracez le vent";
            }
            else
            {
                labelWind.BorderStyle = BorderStyle.None;
                labelWind.BackColor = Color.Transparent;
                labelWind.ForeColor = Color.White;
            }

            if (windMode == windStatus.windDefinedWindMode)
                labelWind.ForeColor = (windMode==windStatus.windDefinedWindMode ? Color.Yellow : Color.White);


            // REGLE
            double dist = 0;
            if (rulesMode)
            {
                dist = ruleLength / EchellePixParNm;
                labelRule.BorderStyle = BorderStyle.FixedSingle;
                labelRule.BackColor = Color.Yellow;
                labelRule.ForeColor = Color.Red;
                labelRule.Text = rm.GetString("nt_mesure", cultInfo) + ":" + string.Format("{0:##0.00} Nm", dist);
            }
            else
            {
                labelRule.Text = "";
                labelRule.BackColor = Color.Transparent;
                labelRule.ForeColor = Color.White;
                labelRule.BorderStyle = BorderStyle.None;
            }


            // Chrono
            DateTime dt;
            try
            {
                dt = new DateTime(2000, 1, 1, 0, 0, 0);
                dt = dt.AddSeconds(chronoValue);
                labelChrono.Text = dt.ToLongTimeString();
            }
            catch (ArgumentOutOfRangeException me)
            {
                Text = me.Message;
            }

        }

        private void getWindEffects(int capAvion, double windSpeed, int windFrom, out double crossWind, out double headWind)
        {
            // ******************************************
            // Calcul des vents de face et de travers
            // ******************************************
            crossWind = windSpeed*Math.Sin((windFrom-capAvion)*Math.PI/180.0);
            crossWind = Math.Round(crossWind, 1);

            // HeadWind
            headWind = windSpeed*Math.Cos((windFrom-capAvion)*Math.PI/180.0);
            headWind = Math.Round(headWind, 1);
        }

        #endregion

        private void Vitesse(int deltaSpeed) // deltaSpeed = 0 (Arrêt) à 7 (300 kT)
        {
            if (niveauVitesse==0)
                turnRate_temp = 0.0f;

            if (0<=niveauVitesse+deltaSpeed  && niveauVitesse+deltaSpeed<7)
                niveauVitesse += deltaSpeed;
            TAS = Speeds[niveauVitesse];

            if (1<=niveauVitesse && !timer_Anim.Enabled)
                timer_Anim.Enabled = true;
            else if (niveauVitesse == 0)
                timer_Anim.Enabled = false;


            TASInNmSec = TAS/3600; // 3600 sec in 1 hour
        }

        private void WinForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Gestion des actions clavier

            switch (e.KeyData)
            {
                case Keys.Right:
                    // A droite
                    if (turnRate<30)
                        turnRate += 1;
                    break;
                case Keys.Left:
                    // A droite
                    if (-30<turnRate)
                        turnRate -= 1;
                    break;
                case Keys.Up:
                    // Accélerer
                    Vitesse(+1);
                    break;
                case Keys.Down:
                    // Ralentir
                    Vitesse(-1);
                    break;
                case Keys.P:
                    // Pause (arrêt anim)
                    paused = !paused;
                    timer_Anim.Enabled=(paused ? false : true);
                    break;
                case Keys.Space:
                    // Aller tout droit
                    turnRate = 0;
                    break;
                case Keys.T:
                    // Traceur de trajectoire
                    traceon = !traceon;
                    deltaX = 0;
                    deltaY = 0;
                    if (traceon)
                        traceSol.Clear();
                    labelTrace.Enabled = (traceon ? true : false);
                    break;
                case Keys.R:
                    // Règle
                    rulesMode = !rulesMode;
                    if (rulesMode && windMode==windStatus.settingWindMode)
                        windMode = windStatus.windDefinedWindMode;
                    if (rulesMode)
                    {
                        ruleStart = new Point(0, 0);
                        ruleEnd = new Point(0, 0);
                    }
                    panel_EspaceAerien.Invalidate();
                    break;
                case Keys.V:
                    // WindMode
                    if (windMode==windStatus.noWind)          // Défine wind
                    {
                        windMode = windStatus.settingWindMode;
                        if (rulesMode)
                            rulesMode = false;
                    }
                    else if (windMode==windStatus.settingWindMode) // vent défini
                    {
                        if (windSpeed==0)
                            windMode = windStatus.noWind;
                        else
                            windMode = windStatus.windDefinedWindMode;
                    }
                    else
                    { // No wind
                        windMode = windStatus.noWind;
                        windSpeed = 0;
                        windSpeedInNmSec = 0;
                        Point p = MoveableGraphic.centerOfObject(panel_EspaceAerien.Bounds);
                        windStart = p;
                        windEnd = p;
                    }
                    break;
                case Keys.B:
                    // Masquer/montrer les balises
                    /*
                    displayNDB_VORWithRadiales=0,  displayNDB_VORWithoutRadiales=1,
                    displayNDB=2, displayNothing=3,
                    displayVORWithoutRadiales=4, displayVORWithRadiales=5
                    */
                    if ((int)vorDisplay<5)
                        vorDisplay++;
                    else
                        vorDisplay = vorStatus.displayNDB_VORWithRadiales;

                    Ndb1.visible = ((int)vorDisplay<3 ? true : false);
                    Ndb2.visible = ((int)vorDisplay<3 ? true : false);

                    Vor1.radialsShow = ((int)vorDisplay%5==0 ? true : false);
                    Vor2.radialsShow = ((int)vorDisplay%5==0 ? true : false);

                    Vor1.visible = ((int)vorDisplay<2 || 3<(int)vorDisplay ? true : false);
                    Vor2.visible = ((int)vorDisplay<2 || 3<(int)vorDisplay ? true : false);
                    break;
                case Keys.X:
                    // Exercice
                    Init(true);
                    e.Handled = true;
                    break;
                case Keys.I:
                    // Initialisation
                    Init(false);
                    break;
                case Keys.E:
                    // Echelle
                    Echelle();
                    break;
                case Keys.H:
                    // A Propos
                    frmAbout AProposForm = new frmAbout();
                    AProposForm.Owner = this;
                    AProposForm.ShowDialog();
                    AProposForm.Dispose();
                    break;
                case Keys.L:
                    // Langue
                    if (cultInfo.Name == "en")
                        cultInfo = new CultureInfo("");
                    else
                        cultInfo = new CultureInfo("en");

                    Update_Langue();
                    break;
                case Keys.C:
                    // Chrono   int chronoState: 0=start 1=stop 2=reset
                    if (chronoState<=1)
                        chronoState++;
                    else
                    {
                        chronoState=0;
                        chronoValue = 0;
                    }

                    if (chronoState==2)
                        chronoValue = 0;

                    labelChrono.ForeColor= (chronoState<1 ? Color.Yellow : Color.Ivory);
                    break;
            }
            e.Handled = true;
            panel_EspaceAerien.Invalidate();
        }

        // ---------------------------------------------------------------------------

        private void Update_Langue()
        {
            // Met à jour l'affichage dans la langue sélectionnée
            try
            {
                lblcmd_clavier.Text = rm.GetString("cmd_clavier", cultInfo);
                lblcmd_obas.Text = rm.GetString("cmd_obas", cultInfo) + " [0-" + Speeds[Speeds.Length-1].ToString()  + " kT]";
                lblcmd_droitgo.Text = rm.GetString("cmd_droitgo", cultInfo);
                lblcmd_espace.Text = rm.GetString("cmd_espace", cultInfo);
                lblcmd_p.Text = rm.GetString("cmd_p", cultInfo);
                lblcmd_i.Text = rm.GetString("cmd_i", cultInfo);
                lblcmd_x.Text = rm.GetString("cmd_x", cultInfo);

                lblcmd_t.Text = rm.GetString("cmd_t", cultInfo);
                lblcmd_r.Text = rm.GetString("cmd_r", cultInfo);
                lblcmd_v.Text = rm.GetString("cmd_v", cultInfo);
                lblcmd_e.Text = rm.GetString("cmd_e", cultInfo);
                lblcmd_c.Text = rm.GetString("cmd_c", cultInfo);
                lblcmd_b.Text = rm.GetString("cmd_b", cultInfo);

                lblcmd_mouse.Text = rm.GetString("cmd_mouse", cultInfo);
                lblmove.Text = rm.GetString("cmd_move", cultInfo);

                lblcmd_l.Text = rm.GetString("cmd_l", cultInfo);
                lblcmd_h.Text = rm.GetString("cmd_h", cultInfo);

                //
                displayInfos();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Erreur", MessageBoxButtons.OK);
            }
        }

        // ---------------------------------------------------------------------------

        private void Echelle() // Echelle (hauteur graphique) = 2 à 20 Nm
        {
            if (niveauEchelle==20)
                niveauEchelle = 2;
            else if (2<niveauEchelle+1  && niveauEchelle+1<=20)
                niveauEchelle++;
            calculEchelle();
            panel_EspaceAerien.Invalidate();
        }


        // ---------------------------------------------------------------------------

    }
}