namespace SOP.Messages.Messages
{
    public class NewOwnerMessage
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? VehicleCode { get; set; }
    }
}