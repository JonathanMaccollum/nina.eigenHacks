using NINA.Plugin;
using NINA.Plugin.Interfaces;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace nina.eigenHacks
{
    [Export(typeof(IPluginManifest))]
    class Plugin : PluginBase
    {
        public override Task Initialize()
        {
            return base.Initialize();
        }

        public override Task Teardown()
        {
            return base.Teardown();
        }
    }
}
