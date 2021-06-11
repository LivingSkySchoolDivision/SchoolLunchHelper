using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lunch_project
{
    public class FoodItem
    {
        private string _Name;
        private string _ID;
        private string _SchoolID;
        private double _Cost;
        private string _Description;

        //private setters are for EF Core
        public string Name { get { return _Name; } private set { _Name = value; } } 
        public string ID { get { return _ID; } private set { _ID = value; } }
        public string SchoolID { get { return _SchoolID; } private set { _SchoolID = value; } }
        public double Cost { get { return _Cost; } private set { _Cost = value; } }
        public string Description { get { return _Description; } private set { _Description = value; } }

        /*
        public FoodItem() //DEBUG
        {
            Name = "";
            ID = "-1";
            SchoolID = "-1";
            Cost = 0.0;
            Description = "";
        }
        */

        public FoodItem(string Name, double Cost)
        {
            _Name = Name;
            //this.SchoolID = "test"; //DEBUG
            _SchoolID = MainWindow.ThisSchool.ID;
            _ID = DateTime.Now.ToString("yyyyMMddHHmmssff") + SchoolID;
            _Cost = Cost;
            _Description = "";
        }

        public FoodItem(string Name, double Cost, string Description) 
        {
            _Name = Name;
            //this.SchoolID = "test"; //DEBUG
            _SchoolID = MainWindow.ThisSchool.ID;
            _ID = DateTime.Now.ToString("yyyyMMddHHmmssff") + SchoolID;
            _Cost = Cost;
            _Description = Description;
        }

        //deserialization constructor
        public FoodItem(string Name, string SchoolID, string ID, double Cost, string Description)
        {
            _Name = Name;
            _SchoolID = SchoolID;
            _ID = ID;
            _Cost = Cost;
            _Description = Description;
        }

        

    }

}
