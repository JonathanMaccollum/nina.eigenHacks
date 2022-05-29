using System;

namespace nina.eigenHacks.Synchronization
{
    public struct CrossProcessBarrierState
    {
        public CrossProcessBarrierState(
            int participantId, 
            string tag = null,
            CrossProcessBarrierStatus? status=null,
            DateTime? heartBeat=null)
        {
            ParticipantId = participantId;
            Status = status ?? CrossProcessBarrierStatus.NotStarted;
            Heartbeat = heartBeat??DateTime.Now;
            Tag = tag??string.Empty;
        }

        public int ParticipantId { get; }
        public CrossProcessBarrierStatus Status { get; }
        public DateTime Heartbeat { get; }
        public string Tag { get; }

        public CrossProcessBarrierState WithUpdatedStatus(CrossProcessBarrierStatus status) =>
            new CrossProcessBarrierState(ParticipantId, Tag, status);
        public CrossProcessBarrierState WithUpdatedTags(string tag) =>
            new CrossProcessBarrierState(ParticipantId, tag, Status);
    }
}
