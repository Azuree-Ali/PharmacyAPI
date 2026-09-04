namespace PharmacyAPI.DTOs.Response
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Error { get; set; }
    }
}
