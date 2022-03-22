using System.Net;

namespace Prinubes.Common.Datamodels
{
    public class ErrorReturnType
    {
        public ErrorReturnType(HttpStatusCode _httpCode, string _errorMessage)
        {
            ErrorCode = (int)_httpCode;
            ErrorMessage = _errorMessage;
        }
        public int ErrorCode;
        public string ErrorMessage { get; set; }
    }
}
