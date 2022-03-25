namespace Prinubes.Common.Models
{
    public class AuthenticateResponse
    {
        public Guid id { get; set; }
        public string emailAddress { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string token { get; set; }
    }

}
