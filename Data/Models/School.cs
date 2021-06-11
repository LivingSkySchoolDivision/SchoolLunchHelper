using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class School
    {
        [Required]
        public string Name { get; set; }

        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }
    }
}
