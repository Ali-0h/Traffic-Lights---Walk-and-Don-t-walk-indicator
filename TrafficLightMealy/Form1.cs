using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TrafficLightMealy
{
    public class Form1 : Form
    {
        private TrafficController controller;
        private Timer uiTimer;
        private Timer flashTimer;
        private bool flashVisible = true;

        // DFA VIEW (scrollable)
        private DfaView dfaView;

        // layout
        private RectangleF headerRect, leftPanelRect, rightPanelRect, footerRect;
        private Rectangle imageRect, pressRect, stateLabelRect;

        // image
        private Image sceneImage;

        // fonts
        private Font headerFont;
        private Font normalFont;
        private Font boldFont;

        public Form1()
        {
            Text = "Traffic Light Mealy Simulator";
            ClientSize = new Size(980, 640);
            DoubleBuffered = true;
            BackColor = Color.FromArgb(20, 20, 20);

            headerFont = new Font("Segoe UI", 14, FontStyle.Bold);
            normalFont = new Font("Segoe UI", 10);
            boldFont = new Font("Segoe UI", 11, FontStyle.Bold);

            controller = new TrafficController();
            controller.StateChanged += (_, __) => Invalidate();
            controller.OutputsUpdated += (_, __) => Invalidate();

            uiTimer = new Timer { Interval = 200 };
            uiTimer.Tick += (_, __) => controller.Tick(uiTimer.Interval);
            uiTimer.Start();

            flashTimer = new Timer { Interval = 400 };
            flashTimer.Tick += (_, __) =>
            {
                flashVisible = !flashVisible;
                Invalidate();
            };
            flashTimer.Start();

            sceneImage = Properties.Resources.GreenCarGoing;

            MouseDown += Form1_MouseDown;
            Resize += (_, __) => LayoutRects();

            LayoutRects();
            CreateDfaView();
        }

        // ---------------- DFA PANEL ----------------

        private void CreateDfaView()
        {
            dfaView = new DfaView();
            Controls.Add(dfaView);
            UpdateDfaBounds();
        }

        private void UpdateDfaBounds()
        {
            if (dfaView == null) return;

            dfaView.Location = new Point(
                (int)rightPanelRect.Left + 10,
                (int)rightPanelRect.Top + 10
            );

            dfaView.Size = new Size(
                (int)rightPanelRect.Width - 20,
                (int)rightPanelRect.Height - 20
            );
        }

        // ---------------- LAYOUT ----------------

        private void LayoutRects()
        {
            int pad = 20;
            int w = ClientSize.Width;
            int h = ClientSize.Height;

            headerRect = new RectangleF(pad, 10, w - pad * 2, 40);
            footerRect = new RectangleF(pad, h - 30, w - pad * 2, 20);

            float mainTop = headerRect.Bottom + 10;
            float mainHeight = h - mainTop - 60;

            leftPanelRect = new RectangleF(pad, mainTop, w * 0.55f - pad * 2, mainHeight);
            rightPanelRect = new RectangleF(leftPanelRect.Right + pad, mainTop, w * 0.45f - pad * 2, mainHeight);

            imageRect = Rectangle.Round(new RectangleF(
                leftPanelRect.Left + 20,
                leftPanelRect.Top + 20,
                leftPanelRect.Width - 40,
                leftPanelRect.Height - 120
            ));

            pressRect = Rectangle.Round(new RectangleF(
                imageRect.Left,
                imageRect.Bottom + 10,
                180,
                44
            ));

            stateLabelRect = Rectangle.Round(new RectangleF(
                imageRect.Left,
                pressRect.Bottom + 6,
                imageRect.Width,
                24
            ));

            UpdateDfaBounds();
        }

        // ---------------- PAINT ----------------

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawHeader(g);
            DrawPanels(g);

            DrawSceneImage(g);
            DrawImageOverlay(g);
            DrawPressButton(g);
            DrawStateLabel(g);

            DrawFooter(g);

            // update DFA state
            if (dfaView != null)
            {
                dfaView.CurrentState = controller.CurrentState;
                dfaView.Invalidate();
            }
        }

        // ---------------- DRAW HELPERS ----------------

        private void DrawHeader(Graphics g)
        {
            DrawCenteredText(
                g,
                "Traffic Light Mealy Machine – Car & Pedestrian Signals Simulator",
                headerFont,
                Brushes.White,
                Rectangle.Round(headerRect)
            );
        }

        private void DrawPanels(Graphics g)
        {
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(35, 35, 35)))
            {
                g.FillRoundedRectangle(sb, leftPanelRect, 10);
                g.FillRoundedRectangle(sb, rightPanelRect, 10);
            }
        }

        private void DrawSceneImage(Graphics g)
        {
            switch (controller.CurrentState)
            {
                case TState.Green:
                    sceneImage = Properties.Resources.GreenCarGoing;
                    break;
                case TState.Yellow:
                    sceneImage = Properties.Resources.YellowCarStop;
                    break;
                default:
                    sceneImage = controller.PedOutput == PedSignal.Walk
                        ? Properties.Resources.RedCrossing
                        : Properties.Resources.GreenFinished;
                    break;
            }

            g.DrawImage(sceneImage, imageRect);
        }

        private void DrawImageOverlay(Graphics g)
        {
            int seconds = GetRemainingSeconds();
            Rectangle timerRect = new Rectangle(imageRect.Right - 110, imageRect.Top + 10, 100, 34);

            using (SolidBrush bg = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                g.FillRectangle(bg, timerRect);

            DrawCenteredText(g, $"{seconds}s", boldFont, Brushes.White, timerRect);

            bool flashingDontWalk = controller.PedOutput == PedSignal.Flashing && !flashVisible;
            string pedText = controller.PedOutput == PedSignal.Walk && !flashingDontWalk
                ? "WALK"
                : "DON'T WALK";

            Brush pedBrush = pedText == "WALK" ? Brushes.Lime : Brushes.OrangeRed;
            Rectangle pedRect = new Rectangle(imageRect.Left + 10, imageRect.Bottom - 36, 160, 28);

            using (SolidBrush bg = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                g.FillRectangle(bg, pedRect);

            DrawCenteredText(g, pedText, boldFont, pedBrush, pedRect);
        }

        private void DrawPressButton(Graphics g)
        {
            bool disabled = controller.IsPedRequestQueued;

            using (SolidBrush sb = new SolidBrush(disabled ? Color.FromArgb(80, 80, 80) : Color.FromArgb(0, 120, 255)))
                g.FillRoundedRectangle(sb, pressRect, 8);

            DrawCenteredText(
                g,
                disabled ? "WAITING..." : "PRESS TO WALK",
                boldFont,
                Brushes.White,
                pressRect
            );
        }

        private void DrawStateLabel(Graphics g)
        {
            string text = $"State: {controller.CurrentState} | Ped: {controller.PedOutput}";
            g.DrawString(text, normalFont, Brushes.LightGray, stateLabelRect);
        }

        private void DrawFooter(Graphics g)
        {
            DrawCenteredText(
                g,
                "Members: Bj De Los Angeles, Kenn Calingasan, Jay Marck Maniegos",
                normalFont,
                Brushes.Gray,
                Rectangle.Round(footerRect)
            );
        }

        // ---------------- LOGIC ----------------

        private int GetRemainingSeconds()
        {
            int total =
                controller.CurrentState == TState.Green ? 30000 :
                controller.CurrentState == TState.Yellow ? 3000 :
                15000;

            int remaining = Math.Max(0, total - controller.ElapsedMilliseconds);
            return (int)Math.Ceiling(remaining / 1000.0);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (pressRect.Contains(e.Location) && !controller.IsPedRequestQueued)
            {
                controller.PressPedButton();
                Invalidate();
            }
        }

        private void DrawCenteredText(Graphics g, string text, Font font, Brush brush, Rectangle rect)
        {
            using (StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                g.DrawString(text, font, brush, rect, sf);
            }
        }
    }

    // ---------------- EXTENSIONS ----------------

    static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, RectangleF rect, float radius)
        {
            using (GraphicsPath p = new GraphicsPath())
            {
                float d = radius * 2;
                p.AddArc(rect.Left, rect.Top, d, d, 180, 90);
                p.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
                p.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                p.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
                p.CloseFigure();
                g.FillPath(brush, p);
            }
        }
    }
}
