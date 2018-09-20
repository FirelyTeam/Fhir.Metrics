using System;

namespace Fhir.Metrics.Tests
{
    public class AssertFailedException : Exception
    {
        public AssertFailedException(string message) : base(message) { }

        public AssertFailedException(string message, Exception inner) : base(message, inner) { }
    }
}