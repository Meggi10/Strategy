﻿using System.Collections.Generic;

namespace Strategy
{
    public class TCell
    {
        public TGame Game;
        public int X;
        public int Y;
        public TPiece Piece;
        public int EventIdx;
        public bool Collision;
        public bool IsVisible;
        public TCell Parent;
        static int[] neighMask = new int[] { -1, 0, 0, 1, 1, 0, 0, -1, -1, -1, -1, 1, 1, 1, 1, -1 };
        public List<TCell> Neighbors
        {
            get
            {
                var neighbors = new List<TCell>();
                for (int i = 0; i < neighMask.Length; i+=2)
                    {
                        var y = Y + neighMask[i];
                        var x = X + neighMask[i + 1];
                        if (x < 0 || x >= Game.Map.Width) continue;
                        if (y < 0 || y >= Game.Map.Height) continue;
                        var cell = Game.Cells[y, x];
                        neighbors.Add(cell);
                    }
                return neighbors;
            }
        }
    }
}