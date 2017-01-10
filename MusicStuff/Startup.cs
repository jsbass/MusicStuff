using Microsoft.Owin;
using MusicStuff;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace MusicStuff
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {

        }
    }
}
