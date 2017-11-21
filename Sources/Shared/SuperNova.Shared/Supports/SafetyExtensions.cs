using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperNova.Shared.Supports
{
    public static class SafetyExtensions
    {
        public static void Execute(ILogger logger, Action action)
        {
            Ensure.ArgumentNotNull(logger, nameof(logger));
            Ensure.ArgumentNotNull(action, nameof(action));
            try
            {
                action();
            }
            catch (Exception exception)
            {
                logger.LogError("Exception occured: {0}", exception.Message);
            }
        }

        public static async Task ExecuteAsync(ILogger logger, Func<Task> asyncAction)
        {
            Ensure.ArgumentNotNull(logger, nameof(logger));
            Ensure.ArgumentNotNull(asyncAction, nameof(asyncAction));
            try
            {
                await asyncAction();
            }
            catch (Exception exception)
            {
                logger.LogError("Exception occured: {0}", exception.Message);
            }
        }

        public static string ToLowercaseAlphaNum(this Guid id)
        {
            return id.ToString("N").Replace("-", string.Empty);
        }
    }
}
