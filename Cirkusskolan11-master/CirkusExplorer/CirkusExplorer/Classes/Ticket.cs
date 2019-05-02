using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Ticket
    {
        //Egenskaper
        public int Id { get; set; }
        public TicketType Type { get; set; }
        public List<ActSeat> ActSeats { get; set; }
        public int CartId { get; set; }
        public int Price
        {
            get
            {
                int price = 0;
                price += Type.Price;
                foreach (var ac in ActSeats)
                {
                    price += ac.TheSeat.Price;
                }
                return price;
            }
        }
        public string Tickettype
        {
            get
            {
                return Type.Name;
            }
        }
        public string TicketInfo
        {
            get
            {
                string showName = "";
                if (ActSeats.Count != 0)
                {
                    showName = ActSeats[0].TheShow.Name + " ";
                }
                return showName + Type.Name + "  " + Type.Price + "kr";
            }
        }
        
        //Konstruktör
        public Ticket()
        {
            ActSeats = new List<ActSeat>();
        }
    }
}
