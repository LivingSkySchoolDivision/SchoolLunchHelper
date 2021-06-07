using System;

namespace lunch_project
{
	public class Student
	{
		public string StudentID { get; } 
		public string Name { get; set; }
		public int SchoolID { get; set; }
		public double Balance { get; set; }
		public string MedicalInfo { get; set; }

		public Student(string StudentID, string Name, int SchoolID, double Balance, string MedicalInfo)
		{
			this.StudentID = StudentID;
			this.Name = Name;
			this.SchoolID = SchoolID;
			this.Balance = Balance;
			this.MedicalInfo = MedicalInfo;
		}
	}
}

