using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(news_engine.Startup))]
namespace news_engine
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
