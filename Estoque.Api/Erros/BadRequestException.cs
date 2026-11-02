namespace Estoque.Api.Erros
{
    public class BadRequestException: Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}
