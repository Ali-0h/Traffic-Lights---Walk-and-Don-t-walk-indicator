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
        private bool walkFlashVisible = true;

        // layout rectangles
        private RectangleF headerRect, leftPanelRect, rightPanelRect, footerRect;
        private RectangleF trafficBodyRect;
        private RectangleF redCircleRect, yellowCircleRect, greenCircleRect;
        private RectangleF walkRect, dontWalkRect, pressRect, stateLabelRect;
        private RectangleF diagramRect, infoRect;

        // fonts
        private Font headerFont;
        private Font titleFont;
        private Font normalFont;
        private Font boldFont;

        public Form1()
        {
            Text = "Traffic Light Mealy Simulator";
            ClientSize = new Size(980, 640);
            MinimumSize = new Size(860, 540);
            DoubleBuffered = true;
            BackColor = Color.FromArgb(245, 247, 250);

            // fonts
            headerFont = new Font("Segoe UI", 14, FontStyle.Bold);
            titleFont = new Font("Segoe UI", 12, FontStyle.Bold);
            normalFont = new Font("Segoe UI", 10, FontStyle.Regular);
            boldFont = new Font("Segoe UI", 10, FontStyle.Bold);

            controller = new TrafficController();
            controller.StateChanged += Controller_StateChanged;
            controller.OutputsUpdated += Controller_OutputsUpdated;

            // timers
            uiTimer = new Timer();
            uiTimer.Interval = 200;
            uiTimer.Tick += UiTimer_Tick;
            uiTimer.Start();

            flashTimer = new Timer();
            flashTimer.Interval = 350;
            flashTimer.Tick += FlashTimer_Tick;
            flashTimer.Start();

            this.MouseDown += Form1_MouseDown;
            this.Resize += Form1_Resize;

            LayoutRects();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            LayoutRects();
            Invalidate();
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            controller.Tick(uiTimer.Interval);
        }

        private void FlashTimer_Tick(object sender, EventArgs e)
        {
            walkFlashVisible = !walkFlashVisible;
            Invalidate();
        }

        private void Controller_OutputsUpdated(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void Controller_StateChanged(object sender, TrafficControllerEventArgs e)
        {
            Invalidate();
        }

        private void LayoutRects()
        {
            int pad = 20;
            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;

            headerRect = new RectangleF(pad, 10, w - pad * 2, 50);
            footerRect = new RectangleF(pad, h - 34, w - pad * 2, 24);

            float mainTop = headerRect.Bottom + 8;
            float mainHeight = h - mainTop - 80;
            float leftW = (w - pad * 3) * 0.48f;
            float rightW = (w - pad * 3) * 0.52f;

            leftPanelRect = new RectangleF(pad, mainTop, leftW, mainHeight);
            rightPanelRect = new RectangleF(pad + leftW + pad, mainTop, rightW, mainHeight);

            float bodyW = 120, bodyH = 320;
            trafficBodyRect = new RectangleF(leftPanelRect.Left + (leftPanelRect.Width - bodyW) / 2f, leftPanelRect.Top + 20, bodyW, bodyH);

            float circleD = 80;
            float cx = trafficBodyRect.Left + (trafficBodyRect.Width - circleD) / 2f;
            redCircleRect = new RectangleF(cx, trafficBodyRect.Top + 30, circleD, circleD);
            yellowCircleRect = new RectangleF(cx, redCircleRect.Bottom + 18, circleD, circleD);
            greenCircleRect = new RectangleF(cx, yellowCircleRect.Bottom + 18, circleD, circleD);

            float pedW = 140, pedH = 70;
            float pedLeft = leftPanelRect.Left + (leftPanelRect.Width - pedW * 2 - 12) / 2f;
            walkRect = new RectangleF(pedLeft, trafficBodyRect.Bottom + 18, pedW, pedH);
            dontWalkRect = new RectangleF(pedLeft + pedW + 12, walkRect.Top, pedW, pedH);

            pressRect = new RectangleF(leftPanelRect.Left + (leftPanelRect.Width - 160) / 2f, walkRect.Bottom + 16, 160, 46);
            stateLabelRect = new RectangleF(leftPanelRect.Left + 10, pressRect.Bottom + 12, leftPanelRect.Width - 20, 28);

            float diagH = rightPanelRect.Height * 0.6f;
            diagramRect = new RectangleF(rightPanelRect.Left + 12, rightPanelRect.Top + 20, rightPanelRect.Width - 24, diagH - 30);
            infoRect = new RectangleF(rightPanelRect.Left + 12, diagramRect.Bottom + 12, rightPanelRect.Width - 24, rightPanelRect.Bottom - (diagramRect.Bottom + 20));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawHeader(g);
            DrawPanels(g);
            DrawTrafficHousing(g);
            DrawLights(g);
            DrawPedBoxes(g);
            DrawPressButton(g);
            DrawStateLabel(g);
            DrawDiagramWithCurvedArrows(g);
            DrawInfoBox(g);
            DrawFooter(g);
        }

        private void DrawHeader(Graphics g)
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            g.DrawString("Traffic Light Mealy Machine – Car & Pedestrian Signals Simulator", headerFont, Brushes.DimGray, headerRect, sf);
        }

        private void DrawPanels(Graphics g)
        {
            // left and right white cards with subtle shadow
            RectangleF leftShadow = leftPanelRect; leftShadow.Offset(4, 6);
            RectangleF rightShadow = rightPanelRect; rightShadow.Offset(4, 6);
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(20, 0, 0, 0))) { g.FillRectangle(sb, leftShadow); g.FillRectangle(sb, rightShadow); }
            using (SolidBrush sb = new SolidBrush(Color.White)) { g.FillRoundedRectangle(sb, leftPanelRect, 8); g.FillRoundedRectangle(sb, rightPanelRect, 8); }
            using (Pen p = new Pen(Color.FromArgb(220, 220, 220))) { g.DrawRoundedRectangle(p, leftPanelRect, 8); g.DrawRoundedRectangle(p, rightPanelRect, 8); }
        }

        private void DrawTrafficHousing(Graphics g)
        {
            GraphicsPath path = RoundedRectPath(trafficBodyRect, 18);
            using (LinearGradientBrush lg = new LinearGradientBrush(trafficBodyRect, Color.FromArgb(26, 35, 45), Color.FromArgb(36, 45, 60), LinearGradientMode.Vertical))
            {
                g.FillPath(lg, path);
            }
            using (Pen pen = new Pen(Color.Black)) g.DrawPath(pen, path);
            path.Dispose();
        }

        private void DrawLights(Graphics g)
        {
            DrawLight(g, redCircleRect, controller.CarOutput == CarLight.Red ? Color.Red : Color.FromArgb(120, 120, 120), controller.CarOutput == CarLight.Red);
            DrawLight(g, yellowCircleRect, controller.CarOutput == CarLight.Yellow ? Color.Gold : Color.FromArgb(120, 120, 120), controller.CarOutput == CarLight.Yellow);
            DrawLight(g, greenCircleRect, controller.CarOutput == CarLight.Green ? Color.FromArgb(0, 180, 110) : Color.FromArgb(120, 120, 120), controller.CarOutput == CarLight.Green);
        }

        private void DrawLight(Graphics g, RectangleF rect, Color color, bool active)
        {
            if (active)
            {
                // glow
                for (int i = 4; i >= 1; i--)
                {
                    RectangleF r = rect;
                    r.Inflate(i * 6, i * 6);
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(20 + i * 8, color))) g.FillEllipse(sb, r);
                }
            }

            // radial-ish fill using PathGradientBrush
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(rect);
            PathGradientBrush pgb = new PathGradientBrush(gp);
            pgb.CenterColor = ControlPaint.Light(color);
            pgb.SurroundColors = new Color[] { ControlPaint.Dark(color) };
            g.FillEllipse(pgb, rect);
            pgb.Dispose();
            gp.Dispose();

            g.DrawEllipse(Pens.Black, rect.X, rect.Y, rect.Width, rect.Height);
        }

        private void DrawPedBoxes(Graphics g)
        {
            DrawPedBox(g, walkRect, "WALK", controller.PedOutput == PedSignal.Walk && (controller.PedOutput != PedSignal.Flashing || walkFlashVisible), Color.FromArgb(0, 150, 80));
            DrawPedBox(g, dontWalkRect, "DON'T WALK", controller.PedOutput == PedSignal.DontWalk || (controller.PedOutput == PedSignal.Flashing && !walkFlashVisible), Color.FromArgb(230, 90, 0));
        }

        private void DrawPedBox(Graphics g, RectangleF rect, string text, bool active, Color activeColor)
        {
            GraphicsPath path = RoundedRectPath(rect, 8);
            using (SolidBrush sb = new SolidBrush(active ? activeColor : Color.FromArgb(242, 244, 246))) g.FillPath(sb, path);
            using (Pen p = new Pen(Color.Gray)) g.DrawPath(p, path);
            StringFormat sf = new StringFormat(); sf.Alignment = StringAlignment.Center; sf.LineAlignment = StringAlignment.Center;
            using (Font f = new Font("Segoe UI", 12, FontStyle.Bold))
            {
                g.DrawString(text, f, new SolidBrush(active ? Color.White : Color.FromArgb(120, 120, 120)), rect, sf);
            }
            path.Dispose();
        }

        private void DrawPressButton(Graphics g)
        {
            GraphicsPath pth = RoundedRectPath(pressRect, 8);
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(32, 127, 255))) g.FillPath(sb, pth);
            using (Pen p = new Pen(Color.FromArgb(18, 90, 230))) g.DrawPath(p, pth);
            StringFormat sf = new StringFormat(); sf.Alignment = StringAlignment.Center; sf.LineAlignment = StringAlignment.Center;
            using (Font f = new Font("Segoe UI", 11, FontStyle.Bold)) g.DrawString("Press to Walk", f, Brushes.White, pressRect, sf);
            pth.Dispose();
        }

        private void DrawStateLabel(Graphics g)
        {
            string text = string.Format("State: {0}   |   Car: {1}   |   Ped: {2}", controller.CurrentState, controller.CarOutput, controller.PedOutput);
            g.DrawString(text, normalFont, Brushes.DimGray, stateLabelRect);
        }

        // --- NEW: curved arrows diagram drawing (no labels) ---
        private void DrawDiagramWithCurvedArrows(Graphics g)
        {
            using (Pen pen = new Pen(Color.FromArgb(200, 200, 200))) g.DrawRoundedRectangle(pen, diagramRect, 8);

            // node positions
            float cx = diagramRect.Left + diagramRect.Width / 2f;
            float top = diagramRect.Top + 24;
            float r = 44f;
            PointF pGreen = new PointF(cx, top + r);
            PointF pYellow = new PointF(cx, top + r * 3 + 18);
            PointF pRed = new PointF(cx, top + r * 5 + 36);

            // draw curved arrows:
            // Green -> Yellow (short downward curve)
            DrawCurvedArrow(g, pGreen, pYellow, 0.0f, 0.0f);

            // Yellow -> Red (straight downward curve)
            DrawCurvedArrow(g, pYellow, pRed, 0.0f, 0.0f);

            // Red -> Green (curved arc on the right)
            DrawCurvedArrow(g, pRed, pGreen, 120f, -120f); // control point offsets create right-side loop

            // draw nodes
            DrawNode(g, pGreen, r, "Car Green\nPed Don't Walk", controller.CurrentState == TState.Green);
            DrawNode(g, pYellow, r, "Car Yellow\nPed Don't Walk", controller.CurrentState == TState.Yellow);
            DrawNode(g, pRed, r, "Car Red\nPed Walk", controller.CurrentState == TState.RedWalk || controller.CurrentState == TState.RedFlash || controller.CurrentState == TState.RedWait);
        }

        // Draw a cubic Bezier curve between two points with arrowhead.
        // cpOffset1/cpOffset2 adjust the control points to curve the path; use 0 for near-straight.
        private void DrawCurvedArrow(Graphics g, PointF start, PointF end, float cpOffset1, float cpOffset2)
        {
            // define control points based on offsets
            PointF cp1 = new PointF(start.X + cpOffset1, start.Y + (end.Y - start.Y) / 2f);
            PointF cp2 = new PointF(end.X + cpOffset2, start.Y + (end.Y - start.Y) / 2f);

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddBezier(start, cp1, cp2, end);
                using (Pen pen = new Pen(Color.FromArgb(100, 80, 90, 120), 2f))
                {
                    pen.EndCap = LineCap.Flat;
                    pen.StartCap = LineCap.Flat;
                    g.DrawPath(pen, path);

                    // draw arrowhead at end: sample tangent direction
                    // approximate derivative of cubic bezier at t = 0.95
                    float t = 0.95f;
                    PointF tangent = BezierTangent(start, cp1, cp2, end, t);
                    DrawArrowHead(g, tangent, end, 10f, pen.Color);
                }
            }
        }

        // compute tangent (derivative) of cubic bezier at t
        private PointF BezierTangent(PointF p0, PointF p1, PointF p2, PointF p3, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            float x = 3 * uu * (p1.X - p0.X) + 6 * u * t * (p2.X - p1.X) + 3 * tt * (p3.X - p2.X);
            float y = 3 * uu * (p1.Y - p0.Y) + 6 * u * t * (p2.Y - p1.Y) + 3 * tt * (p3.Y - p2.Y);
            return new PointF(x, y);
        }

        // draw a triangular arrowhead given direction tangent and end point
        private void DrawArrowHead(Graphics g, PointF tangent, PointF tip, float size, Color color)
        {
            // Normalize tangent
            float len = (float)Math.Sqrt(tangent.X * tangent.X + tangent.Y * tangent.Y);
            if (len < 0.001f) return;
            float dx = tangent.X / len;
            float dy = tangent.Y / len;

            // perpendicular
            float px = -dy;
            float py = dx;

            // points for triangle
            PointF p1 = new PointF(tip.X - dx * size + px * (size * 0.5f), tip.Y - dy * size + py * (size * 0.5f));
            PointF p2 = new PointF(tip.X - dx * size - px * (size * 0.5f), tip.Y - dy * size - py * (size * 0.5f));

            using (SolidBrush sb = new SolidBrush(color))
            {
                PointF[] tri = new PointF[] { tip, p1, p2 };
                g.FillPolygon(sb, tri);
            }
        }

        private void DrawNode(Graphics g, PointF center, float r, string text, bool active)
        {
            RectangleF rect = new RectangleF(center.X - r, center.Y - r, r * 2, r * 2);
            if (active)
            {
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(60, 50, 130, 255))) g.FillEllipse(sb, RectangleF.Inflate(rect, 8, 8));
            }
            using (SolidBrush sb = new SolidBrush(active ? Color.FromArgb(230, 245, 255) : Color.White)) g.FillEllipse(sb, rect);
            g.DrawEllipse(Pens.DarkBlue, rect);
            StringFormat sf = new StringFormat(); sf.Alignment = StringAlignment.Center; sf.LineAlignment = StringAlignment.Center;
            using (Font f = new Font("Segoe UI", 9, FontStyle.Regular)) g.DrawString(text, f, Brushes.DimGray, rect, sf);
        }

        private void DrawInfoBox(Graphics g)
        {
            GraphicsPath path = RoundedRectPath(infoRect, 6);
            using (SolidBrush sb = new SolidBrush(Color.White)) g.FillPath(sb, path);
            using (Pen p = new Pen(Color.Gray)) g.DrawPath(p, path);

            float x = infoRect.Left + 12;
            float y = infoRect.Top + 12;
            g.DrawString("Mealy Machine Model — Traffic Light Controller", titleFont, Brushes.DimGray, x, y);
            y += 28;
            g.DrawString("Inputs:", boldFont, Brushes.Black, x, y);
            y += 20;
            g.DrawString("• timer — automatic time-based transitions", normalFont, Brushes.DimGray, x + 8, y);
            y += 20;
            g.DrawString("• walkButton — pedestrian crossing request", normalFont, Brushes.DimGray, x + 8, y);
            y += 26;
            g.DrawString("Outputs:", boldFont, Brushes.Black, x, y);
            y += 20;
            g.DrawString("• carLight — Red, Yellow, or Green", normalFont, Brushes.DimGray, x + 8, y);
            y += 20;
            g.DrawString("• pedSignal — Walk or Don't Walk (Flashing)", normalFont, Brushes.DimGray, x + 8, y);

            path.Dispose();
        }

        private void DrawFooter(Graphics g)
        {
            StringFormat sf = new StringFormat(); sf.Alignment = StringAlignment.Center; sf.LineAlignment = StringAlignment.Center;
            g.DrawString("Designed by: Alex Chen, Jordan Martinez, Sam Patel", normalFont, Brushes.DimGray, footerRect, sf);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            PointF p = e.Location;
            if (pressRect.Contains(p))
            {
                controller.PressPedButton();
                this.Invalidate(Rectangle.Ceiling(pressRect));
            }
        }

        // --- utility drawing methods (no C#8 features) ---
        private static GraphicsPath RoundedRectPath(RectangleF rect, float radius)
        {
            GraphicsPath gp = new GraphicsPath();
            float d = radius * 2f;
            gp.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            gp.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            gp.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            gp.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }

    // extension helper to draw rounded rectangles (fills)
    static class GraphicsRoundedExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, RectangleF rect, float radius)
        {
            GraphicsPath p = Form1_RoundedRect(rect, radius);
            g.FillPath(brush, p);
            p.Dispose();
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, RectangleF rect, float radius)
        {
            GraphicsPath p = Form1_RoundedRect(rect, radius);
            g.DrawPath(pen, p);
            p.Dispose();
        }

        private static GraphicsPath Form1_RoundedRect(RectangleF rect, float radius)
        {
            GraphicsPath gp = new GraphicsPath();
            float d = radius * 2f;
            gp.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            gp.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            gp.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            gp.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }
}
