using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.IO;

namespace nina.eigenHacks.Synchronization
{
    public static class CrossProcessBarrierStateReader
    {
        internal static int ReadParticipantCount(this MemoryMappedFile mmf)
        {
            using (var a = mmf.CreateViewAccessor())
            {
                return a.ReadInt32(0);
            }
        }
        internal static List<CrossProcessBarrierState> ReadState(this MemoryMappedFile mmf)
        {
            using (var s = mmf.CreateViewStream())
            using (var reader = new BinaryReader(s))
            {
                s.Position = 0;
                var participants = reader.ReadInt32();
                var results = new List<CrossProcessBarrierState>(participants);
                for(var i =0; i< participants; i++)
                {
                    var p = new CrossProcessBarrierState(
                        reader.ReadInt32(),
                        reader.ReadString(),
                        (CrossProcessBarrierStatus)reader.ReadInt32(),
                        DateTime.FromBinary(reader.ReadInt64())
                        );
                    results.Add(p);
                }
                return results;
            }
        }
        internal static void WriteState(this MemoryMappedFile mmf, List<CrossProcessBarrierState> state)
        {
            using (var s = mmf.CreateViewStream())
            using(var writer=new BinaryWriter(s))
            {
                s.Position = 0;
                writer.Write(state.Count);
                foreach(var x in state)
                {
                    writer.Write(x.ParticipantId);
                    writer.Write(x.Tag);
                    writer.Write((int)x.Status);
                    writer.Write(x.Heartbeat.ToBinary());
                }
            }
        }
    }
}
