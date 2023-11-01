using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace IRIS.CrmConnector.API.Hangfire
{
    public class DisableHangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            return true;
        }
    }
}
