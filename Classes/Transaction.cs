using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace lunch_project
{
    public class Transaction
    {
        public double Cost { get; }
        public string StudentID { get; }
        public string StudentName { get; }
        public string FoodID { get; }
        public string FoodName { get; } 
        public string SchoolID { get; } 
        public string SchoolName { get; }
        public DateTime Time { get; }
        public string ID { get; }

        public Transaction(string StudentID, string FoodID, string FoodName, double Cost, string StudentName) 
        {
            this.Cost = Cost; 
            this.StudentID = StudentID; 
            this.StudentName = StudentName; 
            this.FoodID = FoodID;
            this.FoodName = FoodName;
            //this.SchoolID = //could get this from config file
            this.SchoolID = "test"; //DEBUG
            //this.SchoolName = //could get this from config file
            this.SchoolName = ""; //DEBUG
            this.Time = DateTime.Now;
            this.ID = Time.ToString("yyyyMMddHHmmssff") + SchoolID; //ID is a number (stored as a string) generated from the current year, month, day, minute, second, two decimal digits of a second, and the SchoolID. Hours are in 24hr time
        }

        [JsonConstructor]
        public Transaction(double Cost, string StudentID, string StudentName, string FoodID, string FoodName, string SchoolID, string SchoolName, DateTime Time, string ID)
        {
            this.Cost = Cost;
            this.StudentID = StudentID;
            this.StudentName = StudentName;
            this.FoodID = FoodID;
            this.FoodName = FoodName;
            this.SchoolID = SchoolID; 
            this.SchoolName = SchoolName;
            this.Time = Time;
            this.ID = ID; 
        }

        /*
        public Transaction() //DEBUG 
        {
            Cost = 1.23;
            StudentID = "123";
            StudentName = "student1";
            FoodID = 12;
            FoodName = "pizza";
            SchoolID = 1;
            SchoolName = "school1";
            Time = DateTime.Now;
            ID = Time.ToString("yyyyMMddHHmmssff");
        }

        public Transaction(string StudentID) //DEBUG
        {
            Cost = 1.23;
            this.StudentID = StudentID;
            StudentName = "studentName";
            FoodID = 12;
            FoodName = "pizza";
            SchoolID = 1;
            SchoolName = "school1";
            Time = DateTime.Now;
            ID = Time.ToString("yyyyMMddHHmmssff");
        }
        */


    }
}
