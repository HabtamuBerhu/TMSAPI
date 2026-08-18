using System;

namespace TmsApi.Services
{
    /// <summary>
    /// Exception used to test application standard error mappings (ProblemDetails).
    /// </summary>
    public class TmsDatabaseException : Exception
    {
        public TmsDatabaseException(string message) : base(message)
        {
        }
    }
}