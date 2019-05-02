using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirkusExplorer
{
    public class Cirkus
    {
        private Sql sql { get; set; }
        public List<Customer> Customers { get; set; }
        public List<Systemuser> Systemusers { get; set; }
        public List<Show> Shows { get; set; }
        public List<Act> Acts { get; set; }
        public List<Cart> Carts { get; set; }
        public List<Cirkustent> Tents { get; set; }
        public List<Section> Sections { get; set; }
        public List<Ticket> Tickets { get; set; }
        public List<TicketType> Tickettypes { get; set; }
        public List<Authorisation> Auths { get; set; }
        public List<ActSeat> ActSeats { get; set; }
        public Systemuser UserLoggedIn { get; set; }

        //Konstruktör
        public Cirkus()
        {
            sql = new Sql();
            Customers = new List<Customer>();
            Systemusers = new List<Systemuser>();
            Shows = new List<Show>();
            Acts = new List<Act>();
            Carts = new List<Cart>();
            Tents = new List<Cirkustent>();
            Sections = new List<Section>();
            Tickets = new List<Ticket>();
            Tickettypes = new List<TicketType>();
            Auths = new List<Authorisation>();
            ActSeats = new List<ActSeat>();
        }
        //Metoder
        #region Inlogg
        /// <summary>
        /// Metod för att logga in
        /// </summary>
        /// <param name="username">Användarnamn till användarkontot</param>
        /// <param name="password">Lösenord till användarkontot</param>
        /// <returns>Returnerar en bool? true=Administratör, false=Biljettförsäljare, null=Fel inloggningsuppgifter</returns>
        public bool? CheckLogin(string username, string password)
        {
            bool? value = null;
            foreach (var systemuser in Systemusers)
            {
                value = systemuser.LoginUser(username, password);
                if (value == true || value == false)
                {
                    UserLoggedIn = systemuser;
                    return value;
                }
            }
            return value;
        }
        #endregion Inlogg
        //----------------------------------------------------------//
        #region Biljettförsäljning
        /// <summary>
        /// Metod för att se hur många platser de finns på föreställningen
        /// Skickar även tillbaka hur många platser som är tagna i de olika akterna
        /// </summary>
        /// <param name="show">Den valda showen</param>
        /// <param name="cart">Varukorgen där biljetterna läggs till</param>
        /// <param name="takenSeatsInAct">En int array som håller de tagna biljetterna i de olika akterna</param>
        /// <returns>Returnerar antal platser i showen</returns>
        public int NumberOfSeatsInShow(Show show, Cart cart, int[] takenSeatsInAct)
        {
            int seats = 0;
            foreach (var section in show.Tent.Sections)
            {
                foreach (var seat in section.Seats)
                {
                    seats++;
                    foreach (var actseat in ActSeats)
                    {
                        for (int i = 0; i < show.Acts.Count; i++)
                        {
                            if (actseat.TheSeat == seat && actseat.TheShow == show && actseat.TheAct == show.Acts[i])
                            {
                                takenSeatsInAct[i]++;
                                break;
                            }
                        }
                    }
                }
            }
            return seats;
        }
        /// <summary>
        /// Metod för att hämta en TicketType för att använda i andra metoder.
        /// </summary>
        /// <param name="typename">Namnet på den typ ni vill hämta.</param>
        /// <returns>En TicketType</returns>
        public TicketType GetTicketType(string typename)
        {
            foreach (var tickettype in Tickettypes)
            {
                if (tickettype.Name == typename)
                {
                    return tickettype;
                }
            }
            return null;
        }
        /// <summary>
        /// Metod för att ta reda på om läktarplatserna är fulla eller inte och i vilka akter.
        /// </summary>
        /// <param name="show">Den valda showen</param>
        /// <param name="section">Den valda sektionen</param>
        /// <param name="act">Den valda acten annars NULL för hela föreställningen</param>
        /// <returns>En bool?, true om det är ledigt, null om det är upptagen i någon akt, false om alla akter är upptagna</returns>
        public bool? FreeStandInShow(Show show, Section section, Act act, Cart cart)
        {
            int actId = 0;
            bool? free = null;
            int numberOfFullActs = 0;
            int numberOfSeats = section.Seats.Count;
            int[] counter = new int[show.Acts.Count];
            for (int i = 0; i < show.Acts.Count; i++)
            {
                if (act != null && show.Acts[i] == act)
                {
                    actId = i;
                }
                foreach (var ac in ActSeats)
                {
                    if (ac.TheAct == show.Acts[i] && ac.TheSeat.Stand)
                    {
                        counter[i]++;
                    }
                }
                foreach (var ticket in cart.Tickets)
                {
                    foreach (var ac in ticket.ActSeats)
                    {
                        if (ac.TheAct == show.Acts[i] && ac.TheSeat.Stand)
                        {
                            counter[i]++;
                        }
                    }
                }
            }
            foreach (var i in counter)
            {
                if (i == numberOfSeats)
                {
                    numberOfFullActs++;
                }
            }
            if (act == null)
            {
                if (numberOfFullActs == show.Acts.Count)
                {
                    free = false;
                }
                else if (numberOfFullActs > 0 && numberOfFullActs < show.Acts.Count)
                {
                    free = null;
                }
                else
                {
                    free = true;
                }
            }
            else
            {
                if (counter[actId] == numberOfSeats)
                {
                    free = false;
                }
                else
                {
                    free = true;
                }
            }
            return free;
        }
        /// <summary>
        /// Räkna ut ett ID på ett stol för läktarplats som programmet kan hålla tills detta skall matas in i databasen
        /// </summary>
        /// <param name="show">Den valda showen</param>
        /// <returns>Integer som visar var nästa lediga läktarplats är</returns>
        public int NextFreeStandId(Show show, Cart cart)
        {
            int count = 0;
            foreach (var ac in ActSeats)
            {
                if (ac.TheSeat.Stand && ac.TheShow == show)
                {
                    count++;
                }
            }
            foreach (var ticket in cart.Tickets)
            {
                foreach (var ac in ticket.ActSeats)
                {
                    if (ac.TheSeat.Stand && ac.TheShow == show)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        /// <summary>
        /// Metod för att få ut om en föreställnings säten är uppbokade i
        /// alla akter eller bara vissa akter.
        /// </summary>
        /// <param name="show">Vald show</param>
        /// <param name="seat">Valt säte</param>
        /// <param name="act">Vald act annars NULL för hela föreställningen.</param>
        /// <returns></returns>
        public int CheckForBusySeatsIn(Show show, Seat seat, Act act, Cart cart)
        {
            int counter = 0;
            foreach (var actseat in ActSeats)
            {
                if (act == null ? actseat.SeatInSeatAct(seat, show) : actseat.SeatInSeatAct(seat, show, act))
                {
                    counter++;
                }
            }
            foreach (var ticket in cart.Tickets)
            {
                foreach (var actseat in ticket.ActSeats)
                {
                    if (act == null ? actseat.SeatInSeatAct(seat, show) : actseat.SeatInSeatAct(seat, show, act))
                    {
                        counter++;
                    }
                }
            }
            return counter;
        }
        #endregion Biljettförsäljning
        //----------------------------------------------------------//
        #region Akter
        /// <summary>
        /// Metod för att lägga in ny akt
        /// </summary>
        /// <param name="name">Namnet på akten</param>
        /// <param name="starttime">Vilket klockslag akten börjar</param>
        /// <param name="endtime">Vilket klockslag akten slutar</param>
        /// <param name="description">Beskrivning av akten</param>
        /// <param name="price">Priset för akten</param>
        /// <param name="act">Den akten som skall uppdateras, NULL om det skall läggas till ny akt</param>
        /// <returns></returns>
        public void InsertUpdateAct(string name, string starttime, string endtime, string description, int price, Act act)
        {
            bool insert = false;
            if (act == null)
            {
                insert = true;
                act = new Act();
            }
            act.Name = name;
            act.Starttime = starttime;
            act.Endtime = endtime;
            act.Description = description;
            act.Price = price;
            if (insert && sql.InsertAct(act))
            {
                Acts.Add(act);
            }
            else
            {
                sql.UpdateAct(act);
            }
        }
        #endregion Akter
        //----------------------------------------------------------//
        #region Föreställningar
        /// <summary>
        /// Metod för att uppdatera om en föreställning är aktiverad eller inte
        /// </summary>
        /// <param name="show">Den show som skall ändra om den är aktiv eller inte</param>
        /// <param name="visible">True=Om den ska aktiveras, False=Om den ska inaktiveras</param>
        public void UpdateShowVisibility(Show show, bool visible)
        {
            show.Visible = visible;
            sql.UpdateVisibility(show);
        }
        /// <summary>
        /// Metod för att lägga in eller uppdatera en show
        /// </summary>
        /// <param name="show">Den showen som skall uppdateras, NULL om ny skall läggas in</param>
        /// <param name="name">Namnet på föreställningen</param>
        /// <param name="date">Datumet för föreställningen</param>
        /// <param name="summary">Beskrivning av föreställningen</param>
        /// <param name="salestartdate">När biljetterna ska börja säljas</param>
        /// <param name="tent">Vilket tält föreställningen ska hållas i</param>
        /// <param name="acts">Vilka akter som föreställningen innehåller</param>
        public void InsertOrUpdateShow(Show show, string name, string date, string summary, string salestartdate, Cirkustent tent, List<Act> acts)
        {
            bool insert = false;
            if (show == null)
            {
                insert = true;
                show = new Show();
            }
            show.Name = name;
            show.Date = date;
            show.Summary = summary;
            show.SalestartDate = salestartdate;
            show.Tent = tent;
            show.Acts = acts;
            if (insert && sql.InsertShow(show))
            {
                Shows.Add(show);
            }
            else
            {
                sql.UpdateShow(show);
            }
        }
        #endregion Föreställningar
        //----------------------------------------------------------//
        #region Reservation
        /// <summary>
        /// Metod för att lägga till eller uppdatera reservationsuppgifter till en kund
        /// </summary>
        /// <param name="customer">De uppgifter som skall uppdateras, NULL=Lägg till nya</param>
        /// <param name="fname">Förnamnet på kunden</param>
        /// <param name="lname">Efternamnet på kunden</param>
        /// <param name="phonenumb">Kundens telefonnummer</param>
        public void InsertOrUpdateCustomer(Customer customer, string fname, string lname, string phonenumb)
        {
            bool insert = false;
            if (customer == null)
            {
                insert = true;
                customer = new Customer();
            }
            customer.Firstname = fname;
            customer.Lastname = lname;
            customer.Phonenumber = phonenumb;
            if (insert && sql.InsertCustomer(customer))
            {
                Customers.Add(customer);
            }
            else
            {
                sql.UpdateCustomers(customer);
            }
        }
        #endregion Reservation
        //----------------------------------------------------------//
        #region Systemanvändare
        /// <summary>
        /// Metod för att lägga till eller uppdatera en systemanvändare
        /// </summary>
        /// <param name="systemuser">Systemanvändaren att uppdatera, NULL=lägg till ny</param>
        /// <param name="username">Systemanvändarens användarnamn</param>
        /// <param name="password">Systemanvändarens password</param>
        /// <param name="firstname">Systemanvändarens förnamn</param>
        /// <param name="lastname">Systemanvändarens efternamn</param>
        /// <param name="personnumber">Systemanvändarens personnummer</param>
        /// <param name="adress">Systemanvändarens adress</param>
        /// <param name="email">Systemanvändarens epostadress</param>
        /// <param name="auth">Systemanvändarens behörighet</param>
        public void InsertOrUpdateSystemusers(Systemuser systemuser, string username, string password, string firstname, string lastname, string personnumber, string adress, string email, Authorisation auth)
        {
            bool insert = false;
            if(systemuser == null)
            {
                insert = true;
                systemuser = new Systemuser();
            }
            systemuser.Username = username;
            systemuser.Password = password;
            systemuser.Firstname = firstname;
            systemuser.Lastname = lastname;
            systemuser.Personnumber = personnumber;
            systemuser.Adress = adress;
            systemuser.Email = email;
            systemuser.Auth = auth;
            if (insert && sql.InsertSystemuser(systemuser))
            {
                Systemusers.Add(systemuser);
            }
            else
            {
                sql.UpdateSystemuser(systemuser);
            }
        }
        /// <summary>
        /// Metod för att aktivera eller inaktivera en systemanvändare
        /// </summary>
        /// <param name="suser">Systemanvändaren</param>
        /// <param name="auth">Den nya behörighete, NULL=Inaktiverin av användare</param>
        public void UpdateActivateOrInactivateSystemusers(Systemuser suser, Authorisation auth)
        {
            suser.Auth = auth;
            sql.UpdateSystemuser(suser);
        }
        #endregion Systemanvändare
        //----------------------------------------------------------//
        #region Hämta från databasen
        /// <summary>
        /// Metod för att hämta data från databasen som endast behövs hämtas vid start.
        /// </summary>
        public void GetDataWhenProgramStart()
        {
            Tents.Clear();
            Auths.Clear();
            Tickettypes.Clear();
            sql.GetTents(Tents);
            sql.GetAuthorisation(Auths);
            sql.GetTicketType(Tickettypes);

            GetSystemusers();
            GetCartsTicketActSeat();
        }
        public void GetSystemusers()
        {
            Systemusers.Clear();
            sql.GetSystemusers(Systemusers, Auths);
        }
        public void GetCartsTicketActSeat()
        {
            GetShows();
            GetCustomers();
            ActSeats.Clear();
            Tickets.Clear();
            Carts.Clear();
            sql.GetActSeat(ActSeats, Shows);
            sql.GetTickets(Tickets, Tickettypes, ActSeats);
            sql.GetCarts(Carts, Customers, Systemusers, Tickets);
        }
        private void GetCustomers()
        {
            Customers.Clear();
            sql.GetCustomers(Customers);
        }
        private void GetShows()
        {
            Acts.Clear();
            Shows.Clear();
            sql.GetActs(Acts);
            sql.GetShows(Shows, Tents, Acts);
            foreach (var show in Shows)
            {
                if (show.ActiveShowByDate())
                {
                    show.Visible = true;
                    sql.UpdateVisibility(show);
                }
            }
        }
        public Rapport GetRapport(string fromdate, string todate, Show show)
        {
            Rapport rapport = new Rapport();
            sql.GetRapportShows(rapport, show, fromdate, todate);
            return rapport;
        }
        #endregion Hämta från databasen
        //----------------------------------------------------------//
        #region Lägga till i databasen
        /// <summary>
        /// Metod för att lägga in en cart i databasen
        /// med medföljande biljetter och platser
        /// </summary>
        /// <param name="cart">Den carten som ska läggas in</param>
        public void InsertCart(Cart cart)
        {
            cart.Salesman = UserLoggedIn;
            int cartid;
            if (cart.TicketBuyer == null)
            {
                cartid = sql.InsertBuyCart(cart);
            }
            else
            {
                cartid = sql.InsertCart(cart);
            }
            foreach (var ticket in cart.Tickets)
            {
                int ticketid = sql.InsertTicket(cartid, ticket);
                foreach (var actseat in ticket.ActSeats)
                {
                    sql.InsertActseat(ticketid, actseat);
                }
            }
        }
        #endregion Slut för att lägga till i databasen
        //----------------------------------------------------------//
        #region Uppdatera i databasen
        /// <summary>
        /// Metod för att ta bort reservation så att det räknas som ett köp
        /// </summary>
        /// <param name="RemoveCustomerIdFromCart">Den reservationen som skall köpas</param>
        public void RemoveCustomerIDFromCart(Cart RemoveCustomerIdFromCart)
        {
            sql.RemoveCustomerOnCart(RemoveCustomerIdFromCart);
        }
        public string UpdateTickettypesANDSectionPrices(int adultprice, int youthprice, int kidprice, int sitdownprice, int standprice)
        {
            string errormess = "";
            bool error = false;
            if (adultprice == 0)
            {
                error = true;
                errormess += "Du måste fylla i vuxenpris! \r\n";
            }
            if (youthprice == 0)
            {
                error = true;
                errormess += "Du måste fylla i ungdomspris! \r\n";
            }
            if (kidprice == 0)
            {
                error = true;
                errormess += "Du måste fylla i barnpris! \r\n";
            }
            if (sitdownprice == 0)
            {
                error = true;
                errormess += "Du måste fylla i sittplatspris! \r\n";
            }
            if (standprice == 0)
            {
                error = true;
                errormess += "Du måste fylla i sittplatspris! \r\n";
            }
            if (!error)
            {
                sql.UpdateTicketTypePriceANDSectionPrice(adultprice, youthprice, kidprice, sitdownprice, standprice);
                
            }
            return errormess;
        }
        #endregion Slut uppdatera i databasen
        //----------------------------------------------------------//
        #region Tabort i databasen
        public string RemoveReservation(Cart ca)
        {
            string errormess = "";
            {
                if (ca == null)
                {
                    errormess += "Du måste välja en kundvagn att ta bort!";
                }
                else if (ca != null)
                {
                    sql.RemoveReservation(ca);
                }
            }
            return errormess;
        } // Metod för felhantring vid borttagning av kundvagn
        public string Timelimitreservation()
        {
            string errormess = "";
            sql.TimelimitRemoveReservation(Shows);
            return errormess;
        }
        public string RemoveCustomers(Customer customer)
        {
            string errormess = "";
            {
                if (customer == null)
                {
                    errormess += "Du måste välja en kund att ta bort!";
                }
                else if (customer != null)
                {
                    sql.RemoveCustomers(customer);
                    Customers.Remove(customer);
                }
            }
            return errormess;
        } //Metod för felhantering vid borrtagning av kund
        public void RemoveAct(Act act)
        {
            sql.RemoveAct(act);
        }
        #endregion Ta bort i databasen
        //----------------------------------------------------------//
    }
}   // Klass

