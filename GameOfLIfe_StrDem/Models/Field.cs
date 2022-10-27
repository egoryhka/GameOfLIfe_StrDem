using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfLIfe_StrDem.Models
{
    public class Field
    {
        public Field() => cells = new bool[GameSettings.FieldSize, GameSettings.FieldSize];
        private bool[,] cells;

        public void Clean() => cells = new bool[GameSettings.FieldSize, GameSettings.FieldSize];

        public void SetCell(int x, int y, bool alive)
        {
            if (x >= 0 && x < cells.GetLength(0) && y >= 0 && y < cells.GetLength(1))
                cells[x, y] = alive;
        }

        public void SimulationStep()
        {
            // GOF
        }

    }
}
