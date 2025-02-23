using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmpPromatic.CNCRefactor.Common
{
    public enum ResultType
    {
        Success = 0,
        UnknownError = 1,
        FileNotFound = 2,
        FileEmpty = 3,
        FileNotSupported = 4,
        FileNotReadable = 5,
        OperationCancelled = 6
    }
    public record RefactorResult(ResultType ResultType, string? ErrorMessage, string? FileName)
    {
        public static RefactorResult Success(string? fileName) => new RefactorResult(ResultType.Success, default, fileName);
        public static RefactorResult Failure(ResultType resultType, string? errorMessage, string? fileName) => new RefactorResult(resultType, errorMessage, fileName);
    }
}
