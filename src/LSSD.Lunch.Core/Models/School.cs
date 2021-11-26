using System;

namespace LSSD.Lunch
{
    public class School : IGUIDable
    {    
        public Guid Id { get; set; }
        public string Name { get;set; }
    }
}
