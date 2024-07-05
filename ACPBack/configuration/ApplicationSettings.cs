namespace stage_api.configuration
{
    public class ApplicationSettings
    {
        public string JWT_Secret { get; set; }
        public string Client_URL { get; set; }
        public string SendGrid_API_Key { get; set; }
        public string SendGrid_Sender_Email { get; set; }
    }
}