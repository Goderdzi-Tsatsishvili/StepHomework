using Microsoft.IdentityModel.Tokens;

namespace WebSaklaso.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException()
        {
            
        }

        public NotFoundException(string message) : base(message)
        {
            
        }
    }
}
