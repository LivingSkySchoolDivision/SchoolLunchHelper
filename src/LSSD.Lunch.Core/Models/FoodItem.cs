using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel;

namespace LSSD.Lunch
{
    public class FoodItem : IGUIDable
    {    
        public Guid Id { get; set; }
        public string Name { get;set; }
        public Guid SchoolId { get;set; }
        public decimal Cost { get; set; }
        public string Description { get; set; }
    }
}
