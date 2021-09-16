using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel;

namespace Data.Models
{
    public class FoodItem //: IEditableObject
    {
        private string _Name;
        private string _ID;
        private string _SchoolID;
        private decimal _Cost;
        private string _Description;


        [Required]
        public string Name { get { return _Name; } set { _Name = value; } }

        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get { return _ID; } set { _ID = value; } }

        [Required]
        public string SchoolID { get { return _SchoolID; } set { _SchoolID = value; } }

        [Column(TypeName = "decimal(18, 2)"), Required]
        public decimal Cost { get { return _Cost; } set { _Cost = value; } }

        [Required(AllowEmptyStrings = true)]
        public string Description { get { return _Description; } set { _Description = value; } }

        
        
        public FoodItem()
        {

        }
        

        public FoodItem(string Name, string ID, string SchoolID, decimal Cost, string Description)
        {
            _Name = Name;
            _SchoolID = SchoolID;
            _ID = ID;
            _Cost = Cost;
            _Description = Description;
        }

        public FoodItem(string Name, decimal Cost, string SchoolID)
        {
            _Name = Name;
            _SchoolID = SchoolID;
            _ID = DateTime.Now.ToString("yyyyMMddHHmmssff") + SchoolID;
            _Cost = Cost;
            _Description = "";
        }

        public FoodItem(string Name, decimal Cost, string Description, string SchoolID)
        {
            _Name = Name;
            _SchoolID = SchoolID;
            _ID = DateTime.Now.ToString("yyyyMMddHHmmssff") + SchoolID;
            _Cost = Cost;
            _Description = Description;
        }

        /*
        public void BeginEdit()
        {
            Console.WriteLine("begin edit - fooditem class"); //DEBUG
        }

        public void CancelEdit()
        {
            Console.WriteLine("cancel edit - fooditem class"); //DEBUG
        }

        public void EndEdit()
        {
            Console.WriteLine("end edit - fooditem class"); //DEBUG
        }
        */
    }
}
