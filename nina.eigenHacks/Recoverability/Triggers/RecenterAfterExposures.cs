using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Profile.Interfaces;
using NINA.Sequencer.Container;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Validations;
using NINA.Equipment.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Locale;
using NINA.Astrometry;
using NINA.Sequencer.SequenceItem.Platesolving;
using NINA.Core.Utility;
using NINA.Sequencer.Utility;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.Core.Enum;
using NINA.PlateSolving;
using NINA.Core.Utility.WindowService;
using NINA.Equipment.Interfaces;
using System.Linq;
using NINA.Sequencer.Trigger;

namespace nina.eigenHacks.Recoverability.Triggers
{

    [ExportMetadata("Name", "Center after Exposure Count (Alpha)")]
    [ExportMetadata("Description", "A trigger to initiate a recentering after a fixed number of exposures have been taken.")]
    [ExportMetadata("Icon", "TargetWithArrowSVG")]
    [ExportMetadata("Category", "Lbl_SequenceCategory_Telescope")]
    [Export(typeof(ISequenceTrigger))]
    [JsonObject(MemberSerialization.OptIn)]
    public class CenterAfterExposureCount : SequenceTrigger, IValidatable
    {
        private readonly IProfileService profileService;
        private readonly ITelescopeMediator telescopeMediator;
        private readonly IFilterWheelMediator filterWheelMediator;
        private readonly IGuiderMediator guiderMediator;
        private readonly IImagingMediator imagingMediator;
        private readonly IImageSaveMediator imageSavedMediator;
        private readonly ICameraMediator cameraMediator;
        private readonly IDomeMediator domeMediator;
        private readonly IDomeFollower domeFollower;
        private readonly IImageSaveMediator imageSaveMediator;

        [ImportingConstructor]
        public CenterAfterExposureCount(
            IProfileService profileService, 
            ITelescopeMediator telescopeMediator, 
            IFilterWheelMediator filterWheelMediator, 
            IGuiderMediator guiderMediator,
            IImagingMediator imagingMediator, 
            ICameraMediator cameraMediator, 
            IDomeMediator domeMediator, 
            IDomeFollower domeFollower, 
            IImageSaveMediator imageSaveMediator,
            IImageSaveMediator imageSavedMediator) : base()
        {
            this.profileService = profileService;
            this.telescopeMediator = telescopeMediator;
            this.filterWheelMediator = filterWheelMediator;
            this.guiderMediator = guiderMediator;
            this.imagingMediator = imagingMediator;
            this.cameraMediator = cameraMediator;
            this.domeMediator = domeMediator;
            this.domeFollower = domeFollower;
            this.imageSaveMediator = imageSaveMediator;
            AfterExposures = 10;
            this.imageSavedMediator = imageSavedMediator;
        }

        private CenterAfterExposureCount(CenterAfterExposureCount cloneMe) 
            : this(
                  cloneMe.profileService, 
                  cloneMe.telescopeMediator, 
                  cloneMe.filterWheelMediator, 
                  cloneMe.guiderMediator, 
                  cloneMe.imagingMediator, 
                  cloneMe.cameraMediator, 
                  cloneMe.domeMediator, 
                  cloneMe.domeFollower, 
                  cloneMe.imageSaveMediator, 
                  cloneMe.imageSavedMediator)
        {
            CopyMetaData(cloneMe);
        }

        public override object Clone()
        {
            return new CenterAfterExposureCount(this)
            {
                TriggerRunner = (SequentialContainer)TriggerRunner.Clone(),
                AfterExposures = AfterExposures,
            };
        }

        private IList<string> issues = new List<string>();

        public IList<string> Issues
        {
            get => new List<string>(issues.ToList().AsReadOnly());
            set
            {
                issues = value;
                RaisePropertyChanged();
            }
        }


        public override async Task Execute(ISequenceContainer context, IProgress<ApplicationStatus> progress, CancellationToken token)
        {
            var coordinates = ItemUtility.RetrieveContextCoordinates(this.Parent);
            if (coordinates?.Coordinates == null)
                return;

            ExposuresTaken = 0;
            var centerSequenceItem = new Center(
                profileService,
                telescopeMediator,
                imagingMediator,
                filterWheelMediator,
                guiderMediator,
                domeMediator,
                domeFollower,
                new PlateSolverFactoryProxy(), new WindowServiceFactory())
            {
                Coordinates = new InputCoordinates(
                    coordinates?.Coordinates)
            };
            await centerSequenceItem.Execute(progress, token);
        }

        private int afterExposures;
        private int exposuresTaken = 0;

        [JsonProperty]
        public int AfterExposures
        {
            get => afterExposures;
            set
            {
                afterExposures = value;
                RaisePropertyChanged();
            }
        }
        public int ExposuresTaken
        {
            get => exposuresTaken;
            set{
                exposuresTaken = value;
                RaisePropertyChanged();
            }
        }

        public override bool ShouldTrigger(ISequenceItem previousItem, ISequenceItem nextItem)
        {
            if (nextItem == null) { return false; }
            RaisePropertyChanged(nameof(ExposuresTaken));
            if (ExposuresTaken >= AfterExposures)
            {
                Logger.Info($"Image progress exceeded threshold: {ExposuresTaken} / {AfterExposures} exposures");
                return true;
            }
            return false;
        }

        public override void SequenceBlockInitialize()
        {
            ExposuresTaken = 0;
            imageSavedMediator.ImageSaved += ImageSavedMediator_ImageSaved;
        }

        private void ImageSavedMediator_ImageSaved(object sender, ImageSavedEventArgs e)
        {
            ExposuresTaken += 1;
        }

        public override void SequenceBlockTeardown()
        {
            imageSavedMediator.ImageSaved -= ImageSavedMediator_ImageSaved;
        }


        public override void AfterParentChanged()
        {
            if (Parent == null)
            {
                SequenceBlockTeardown();
            }
            else
            {
                Validate();
                if (Parent.Status == SequenceEntityStatus.RUNNING)
                {
                    SequenceBlockInitialize();
                }
            }
        }

        public override string ToString()
        {
            return $"Trigger: {nameof(CenterAfterExposureCount)}, Progress: {ExposuresTaken}/{AfterExposures}";
        }

        public bool Validate()
        {
            try
            {
                var issues = new List<string>();
                if (ItemUtility.RetrieveContextCoordinates(this.Parent) == null)
                {
                    issues.Add(Loc.Instance["LblNoTarget"]);
                }
                if (!cameraMediator.GetInfo().Connected)
                {
                    issues.Add(Loc.Instance["LblCameraNotConnected"]);
                }
                if (!telescopeMediator.GetInfo().Connected)
                {
                    issues.Add(Loc.Instance["LblTelescopeNotConnected"]);
                }
                Issues = issues;
                foreach (var issue in issues)
                {
                    Logger.Warning($"{nameof(CenterAfterExposureCount)} Validation Failure: {issue}");
                }
                return issues.Any();
            }
            catch(Exception ee)
            {
                Logger.Warning($"{nameof(CenterAfterExposureCount)} Validation Failure: {ee}");
                throw;
            }
        }
    }
}