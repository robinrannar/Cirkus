using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Customer
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Phonenumber { get; set; }
        public string PrintCustomer
        {
            get
            {
                return "Namn: " + Firstname +" " + Lastname + "\tTelefonnummer: " + Phonenumber;
            }
        }
    }
}
