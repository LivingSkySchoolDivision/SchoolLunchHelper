using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LSSD.Lunch
{
    public class Transaction : IGUIDable
    {    
        public Guid Id { get; set; }

        public DateTime Timestamp { get; set; }
        public Guid StudentID { get; set; }
        public Guid FoodItem { get; set; }
        public Guid SchoolId { get; set; }

        public string StudentName { get; set; }
        public string FoodName { get; set; }
        public string SchoolName { get; set; }

        public decimal Cost { get;set; }
    }
}
