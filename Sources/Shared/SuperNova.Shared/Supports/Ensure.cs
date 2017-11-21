using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SuperNova.Shared.Supports
{
    public static class Ensure
    {
        /// <summary>
        /// Ensures that the specified argument is not null.
        /// </summary>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="argument">The argument.</param>
        [DebuggerStepThrough]        
        public static void ArgumentNotNull(object argument, string argumentName = "")
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Ensures that the specified argument is not null.
        /// </summary>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="argument">The argument.</param>
        [DebuggerStepThrough]
        public static void ArgumentNotNullOrEmpty(string argument, string argumentName = "")
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Ensures that the specified argument is not null.
        /// </summary>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="argument">The argument.</param>
        [DebuggerStepThrough]
        public static void ArgumentNotNullOrWhiteSpace(
            string argument, string argumentName = "")
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}
