namespace Sample.Core.Messages.v1
{
    public class ShipmentV1
    {
        public string Id { get; set; }
        public OrderV1 Order { get; set; }
        public LocationV1 Location { get; set; }
    }
}
