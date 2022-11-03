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
        public GameState State { get; set; }
        public Game(Player p1, Player p2) { P1 = p1; P2 = p2; }

        public void Start()
        {

        }

        public Player GetOpponent(Player me) => P1 == me ? P2 : P1;

        public Player GetWinner()
        {
            int p1C = P1.Field.GetAliveCellsCount();
            int p2C = P2.Field.GetAliveCellsCount();

            if (p1C == p2C) return null;
            return p1C > p2C ? P1 : P2;
        }
    }

    public enum GameState
    {
        Starting,
        Drawing,
        Simulation,
        End,
    }
}
