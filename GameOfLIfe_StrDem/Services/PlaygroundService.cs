using GameOfLIfe_StrDem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfLIfe_StrDem.Services
{
    public class PlaygroundService
    {
        public List<Player> Players { get; set; } = new List<Player>() /*TestForView*/;

        private static List<Player> TestForView => new List<Player>()
        {
            new Player("","Name"),
            new Player("","Name1"),
            new Player("","Name2"),
            new Player("","Name3"),
            new Player("","Name4"),
            new Player("","Name5"),
            new Player("","Name6"),
            new Player("","Name7"),
            new Player("","Name8"),
            new Player("","Name9"),
            new Player("","NameLongNameLongNameLongNameLongNameLong"),
            new Player("","Name"),
            new Player("","Name"),
            new Player("","Name"),
            new Player("","Name"),
            new Player("","Name"),
            new Player("","Name"),
            new Player("","Name"),
        };
    }
}
