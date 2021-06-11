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
		/*
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
        */

		private string _StudentID;
		private string _Name;
		private string _SchoolID;
		private double _Balance;
		private string _MedicalInfo;

		//private setter is for EF Core
		public string StudentID { get { return _StudentID; } private set { _StudentID = value; } }
		public string Name { get { return _Name; } private set { _Name = value; } }
		public string SchoolID { get { return _SchoolID; } set { _SchoolID = value; } }
		public double Balance { get { return _Balance; } set { _Balance = value; } }
		public string MedicalInfo { get { return _MedicalInfo; } set { _MedicalInfo = value; } }

		private Student(string StudentID, string Name, string SchoolID, double Balance, string MedicalInfo)
		{
			_StudentID = StudentID;
			_Name = Name;
			_SchoolID = SchoolID;
			_Balance = Balance;
			_MedicalInfo = MedicalInfo;
		}
	}
}
