using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Cirkus
    {
        public List<Show> Shows { get; set; }
        public List<Act> Acts { get; set; }
        public List<TicketType> TicketTypes { get; set; }
        public List<ActSeat> ActSeats { get; set; }
        public List<Cart> Carts { get; set; }
        public Cart ActiveCart { get; set; }
        public Show ActiveShow { get; set; }

        public Cirkus()
        {
            Shows = new List<Show>();
            Acts = new List<Act>();
            TicketTypes = new List<TicketType>();
            ActSeats = new List<ActSeat>();
            Carts = new List<Cart>();
        }
    }
}