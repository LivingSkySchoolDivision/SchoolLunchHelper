using System;

namespace lunch_project
{
	public class Student
	{
		public string StudentID { get; } 
		public string Name { get; set; }
		public string SchoolID { get; set; }
		public double Balance { get; set; }
		public string MedicalInfo { get; set; }

		public Student(string StudentID, string Name, string SchoolID, double Balance, string MedicalInfo)
		{
			this.StudentID = StudentID;
			this.Name = Name;
			this.SchoolID = SchoolID;
			this.Balance = Balance;
			this.MedicalInfo = MedicalInfo;
		}
	}
}

