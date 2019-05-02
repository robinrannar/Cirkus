using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Ticket
    {
        public int Id { get; set; }
        public bool Reservation = false;
        public int Price
        {
            get
            {
                return Type.Price;
            }
        }
        public int Customer;
        public TicketType Type { get; set; }
        public List<ActSeat> ActSeats { get; set; }


        public string Tickettype
        {
            get
            {
                return Type.Name;
            }
        }

        public Ticket()
        {
            ActSeats = new List<ActSeat>();
        }

        
    }
}
