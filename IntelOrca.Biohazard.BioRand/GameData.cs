﻿using System.Linq;

namespace IntelOrca.Biohazard.BioRand
{
    internal class GameData
    {
        public RandomizedRdt[] Rdts { get; }

        public GameData(RandomizedRdt[] rdts)
        {
            Rdts = rdts;
        }

        public RandomizedRdt? GetRdt(RdtId rtdId)
        {
            return Rdts.FirstOrDefault(x => x.RdtId == rtdId);
        }
    }
}
