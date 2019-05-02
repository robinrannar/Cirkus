using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Cirkustent
    {
        //Egenskaper
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Section> Sections { get; set; }

        //Konstruktör
        public Cirkustent()
        {
            Sections = new List<Section>();
        }
    }
}
