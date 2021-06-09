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

        public double Cost { get { return _Cost; } }
        public string StudentID { get { return _StudentID; } }
        public string StudentName { get { return _StudentName; } }
        public string FoodID { get { return _FoodID; } }
        public string FoodName { get { return _FoodName; } } 
        public string SchoolID { get { return _SchoolID; } } 
        public string SchoolName { get { return _SchoolName; } }
        public DateTime Time { get { return _Time; } }
        public string ID { get { return _ID; } }

        public Transaction(string StudentID, string FoodID, string FoodName, double Cost, string StudentName) 
        {
            this._Cost = Cost; 
            this._StudentID = StudentID; 
            this._StudentName = StudentName; 
            this._FoodID = FoodID;
            this._FoodName = FoodName;
            this._SchoolID = MainWindow.ThisSchool.ID;
            //this.SchoolID = "test"; //DEBUG
            this._SchoolName = MainWindow.ThisSchool.Name;
            //this.SchoolName = ""; //DEBUG
            this._Time = DateTime.Now;
            this._ID = Time.ToString("yyyyMMddHHmmssff") + SchoolID; //ID is a number (stored as a string) generated from the current year, month, day, minute, second, two decimal digits of a second, and the SchoolID. Hours are in 24hr time
        }

        [JsonConstructor]
        public Transaction(double Cost, string StudentID, string StudentName, string FoodID, string FoodName, string SchoolID, string SchoolName, DateTime Time, string ID)
        {
            this._Cost = Cost;
            this._StudentID = StudentID;
            this._StudentName = StudentName;
            this._FoodID = FoodID;
            this._FoodName = FoodName;
            this._SchoolID = SchoolID; 
            this._SchoolName = SchoolName;
            this._Time = Time;
            this._ID = ID; 
        }



    }
}
