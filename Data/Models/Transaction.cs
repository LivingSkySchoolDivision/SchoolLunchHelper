using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    class Transaction
    {
        [Column(TypeName = "decimal(18, 2)"), Required]
        public double Cost { get; set; }

        [Required]
        public string StudentID { get; set; }

        [Required]
        public string StudentName { get; set; }

        [Required]
        public string FoodID { get; set; }

        [Required]
        public string FoodName { get; set; }

        [Required]
        public string SchoolID { get; set; }

        [Required]
        public string SchoolName { get; set; }

        [Required]
        public DateTime Time { get; set; }

        [Required]
        public string ID { get; set; }
    }
}
