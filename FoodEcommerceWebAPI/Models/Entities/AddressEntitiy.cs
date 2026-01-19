namespace FoodEcommerceWebAPI.Models.Entities
{
    public class AddressEntitiy
    {
        public int ID { get; set; }
        public required int Userid { get; set; }
        public required string streetAddress { get; set; }
        public required string city { get; set; }
        public required string state { get; set; }
        public required string zipCode { get; set; }
        public required bool IsDefault { get; set; }

    }
}
