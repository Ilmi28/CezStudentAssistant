using System.Net;

namespace CezStudentAssistant.API.Responses
{
    /// <summary>
    /// Represents a standard response returned by an API, including status information and an optional message.
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string? Message { get; set; }

    }

    /// <summary>
    /// Represents a generic API response that includes a strongly typed data payload.
    /// </summary>
    /// <remarks>Use this class to encapsulate the result of an API operation along with its associated data.
    /// This is useful for returning both status information and a typed result from API endpoints.</remarks>
    /// <typeparam name="T">The type of the data payload included in the response. Must be a reference type.</typeparam>
    public class ApiResponse<T> : ApiResponse
        where T : class
    {
        public T? Data { get; set; }
    }   
}
