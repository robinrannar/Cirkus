using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Act
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Starttime { get; set; }
        public string Endtime { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public string PrintActs
        {
            get
            {
                return "Namn:" + Name  + "\tPris:" + Price + "" + "\tStarttid:" + Starttime + "\tSluttid:" + Endtime;
            }
        }
    }
}
