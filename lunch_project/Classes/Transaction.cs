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
        private double _Cost;
        private string _StudentID;
        private string _StudentName;
        private string _FoodID;
        private string _FoodName;
        private string _SchoolID;
        private string _SchoolName;
        private DateTime _Time;
        private string _ID;

        //private setters are for EF Core
        public double Cost { get { return _Cost; } private set { _Cost = value; } }
        public string StudentID { get { return _StudentID; } private set { _StudentID = value; } }
        public string StudentName { get { return _StudentName; } private set { _StudentName = value; } }
        public string FoodID { get { return _FoodID; } private set { _FoodID = value; } }
        public string FoodName { get { return _FoodName; } private set { _FoodName = value; } } 
        public string SchoolID { get { return _SchoolID; } private set { _SchoolID = value; } } 
        public string SchoolName { get { return _SchoolName; } private set { _SchoolName = value; } }
        public DateTime Time { get { return _Time; } private set { _Time = value; } }
        public string ID { get { return _ID; } private set { _ID = value; } }

        public Transaction(string StudentID, string FoodID, string FoodName, double Cost, string StudentName) 
        {
            _Cost = Cost; 
            _StudentID = StudentID; 
            _StudentName = StudentName; 
            _FoodID = FoodID;
            _FoodName = FoodName;
            _SchoolID = MainWindow.ThisSchool.ID;
            //this.SchoolID = "test"; //DEBUG
            _SchoolName = MainWindow.ThisSchool.Name;
            //this.SchoolName = ""; //DEBUG
            _Time = DateTime.Now;
            _ID = Time.ToString("yyyyMMddHHmmssff") + SchoolID; //ID is a number (stored as a string) generated from the current year, month, day, minute, second, two decimal digits of a second, and the SchoolID. Hours are in 24hr time
        }

        [JsonConstructor]
        public Transaction(double Cost, string StudentID, string StudentName, string FoodID, string FoodName, string SchoolID, string SchoolName, DateTime Time, string ID)
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



    }
}
