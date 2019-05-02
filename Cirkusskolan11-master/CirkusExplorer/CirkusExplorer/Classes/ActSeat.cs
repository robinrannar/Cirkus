using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class ActSeat
    {
        //Egenskaper
        public Act TheAct { get; set; }
        public Seat TheSeat { get; set; }
        public Show TheShow { get; set; }
        public int TicketId { get; set; }
        public string SeatName
        {
            get
            {
                int actnumber = 0;
                for (int i = 0; i < TheShow.Acts.Count; i++)
                {
                    if (TheShow.Acts[i] == TheAct)
                    {
                        actnumber = i+1;
                        break;
                    }
                }
                return "\t Plats: " + TheSeat.Name + "\t Akt " + actnumber;
            } 
        }
        
        //Metoder
        public bool SeatInSeatAct(Seat seat, Show show)
        {
            if (seat == TheSeat && TheShow == show)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool SeatInSeatAct(Seat seat, Show show, Act act)
        {
            if (seat == TheSeat && TheShow == show && TheAct == act)
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
