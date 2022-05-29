using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Sequencer.Container;
using NINA.Sequencer.SequenceItem;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;


namespace nina.eigenHacks.Synchronization.Instructions
{
    [ExportMetadata("Name", "Signal Ready and Wait")]
    [ExportMetadata("Description", "Signals that the sequence has reached a Barrier and waits for other participants to arrive.")]
    [ExportMetadata("Category", "Synchronization")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class SignalReadyAndWait: SequenceItem
    {
        private string tag=string.Empty;
        private int waitForSeconds=30;
        private static readonly ICrossProcessBarrier _globalBarrier
            =new CrossProcessBarrier(nameof(SignalReadyAndWait));
        static SignalReadyAndWait()
        {
            _globalBarrier.Start();
            AppDomain.CurrentDomain.DomainUnload += (_, __) =>
            {
                _globalBarrier.Dispose();
            };
            AppDomain.CurrentDomain.ProcessExit += (_, __) =>
            {
                _globalBarrier.Dispose();
            };
        }


        [ImportingConstructor]
        public SignalReadyAndWait()
        {
        }

        [JsonProperty]
        public string Tag { get => tag; set { tag = value; RaisePropertyChanged(); } }
        [JsonProperty]
        public int WaitForSeconds { 
            get => waitForSeconds; 
            set { waitForSeconds = Math.Max(0,value); RaisePropertyChanged(); } 
        }

        public override object Clone()
        {
            return new SignalReadyAndWait
            {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description,
                Tag = Tag,
                WaitForSeconds = WaitForSeconds
            };
        }

        private SychronizationScope FindSynchronizationScope(
            ISequenceContainer x) =>
            x == null
                ? null
                : x as SychronizationScope ??
                FindSynchronizationScope(x.Parent);

        public override async Task Execute(
            IProgress<ApplicationStatus> progress, 
            CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            var barrier = FindSynchronizationScope(Parent)
                ?.GetSynchronizationBarrier() ?? _globalBarrier;

            var tag = Tag??string.Empty;
            var waitForSeconds = WaitForSeconds;

            if (!string.IsNullOrWhiteSpace(tag)){
                tag = " " + tag + ".";
            }
            try
            {
                var cts = new CancellationTokenSource();
                var progressTask = Task.Run(async () =>
                {
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    while (!cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, cts.Token);
                        var secondsRemaining = Math.Floor(WaitForSeconds - sw.Elapsed.TotalSeconds);
                        var participantsRemaining = barrier.GetNumberOfRemainingParticipants(tag);
                        progress.Report(new ApplicationStatus
                        {
                            Source = barrier.Name,
                            Status = $"Waiting for {participantsRemaining} participants at barrier {tag}. (Bypass in {secondsRemaining}s)."
                        });
                    }
                },cts.Token);
                await barrier.SetReadyAndWaitAsync(waitForSeconds, tag, token);
                cts.Cancel();
                await progressTask;
            }
            finally
            {
                progress.Report(new ApplicationStatus
                {
                    Source = barrier.Name,
                    Status = ""
                });
            }
            
        }
    }
}
