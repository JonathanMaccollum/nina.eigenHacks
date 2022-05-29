using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace nina.eigenHacks.Synchronization
{

    public sealed class CrossProcessBarrier : IDisposable, ICrossProcessBarrier
    {
        private bool isDisposed = false;
        private readonly Mutex SynchronizedAccessToMMF;
        private readonly MemoryMappedFile MMF;
        private readonly int participantId;
        private readonly string name;
        public string Name => name;
        public CrossProcessBarrier(string instanceName)
        {
            this.name = instanceName;
            SynchronizedAccessToMMF = new Mutex(false, typeof(CrossProcessBarrier).FullName + ".Mutex." + instanceName);
            MMF = MemoryMappedFile.CreateOrOpen(typeof(CrossProcessBarrier).FullName + ".MMF." + instanceName, 2048);

            SynchronizedAccessToMMF.WaitOne();
            try
            {
                var currentState = MMF.ReadState();
                participantId = currentState.Count + 1;
                currentState.Add(new CrossProcessBarrierState(participantId, null, CrossProcessBarrierStatus.NotStarted));
                MMF.WriteState(currentState);
            }
            finally
            {
                this.SynchronizedAccessToMMF.ReleaseMutex();
            }
        }

        public int GetTotalParticipantCount() =>
            MMF.ReadParticipantCount();

        public IEnumerable<CrossProcessBarrierState> GetAllParticipantState() =>
            MMF.ReadState().AsReadOnly();
        public bool AreAllActiveParticipantsReadyAndWaiting(string tag)
        {
            SynchronizedAccessToMMF.WaitOne();
            try
            {
                var peers = GetAllParticipantState()
                    .Where(x => x.Status == CrossProcessBarrierStatus.Started
                    || x.Status == CrossProcessBarrierStatus.ReadyAndWaiting);
                if (string.IsNullOrWhiteSpace(tag))
                    return !peers
                        .Any(s => s.Status == CrossProcessBarrierStatus.Started);

                return peers.All(s =>
                    s.Status == CrossProcessBarrierStatus.ReadyAndWaiting
                    && s.Tag == tag);
            }
            finally
            {
                SynchronizedAccessToMMF.ReleaseMutex();
            }
        }
        public int GetNumberOfRemainingParticipants(string tag)
        {
            var peers = GetAllParticipantState()
                    .Where(x => x.Status == CrossProcessBarrierStatus.Started
                    || x.Status == CrossProcessBarrierStatus.ReadyAndWaiting);
            if (string.IsNullOrWhiteSpace(tag))
                return peers.Count(s => s.Status == CrossProcessBarrierStatus.Started);

            return peers.Count(s =>
                s.Status == CrossProcessBarrierStatus.Started
                && string.IsNullOrWhiteSpace(s.Tag));
        }
        public void Start() => SetParticipantStatus(CrossProcessBarrierStatus.Started);
        public bool SetReadyAndWait(int waitForSeconds, string tag, CancellationToken cancel = default)
        {
            if (cancel.IsCancellationRequested)
            {
                return false;
            }
            SetParticipantStatus(CrossProcessBarrierStatus.ReadyAndWaiting, tag);
            try
            {
                var sw = new Stopwatch();
                sw.Start();
                while (sw.Elapsed.TotalSeconds < waitForSeconds)
                {
                    if (AreAllActiveParticipantsReadyAndWaiting(tag))
                    {
                        Thread.Sleep(200);
                        return true;
                    }
                    if (cancel.IsCancellationRequested)
                    {
                        return false;
                    }
                    Thread.Sleep(50);
                }
                return false;
            }
            finally
            {
                SetParticipantStatus(CrossProcessBarrierStatus.Started, tag: null);
            }
        }
        public Task<bool> SetReadyAndWaitAsync(int waitForSeconds, string tag = null, CancellationToken cancel = default) =>
            Task.Run(() => SetReadyAndWait(waitForSeconds, tag, cancel));

        private void SetParticipantStatus(CrossProcessBarrierStatus status, string tag = null)
        {
            SynchronizedAccessToMMF.WaitOne();
            try
            {
                var state = MMF.ReadState();
                var index = state.FindIndex(x => x.ParticipantId == participantId);
                state[index] = state[index].WithUpdatedStatus(status).WithUpdatedTags(tag);
                MMF.WriteState(state);
            }
            finally
            {
                SynchronizedAccessToMMF.ReleaseMutex();
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            SetParticipantStatus(CrossProcessBarrierStatus.Disposed);
            SynchronizedAccessToMMF.Dispose();
            MMF.Dispose();
        }
    }
    public sealed class ReadOnlyCrossProcessBarrier : IDisposable
    {
        private bool isDisposed = false;
        private readonly MemoryMappedFile MMF;

        public ReadOnlyCrossProcessBarrier(string instanceName)
        {
            MMF = MemoryMappedFile.CreateOrOpen(typeof(CrossProcessBarrier).FullName + ".MMF." + instanceName, 1024);
        }

        public int GetTotalParticipantCount() =>
            MMF.ReadParticipantCount();

        public IEnumerable<CrossProcessBarrierState> GetAllParticipantState() =>
            MMF.ReadState().AsReadOnly();

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            MMF.Dispose();
        }
    }
}
