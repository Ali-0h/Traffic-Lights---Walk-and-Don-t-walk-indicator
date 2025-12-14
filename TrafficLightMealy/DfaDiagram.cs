using System.Drawing;
using System.Drawing.Drawing2D;

namespace TrafficLightMealy
{
    public class DfaDiagram
    {
        private readonly string[] labels =
        {
            "Green",
            "Yellow",
            "Red",
            "Red Wait",
            "Red Crossing",
            "Green Finish"
        };

        private readonly TState[] states =
        {
            TState.Green,
            TState.Yellow,
            TState.RedWalk,
            TState.RedWait,
            TState.RedFlash,
            TState.GreenFinished
        };

        public void Draw(Graphics g, Rectangle area, TState current)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawTitle(g, area);

            int count = labels.Length;
            int radius = 26;
            int spacing = (area.Width - 40) / (count - 1);
            int centerY = area.Top + 100;

            Point[] centers = new Point[count];

            for (int i = 0; i < count; i++)
            {
                centers[i] = new Point(
                    area.Left + 2 + spacing * i + spacing / 3,
                    centerY
                );
            }

            // Draw normal transitions
            for (int i = 0; i < count - 1; i++)
            {
                DrawArrow(g, centers[i], centers[i + 1]);
            }

            // Transition labels
            DrawLabel(g, "Timeout", Mid(centers[0], centers[1]));
            DrawLabel(g, "Timeout", Mid(centers[1], centers[2]));
            DrawLabel(g, "Walk = true", Mid(centers[2], centers[3]));
            DrawLabel(g, "Timeout", Mid(centers[3], centers[4]));
            DrawLabel(g, "Timeout", Mid(centers[4], centers[5]));

            // Reset arcs
            DrawResetArc(g, centers[5], centers[0], "reset", -60);
            DrawResetArc(g, centers[2], centers[0], "reset", -30);

            // Draw states last (on top)
            for (int i = 0; i < count; i++)
            {
                DrawState(g, centers[i], radius, labels[i], current == states[i]);
            }
        }

        private void DrawTitle(Graphics g, Rectangle area)
        {
            using (Font f = new Font("Segoe UI", 11, FontStyle.Bold))
            {
                g.DrawString(
                    "Mealy Machine – State Diagram",
                    f,
                    Brushes.White,
                    area.Left + 10,
                    area.Top + 10
                );
            }
        }

        private void DrawState(Graphics g, Point c, int r, string text, bool active)
        {
            Rectangle rect = new Rectangle(c.X - r, c.Y - r, r * 2, r * 2);

            using (Brush b = new SolidBrush(active ? Color.DarkGreen : Color.FromArgb(60, 60, 60)))
                g.FillEllipse(b, rect);

            g.DrawEllipse(Pens.White, rect);

            using (Font f = new Font("Segoe UI", 8, FontStyle.Bold))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(text, f, Brushes.White, rect, sf);
            }
        }

        private void DrawArrow(Graphics g, Point from, Point to)
        {
            using (Pen p = new Pen(Color.White, 2))
            {
                p.CustomEndCap = new AdjustableArrowCap(4, 4);
                g.DrawLine(p, from.X + 26, from.Y, to.X - 26, to.Y);
            }
        }

        private void DrawResetArc(Graphics g, Point from, Point to, string label, int height)
        {
            Point c1 = new Point(from.X, from.Y + height);
            Point c2 = new Point(to.X, to.Y + height);

            using (Pen p = new Pen(Color.White, 2))
            {
                p.CustomEndCap = new AdjustableArrowCap(4, 4);
                g.DrawBezier(p, from, c1, c2, to);
            }

            Point labelPos = new Point((from.X + to.X) / 2, from.Y + height - 10);
            DrawLabel(g, label, labelPos);
        }

        private void DrawLabel(Graphics g, string text, Point pos)
        {
            using (Font f = new Font("Segoe UI", 8))
            {
                SizeF s = g.MeasureString(text, f);
                g.DrawString(text, f, Brushes.White,
                    pos.X - s.Width / 2,
                    pos.Y + 6);
            }
        }

        private Point Mid(Point a, Point b)
        {
            return new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2);
        }
    }
}
