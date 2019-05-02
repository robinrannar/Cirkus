using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Show
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string Summary { get; set; }
        public string Salestartdate { get; set; }
        public string Salestopdate { get; set; }
        public bool Visible { get; set; }
        public Circustent Tent { get; set; }
        public List<Act> Acts { get; set; }

        public Show(int Id, string Name, string Date, string Summary, string Salestartdate, string Salestopdate, bool Visible, Circustent Tent )
        {
            this.Id = Id;
            this.Name = Name;
            this.Date = Date;
            this.Summary = Summary;
            this.Salestartdate = Salestartdate;
            this.Salestopdate = Salestopdate;
            this.Visible = Visible;
            this.Tent = Tent;
            Acts = new List<Act>();
        }
    }
}
