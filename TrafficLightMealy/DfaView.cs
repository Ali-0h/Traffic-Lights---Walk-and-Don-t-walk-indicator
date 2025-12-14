using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TrafficLightMealy
{
    public class DfaView : Panel
    {
        private readonly DfaDiagram diagram = new DfaDiagram();
        public TState CurrentState { get; set; }

        public DfaView()
        {
            DoubleBuffered = true;
            AutoScroll = true;
            BackColor = Color.FromArgb(30, 30, 30);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Apply scroll offset
            g.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y);

            Rectangle diagramArea = new Rectangle(0, 0, Width - 20, 260);
            diagram.Draw(g, diagramArea, CurrentState);

            DrawSystemExplanation(g, new Rectangle(20, 300, Width - 60, 600));

            // Define scrollable size
            AutoScrollMinSize = new Size(0, 950);
        }

        private void DrawSystemExplanation(Graphics g, Rectangle area)
        {
            using (Font titleFont = new Font("Segoe UI", 10, FontStyle.Bold))
            using (Font bodyFont = new Font("Segoe UI", 9))
            {
                g.DrawString("How the System Works", titleFont, Brushes.White,
                    area.Left, area.Top);

                string text =
@"This traffic light controller is modeled as a Mealy Machine.

• Outputs depend on both the current state and inputs.
• Inputs are:
  - Timer expiration
  - Pedestrian button press

SYSTEM FLOW:
1. Green:
   Cars move, pedestrians wait.
   A pedestrian request may be queued.

2. Yellow:
   Transition warning state.
   If a pedestrian is queued, system moves to Red Walk.

3. Red Walk:
   Cars stop.
   Pedestrians may cross safely.

4. Red Flash:
   Pedestrian signal flashes DON'T WALK.
   Crossing time is ending.

5. Green Finish:
   All pedestrians cleared.
   System resets back to Green.

WHY MEALY?
Outputs change immediately during transitions,
not only when entering a new state.";

                g.DrawString(
                    text,
                    bodyFont,
                    Brushes.Gainsboro,
                    new Rectangle(area.Left, area.Top + 30, area.Width, area.Height)
                );
            }
        }
    }
}
