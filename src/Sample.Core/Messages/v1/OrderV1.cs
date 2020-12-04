
namespace Sample.Core.Messages.v1
{
    public class OrderV1
    {
        public string Id { get; set; }
        public int Amount { get; set; }
        public string ArticleNumber { get; set; }
        public CustomerV1 Customer { get; set; }
    }
}
