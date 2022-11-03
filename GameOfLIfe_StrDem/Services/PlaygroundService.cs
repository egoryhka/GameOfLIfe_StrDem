using GameOfLIfe_StrDem.Models;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLIfe_StrDem.Services
{
    public class PlaygroundService
    {
        public List<Player> Players { get; set; } = new List<Player>() /*TestForView*/;
        public Dictionary<string, Game> Games { get; set; } = new Dictionary<string, Game>();

        private static List<Player> TestForView => new List<Player>()
        {
            new Player("","twitterboy"),
            new Player("","Cash choppa"),
            new Player("","БАЗЗИКС tali"),
            new Player("","lil Shev"),
            new Player("","ArtPtz"),
            new Player("","Lil_rayzik"),
            new Player("","HANTLEYS"),
            new Player("","lil Xanex"),
            new Player("","nickolas"),
            new Player("","bullys"),
            new Player("","Big Niko"),
            new Player("","lil freat"),
            new Player("","RRYAN"),
            new Player("","triplefake"),
            new Player("","Insulter"),
            new Player("","JeryL"),
            new Player("","Pulito"),
            new Player("","Danki"),
            new Player("","YEKK180"),
            new Player("","Holar"),
            new Player("","MC Nells"),
            new Player("","Slvme Gugio"),
            new Player("","mbo SouthPaw"),
            new Player("","Marijan"),
            new Player("","Annakonda"),
            new Player("","Narkoekho"),
            new Player("","OG Livan"),
            new Player("","McPleh"),
            new Player("","5.R.S.H T"),
            new Player("","OMMY NINETANNameLongNameLongNameLongNameLongNameLong"),
        };


        public Player GetPlayer(string Id) => Players.FirstOrDefault(x => x.Id == Id);
    }
}
