using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Systemuser
    {
        //Egenskaper
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Personnumber { get; set; }
        public string Adress { get; set; }
        public string Email { get; set; }
        public Authorisation Auth { get; set; }
        public string PrintSystemuser
        {
            get { return "Förnamn:" + Firstname + "\tEfternamn:" + Lastname + "\t\tBehörighet:" + Auth.Type; }
        }
        public string PrintInactiveSystemuser
        {
            get { return "Förnamn:" + Firstname + "\tEfternamn:" + Lastname; }
        }

        //Konstruktör
        public Systemuser()
        {

        }
        //Metoder
        /// <summary>
        /// Metod för att kolla om inloggningsuppgifter hör till denna systemanvändare
        /// </summary>
        /// <param name="username">Användarnamn för användarkontot</param>
        /// <param name="password">Lösenord för användarkontot</param>
        /// <returns>
        ///     True=Rätt uppgifter och användarkontot har administratörsbehörighet
        ///     False=Rätt uppgifter och användarkontot har biljettförsäljningsbehörighet
        ///     Null=Fel uppgifter
        /// </returns>
        public bool? LoginUser(string username, string password)
        {
            bool? value = null;
            if (Username == username && Password == password)
            {
                if (Auth.Type == "Administratör")
                {
                    value = true;
                }
                else
                {
                    value = false;
                }
            }
            return value;
        }
    }
}
