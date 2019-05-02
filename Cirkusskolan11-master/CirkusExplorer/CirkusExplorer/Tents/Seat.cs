using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Seat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }

        public Seat(int Id, string Name, Section section)
        {
            this.Id = Id;
            this.Name = Name;
            Price = section.Price;
        }
    }
}
