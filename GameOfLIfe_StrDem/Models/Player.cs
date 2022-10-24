using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfLIfe_StrDem.Models
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int WinCount { get; set; }
        public int LoseCount { get; set; }
        public int GamesCount { get; set; }
        public float Skill { get; private set; }

        public bool InGame { get; set; }
        public string GameId { get; set; }

        public Player(string id, string name)
        {
            Id = id; Name = name;
        }


        private float oldest = float.NaN;
        public void RecalcSkill(int aliveCellsCount, int windowWidth)
        {
            float percentOfAlive = 100f * aliveCellsCount / (GameSettings.FieldSize * GameSettings.FieldSize);

            if (oldest == float.NaN)
            {
                oldest = percentOfAlive;
                Skill = oldest;
            }
            else
            {
                //if (GamesCount < windowWidth)
                //{
                //    sum += point;
                //    w++;
                //}
                //else
                //{
                //    oldest = windowPoints.Dequeue();
                //    sum += point - oldest;

                //}
            }
        }


    }
}
