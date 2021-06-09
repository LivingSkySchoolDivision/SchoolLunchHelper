using System;

namespace lunch_project
{
	public class Student
	{
		private string _StudentID;
		private string _Name;
		private string _SchoolID;
		private double _Balance;
		private string _MedicalInfo;

		public string StudentID { get { return _StudentID; } } 
		public string Name { get { return _Name; } set { _Name = value; } }
		public string SchoolID { get { return _SchoolID; } set { _SchoolID = value; } }
		public double Balance { get { return _Balance; } set { _Balance = value; } }
		public string MedicalInfo { get { return _MedicalInfo; } set { _MedicalInfo = value; } }

		public Student(string StudentID, string Name, string SchoolID, double Balance, string MedicalInfo)
		{
			_StudentID = StudentID;
			_Name = Name;
			_SchoolID = SchoolID;
			_Balance = Balance;
			_MedicalInfo = MedicalInfo;
		}
	}
}

