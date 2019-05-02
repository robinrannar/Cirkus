using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Rapport
    {
        public List<RapportShow> RapportShows { get; set; }
        public Rapport()
        {
            RapportShows = new List<RapportShow>();
        }
        public int GetTotalTickets()
        {
            int count = 0;
            foreach (var rapportshow in RapportShows)
            {
                count += rapportshow.GetTotalTickets();
            }
            return count;
        }
        public int GetTotalTickets(string tickettype)
        {
            int count = 0;
            foreach (var rapportshow in RapportShows)
            {
                count += rapportshow.GetTotalTickets(tickettype);
            }
            return count;
        }
        public int GetTotalSumma()
        {
            int count = 0;
            foreach (var rapportshow in RapportShows)
            {
                count += rapportshow.GetTotalSumma();
            }
            return count;
        }
        public int GetTotalSumma(string tickettype)
        {
            int count = 0;
            foreach (var rapportshow in RapportShows)
            {
                count += rapportshow.GetTotalSumma(tickettype);
            }
            return count;
        }
    }
    public class RapportShow
    {
        //Egenskaper
        public List<RapportTicket> RapportTickets { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        
        //Konstruktör
        public RapportShow()
        {
            RapportTickets = new List<RapportTicket>();
        }
        //Metoder
        public int GetTotalTickets()
        {
            int count = 0;
            foreach (var tickets in RapportTickets)
            {
                count += tickets.Count;
            }
            return count;
        }
        public int GetTotalTickets(string tickettype)
        {
            int count = 0;
            foreach (var tickets in RapportTickets)
            {
                if (tickets.Tickettypes == tickettype)
                {
                    count += tickets.Count;
                }
            }
            return count;
        }
        public int GetTotalSumma()
        {
            int count = 0;
            foreach (var tickets in RapportTickets)
            {
                count += tickets.Summa;
            }
            return count;
        }
        public int GetTotalSumma(string tickettype)
        {
            int count = 0;
            foreach (var tickets in RapportTickets)
            {
                if (tickets.Tickettypes == tickettype)
                {
                    count += tickets.Summa;
                }
            }
            return count;
        }
    }
    public class RapportTicket
    {
        public string Tickettypes { get; set; }
        public int Summa { get; set; }
        public int Count { get; set; }
    }
}
