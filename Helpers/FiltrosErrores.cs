using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiDescargaSriV9.Helpers
{
    public class FiltrosErrores : ExceptionFilterAttribute
    {
        private readonly ILogger<FiltrosErrores> logger;

        public FiltrosErrores(ILogger<FiltrosErrores> logger)
        {
            this.logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            logger.LogError(context.Exception, context.Exception.Message);
            base.OnException(context);
        }
    }
}
