using Microsoft.Owin;
using Owin;

namespace Beta_System
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}