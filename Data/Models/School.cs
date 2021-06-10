using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    class School
    {
        [Required]
        public string Name { get; set; }

        [Key, Required]
        public string ID { get; set; }
    }
}
