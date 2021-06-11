using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class Student
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string StudentID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string SchoolID { get; set; }

        [Column(TypeName = "decimal(18, 2)"), Required] //computed column - sum of student's transactions
        public double Balance { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string MedicalInfo { get; set; }
    }
}
