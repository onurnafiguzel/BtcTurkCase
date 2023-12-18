namespace BtcTurkApi.Data.Entities
{
    public class CreateInstrutionRequest
    {    
        public int UserId { get; set; }
        public int Amount { get; set; }
        public int Date { get; set; }
        public bool Sms { get; set; }
        public bool Email { get; set; }
        public bool PushNotification { get; set; }
    }
}
