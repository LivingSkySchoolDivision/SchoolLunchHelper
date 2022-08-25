using System;

namespace LSSD.Lunch
{
    public class HistoricalBalance : IGUIDable
    {    
        public Guid Id { get; set; }
        public string DateTime { get; set; }
        public Guid StudentID { get; set; }
        public decimal Amount { get; set; }
    }
}
