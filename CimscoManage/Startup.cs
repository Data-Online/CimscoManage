using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CimscoManage.Startup))]
namespace CimscoManage
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
