using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Egnoramoose
{
    public partial class Form1 : Form
    {

        private Board Board { get; set; }
        public Form1()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            bool initialLoad = false;
            if (Board == null)
            {
                initialLoad = true;
                Board = new Board();
                Board.Build();
            }
            Board.Draw(e.Graphics);
            if (!initialLoad && Board.GetSelectedSpace() == null && !Board.CheckForAnyJumps())
            {
                int remainingPegs = Board.GetOccupiedSpaces();
                string message = $"You left {remainingPegs} pegs stranded.{Environment.NewLine}";
                switch (remainingPegs)
                {
                    case 1:
                        message += "You're genius.";
                        break;
                    case 2:
                        message += "You're purty smart.";
                        break;
                    case 3:
                        message += "You're just plain dumb.";
                        break;
                    default:
                        message += "You're just plain 'EG-NO-RA-MOOSE.'";
                        break;
                }
                message += $"{Environment.NewLine}Play again?";
                DialogResult dialogResult = MessageBox.Show(message, "Game Over", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    Board.Build();
                    Board.Draw(e.Graphics);
                }
                else
                {
                    Close();
                }
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (Board.OnClicked(e.X, e.Y))
            {
                Refresh();
            }
        }
    }
}
