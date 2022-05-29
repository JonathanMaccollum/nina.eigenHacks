using System.ComponentModel.Composition;
using System.Windows;

namespace nina.eigenHacks
{
    /// <summary>
    /// Interaction logic for DataTemplates.xaml
    /// </summary>
    [Export(typeof(ResourceDictionary))]
    public partial class SignalReadyAndWaitTemplate : ResourceDictionary
    {
        public SignalReadyAndWaitTemplate()
        {
            InitializeComponent();
        }
    }
}
