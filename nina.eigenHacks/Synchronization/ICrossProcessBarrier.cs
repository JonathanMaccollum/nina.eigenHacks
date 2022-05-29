using System;
using System.Threading;
using System.Threading.Tasks;

namespace nina.eigenHacks.Synchronization
{
    public interface ICrossProcessBarrier:IDisposable
    {
        string Name { get; }
        void Start();
        bool SetReadyAndWait(
            int waitForSeconds, 
            string tag, 
            CancellationToken cancel = default);
        Task<bool> SetReadyAndWaitAsync(
            int waitForSeconds, 
            string tag = null, 
            CancellationToken cancel = default);
        int GetNumberOfRemainingParticipants(string tag);
    }
}