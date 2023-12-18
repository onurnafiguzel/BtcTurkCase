namespace BtcTurkApi.Data.Entities
{
    public class UpdateInstrutionRequest
    {      
        public int UserId { get; set; }
        public int Amount { get; set; }
        public int Date { get; set; }
        public bool State { get; set; }
    }
}
