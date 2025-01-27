﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IntelOrca.Biohazard.Extensions;
using IntelOrca.Biohazard.Model;
using IntelOrca.Biohazard.Room;
using IntelOrca.Biohazard.Script;
using IntelOrca.Biohazard.Script.Opcodes;

namespace IntelOrca.Biohazard.BioRand
{
    public class RandomizedRdt
    {
        private List<(EmrFlags, double)> _emrScales = new List<(EmrFlags, double)>();

        public BioVersion Version => RdtFile.Version;
        public IRdt RdtFile { get; set; }
        public RdtId RdtId { get; }
        public string? OriginalPath { get; set; }
        public string? ModifiedPath { get; set; }
        public string? Script { get; set; }
        public string? ScriptDisassembly { get; set; }
        public string? ScriptListing { get; set; }
        public OpcodeBase[] Opcodes { get; set; } = new OpcodeBase[0];
        public ScriptAst? Ast { get; set; }
        public List<KeyValuePair<int, byte>> Patches { get; } = new List<KeyValuePair<int, byte>>();
        public List<OpcodeBase> AdditionalOpcodes { get; } = new List<OpcodeBase>();

        public IEnumerable<OpcodeBase> AllOpcodes => AdditionalOpcodes.Concat(Opcodes);
        public IEnumerable<IDoorAotSetOpcode> Doors => AllOpcodes.OfType<IDoorAotSetOpcode>();
        public IEnumerable<SceEmSetOpcode> Enemies => AllOpcodes.OfType<SceEmSetOpcode>();
        public IEnumerable<IItemAotSetOpcode> Items => AllOpcodes.OfType<IItemAotSetOpcode>();
        public IEnumerable<AotResetOpcode> Resets => AllOpcodes.OfType<AotResetOpcode>();
        public IEnumerable<SceItemGetOpcode> ItemGets => AllOpcodes.OfType<SceItemGetOpcode>();
        public IEnumerable<XaOnOpcode> Sounds => AllOpcodes.OfType<XaOnOpcode>();

        public RandomizedRdt(IRdt rdtFile, RdtId rdtId)
        {
            RdtFile = rdtFile;
            RdtId = rdtId;
        }

        public void SetDoorTarget(int id, RdtId target, DoorEntrance destination, RdtId originalId, bool noCompareRewrite = false)
        {
            foreach (var door in Doors)
            {
                if (door.Id == id)
                {
                    door.NextX = destination.X;
                    door.NextY = destination.Y;
                    door.NextZ = destination.Z;
                    door.NextD = destination.D;
                    door.Target = target;
                    door.NextCamera = destination.Camera;
                    door.NextFloor = destination.Floor;
                }
            }
            foreach (var reset in Resets)
            {
                if (reset.Id == id && reset.SCE == 1)
                {
                    reset.NextX = destination.X;
                    reset.NextY = destination.Y;
                    reset.NextZ = destination.Z;
                }
            }
            if (!noCompareRewrite)
            {
                foreach (var cmp in Opcodes.OfType<CmpOpcode>())
                {
                    var oldValue = (short)(((originalId.Stage + 1) << 8) | originalId.Room);
                    var newValue = (short)(((target.Stage + 1) << 8) | target.Room);
                    if (cmp.Flag == 27 && cmp.Value == oldValue)
                    {
                        cmp.Value = newValue;
                    }
                }
            }
        }

        public void EnsureDoorUnlock(int id, byte lockId)
        {
            foreach (var door in Doors)
            {
                if (door.Id == id)
                {
                    door.LockId = lockId;
                    if (door.LockType == 0)
                        door.LockType = 254;
                }
            }
        }

        public void SetDoorLock(int id, byte lockId, byte lockType = 255)
        {
            foreach (var door in Doors)
            {
                if (door.Id == id)
                {
                    door.LockId = lockId;
                    door.LockType = lockType;
                }
            }
        }

        public void SetDoorUnlock(int id, byte lockId) => SetDoorLock(id, lockId, 254);

        public void RemoveDoorUnlock(int id)
        {
            foreach (var door in Doors)
            {
                if (door.Id == id && door.LockType == 254)
                {
                    door.LockId = 0;
                    door.LockType = 0;
                }
            }
        }

        public void RemoveDoorLock(int id) => SetDoorLock(id, 0, 0);

        public void SetItem(byte id, ushort type, ushort amount)
        {
            foreach (var item in Items)
            {
                if (item.Id == id)
                {
                    item.Type = type;
                    item.Amount = amount;
                }
            }
            foreach (var reset in Resets)
            {
                if (reset.Id == id && reset.SCE == 2 && reset.Data0 != 0)
                {
                    reset.Data0 = type;
                    reset.Data1 = amount;
                }
            }
        }

