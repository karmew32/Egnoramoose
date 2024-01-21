using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Egnoramoose
{
    public class Board
    {
        private const int ROWS = 5;
        private const int Y_INITIAL = 50;
        private const int Y_OFFSET = 50;
        private const int X_INITIAL = 250;
        private const int X_OFFSET = 25;
        private List<Space> Spaces { get; set; }

        public Board()
        {
            Spaces = new List<Space>();
        }
        public void Build()
        {
            Spaces = new List<Space>();
            Color[] colors = { Color.Yellow, Color.Blue, Color.Orange };
            int[] colorOccurrences = { 0, 0, 0 };
            int vacantSpaceIndex = new Random(Guid.NewGuid().GetHashCode()).Next(ROWS * (ROWS + 1) / 2);
            for (int i = 0; i < ROWS; i++)
            {
                int y = Y_INITIAL + (Y_OFFSET * i);
                for (int j = 0; j <= i; j++)
                {
                    int x = X_INITIAL - (X_OFFSET * i) + (X_OFFSET * 2 * j);

                    int colorIndex;
                    do
                    {
                        colorIndex = new Random(Guid.NewGuid().GetHashCode()).Next(colors.Length);
                    } 
                    while (colorOccurrences[colorIndex] == 5);

                    colorOccurrences[colorIndex]++;
                    int index = i + j + (i * (i - 1) / 2);
                    SpaceState state = index == vacantSpaceIndex ? SpaceState.VACANT : SpaceState.OCCUPIED;
                    Space space = new Space(colors[colorIndex], new Point(x, y), i, j, state);
                    Spaces.Add(space);
                }
            }
        }

        public void Draw(Graphics graphics)
        {
            Spaces.ForEach(space =>
            {
                space.Draw(graphics);
            });
        }
        public bool OnClicked(int x, int y)
        {
            Space clickedSpace = GetClickedSpace(x, y);
            Space selectedSpace = GetSelectedSpace();
            if (clickedSpace != null)
            {
                switch (clickedSpace.State)
                {
                    case SpaceState.VACANT:
                        if (selectedSpace != null) //attempting a move
                        {
                            if (CheckJump(selectedSpace, clickedSpace, out string directionRelation))
                            {
                                clickedSpace.Color = selectedSpace.Color;
                                MethodInfo getNeighborMethod = GetType().GetMethod($"Get{directionRelation}", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Space) }, null);
                                Space neighbor = (Space)getNeighborMethod.Invoke(this, new object[] { selectedSpace });
                                selectedSpace.State = SpaceState.VACANT;
                                neighbor.State = SpaceState.VACANT;
                                clickedSpace.State = SpaceState.OCCUPIED;
                            }
                            else
                            {
                                selectedSpace.State = SpaceState.OCCUPIED;
                            }
                        }
                        else return false;
                        break;
                    case SpaceState.OCCUPIED:
                        if (selectedSpace != null)
                        {
                            selectedSpace.State = SpaceState.OCCUPIED; //there should be only one selected space at a time 
                        }
                        clickedSpace.State = SpaceState.SELECTED;
                        break;
                    case SpaceState.SELECTED:
                        clickedSpace.State = SpaceState.OCCUPIED;
                        break;
                }
                return true;
            }
            else if (selectedSpace != null) //clicking away
            {
                selectedSpace.State = SpaceState.OCCUPIED;
                return true;
            }
            return false;
        }

        public bool CheckForAnyJumps()
        {
            Type type = GetType();
            IEnumerable<Space> occupiedSpaces = Spaces.Where(space => space.State != SpaceState.VACANT);
            string[] directions = { "Left", "Right" };
            string[] relations = { "Parent", "Child", "Sibling" };
            return occupiedSpaces.Any(space =>
            {
                foreach (string direction in directions)
                {
                    foreach (string relation in relations)
                    {
                        MethodInfo getNeighborMethod = type.GetMethod($"Get{direction}{relation}", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Space) }, null);
                        object firstObj = getNeighborMethod.Invoke(this, new object[] { space });
                        if (firstObj != null)
                        {
                            Space firstNeighbor = (Space)firstObj;
                            if (firstNeighbor.State == SpaceState.OCCUPIED)
                            {
                                object secondObj = getNeighborMethod.Invoke(this, new object[] { firstNeighbor });
                                if (secondObj != null)
                                {
                                    Space secondNeighbor = (Space)secondObj;
                                    if (secondNeighbor.State == SpaceState.VACANT) return true;
                                }
                            }
                        }
                    }
                }
                return false;
            });
        }

        private bool CheckJump(Space selectedSpace, Space destSpace, out string directionRelation)
        {
            directionRelation = null;
            Type type = GetType();
            string[] directions = { "Left", "Right" };
            string[] relations = { "Parent", "Child", "Sibling" };
            foreach (string direction in directions)
            {
                foreach (string relation in relations)
                {
                    MethodInfo jumpMethod = type.GetMethod($"Check{direction}{relation}", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Space), typeof(Space) }, null);
                    if ((bool)jumpMethod.Invoke(this, new object[] { selectedSpace, destSpace })) {
                        directionRelation = $"{direction}{relation}";
                        return true;
                    }
                }
            }
            return false;
        }

        public int GetOccupiedSpaces()
        {
            return Spaces.Where(space => space.State == SpaceState.OCCUPIED).Count();
        }

        private bool CheckLeftParent(Space selectedSpace, Space destSpace)
        {
            Space leftParent = GetLeftParent(selectedSpace);
            return leftParent != null && leftParent.State == SpaceState.OCCUPIED && leftParent.Equals(GetRightChild(destSpace));
        }

        private bool CheckRightParent(Space selectedSpace, Space destSpace)
        {
            Space rightParent = GetRightParent(selectedSpace);
            return rightParent != null && rightParent.State == SpaceState.OCCUPIED && rightParent.Equals(GetLeftChild(destSpace));
        }

        private bool CheckLeftChild(Space selectedSpace, Space destSpace)
        {
            Space leftChild = GetLeftChild(selectedSpace);
            return leftChild != null && leftChild.State == SpaceState.OCCUPIED && leftChild.Equals(GetRightParent(destSpace));
        }

        private bool CheckRightChild(Space selectedSpace, Space destSpace)
        {
            Space rightChild = GetRightChild(selectedSpace);
            return rightChild != null && rightChild.State == SpaceState.OCCUPIED && rightChild.Equals(GetLeftParent(destSpace));
        }

        private bool CheckLeftSibling(Space selectedSpace, Space destSpace)
        {
            Space leftSibling = GetLeftSibling(selectedSpace);
            return leftSibling != null && leftSibling.State == SpaceState.OCCUPIED && leftSibling.Equals(GetRightSibling(destSpace));
        }

        private bool CheckRightSibling(Space selectedSpace, Space destSpace)
        {
            Space rightSibling = GetRightSibling(selectedSpace);
            return rightSibling != null && rightSibling.State == SpaceState.OCCUPIED && rightSibling.Equals(GetLeftSibling(destSpace));
        }

        private Space GetClickedSpace(int x, int y)
        {
            try
            {
                return Spaces.Find(s => s.IsHit(x, y));
            }
            catch (ArgumentNullException e)
            {
                return null;
            }
        }

        public Space GetSelectedSpace()
        {
            try
            {
                return Spaces.Find(s => s.State == SpaceState.SELECTED);
            }
            catch (ArgumentNullException e)
            {
                return null;
            }
        }

        private Space GetLeftParent(Space space)
        {
            try
            {
                return Spaces.Find(s => s.Row == space.Row - 1 && s.Offset == space.Offset - 1);
            }
            catch (ArgumentNullException e)
            {
                return null;
            }
        }

        private Space GetRightParent(Space space)
        {
            try
            {
                return Spaces.Find(s => s.Row == space.Row - 1 && s.Offset == space.Offset);
            }
            catch (ArgumentNullException e)
            {
                return null;
            }
        }

        private Space GetLeftSibling(Space space)
        {
            try
            {
                return Spaces.Find(s => s.Row == space.Row && s.Offset == space.Offset - 1);
            }
            catch (ArgumentNullException e)
            {
                return null;
            }
        }

        private Space GetRightSibling(Space space)
        {
            try
            {
                return Spaces.Find(s => s.Row == space.Row && s.Offset == space.Offset + 1);
            }
            catch (ArgumentNullException e)
            {
                return null;
            }
        }

        private Space GetLeftChild(Space space)
        {
            try
            {
                return Spaces.Find(s => s.Row == space.Row + 1 && s.Offset == space.Offset);
            }
            catch (ArgumentNullException e)
            {
                return null;
            }
        }

        private Space GetRightChild(Space space)
        {
            try
            {
                return Spaces.Find(s => s.Row == space.Row + 1 && s.Offset == space.Offset + 1);
            }
            catch (ArgumentNullException e)
            {
                return null;
            }
        }
    }
}
