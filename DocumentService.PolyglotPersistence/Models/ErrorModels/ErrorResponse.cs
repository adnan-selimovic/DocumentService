namespace DocumentService.PolyglotPersistence.Models.ErrorModels
{
    public class ErrorResponse
    {
        public ErrorResponse(string document_name, ErrorType baseError)
        {
            this.code = baseError.code;
            this.message = $"'{document_name}' " + baseError.message;
        }

        public int status { get; set; } = 400; // BadRequest().StatusCode
        public string code { get; set; }
        public string message { get; set; }
    }
}
