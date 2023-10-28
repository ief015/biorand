﻿using System;
using System.IO;
using System.Linq;
using IntelOrca.Biohazard.Script.Opcodes;

namespace IntelOrca.Biohazard.BioRand.RE1
{
    internal class Re1DoorHelper : IDoorHelper
    {
        public byte[] GetReservedLockIds() => new byte[0];

        public void Begin(RandoConfig config, GameData gameData, Map map)
        {
            ApplyPatches(config, gameData);

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
                    door.Target = GetRE1FixedId(target);
                }
            }
        }

        public void End(RandoConfig config, GameData gameData, Map map)
        {
            if (config.RandomDoors)
            {
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
            else
            {
                var rdt104 = gameData.GetRdt(new RdtId(0, 0x04));
                if (rdt104 != null)
                {
                    var door = rdt104.Doors.FirstOrDefault(x => x.Id == 2);
                    if (door != null)
                    {
                        door.NextX = 12700;
                        door.NextY = -7200;
                        door.NextZ = 3300;
                    }
                }


                // FixStageDoor(gameData, new RdtId(1, 0x0B), 5, 6);
                // FixStageDoor(gameData, new RdtId(6, 0x0C), 0);
                // 
                // FixStageDoor(gameData, new RdtId(5, 0x04), 1, 0);
                // FixStageDoor(gameData, new RdtId(5, 0x0F), 0);

                // var rdt104 = gameData.GetRdt(new RdtId(0, 0x04));
                // if (rdt104 != null)
                // {
                //     var barDoor = (DoorAotSeOpcode)rdt104.Doors.First(x => x.Target.Room == 0x0F);
                //     rdt104.Nop(barDoor.Offset);
                //     rdt104.AdditionalOpcodes.Add(new UnknownOpcode(0, 0x01, new byte[] { 0x20 }));
                //     rdt104.AdditionalOpcodes.Add(new UnknownOpcode(0, 0x04, new byte[] { 0x00, 0x00, 0x00 }));
                //     rdt104.AdditionalOpcodes.Add(barDoor);
                //     barDoor.NextStage = 5;
                //     rdt104.AdditionalOpcodes.Add(new UnknownOpcode(0, 0x02, new byte[] { 0x1C }));
                //     barDoor = CloneDoor(barDoor);
                //     barDoor.NextStage = 0;
                //     rdt104.AdditionalOpcodes.Add(barDoor);
                // }

                // var missingRooms = Re1Randomiser.MissingRooms.ToHashSet();
                // foreach (var rdt in gameData.Rdts)
                // {
                //     var currentStage = rdt.RdtId.Stage;
                //     if (missingRooms.Contains(rdt.RdtId))
                //     {
                //         // foreach (var door in rdt.Doors)
                //         // {
                //         //     var target = door.Target;
                //         //     door.Target = new RdtId(rdt.RdtId.Stage, target.Room);
                //         // }
                //     }
                //     else
                //     {
                //         foreach (var door in rdt.Doors)
                //         {
                //             var target = door.Target;
                //             if (target.Stage == 0xFF)
                //                 target = new RdtId(currentStage, target.Room);
                //             if (missingRooms.Contains(target))
                //             {
                //                 door.Target = new RdtId(currentStage + 5, target.Room);
                //             }
                //         }
                //     }
                // }
            }
        }

        private static void FixStageDoor(GameData gameData, RdtId rdtId, int doorId, int? forceStage = null)
        {
            var stageM1 = (byte)(rdtId.Stage == 0 || rdtId.Stage == 5 ? 0 : 1);
            var stageM2 = (byte)(stageM1 + 5);

            var rdt = gameData.GetRdt(rdtId);
            if (rdt != null)
            {
                var door = (DoorAotSeOpcode)rdt.Doors.First(x => x.Id == doorId);
                if (forceStage != null)
                {
                    door.NextStage = (byte)forceStage.Value;
                }
                else
                {
                    rdt.Nop(door.Offset);
                    rdt.AdditionalOpcodes.Add(new UnknownOpcode(0, 0x01, new byte[] { 0x20 }));
                    rdt.AdditionalOpcodes.Add(new UnknownOpcode(0, 0x04, new byte[] { 0x00, 0x00, 0x00 }));
                    rdt.AdditionalOpcodes.Add(door);
                    door.NextStage = stageM2;
                    rdt.AdditionalOpcodes.Add(new UnknownOpcode(0, 0x02, new byte[] { 0x1C }));
                    door = CloneDoor(door);
                    door.NextStage = stageM1;
                    rdt.AdditionalOpcodes.Add(door);
                }
            }
        }

        private static DoorAotSeOpcode CloneDoor(IDoorAotSetOpcode src)
        {
            using var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            ((OpcodeBase)src).Write(bw);

            ms.Position = 0;
            var br = new BinaryReader(ms);
            var result = DoorAotSeOpcode.Read(br, 0);
            return result;
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

        private RdtId GetRE1FixedId(RdtId rdtId)
        {
            if (Re1Randomiser.MissingRooms.Contains(rdtId))
            {
                if (rdtId.Stage == 0 || rdtId.Stage == 1)
                    return new RdtId(rdtId.Stage + 5, rdtId.Room);
                else if (rdtId.Stage == 5 || rdtId.Stage == 6)
                    return new RdtId(rdtId.Stage - 5, rdtId.Room);
            }
            return rdtId;
        }

        private void ApplyPatches(RandoConfig config, GameData gameData)
        {
            ShotgunOnWallFix(config, gameData);
            AllowPartnerItemBoxes(gameData);

            if (!config.RandomDoors)
                return;

            ForceTyrant2(gameData);
        }

        private void ShotgunOnWallFix(RandoConfig config, GameData gameData)
        {
            if (!config.RandomItems)
                return;

            var rdt = gameData.GetRdt(new RdtId(0, 0x16));
            if (rdt == null)
                return;

            rdt.Nop(0x1FE16);
        }

        private void AllowPartnerItemBoxes(GameData gameData)
        {
            // Remove partner check for these two item boxes
            // This is so Rebecca can use the item boxes
            // Important for Chris 8-inventory because the inventory
            // is now shared for both him and Rebecca and player
            // might need to make space for more items e.g. (V-JOLT)
            var room = gameData.GetRdt(new RdtId(0, 0x00));
            room?.Nop(0x10C92);

            room = gameData.GetRdt(new RdtId(3, 0x03));
            room?.Nop(0x1F920);
        }

        private void ForceTyrant2(GameData gameData)
        {
            var room = gameData.GetRdt(new RdtId(2, 0x03));
            room?.AdditionalOpcodes.Add(new UnknownOpcode(0, 0x05, new byte[] { 0x00, 43, 0 }));
        }
    }
}
