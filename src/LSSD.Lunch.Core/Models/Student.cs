using System;

namespace LSSD.Lunch
{
    public class Student : IGUIDable
    {    
        public Guid Id { get; set; }
		public string StudentId { get; set; }
		public string Name { 
			get 
			{
				return FirstName + " " + LastName;
			}
		}
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public Guid SchoolId { get; set; }
		public string MedicalInfo { get; set; }
		public bool IsActive { get; set; }
		public string HomeRoom { get; set; }
		public string SchoolName { get; set; }
	}
}
