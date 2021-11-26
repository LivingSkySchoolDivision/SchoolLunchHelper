using System;

namespace LSSD.Lunch
{

    // Our transactions are only ever 1 item

    public class FoodScanTransaction
    {            
        public string StudentNumber { get; set; }                
        public Guid FoodItemID { get; set; }
    }
}
