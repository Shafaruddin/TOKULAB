using System.Net;
using System.Reflection;
using Abp.AspNetCore.Mvc.Extensions;
using Abp.AspNetCore.Mvc.Results;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Events.Bus;
using Abp.Events.Bus.Exceptions;
using Abp.Runtime.Validation;
using Abp.UI;
using Abp.Web.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IRIS.CrmConnector.API.Filters
{
    public class ExceptionFilter : IExceptionFilter, ITransientDependency
    {
        private ILogger Logger { get; set; }

        public IEventBus EventBus { get; set; }

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            Logger = logger;
            EventBus = NullEventBus.Instance;
        }

        //public ExceptionFilter(ILogger<ExceptionFilter> logger)
        //{
        //    Logger = logger;
        //    EventBus = NullEventBus.Instance;
        //}

        public void OnException(ExceptionContext context)
        {
            var telemetry = new TelemetryClient();
            telemetry.TrackException(context.Exception);

            if (!context.ActionDescriptor.IsControllerAction())
            {
                return;
            }

            var wrapResultAttribute =
                ReflectionHelper.GetSingleAttributeOfMemberOrDeclaringTypeOrDefault(
                    context.ActionDescriptor.GetMethodInfo(),
                    new WrapResultAttribute()
                );

            if (wrapResultAttribute.LogError)
            {
                Logger.LogError(context.Exception, context.Exception.Message);
                Logger.LogError(context.Exception.StackTrace);
            }

            HandleAndWrapException(context, wrapResultAttribute);
        }

        protected virtual void HandleAndWrapException(ExceptionContext context, WrapResultAttribute wrapResultAttribute)
        {
            if (!ActionResultHelper.IsObjectResult(context.ActionDescriptor.GetMethodInfo().ReturnType))
            {
                return;
            }

            context.HttpContext.Response.StatusCode = GetStatusCode(context, wrapResultAttribute.WrapOnError);

            if (!wrapResultAttribute.WrapOnError)
            {
                return;
            }

            context.Result = new ObjectResult(
                new AjaxResponse(
                    new ErrorInfo(context.Exception.Message),
                    context.Exception is AbpAuthorizationException
                )
            );

            EventBus.Trigger(this, new AbpHandledExceptionData(context.Exception));

            context.Exception = null; //Handled!
        }

        protected virtual int GetStatusCode(ExceptionContext context, bool wrapOnError)
        {
            if (context.Exception is AbpAuthorizationException)
            {
                return context.HttpContext.User.Identity.IsAuthenticated
                    ? (int)HttpStatusCode.Forbidden
                    : (int)HttpStatusCode.Unauthorized;
            }

            if (context.Exception is AbpValidationException)
            {
                return (int)HttpStatusCode.BadRequest;
            }

            if (context.Exception is EntityNotFoundException)
            {
                return (int)HttpStatusCode.NotFound;
            }

            if (wrapOnError)
            {
                if (context.Exception is UserFriendlyException)
                {
                    var exception = (UserFriendlyException)context.Exception;
                    if (exception.Code > 0)
                    {
                        return exception.Code;
                    }
                }

                return (int)HttpStatusCode.InternalServerError;
            }

            return context.HttpContext.Response.StatusCode;
        }

        
    }
}