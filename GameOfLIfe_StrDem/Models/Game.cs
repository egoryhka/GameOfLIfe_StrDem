using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfLIfe_StrDem.Models
{
    public class Game
    {
        public Player P1 { get; private set; }
        public Player P2 { get; private set; }
        private bool[,] _p1Field, _p2Field;
        public Game(Player p1, Player p2)
        {
            P1 = p1; P2 = p2;
            _p1Field = new bool[GameSettings.FieldSize, GameSettings.FieldSize];
            _p2Field = new bool[GameSettings.FieldSize, GameSettings.FieldSize];
        }

        public void Start()
        {

        }

        public void SetCell(int x, int y, bool alive)
        {

        }

        private void Step()
        {

        }

    }
}