        public void SetItemAt(int offset, ushort type, ushort amount)
        {
            var opcode = Opcodes.FirstOrDefault(x => x.Offset == offset);
            if (opcode is IItemAotSetOpcode item)
            {
                item.Type = type;
                item.Amount = amount;
            }
            else if (opcode is AotResetOpcode reset)
            {
                reset.Data0 = type;
                reset.Data1 = amount;
            }
        }

        public void SetEnemy(byte id, byte type, byte state, byte ai, byte soundBank, byte texture)
        {
            foreach (var enemy in Enemies)
            {
                if (enemy.Id == id)
                {
                    enemy.Type = type;
                    enemy.State = state;
                    enemy.Ai = ai;
                    enemy.SoundBank = soundBank;
                    enemy.Texture = texture;
                }
            }
        }

        public void Nop(int offset)
        {
            Nop(offset, offset);
        }

        public void Nop(int beginOffset, int endOffset)
        {
            for (int i = 0; i < Opcodes.Length; i++)
            {
                var opcode = Opcodes[i];
                if (opcode.Offset >= beginOffset && opcode.Offset <= endOffset)
                {
                    if (!(opcode is UnknownOpcode unk))
                    {
                        unk = new UnknownOpcode(opcode.Offset, 0, new byte[opcode.Length - 1]);
                        Opcodes[i] = unk;
                    }
                    unk.NopOut(Version);
                }
                else if (opcode.Offset > endOffset)
                {
                    break;
                }
            }
        }

        public IDoorAotSetOpcode ConvertToDoor(byte id, byte texture, byte? key, byte? lockId)
        {
            var aotSet = Opcodes.OfType<IAotSetOpcode>().LastOrDefault(x => x.Id == id);
            if (aotSet == null)
                throw new Exception($"Failed to find aot_set for id {id}");

            IDoorAotSetOpcode door;
            if (aotSet is AotSetOpcode aotSet2p)
            {
                if (Version == BioVersion.Biohazard1)
                {
                    door = new DoorAotSeOpcode()
                    {
                        Length = 26,
                        Opcode = (byte)OpcodeV1.DoorAotSe,
                        Id = aotSet2p.Id,
                        X = aotSet2p.X,
                        Z = aotSet2p.Z,
                        W = aotSet2p.W,
                        D = aotSet2p.D,
                        Re1UnkA = 3,
                        Re1UnkB = 0,
                        Animation = texture,
                        Re1UnkC = 7,
                        LockId = lockId ?? 0,
                        LockType = key ?? 0,
                        Free = 129
                    };
                }
                else
                {
                    door = new DoorAotSeOpcode()
                    {
                        Length = 32,
                        Opcode = Version == BioVersion.Biohazard2 ? (byte)OpcodeV2.DoorAotSe : (byte)OpcodeV3.DoorAotSe,
                        Id = aotSet2p.Id,
                        SCE = 1,
                        SAT = aotSet2p.SAT,
                        Floor = aotSet2p.Floor,
                        Super = aotSet2p.Super,
                        X = aotSet2p.X,
                        Z = aotSet2p.Z,
                        W = aotSet2p.W,
                        D = aotSet2p.D,
                        Texture = texture,
                        LockType = key ?? 0,
                        LockId = lockId ?? 0
                    };
                }
            }
            else if (aotSet is AotSet4pOpcode aotSet4p)
            {
                door = new DoorAotSet4pOpcode()
                {
                    Length = 40,
                    Opcode = Version == BioVersion.Biohazard2 ? (byte)OpcodeV2.DoorAotSet4p : (byte)OpcodeV3.DoorAotSet4p,
                    Id = aotSet.Id,
                    SCE = 1,
                    SAT = aotSet.SAT,
                    Floor = aotSet.Floor,
                    Super = aotSet.Super,
                    X0 = aotSet4p.X0,
                    Z0 = aotSet4p.Z0,
                    X1 = aotSet4p.X1,
                    Z1 = aotSet4p.Z1,
                    X2 = aotSet4p.X2,
                    Z2 = aotSet4p.Z2,
                    X3 = aotSet4p.X3,
                    Z3 = aotSet4p.Z3,
                    Texture = texture,
                    LockType = key ?? 0,
                    LockId = lockId ?? 0
                };
            }
            else
            {
                throw new NotSupportedException("Unexpected door kind.");
            }
            AdditionalOpcodes.Add((OpcodeBase)door);
            Nop(aotSet.Offset);
            return door;
        }

        public int? ScaleEmrY(EmrFlags flags, double scale)
        {
            if (_emrScales.Any(x => x.Item1 == flags))
                return null;

            var rbj = ((Rdt2)RdtFile).RBJ;
            for (int i = 0; i < rbj.Count; i++)
            {
                var emrFlags = rbj[i].Flags;
                if ((emrFlags & flags) != 0)
                {
                    _emrScales.Add((flags, scale));
                    return i;
                }
            }
            return null;
        }

