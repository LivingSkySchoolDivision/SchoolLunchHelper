using System;

namespace LSSD.Lunch
{

    // Our transactions are only ever 1 item

    public class Transaction : IGUIDable
    {
        public Guid Id { get; set; }
        public DateTime TimestampUTC { get; set; }


        public string StudentNumber { get; set; }
        public string StudentName { get; set; }
        public string ItemDescription { get; set; }
        public decimal Amount { get; set; }


        // Optional things
        public Guid? FoodItemID { get; set; }
        public Guid? StudentID { get; set; }

    }
}
