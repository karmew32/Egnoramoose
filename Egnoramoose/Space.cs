using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Egnoramoose
{
    public enum SpaceState
    {
        VACANT,
        OCCUPIED,
        SELECTED
    }
    public class Space
    {
        private const int RADIUS = 20;
        public Point Location { get; private set; }
        public int Row { get; private set; }
        public int Offset { get; private set; }
        public Color Color { get; set; }
        public Rectangle BoundingBox { get; private set; }
        public SpaceState State { get; set; }

        public Space(Color color, Point location, int row, int offset, SpaceState state = SpaceState.OCCUPIED)
        {
            Color = color;
            Location = location;
            Row = row;
            Offset = offset;
            State = state;
            BoundingBox = new Rectangle(new Point(location.X - RADIUS, location.Y - RADIUS), new Size(RADIUS * 2, RADIUS * 2));
        }

        public bool IsHit(int x, int y)
        {
            return BoundingBox.Contains(x, y);
        }

        public void Draw(Graphics g)
        {
            Color penColor = default;
            Color brushColor = default;
            switch (State)
            {
                case SpaceState.VACANT:
                    penColor = Color.Black;
                    brushColor = Color.White;
                    break;
                case SpaceState.OCCUPIED:
                    penColor = Color.Black;
                    brushColor = Color;
                    break;
                case SpaceState.SELECTED:
                    penColor = Color.Red;
                    brushColor = GetTranslucentColor();
                    break;
                default:
                    break;
            }
            using (Pen pen = new Pen(penColor, 5))
            {
                g.DrawCircle(pen, Location.X, Location.Y, RADIUS);
            }
            using (Brush brush = new SolidBrush(brushColor))
            {
                g.FillCircle(brush, Location.X, Location.Y, RADIUS);
            }
        }

        private Color GetTranslucentColor()
        {
            byte newRed = Color.R;
            byte newGreen = Color.G;
            byte newBlue = Color.B;
            if (Color.R < 255)
            {
                newRed += (byte)((255 - Color.R) / 2);
            }
            if (Color.G < 255)
            {
                newGreen += (byte)((255 - Color.G) / 2);
            }
            if (Color.B < 255)
            {
                newBlue += (byte)((255 - Color.B) / 2);
            }
            return Color.FromArgb(255, newRed, newGreen, newBlue);
        }
    }
}
