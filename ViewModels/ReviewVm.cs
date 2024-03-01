namespace Kheti.ViewModels
{
    public class ReviewVm
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
        public Guid ProductId { get; set; }

        public string UserId { get; set; }
    }
}
