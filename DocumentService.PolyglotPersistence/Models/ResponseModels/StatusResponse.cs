namespace DocumentService.PolyglotPersistence.Models.ResponseModels
{
    public class StatusResponse
    {
        public StatusResponse(int code, object? result)
        {
            this.code = code;
            this.result = result;
        }

        public int code { get; set; }
        public object? result { get; set; }
    }
}
