using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class ActSeat
    {
        public Show TheShow { get; set; }
        public Act TheAct { get; set; }
        public Seat TheSeat { get; set; }


        public bool SeatInSeatAct (Seat seat, Show show)
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
