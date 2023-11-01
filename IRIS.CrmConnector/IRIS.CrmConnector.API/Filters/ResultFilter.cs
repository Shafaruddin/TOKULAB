using Abp.AspNetCore.Mvc.Extensions;
using Abp.AspNetCore.Mvc.Results.Wrapping;
using Abp.Dependency;
using Abp.Web.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IRIS.CrmConnector.API.Filters
{
    public class ResultFilter : IResultFilter, ITransientDependency
    {
        private readonly IAbpActionResultWrapperFactory _actionResultWrapperFactory;

        public ResultFilter(IAbpActionResultWrapperFactory actionResultWrapper)
        {
            _actionResultWrapperFactory = actionResultWrapper;
        }

        public virtual void OnResultExecuting(ResultExecutingContext context)
        {
            if (!context.ActionDescriptor.IsControllerAction())
            {
                return;
            }

            var methodInfo = context.ActionDescriptor.GetMethodInfo();

            var wrapResultAttribute =
                ReflectionHelper.GetSingleAttributeOfMemberOrDeclaringTypeOrDefault(
                    methodInfo,
                    new WrapResultAttribute()
                );

            if (!wrapResultAttribute.WrapOnSuccess)
            {
                return;
            }

            _actionResultWrapperFactory.CreateFor(context).Wrap(context);
        }

        public virtual void OnResultExecuted(ResultExecutedContext context)
        {
            //no action
        }
    }
}