        public void Save()
        {
            using (var ms = new MemoryStream(RdtFile.Data.ToArray()))
            {
                var bw = new BinaryWriter(ms);
                foreach (var opcode in Opcodes)
                {
                    ms.Position = opcode.Offset;
                    opcode.Write(bw);
                }
                foreach (var patch in Patches)
                {
                    ms.Position = patch.Key;
                    bw.Write(patch.Value);
                }
                if (Version == BioVersion.Biohazard1)
                    RdtFile = new Rdt1(ms.ToArray());
                else
                    RdtFile = new Rdt2(Version, ms.ToArray());
            }

            // HACK do not play around with EMRs for RE 2, 409 because it crashes the room
            if (!(Version == BioVersion.Biohazard2 && RdtId == new RdtId(3, 9)))
                UpdateEmrs();
            PrependOpcodes();

            Directory.CreateDirectory(Path.GetDirectoryName(ModifiedPath!)!);
            File.WriteAllBytes(ModifiedPath!, RdtFile.Data.ToArray());
        }

        private void UpdateEmrs()
        {
            if (_emrScales.Count == 0)
                return;

            var rdtBuilder = ((Rdt2)RdtFile).ToBuilder();
            var rbjBuilder = rdtBuilder.RBJ.ToBuilder();
            foreach (var (flags, scale) in _emrScales)
            {
                for (int i = 0; i < rbjBuilder.Animations.Count; i++)
                {
                    var animation = rbjBuilder.Animations[i];
                    var emrFlags = animation.Flags;
                    if (emrFlags == flags)
                    {
                        rbjBuilder.Animations[i] = animation.WithEmr(animation.Emr.Scale(scale));
                    }
                    else if ((emrFlags & flags) != 0)
                    {
                        rbjBuilder.Animations.Add(animation.WithFlags(emrFlags & ~flags));
                        rbjBuilder.Animations[i] = animation
                            .WithFlags(flags)
                            .WithEmr(animation.Emr.Scale(scale));
                    }
                }
            }
            rdtBuilder.RBJ = rbjBuilder.ToRbj();
            RdtFile = rdtBuilder.ToRdt();
        }

        private void PrependOpcodes()
        {
            if (AdditionalOpcodes.Count == 0)
                return;

            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            foreach (var opcode in AdditionalOpcodes)
            {
                opcode.Write(bw);
            }

            if (RdtFile is Rdt1 rdt1)
            {
                var rdtBuilder = rdt1.ToBuilder();
                var scdBuilder = rdtBuilder.InitSCD.ToBuilder();
                bw.Write(scdBuilder.Procedures[0].Data);
                scdBuilder.Procedures[0] = new ScdProcedure(BioVersion.Biohazard1, ms.ToArray());
                rdtBuilder.InitSCD = scdBuilder.ToContainer();
                RdtFile = rdtBuilder.ToRdt();
            }
            else if (RdtFile is Rdt2 rdt2)
            {
                var rdtBuilder = rdt2.ToBuilder();
                var scdBuilder = rdtBuilder.SCDINIT.ToBuilder();
                bw.Write(scdBuilder.Procedures[0].Data);
                scdBuilder.Procedures[0] = new ScdProcedure(scdBuilder.Version, ms.ToArray());
                rdtBuilder.SCDINIT = scdBuilder.ToProcedureList();
                RdtFile = rdtBuilder.ToRdt();
            }
        }

        public void Print()
        {
            Console.WriteLine(RdtId);
            Console.WriteLine("------------------------");
            foreach (var door in Doors)
            {
                Console.WriteLine("DOOR  #{0:X2}: {1:X}{2:X2} {3} {4} {5} ({6})",
                    door.Id,
                    door.NextStage + 1,
                    door.NextRoom,
                    door.Animation,
                    door.LockId,
                    door.LockType,
                    door.LockType == 0xFF ? "side" : door.LockType.ToString());
            }
            foreach (var item in Items)
            {
                Console.WriteLine("ITEM  #{0:X2}: {1} x{2}",
                    item.Id,
                    item.Type,
                    item.Amount);
            }
            foreach (var reset in Resets)
            {
                if (Items.Any(x => x.Id == reset.Id))
                {
                    Console.WriteLine("RESET #{0:X2}: {1} x{2}",
                        reset.Id,
                        reset.Data0,
                        reset.Data1);
                }
            }
            Console.WriteLine("------------------------");
            Console.WriteLine();
        }

        public override string ToString()
        {
            return RdtId.ToString();
        }

        internal struct NopSequence
        {
            public NopSequence(int offset, int length)
            {
                Offset = offset;
                Length = length;
            }

            public int Offset { get; set; }
            public int Length { get; set; }
        }
    }
}
