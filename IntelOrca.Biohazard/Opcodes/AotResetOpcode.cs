﻿using System.Diagnostics;
using System.IO;
using IntelOrca.Biohazard.Opcodes;

namespace IntelOrca.Biohazard
{
    [DebuggerDisplay("{Opcode}")]
    internal class AotResetOpcode : OpcodeBase
    {
        public override Opcode Opcode => Opcode.AotReset;
        public override int Length => 10;

        public byte Id { get; set; }
        public byte SCE { get; set; }
        public byte SAT { get; set; }
        public ushort Data0 { get; set; }
        public ushort Data1 { get; set; }
        public ushort Data2 { get; set; }

        public static AotResetOpcode Read(BinaryReader br, int offset)
        {
            return new AotResetOpcode()
            {
                Offset = offset,
                Id = br.ReadByte(),
                SCE = br.ReadByte(),
                SAT = br.ReadByte(),
                Data0 = br.ReadUInt16(),
                Data1 = br.ReadUInt16(),
                Data2 = br.ReadUInt16(),
            };
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write((byte)Opcode);
            bw.Write(Id);
            bw.Write(SCE);
            bw.Write(SAT);
            bw.Write(Data0);
            bw.Write(Data1);
            bw.Write(Data2);
        }
    }
}