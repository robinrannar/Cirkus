using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Seat
    {
        //Egenskaper
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public bool Stand { get; set; }

        //Konstruktör
        public Seat(Section section)
        {
            Stand = true;
            Price = section.Price;
        }
    }
}
