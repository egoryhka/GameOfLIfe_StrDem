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
        public Game(Player p1, Player p2) { P1 = p1; P2 = p2; }

        public void Start()
        {

        }

    }
}
