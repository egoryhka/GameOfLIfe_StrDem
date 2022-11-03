using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfLIfe_StrDem.Models
{
    public class Field
    {
        public Field() => cells = new bool[GameSettings.FieldSize, GameSettings.FieldSize];
        public bool[,] cells { get; set; }

        public void Clean() => cells = new bool[GameSettings.FieldSize, GameSettings.FieldSize];

        public void SetCell(int x, int y, bool alive)
        {
            if (x >= 0 && x < cells.GetLength(0) && y >= 0 && y < cells.GetLength(1))
                cells[x, y] = alive;
        }

        public int GetAliveCellsCount()
        {
            int aliveCount = 0;
            for (int i = 0; i < GameSettings.FieldSize; i++)
            {
                for (int j = 0; j < GameSettings.FieldSize; j++)
                {
                    if (cells[i, j]) aliveCount++;
                }
            }
            return aliveCount;
        }

        public void SimulationStep()
        {
            bool[,] buf = (bool[,])cells.Clone();

            // GOF
            for (int i = 0; i < GameSettings.FieldSize; i++)
            {
                for (int j = 0; j < GameSettings.FieldSize; j++)
                {
                    bool cell = cells[i, j];
                    int neighbourCount = 0;

                    if (CheckBounds(i - 1, j - 1) && cells[i - 1, j - 1]) neighbourCount++;
                    if (CheckBounds(i - 1, j) && cells[i - 1, j]) neighbourCount++;
                    if (CheckBounds(i - 1, j + 1) && cells[i - 1, j + 1]) neighbourCount++;
                    if (CheckBounds(i, j + 1) && cells[i, j + 1]) neighbourCount++;
                    if (CheckBounds(i + 1, j + 1) && cells[i + 1, j + 1]) neighbourCount++;
                    if (CheckBounds(i + 1, j) && cells[i + 1, j]) neighbourCount++;
                    if (CheckBounds(i + 1, j - 1) && cells[i + 1, j - 1]) neighbourCount++;
                    if (CheckBounds(i, j - 1) && cells[i, j - 1]) neighbourCount++;

                    // Правила
                    if (!cell && neighbourCount == 3) buf[i, j] = true;
                    if (cell && (neighbourCount < 2 || neighbourCount > 3)) buf[i, j] = false;

                }
            }

            cells = buf;
        }

        private bool CheckBounds(int i, int j) => i >= 0 && i < GameSettings.FieldSize && j >= 0 && j < GameSettings.FieldSize;

    }
}
