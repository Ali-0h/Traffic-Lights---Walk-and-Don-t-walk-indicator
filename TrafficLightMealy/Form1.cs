using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace TrafficLightMealy
{
    public class Form1 : Form
    {
        private TrafficController controller;
        private Timer uiTimer;
        private Timer flashTimer;
        private bool flashVisible = true;

        private DfaDiagram dfa = new DfaDiagram();

        // Right panel content
        private Panel dfaPanel;
        private Panel descriptionPanel;
        private Label descriptionLabel;

        // layout
        private RectangleF headerRect, leftPanelRect, rightPanelRect, footerRect;
        private Rectangle imageRect, pressRect, stateLabelRect;

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

            // 🔴 IMPORTANT: only repaint what is needed
            controller.StateChanged += (s, e) =>
            {
                dfaPanel.Invalidate();
                Invalidate(imageRect);
            };

            uiTimer = new Timer { Interval = 200 };
            uiTimer.Tick += (s, e) => controller.Tick(uiTimer.Interval);
            uiTimer.Start();

            flashTimer = new Timer { Interval = 1000 };
            flashTimer.Tick += (s, e) =>
            {
                flashVisible = !flashVisible;
                Invalidate(imageRect); // image only
            };
            flashTimer.Start();

            sceneImage = Properties.Resources.GreenCarGoing;

            MouseDown += Form1_MouseDown;
            Resize += (s, e) => LayoutRects();

            CreateRightPanelContent();
            LayoutRects();
            UpdateDescription();
        }

        // ================= RIGHT PANEL =================

        private void CreateRightPanelContent()
        {
            dfaPanel = new Panel
            {
                BackColor = Color.Transparent
            };

            // ✅ Enable double buffering on DFA panel (CRITICAL)
            typeof(Panel).InvokeMember(
                "DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                dfaPanel,
                new object[] { true }
            );

            descriptionPanel = new Panel
            {
                AutoScroll = true,
                BackColor = Color.FromArgb(25, 25, 25)
            };

            descriptionLabel = new Label
            {
                ForeColor = Color.Gainsboro,
                Font = normalFont,
                AutoSize = false
            };

            descriptionPanel.Controls.Add(descriptionLabel);
            Controls.Add(dfaPanel);
            Controls.Add(descriptionPanel);

            dfaPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                dfa.Draw(
                    e.Graphics,
                    new Rectangle(0, 0, dfaPanel.Width, dfaPanel.Height),
                    controller.CurrentState
                );
            };
        }

        // ================= LAYOUT =================

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

            pressRect = new Rectangle(imageRect.Left, imageRect.Bottom + 10, 180, 44);
            stateLabelRect = new Rectangle(imageRect.Left, pressRect.Bottom + 6, imageRect.Width, 24);

            int dfaHeight = 220;

            dfaPanel.Bounds = new Rectangle(
                (int)rightPanelRect.Left + 10,
                (int)rightPanelRect.Top + 10,
                (int)rightPanelRect.Width - 20,
                dfaHeight
            );

            descriptionPanel.Bounds = new Rectangle(
                dfaPanel.Left,
                dfaPanel.Bottom + 10,
                dfaPanel.Width,
                (int)rightPanelRect.Height - dfaHeight - 30
            );

            descriptionLabel.Bounds = new Rectangle(
                10, 10,
                descriptionPanel.Width - 30,
                1000
            );
        }

        // ================= PAINT =================

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            DrawHeader(e.Graphics);
            DrawPanels(e.Graphics);
            DrawSceneImage(e.Graphics);
            DrawImageOverlay(e.Graphics);
            DrawPressButton(e.Graphics);
            DrawStateLabel(e.Graphics);
            DrawFooter(e.Graphics);
        }

        // ================= DRAW HELPERS =================

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
                    sceneImage = Properties.Resources.GreenFinished;
                    break;
            }

            g.DrawImage(sceneImage, imageRect);
        }

        private void DrawImageOverlay(Graphics g)
        {
            int total =
                controller.CurrentState == TState.Green ? 30000 :
                controller.CurrentState == TState.Yellow ? 3000 :
                15000;

            int remaining = Math.Max(0, total - controller.ElapsedMilliseconds);
            int seconds = (int)Math.Ceiling(remaining / 1000.0);

            Rectangle timerRect = new Rectangle(
                imageRect.Right - 110,
                imageRect.Top + 10,
                100,
                34
            );

            using (SolidBrush bg = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                g.FillRectangle(bg, timerRect);

            DrawCenteredText(g, $"{seconds}s", boldFont, Brushes.White, timerRect);
        }

        private void DrawPressButton(Graphics g)
        {
            bool disabled = controller.IsPedRequestQueued;

            using (SolidBrush sb = new SolidBrush(disabled ? Color.Gray : Color.FromArgb(0, 120, 255)))
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
            g.DrawString(
                "State: " + controller.CurrentState,
                normalFont,
                Brushes.LightGray,
                stateLabelRect
            );
        }

        private void DrawFooter(Graphics g)
        {
            DrawCenteredText(
                g,
                "Mealy Machine Traffic Controller Demo",
                normalFont,
                Brushes.Gray,
                Rectangle.Round(footerRect)
            );
        }

        private void UpdateDescription()
        {
            descriptionLabel.Text =
        @"FORMAL DESCRIPTION (MEALY MACHINE)

Machine Description:
The traffic intersection control system is modeled as a Mealy Machine, where the outputs depend on both the current state and the input conditions. This allows the system to respond immediately to events such as timeouts and pedestrian requests, ensuring safe and efficient coordination between vehicle and pedestrian traffic.

Start State:
Q0 (Green)

Set of States (Q):
Q = { 
    Q0 (Green), 
    Q1 (Yellow), 
    Q2 (Red), 
    Q3 (Red Wait), 
    Q4 (Red Crossing), 
    Q5 (Green Finish) 
}

Input Alphabet (Σ):
Σ = { timeout, walk = true, no_request, reset }

Output Alphabet (Λ):
Λ = { 
    CarSignal = { Green, Yellow, Red }, 
    PedestrianSignal = { Walk, DontWalk } 
}

Final State:
Q5 (Green Finish)

State Traversal / Transition Function (δ):

Q0 (Green)
    -- timeout --> Q1 (Yellow)

Q1 (Yellow)
    -- timeout --> Q2 (Red)

Q2 (Red)
    -- walk = true --> Q3 (Red Wait)
    -- no_request --> Q0 (Green)

Q3 (Red Wait)
    -- timeout --> Q4 (Red Crossing)

Q4 (Red Crossing)
    -- timeout --> Q5 (Green Finish)

Q5 (Green Finish)
    -- reset --> Q0 (Green)

State Traversal Description:
The system begins in state Q0 (Green), where vehicles are allowed to proceed and pedestrians are restricted.
After a timeout occurs, the system transitions from Q0 to Q1 (Yellow) as a warning phase for vehicles.
Following another timeout, the system moves from Q1 to Q2 (Red), stopping vehicle traffic completely.

While in state Q2 (Red), the system evaluates pedestrian input.
If a pedestrian request is detected (walk = true), the system transitions to Q3 (Red Wait) to ensure a safe delay before crossing.
If no pedestrian request is present, the system returns directly from Q2 to Q0 (Green).

From Q3 (Red Wait), a timeout causes the system to move to Q4 (Red Crossing), allowing pedestrians to cross while vehicles remain stopped.
After the crossing period ends due to a timeout, the system transitions to Q5 (Green Finish).
Finally, upon receiving a reset input, the system returns from Q5 to Q0 (Green), resuming normal traffic operation.";

            // 🔥 THIS IS THE MAGIC
            descriptionLabel.Height = TextRenderer.MeasureText(
                descriptionLabel.Text,
                descriptionLabel.Font,
                new Size(descriptionLabel.Width, int.MaxValue),
                TextFormatFlags.WordBreak
            ).Height;
        }





        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (pressRect.Contains(e.Location) && !controller.IsPedRequestQueued)
                controller.PressPedButton();
        }

        private void DrawCenteredText(Graphics g, string text, Font font, Brush brush, Rectangle rect)
        {
            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(text, font, brush, rect, sf);
        }
    }

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
