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
        public int ID { get; }
        public int SchoolID { get; }
        public double Cost { get; set; }
        public string Description { get; set; }


        public FoodItem() //DEBUG
        {
            Name = "";
            ID = -1;
            SchoolID = -1;
            Cost = 0.0;
            Description = "";
        }

        public FoodItem(string Name, int ID, int SchoolID, double Cost) //need to generate unique ID instead of passing it in
        {
            this.Name = Name;
            this.ID = ID;
            this.SchoolID = SchoolID;
            this.Cost = Cost;
            Description = "";
        }

        public FoodItem(string Name, int ID, int SchoolID, double Cost, string Description) //need to generate unique ID instead of passing it in
        {
            this.Name = Name;
            this.ID = ID;
            this.SchoolID = SchoolID;
            this.Cost = Cost;
            this.Description = Description;
        }

        

    }

}
