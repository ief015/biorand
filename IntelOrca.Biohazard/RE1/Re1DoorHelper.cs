﻿using System;

namespace IntelOrca.Biohazard.RE1
{
    internal class Re1DoorHelper : IDoorHelper
    {
        public void Begin(RandoConfig config, GameData gameData, Map map)
        {
            if (!config.RandomDoors)
                return;

            // For RE 1 doors that have 0 as target stage, that means keep the stage
            // the same. This replaces every door with an explicit stage to simplify things
            foreach (var rdt in gameData.Rdts)
            {
                if (!ShouldFixRE1Rdt(config, map, rdt.RdtId))
                    continue;

                foreach (var door in rdt.Doors)
                {
                    var target = door.Target;
                    if (target.Stage == 255)
                        target = new RdtId(rdt.RdtId.Stage, target.Room);
                    door.Target = GetRE1FixedId(map, target);
                }
            }
        }

        public void End(RandoConfig config, GameData gameData, Map map)
        {
            if (!config.RandomDoors)
                return;

            // Revert the door ID changes we made in begin
            // It probably isn't necessary that we do this, but it seems neater
            foreach (var rdt in gameData.Rdts)
            {
                if (!ShouldFixRE1Rdt(config, map, rdt.RdtId))
                    continue;

                foreach (var door in rdt.Doors)
                {
                    var target = door.Target;
                    if (target.Stage == rdt.RdtId.Stage)
                    {
                        target = new RdtId(255, target.Room);
                        door.Target = target;
                    }
                }
            }
        }

        private bool ShouldFixRE1Rdt(RandoConfig config, Map map, RdtId rdtId)
        {
            var room = map.GetRoom(rdtId);
            if (room == null || room.DoorRando == null)
                return true;

            foreach (var spec in room.DoorRando)
            {
                if (spec.Player != null && spec.Player != config.Player)
                    continue;
                if (spec.Scenario != null && spec.Scenario != config.Scenario)
                    continue;

                if (spec.Category != null)
                {
                    var category = (DoorRandoCategory)Enum.Parse(typeof(DoorRandoCategory), spec.Category, true);
                    if (category == DoorRandoCategory.Exclude)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private RdtId GetRE1FixedId(Map map, RdtId rdtId)
        {
            var rooms = map.Rooms!;
            if (rdtId.Stage == 0 || rdtId.Stage == 1)
            {
                if (!rooms.ContainsKey(rdtId.ToString()))
                    return new RdtId(rdtId.Stage + 5, rdtId.Room);
            }
            else if (rdtId.Stage == 5 || rdtId.Stage == 6)
            {
                if (!rooms.ContainsKey(rdtId.ToString()))
                    return new RdtId(rdtId.Stage - 5, rdtId.Room);
            }
            return rdtId;
        }
    }
}