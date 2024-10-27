using System;

namespace Rolling.Models
{
    public class ListRentalsModel
    {
        public int Id { get; set; }
        public Guid CarId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}