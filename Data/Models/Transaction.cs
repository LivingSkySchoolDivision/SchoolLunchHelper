using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Data.Models
{
    public class Transaction
    {
        private decimal _Cost; //cost for transactions that remove money from a student's balance is positive, if money is added to a balance the cost is negative
        private string _StudentID;
        private string _StudentName;
        private string _FoodID;
        private string _FoodName;
        private string _SchoolID;
        private string _SchoolName;
        private DateTime _Time;
        private string _ID;


        [Column(TypeName = "decimal(18, 2)"), Required]
        public decimal Cost { get { return _Cost; } set { _Cost = value; } }

        [Required]
        public string StudentID { get { return _StudentID; } set { _StudentID = value; } }

        [Required]
        public string StudentName { get { return _StudentName; } set { _StudentName = value; } }

        [Required]
        public string FoodID { get { return _FoodID; } set { _FoodID = value; } }

        [Required]
        public string FoodName { get { return _FoodName; } set { _FoodName = value; } }

        [Required]
        public string SchoolID { get { return _SchoolID; } set { _SchoolID = value; } }

        [Required]
        public string SchoolName { get { return _SchoolName; } set { _SchoolName = value; } }

        [Required]
        public DateTime Time { get { return _Time; } set { _Time = value; } }

        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get { return _ID; } set { _ID = value; } }

        //deserialization constructor
        public Transaction()
        {

        }
        
        //[JsonConstructor]
        public Transaction(decimal Cost, string StudentID, string StudentName, string FoodID, string FoodName, string SchoolID, string SchoolName, DateTime Time, string ID)
        {
            _Cost = Cost;
            _StudentID = StudentID;
            _StudentName = StudentName;
            _FoodID = FoodID;
            _FoodName = FoodName;
            _SchoolID = SchoolID;
            _SchoolName = SchoolName;
            _Time = Time;
            _ID = ID;
        }

        public Transaction(string StudentID, string FoodID, string FoodName, decimal Cost, string StudentName, string SchoolID, string SchoolName)
        {
            _Cost = Cost;
            _StudentID = StudentID;
            _StudentName = StudentName;
            _FoodID = FoodID;
            _FoodName = FoodName;
            _SchoolID = SchoolID;
            _SchoolName = SchoolName;
            _Time = DateTime.Now;
            _ID = StudentID + Time.ToString("yyyyMMddHHmmssff"); //ID is a number (stored as a string) generated from the current year, month, day, minute, second, two decimal digits of a second, and the student's student number. Hours are in 24hr time
        }

        public override string ToString()
        {
            return "Cost = $" + Cost.ToString() + ", StudentID = " + StudentID + ", StudentName = " + StudentName + ", FoodID = " + FoodID + ", FoodName = " + FoodName + ", SchoolID = " + SchoolID + ", SchoolName = " + SchoolName + ", Time = " + Time.ToString() + ", ID = " + ID;
        }

    }
}
