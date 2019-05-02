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
        public string SalestartDate { get; set; }
        public bool Visible { get; set; }
        public Cirkustent Tent { get; set; }
        public List<Act> Acts { get; set; }

        public Show()
        {
            Acts = new List<Act>();
        }
   
        public bool NotOldShow()
        {
            DateTime today = DateTime.Today;
            DateTime dt = Convert.ToDateTime(Date);
            if (dt >= today)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ActiveShowByDate()
        {
            DateTime today = DateTime.Today;
            DateTime dt = Convert.ToDateTime(SalestartDate);
            if (dt <= today)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
