using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Cart
    {
        //Egenskaper
        public int Id { get; set; }
        public List<Ticket> Tickets { get; set; }
        public Systemuser Salesman { get; set; }
        public Customer TicketBuyer { get; set; }
        public string PrintCustomer
        {
            get
            {
                return TicketBuyer.PrintCustomer;
            }
        }
        public int GetCartPrice
        {
            get
            {
                int price = 0;
                foreach (var ticket in Tickets)
                {
                    price += ticket.Price;
                }
                return price;
            }
        }
        //Kontruktör
        public Cart ()
        {
            Tickets = new List<Ticket>();
        }
        //Metoder
        /// <summary>
        /// Metod för att lägga till biljett i cart.
        /// </summary>
        /// <param name="show">Vald föreställning</param>
        /// <param name="type">Vald biljettyp</param>
        /// <returns></returns>
        public Ticket AddTicket(TicketType type)
        {
            Ticket ticket = new Ticket()
            {
                Type = type
            };
            Tickets.Add(ticket);
            return ticket;
        }
        /// <summary>
        /// Metod för att ta bort biljett och tillhörande object.
        /// </summary>
        /// <param name="ticket">Den biljetten som skall tas bort.</param>
        /// <returns>Returnerar en lista med ID på de sätena som är bortagna.</returns>
        public List<Seat> RemoveTicket(string tickettype)
        {
            List<Seat> acseats = new List<Seat>();
            for (int i = Tickets.Count - 1; i >= 0; i--)
            {
                if (Tickets[i].Tickettype == tickettype)
                {
                    foreach (var ac in Tickets[i].ActSeats)
                    {
                        acseats.Add(ac.TheSeat);
                    }
                    Tickets[i].ActSeats.Clear();
                    Tickets.Remove(Tickets[i]);
                    break;
                }   
            }
            return acseats;
        }
        /// <summary>
        /// Medtod för att returnera hur många av en viss typ biljett som finns i carten.
        /// </summary>
        /// <param name="type">Namn på biljettypen som vi vill veta hur många som finns.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Metod för att lägga till en plats till en biljett i cart
        /// </summary>
        /// <param name="show">Den valda showen</param>
        /// <param name="seat">Den valda platsen</param>
        /// <param name="pxcolor">Färg på sätet man klickar på</param>
        /// <param name="act">Vald act annars NULL för hela föreställningen.</param>
        /// <returns></returns>
        public string AddSeatToCart(Show show, Seat seat, string pxcolor, Act act)
        {
            string color = "";
            bool added = false;
            ActSeat actseat = new ActSeat()
            {
                TheAct = act,
                TheSeat = seat,
                TheShow = show
            };

            if (act == null && pxcolor != "DarkRed")
            {
                foreach (var ticket in Tickets)
                {
                    if (ticket.ActSeats.Count == 0)
                    {
                        for (int i = 0; i < show.Acts.Count; i++)
                        {
                            ActSeat ac = new ActSeat()
                            {
                                TheAct = show.Acts[i],
                                TheSeat = seat,
                                TheShow = show
                            };
                            ticket.ActSeats.Add(ac);
                        }
                        added = true;
                        color = "DarkRed";
                        break;
                    }
                }
            }
            else if (act != null && pxcolor != "DarkRed")
            {
                bool next = false;
                foreach (var ticket in Tickets)
                {
                    foreach (var ac in ticket.ActSeats)
                    {
                        if (ac.TheAct == act)
                        {
                            next = true;
                            break;
                        }
                    }
                    if (next)
                    {
                        next = false;
                        continue;
                    }
                    if (ticket.ActSeats.Count < show.Acts.Count)
                    {
                        ActSeat ac = new ActSeat()
                        {
                            TheAct = act,
                            TheSeat = seat,
                            TheShow = show
                        };
                        ticket.ActSeats.Add(ac);
                        added = true;
                        color = "DarkRed";
                        break;
                    }
                }
            }
            if (!added)
            {
                if (act == null)
                {
                    List<ActSeat> rmseats = new List<ActSeat>();
                    foreach (var ticket in Tickets)
                    {
                        foreach (var ac in ticket.ActSeats)
                        {
                            if (ac.TheSeat == seat && ac.TheShow == show)
                            {
                                rmseats.Add(ac);
                            }
                        }
                        foreach (var seats in rmseats)
                        {
                            ticket.ActSeats.Remove(seats);
                        }
                    }
                }
                else
                {
                    foreach (var ticket in Tickets)
                    {
                        foreach (var ac in ticket.ActSeats)
                        {
                            if (ac.TheSeat == seat && ac.TheShow == show && ac.TheAct == act)
                            {
                                ticket.ActSeats.Remove(ac);
                                break;
                            }
                        }
                    }
                }
                color = "White";
            }
            return color;
        }
        /// <summary>
        /// Metod för att ta bort en läktarplats från varukorgen
        /// </summary>
        /// <param name="act">Valda acten annars NULL för hela föreställningen</param>
        /// <param name="section">Vald section</param>
        /// <param name="show">Vald show</param>
        public void RemoveStandFromCart(Act act, Section section, Show show)
        {
            if (act == null)
            {
                List<ActSeat> rmseats = new List<ActSeat>();
                for (int i = Tickets.Count - 1; i >= 0; i--)
                {
                    foreach (var ac in Tickets[i].ActSeats)
                    {
                        foreach (var seat in section.Seats)
                        {
                            if (ac.TheSeat == seat && ac.TheShow == show)
                            {
                                if (rmseats.Count() == show.Acts.Count())
                                {
                                    return;
                                }
                                rmseats.Add(ac);
                            }
                        }
                    }
                    foreach (var seats in rmseats)
                    {
                        Tickets[i].ActSeats.Remove(seats);
                    }
                }
            }
            else
            {
                foreach (var ticket in Tickets)
                {
                    foreach (var ac in ticket.ActSeats)
                    {
                        foreach (var seat in section.Seats)
                        {
                            if (seat == ac.TheSeat && ac.TheAct == act && ac.TheShow == show)
                            {
                                ticket.ActSeats.Remove(ac);
                                return;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Metod för att hämta ticketinfo och alla sitt/läktarplatser som hör till.
        /// </summary>
        /// <returns>En lista med strings i ordning med biljettnamn, pris och sittplatser med pris.</returns>
        public List<string> GetTicketAndActSeats()
        {
            List<string> result = new List<string>();
            foreach (var ticket in Tickets)
            {
                result.Add(ticket.TicketInfo);
                foreach (var seat in ticket.ActSeats)
                {
                    result.Add(seat.SeatName);
                }
            }
            return result;
        }
        public bool AllTicketsHaveSeats()
        {
            foreach (var ticket in Tickets)
            {
                if (ticket.ActSeats.Count == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
