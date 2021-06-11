using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class FoodItem
    {
        [Required]
        public string Name { get; set; }

        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }

        [Required] 
        public string SchoolID { get; set; }

        [Column(TypeName = "decimal(18, 2)"), Required]
        public double Cost { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Descriptiion { get; set; }
    }
}
