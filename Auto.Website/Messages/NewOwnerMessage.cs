using System;

namespace Auto.Website.Messages
{
    public class NewOwnerMessage
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string VehicleCode { get; set; }
        public DateTime ListedAtUtc { get; set; }
    }
}