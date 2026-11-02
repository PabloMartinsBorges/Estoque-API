namespace Estoque.Api.Erros
{
    public class NotFoundException: Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
