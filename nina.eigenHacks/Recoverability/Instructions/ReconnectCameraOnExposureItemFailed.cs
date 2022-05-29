using Newtonsoft.Json;
using NINA.Core.Enum;
using NINA.Core.Model;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Sequencer.Container;
using NINA.Sequencer.Interfaces;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Trigger;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace nina.eigenHacks.Recoverability.Instructions
{
    [ExportMetadata("Name", "Reconnect Cam on ExposureItem Failure")]
    [ExportMetadata("Description", "Reconnects camera after failed download")]
    [ExportMetadata("Category", "eigenHacks.Recoverability")]
    [Export(typeof(ISequenceTrigger))]
    [JsonObject(MemberSerialization.OptIn)]
    public class ReconnectCameraOnExposureItemFailed : SequenceTrigger
    {
        private readonly ICameraMediator cameraMediator;

        [ImportingConstructor]
        public ReconnectCameraOnExposureItemFailed(
            ICameraMediator cameraMediator)
        {
            this.cameraMediator = cameraMediator;
        }


        public override object Clone()
        {
            return new ReconnectCameraOnExposureItemFailed(cameraMediator)
            {
                DelaySecondsBeforeDisconnect = DelaySecondsBeforeDisconnect,
                DelaySecondsBeforeReconnect = DelaySecondsBeforeReconnect,
                DelaySecondsAfterReconnect = DelaySecondsAfterReconnect,
            };
        }

        private int delaySecondsBeforeDisconnect = 0;
        private int delaySecondsBeforeReconnect = 5;
        private int delaySecondsAfterReconnect = 5;
        private int reconnectCount;

        [JsonProperty]
        public int DelaySecondsBeforeDisconnect
        {
            get => delaySecondsBeforeDisconnect;
            set
            {
                delaySecondsBeforeDisconnect = value;
                RaisePropertyChanged();
            }
        }


        [JsonProperty]
        public int DelaySecondsBeforeReconnect
        {
            get => delaySecondsBeforeReconnect;
            set
            {
                delaySecondsBeforeReconnect = value;
                RaisePropertyChanged();
            }
        }


        [JsonProperty]
        public int DelaySecondsAfterReconnect
        {
            get => delaySecondsAfterReconnect;
            set
            {
                delaySecondsAfterReconnect = value;
                RaisePropertyChanged();
            }
        }
        [JsonProperty]
        public int ReconnectCount
        {
            get => reconnectCount;
            set
            {
                reconnectCount = value;
                RaisePropertyChanged();
            }
        }
        public override async Task Execute(
            ISequenceContainer context,
            IProgress<ApplicationStatus> progress,
            CancellationToken token)
        {
            if (delaySecondsBeforeDisconnect > 0)
            {
                await Task.Delay(delaySecondsBeforeDisconnect * 1000);
            }
            await cameraMediator.Disconnect();
            if (delaySecondsBeforeReconnect > 0)
            {
                await Task.Delay(delaySecondsBeforeReconnect * 1000);
            }
            await cameraMediator.Connect();
            if (delaySecondsAfterReconnect > 0)
            {
                await Task.Delay(delaySecondsAfterReconnect * 1000);
            }
            ReconnectCount++;
        }

        public override bool ShouldTrigger(
            ISequenceItem previousItem,
            ISequenceItem nextItem)
        {
            return previousItem is IExposureItem
                && previousItem.Status == SequenceEntityStatus.FAILED;
        }
    }
}
