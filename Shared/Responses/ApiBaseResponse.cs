
namespace Shared.Responses;

public abstract class ApiBaseResponse
{
    public int StatusCode { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    protected ApiBaseResponse(int statusCode,bool success, string message)
    {
        StatusCode = statusCode;
        Success = success;
        Message = message;
    }

}
