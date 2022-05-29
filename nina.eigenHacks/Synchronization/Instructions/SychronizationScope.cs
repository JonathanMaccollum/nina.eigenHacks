using Newtonsoft.Json;
using nina.eigenHacks.Synchronization;
using NINA.Core.Model;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.Container;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Trigger;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace nina.eigenHacks.Synchronization.Instructions
{
    [ExportMetadata("Name", "Synchronization Scope")]
    [ExportMetadata("Description", "Provides a cross-process barrier for synchronizing multiple instances.")]
    [ExportMetadata("Category", "Synchronization")]
    [Export(typeof(ISequenceItem))]
    [Export(typeof(ISequenceContainer))]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class SychronizationScope: SequentialContainer
    {
        private string scopeName = string.Empty;
        private ICrossProcessBarrier _barrier;

        [ImportingConstructor]
        public SychronizationScope() { }

        [JsonProperty]
        public string ScopeName { get => scopeName; set { scopeName = value; RaisePropertyChanged(); } }

        public override object Clone()
        {
            return new SychronizationScope
            {
                Icon = Icon,                
                Name = Name,
                Category = Category,
                Description = Description,
                ScopeName=ScopeName,
                Items = new ObservableCollection<ISequenceItem>(Items.Select(i => i.Clone() as ISequenceItem)),
                Triggers = new ObservableCollection<ISequenceTrigger>(Triggers.Select(t => t.Clone() as ISequenceTrigger)),
                Conditions = new ObservableCollection<ISequenceCondition>(Conditions.Select(t => t.Clone() as ISequenceCondition))
            };
        }
        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            try
            {
                _barrier = new CrossProcessBarrier(ScopeName);
                _barrier.Start();
                await base.Execute(progress, token);
            }
            finally
            {
                _barrier.Dispose();
            }            
        }

        public ICrossProcessBarrier GetSynchronizationBarrier() => _barrier;
    }
}
