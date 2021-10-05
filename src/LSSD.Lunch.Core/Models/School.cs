using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSSD.Lunch
{
    public class School : IGUIDable
    {    
        public Guid Id { get; set; }
        public string Name { get;set; }
    }
}
