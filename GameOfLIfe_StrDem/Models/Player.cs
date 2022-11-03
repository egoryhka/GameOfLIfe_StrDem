using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace GameOfLIfe_StrDem.Models
{
    public class Player
    {
        public string Id { get; set; }
        public string GameId { get; set; }

        public string Name { get; set; }
        public int Points { get; private set; }
        public Field Field { get; set; } = new Field();

        public bool Inviting { get; set; }
        public bool Filtering { get; set; }
        public bool Ready { get; set; }
        public bool ReadyToNextState { get; set; }


        public Player(string id, string name)
        {
            Id = id; Name = name;
        }

        public void AddPoints() => Points++;

    }

    public class PlayerDto
    {
        public string Id { get; set; }
        public string GameId { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }

        public bool Inviting { get; set; }
        public bool Filtering { get; set; }
        public bool Ready { get; set; }
    }

    public class PlayerToDtoProfile : Profile
    {
        public PlayerToDtoProfile()
        {
            CreateMap<Player, PlayerDto>();
        }
    }
}
