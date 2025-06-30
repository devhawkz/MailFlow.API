
namespace Shared.Responses;
public sealed class ApiResponse<TResult> : ApiBaseResponse
{
    public TResult Result { get; set; }
    
    public ApiResponse(TResult result,int statusCode, bool success, string message) : base(statusCode, success,message) 
    {
        Result = result;
    }

    public static ApiResponse<TResult> Ok(TResult? result = default, int statusCode = 200, string message = "Success")
        => new ApiResponse<TResult>(result!, statusCode, true, message);


    public static ApiResponse<TResult> Error(string message, int statusCode = 400, TResult? result = default)
        => new ApiResponse<TResult>(result!, statusCode, false, message);
} 
