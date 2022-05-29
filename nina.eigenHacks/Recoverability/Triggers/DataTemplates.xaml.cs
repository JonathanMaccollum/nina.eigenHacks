using System.Windows;
using System.ComponentModel.Composition;

namespace nina.eigenHacks.Recoverability.Triggers
{
    [Export(typeof(ResourceDictionary))]
    public partial class DataTemplates
    {
        public DataTemplates()
        {
            InitializeComponent();
        }
    }
}
