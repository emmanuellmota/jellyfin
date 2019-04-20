using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Authentication
{
    public class AuthenticationAccountResult
    {
        public Account Account { get; set; }
        public string AccessToken { get; set; }
        public string ServerId { get; set; }
    }
}
