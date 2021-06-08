using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lunch_project
{
    public class FoodItem
    {
        public string Name { get; set; } 
        public string ID { get; }
        public string SchoolID { get; }
        public double Cost { get; set; }
        public string Description { get; set; }

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
            this.Name = Name;
            //this.SchoolID = "test"; //DEBUG
            this.SchoolID = MainWindow.ThisSchool.ID;
            this.ID = DateTime.Now.ToString("yyyyMMddHHmmssff") + SchoolID;
            this.Cost = Cost;
            Description = "";
        }

        public FoodItem(string Name, double Cost, string Description) 
        {
            this.Name = Name;
            //this.SchoolID = "test"; //DEBUG
            this.SchoolID = MainWindow.ThisSchool.ID;
            this.ID = DateTime.Now.ToString("yyyyMMddHHmmssff") + SchoolID;
            this.Cost = Cost;
            this.Description = Description;
        }

        //deserialization constructor
        public FoodItem(string Name, string SchoolID, string ID, double Cost, string Description)
        {
            this.Name = Name;
            this.SchoolID = SchoolID;
            this.ID = ID;
            this.Cost = Cost;
            this.Description = Description;
        }

        

    }

}
