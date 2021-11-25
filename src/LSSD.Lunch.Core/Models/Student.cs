using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSD.Lunch
{
    public class Student : IGUIDable
    {    
        public Guid Id { get; set; }

		public string StudentId { get; set; }
		public string Name { get;set ;}
		public Guid SchoolId { get; set; }
		public string MedicalInfo { get; set; }
		public bool IsActive { get; set; }
	}
}
