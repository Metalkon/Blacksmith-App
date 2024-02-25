namespace Shared_Classes.Models
{
    public class UserConfirmDTO
    {
        public string Code { get; set; }
        public UserDTO User { get; set; }

        public UserConfirmDTO()
        {
            User = new UserDTO();
        }
    }
}
