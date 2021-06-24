﻿using System;
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
		private string _StudentID;
		private string _Name;
		private string _SchoolID;
		private decimal _Balance;
		private string _MedicalInfo;
			
		//private setter is for EF Core
		[Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string StudentID { get { return _StudentID; } private set { _StudentID = value; } }

		[Required]
		public string Name { get { return _Name; } private set { _Name = value; } }

		[Required]
		public string SchoolID { get { return _SchoolID; } set { _SchoolID = value; } }

		[Column(TypeName = "decimal(18, 2)"), Required] //computed column - sum of student's transactions
		public decimal Balance { get { return _Balance; } set { _Balance = value; } }

		[Required(AllowEmptyStrings = true)]
		public string MedicalInfo { get { return _MedicalInfo; } set { _MedicalInfo = value; } }


		public Student(string StudentID, string Name, string SchoolID, decimal Balance, string MedicalInfo)
		{
			_StudentID = StudentID;
			_Name = Name;
			_SchoolID = SchoolID;
			_Balance = Balance;
			_MedicalInfo = MedicalInfo;
		}

	}
}
