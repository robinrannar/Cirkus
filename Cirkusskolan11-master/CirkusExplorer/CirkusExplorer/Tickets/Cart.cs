using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Cart
    {
        public int Id { get; set; }
        public List<Ticket> Tickets { get; set; }
        public string Salesman { get; set; } //Hårdkodad
        public string Customer { get; set; } //Hårdkodad

        public Cart()
        {
            Tickets = new List<Ticket>();
            Salesman = "Dennis";
            Customer = "Greger";
        }

        public Cart(Cart cart)
        {
            this.Id = cart.Id;
            this.Tickets = cart.Tickets;
            this.Salesman = cart.Salesman;
            this.Customer = cart.Customer;
        }

        public Ticket AddTicket(Show show, TicketType type)
        {
            Ticket ticket = new Ticket() { Type = type };
            Tickets.Add(ticket);
            return ticket;
        }

        public bool RemoveTicket(Ticket ticket)
        {
            return Tickets.Remove(ticket);
        }

        public string NumberOfTicketsOfType(string type)
        {
            int count = 0;
            foreach (var ticket in Tickets)
            {
                if (ticket.Type.Name == type)
                {
                    count++;
                }
            }
            return count.ToString();
        }
    }
}
