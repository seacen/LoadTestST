using System;

namespace LoadTestST.Models
{
    public class LoadTestResultException : Exception
    {
        public LoadTestResultException(string message) : base(message)
        {
        }
    }
}