using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfLIfe_StrDem.Models
{
    public class Player
    {
        public string Id { get; set; }
        public string GameId { get; set; }

        public string Name { get; set; }
        public Field Field { get; set; } = new Field();

        public bool Inviting { get; set; }
        public bool Filtering { get; set; }

        public Player(string id, string name)
        {
            Id = id; Name = name;
        }

    }
}
