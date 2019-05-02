using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Circustent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Section> Sections { get; set; }

        public Circustent()
        {
            Sections = new List<Section>();
        }
    }
}
