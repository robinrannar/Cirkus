using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pgEngine;
using System.Configuration;
using System.Data;

namespace CirkusExplorer
{
    public class Sql
    {
        public string connection = ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
        public DataTable dt;

        #region Endast metoder för att hämta från databasen 

        #region Hämta vid programstart
        //Metod som hämtar alla behörigheter
        public void GetAuthorisation(List<Authorisation> authorisationlist)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT * FROM authorisations";
            dt = db.Query(sql);
            Authorisation auth;

            foreach (DataRow dr in dt.Rows)
            {
                auth = new Authorisation()
                {
                    Id = (int)dr["id"],
                    Type = (string)dr["type"],
                };
                authorisationlist.Add(auth);
            }
        }
        //Metod för att hämta cirkustält
        public void GetTents(List<Cirkustent> tentlist)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT * FROM circustents;";
            dt = db.Query(sql);
            Cirkustent tent;
            foreach (DataRow t in dt.Rows)
            {
                tent = new Cirkustent()
                {
                    Id = (int)t["id"],
                    Name = (string)t["name"]
                };
                GetSections(tent);
                tentlist.Add(tent);
            }
        }
        //Metod för att hämta sektioner och dess platser
        public void GetSections(Cirkustent tent)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT * FROM sections WHERE (sections.circustent) = (@TentId);";
            db.AddParameter("@TentId", tent.Id);
            dt = db.Query(sql);
            Section section;
            foreach (DataRow dr in dt.Rows)
            {
                section = new Section()
                {
                    Id = (int)dr["id"],
                    Name = (string)dr["name"],
                    Price = (int)dr["price"],
                };
                GetSeats(section);
                tent.Sections.Add(section);
            }
        }
        //Metod för att hämta alla säten till en viss sektion
        public void GetSeats(Section section)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT * FROM seats WHERE (seats.section) = (@SectionId) ORDER BY id;";
            db.AddParameter("@SectionId", section.Id);
            dt = db.Query(sql);
            Seat seat;
            foreach (DataRow dr in dt.Rows)
            {
                seat = new Seat(section)
                {
                    Name = (string)dr["name"],
                    Id = (int)dr["id"]
                };
                if (seat.Name != "Stand")
                {
                    seat.Stand = false;
                }
                section.Seats.Add(seat);
            }
        }
        //Metod för att hämta biljettyper från databasen
        public void GetTicketType(List<TicketType> tickettypelist)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT * FROM tickettypes;";
            dt = db.Query(sql);
            TicketType ticktyp;
            foreach (DataRow dr in dt.Rows)
            {
                ticktyp = new TicketType()
                {
                    Id = (int)dr["id"],
                    Name = (string)dr["name"],
                    Price = (int)dr["price"]
                };
                tickettypelist.Add(ticktyp);
            }
        }
        #endregion Hämta vid programstart
        //Metod för att hämta akter
        public List<Act> GetActs(List<Act> actlist)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT * FROM acts";
            dt = db.Query(sql);
            Act acts;
            foreach (DataRow dr in dt.Rows)
            {
                acts = new Act()
                {
                    Id = (int)dr["id"],
                    Name = (string)dr["name"],
                    Starttime = (string)dr["starttime"],
                    Endtime = (string)dr["endtime"],
                    Description = (string)dr["description"],
                    Price = (int)dr["price"],
                };
                actlist.Add(acts);
            }
            return actlist;
        }
        //Metod för att hämta föreställningar
        public void GetShows(List<Show> showlist, List<Cirkustent> tentlist, List<Act> acts)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT * FROM shows;";
            dt = db.Query(sql);
            Show show;
            foreach (DataRow dr in dt.Rows)
            {
                show = new Show();
                {
                    show.Id = (int)dr["id"];
                    show.Name = (string)dr["name"];
                    show.Date = (string)dr["date"];
                    show.Summary = (string)dr["summary"];
                    show.SalestartDate = (string)dr["salestartdate"];
                    show.Visible = (bool)dr["visible"];
                };
                foreach (var tent in tentlist)
                {
                    if (tent.Id == (int)dr["circustent"])
                    {
                        show.Tent = tent;
                        break;
                    }
                }
                GetShowActs(show, acts);
                //DateTime today = DateTime.Today;
                //DateTime dt = Convert.ToDateTime(show.Date);
                //if (dt >= today)
                //{ //TODO bortmarkerat för att dessa rader funkar inte med rapportsystemet.
                    showlist.Add(show);
                //}
            }
        }//Hämtar alla shower som skall vara. Finns en check som kollar mot dagens datum. 
        public void GetShowActs(Show show, List<Act> acts)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT * FROM showacts;";
            dt = db.Query(sql);
            foreach (DataRow dr in dt.Rows)
            {
                foreach (var act in acts)
                {
                    if (act.Id == (int)dr["act"] && show.Id == (int)dr["show"])
                    {
                        show.Acts.Add(act);
                    }
                }
            }
        }
        //Metod som används för att hämta rätt showact id när ny actseat ska sparas till databasen.
        public int GetShowActs(ActSeat actseat)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT * FROM showacts;";
            dt = db.Query(sql);
            foreach (DataRow dr in dt.Rows)
            {
                if (actseat.TheAct.Id == (int)dr["act"] && actseat.TheShow.Id == (int)dr["show"])
                {
                    return (int)dr["id"];
                }
            }
            return 0;
        }
        //Hämtar alla actseats
        public List<ActSeat> GetActSeat(List<ActSeat> actseatslist, List<Show> shows)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT * FROM actseats INNER JOIN showacts ON showacts.id = actseats.showact;";
            dt = db.Query(sql);
            ActSeat actseat;
            foreach (DataRow dr in dt.Rows)
            {
                actseat = new ActSeat() { TicketId = (int)dr["ticket"] };
                foreach (var show in shows)
                {
                    if (show.Id == (int)dr["show"])
                    {
                        actseat.TheShow = show;
                        foreach (var act in show.Acts)
                        {
                            if (act.Id == (int)dr["act"])
                            {
                                actseat.TheAct = act;
                                break;
                            }
                        }
                        foreach (var section in show.Tent.Sections)
                        {
                            foreach (var seat in section.Seats)
                            {
                                if (seat.Id == (int)dr["seat"])
                                {
                                    actseat.TheSeat = seat;
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
                actseatslist.Add(actseat);
            }
            return actseatslist;
        }
        //Metod för att hämta kunder från databasen
        public void GetCustomers(List<Customer> customerlist)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT * FROM customers";
            dt = db.Query(sql);
            Customer customer;

            foreach (DataRow dr in dt.Rows)
            {
                customer = new Customer()
                {
                    Id = (int)dr["id"],
                    Firstname = (string)dr["firstname"],
                    Lastname = (string)dr["lastname"],
                    Phonenumber = (string)dr["phonenumber"],
                };
                customerlist.Add(customer);
            }
        }
        //Metod som hämtar alla Systemusers
        public void GetSystemusers(List<Systemuser> systemuserlist, List<Authorisation> auths)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT * FROM systemusers LEFT JOIN authorisations ON systemusers.authorisation = authorisations.id";
            dt = db.Query(sql);
            Systemuser user;
            foreach (DataRow dr in dt.Rows)
            {
                user = new Systemuser();
                {
                    user.Id = (int)dr["id"];
                    user.Username = (string)dr["username"];
                    user.Password = (string)dr["password"];
                    user.Firstname = (string)dr["firstname"];
                    user.Lastname = (string)dr["lastname"];
                    user.Personnumber = (string)dr["personnumber"];
                    user.Adress = (string)dr["adress"];
                    user.Email = (string)dr["email"];
                };
                foreach (var auth in auths)
                {
                    if (dr["authorisation"].GetType() == typeof(int) && auth.Id == (int)dr["authorisation"])
                    {
                        user.Auth = auth;
                        break;
                    }
                }
                systemuserlist.Add(user);
            }
        }
        //Metod för att hämtar alla  biljetter
        public void GetTickets(List<Ticket> ticketlist, List<TicketType> tickettypes, List<ActSeat> actseats)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT * FROM tickets;";
            dt = db.Query(sql);
            Ticket ticket;

            foreach (DataRow dr in dt.Rows)
            {
                ticket = new Ticket();
                {
                    ticket.Id = (int)dr["id"];
                    ticket.CartId = (int)dr["cart"];
                };
                foreach (var tickettype in tickettypes)
                {
                    if (tickettype.Id == (int)dr["tickettypes"])
                    {
                        ticket.Type = tickettype;
                        break;
                    }
                }
                foreach (var actseat in actseats)
                {
                    if (actseat.TicketId == (int)dr["id"])
                    {
                        ticket.ActSeats.Add(actseat);
                    }
                }
                ticketlist.Add(ticket);
            }
        }
        //Metod för att hämta Carts
        public void GetCarts(List<Cart> carts, List<Customer> customers, List<Systemuser> systemusers, List<Ticket> tickets)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT * FROM carts;";
            dt = db.Query(sql);
            Cart cart;
            foreach (DataRow dr in dt.Rows)
            {
                cart = new Cart()
                {
                    Id = (int)dr["id"],
                };
                foreach (var customer in customers)
                { 
                    if (dr["customer"].GetType() != typeof(DBNull) && customer.Id == (int)(dr["customer"]))
                    {
                        cart.TicketBuyer = customer;
                        break;
                    }
                }
                foreach (var systemuser in systemusers)
                {
                    if (systemuser.Id == (int)dr["systemuser"])
                    {
                        cart.Salesman = systemuser;
                        break;
                    }
                }
                foreach (var ticket in tickets)
                {
                    if (ticket.CartId == (int)dr["id"])
                    {
                        cart.Tickets.Add(ticket);
                    }
                }
                carts.Add(cart);
            }
        }
        //Metod för att hämta första bästa lediga läktarplats
        public int GetFreeStandId(ActSeat actSeat)
        {
            int seatId = 0;
            Postgres db = new Postgres(connection);
            string sql = "SELECT seats.id AS id FROM seats " +
                            "WHERE seats.name = 'Stand' " +
                            "AND seats.id NOT IN" +
                            "(SELECT actseats.seat FROM actseats " +
                            "INNER JOIN showacts ON showacts.show = @ShowId AND showacts.act = @ActId) " +
                            "LIMIT 1;";
            db.AddParameter("@ShowId", actSeat.TheShow.Id);
            db.AddParameter("@ActId", actSeat.TheAct.Id);
            dt = db.Query(sql);
            foreach (DataRow dr in dt.Rows)
            {
                seatId = (int)dr["id"];
            }
            return seatId;
        }
        //Metod för att hämta ut föreställningar till rapporten.
        public void GetRapportShows(Rapport rapport, Show show, string fromdate, string todate)
        {
            int showid = 0;
            Postgres db = new Postgres(connection);
            string sql = "SELECT DISTINCT shows.id AS id, shows.name AS name, shows.date AS date FROM shows " +
                "INNER JOIN showacts ON shows.id = showacts.show ";
            if (fromdate != "" && todate != "")
            {
                sql += "WHERE date >= @FromDate AND date <= @ToDate "; 
                if (show != null)
                {
                    showid = show.Id;
                    sql += "AND shows.id = @ShowId "; ;
                }
            }
            else if (show != null)
            {
                showid = show.Id;
                sql += "WHERE shows.id = @ShowId ";
            }
            db.AddParameter("@ShowId", showid);
            db.AddParameter("@FromDate", fromdate);
            db.AddParameter("@ToDate", todate);
            
            dt = db.Query(sql);
            foreach (DataRow dr in dt.Rows)
            {
                RapportShow theshow = new RapportShow()
                {
                    Name = (string)dr["name"],
                    Date = (string)dr["date"]
                };
                GetRapportTickets(theshow, (int)dr["id"]);
                rapport.RapportShows.Add(theshow);
            }
        }
        //Metod för att hämta ut biljetter till föreställningsrapporterna.
        public void GetRapportTickets(RapportShow rapporttickets, int showid)
        {
            Postgres db = new Postgres(connection);
            string sql = "SELECT COUNT(tickets.id) AS tic, tickettypes.name AS name, SUM(tickets.price) AS price FROM tickets " +
                "INNER JOIN tickettypes on tickettypes.id = tickets.tickettypes " +
                "WHERE tickets.id IN " +
                "(SELECT tickets.id FROM tickets " +
                "INNER JOIN actseats ON actseats.ticket = tickets.id " +
                "INNER JOIN showacts ON showacts.id = actseats.showact " +
                "INNER JOIN shows ON showacts.show = @ShowId " +
                "WHERE tickets.cart NOT IN (SELECT carts.id FROM carts WHERE carts.customer >= 0)) " +
                "GROUP BY tickettypes.name;";
            db.AddParameter("@ShowId", showid);
            dt = db.Query(sql);
            foreach (DataRow dr in dt.Rows)
            {
                RapportTicket theticket = new RapportTicket()
                {
                    Tickettypes = (string)dr["name"],
                    Summa = Convert.ToInt32(dr["price"]),
                    Count = Convert.ToInt32(dr["tic"])
                };
                rapporttickets.RapportTickets.Add(theticket);
            }
        }
        public void TimelimitRemoveReservation(List<Show> shows)
        {
            Postgres db = new Postgres(connection);
            List<Cart> carts = new List<Cart>();
            DateTime today = DateTime.Today;
            today = today.AddDays(7);
            string tday = today.ToString("yyyy-MM-dd");
            foreach (Show show in shows)
            {
                string sql = "SELECT DISTINCT carts.id, shows.date from carts INNER JOIN tickets on tickets.cart = carts.id INNER JOIN actseats on actseats.ticket = tickets.id INNER JOIN showacts on showacts.id = actseats.showact INNER JOIN shows on shows.id = showacts.show WHERE shows.date <= @Date AND carts.customer IS NOT NULL AND shows.id = @Showid; ";
                db.AddParameter("@Date", tday);
                db.AddParameter("@Showid", show.Id);
                dt = db.Query(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    Cart cart = new Cart()
                    {
                        Id = (int)dr["id"]
                    };
                    carts.Add(cart);
                }
            }
            foreach (Cart cart in carts)
            {
                RemoveReservation(cart);
            }

        }
        #endregion Endast metoder för att hämta från databasen
        //----------------------------------------------------------//
        #region Endast metoder för att mata in data i databasen
        /// <summary>
        /// Metod för att lägga till en systemanvändare i databasen
        /// </summary>
        /// <param name="systemuser">En systemanvändare utan ID</param>
        /// <returns>True=Operationen lyckades, False=Lyckades inte</returns>
        public bool InsertSystemuser(Systemuser systemuser)
        {
            int systemuserId;
            Postgres db = new Postgres(connection);
            string sql = "INSERT INTO systemusers(username,password,firstname,lastname,personnumber,adress,email,authorisation) " +
                        "VALUES (@Username,@Password,@Firstname,@Lastname,@Personnumber,@Adress,@Email,@Auth) RETURNING systemusers.id";
            db.AddParameter("@Username", systemuser.Username);
            db.AddParameter("@Password", systemuser.Password);
            db.AddParameter("@Firstname", systemuser.Firstname);
            db.AddParameter("@Lastname", systemuser.Lastname);
            db.AddParameter("@Personnumber", systemuser.Personnumber);
            db.AddParameter("@Adress", systemuser.Adress);
            db.AddParameter("@Email", systemuser.Email);
            db.AddParameter("@Auth", systemuser.Auth.Id);
            db.ExecuteScalar(sql, out systemuserId);
            if (systemuserId >= 0)
            {
                systemuser.Id = systemuserId;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Metod för att lägga in ny reservationskund i databasen
        /// </summary>
        /// <param name="customer">En customer utan ID som ska läggas till i databasen</param>
        /// <returns>True=Operationen lyckades, False=lyckades inte</returns>
        public bool InsertCustomer(Customer customer)
        {
            int customerId;
            Postgres db = new Postgres(connection);
            string sql = "INSERT INTO customers (firstname, lastname, phonenumber) VALUES (@Fname, @Lname, @Phonenmr) RETURNING customers.id;";
            db.AddParameter("@Fname", customer.Firstname);
            db.AddParameter("@Lname", customer.Lastname);
            db.AddParameter("@Phonenmr", customer.Phonenumber);
            db.ExecuteScalar(sql, out customerId);
            if(customerId >= 0)
            {
                customer.Id = customerId;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Metod för att lägga in ny föreställning i databasen
        /// </summary>
        /// <param name="show">En föreställning utan ID som skall läggas till</param>
        /// <returns>True=Operationen lyckades, False=Lyckades inte</returns>
        public bool InsertShow(Show show)
        {
            int showId;
            Postgres db = new Postgres(connection);
            string sql = "INSERT INTO shows (name, date, summary, salestartdate, visible, circustent) VALUES (@Name, @Date, @Summary, @SalestartDate, @Visible, @Tent) RETURNING shows.id;";
            db.AddParameter("@Name", show.Name);
            db.AddParameter("@Date", show.Date);
            db.AddParameter("@Summary", show.Summary);
            db.AddParameter("@SalestartDate", show.SalestartDate);
            db.AddParameter("@Visible", false);
            db.AddParameter("@Tent", show.Tent.Id);
            db.ExecuteScalar(sql, out showId);
            foreach (var act in show.Acts)
            {
                InsertShowAct(showId, act);
            }
            if (showId >= 0)
            {
                show.Id = showId;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Metod för att lägga till ny act i databasen.
        /// </summary>
        /// <param name="act">En akt utan ID som skall läggas till i databasen</param>
        /// <returns>True=Operationen lyckades, False=Lyckades inte</returns>
        public bool InsertAct(Act act)
        {
            int actId;
            Postgres db = new Postgres(connection);
            string sql = "INSERT INTO acts (name, starttime, endtime, description, price) VALUES (@Name, @Starttime, @Endtime, @Description, @Price) RETURNING acts.id";
            db.AddParameter("@Name", act.Name);
            db.AddParameter("@Starttime", act.Starttime);
            db.AddParameter("@Endtime", act.Endtime);
            db.AddParameter("@Description", act.Description);
            db.AddParameter("@Price", act.Price);
            db.ExecuteScalar(sql, out actId);
            if (actId >= 0)
            {
                act.Id = actId;
                return true;
            }
            return false;
        }
        public void InsertShowAct(int showId, Act theact)
        {
            Postgres db = new Postgres(connection);
            string sql = "INSERT INTO showacts (show, act) VALUES (@Cirkusshow, @Theact)";
            db.AddParameter("@Cirkusshow", showId);
            db.AddParameter("@Theact", theact.Id);
            db.NonQuery(sql);
        } // metod för att lägga till akt i föreställning
        //metod för att koppla samma kund och försäljare 
        public int InsertCart(Cart cart)
        {
            int id;
            Postgres db = new Postgres(connection);
            string sql = "INSERT INTO carts(customer, systemuser) VALUES (@Customer, @Systemuser) RETURNING carts.id;";
            db.AddParameter("@Customer", cart.TicketBuyer.Id);
            db.AddParameter("@Systemuser", cart.Salesman.Id);
            db.ExecuteScalar(sql, out id);
            return id;
        }
        //metod för lägga till biljett i databasen
        public int InsertBuyCart(Cart cart)
        {
            int id;
            Postgres db = new Postgres(connection);
            string sql = "INSERT INTO carts(systemuser) VALUES (@Systemuser) RETURNING carts.id;";
            db.AddParameter("@Systemuser", cart.Salesman.Id);
            db.ExecuteScalar(sql, out id);
            return id;
        }
        public int InsertTicket(int cart, Ticket ticket)
        {
            int id;
            Postgres db = new Postgres(connection);
            string sql = "INSERT INTO tickets(cart, price, tickettypes) VALUES (@Cart, @Price, @Tickettypes) RETURNING tickets.id;";
            db.AddParameter("@Cart", cart);
            db.AddParameter("@Price", ticket.Price);
            db.AddParameter("@Tickettypes", ticket.Type.Id);
            db.ExecuteScalar(sql, out id);
            return id;
        }
        //metod för att koppla samman biljett med act och plats
        public void InsertActseat(int ticketid, ActSeat actseat)
        {
            Postgres db = new Postgres(connection);
            int seatId = actseat.TheSeat.Id;
            if (actseat.TheSeat.Stand)
            {
                seatId = GetFreeStandId(actseat);
            }
            string sql = "INSERT INTO actseats(showact, seat, ticket) VALUES (@Showact, @Seat, @Ticket);";
            db.AddParameter("@Showact", GetShowActs(actseat));
            db.AddParameter("@Seat", seatId);
            db.AddParameter("@Ticket", ticketid);
            db.NonQuery(sql);
        }
        #endregion Endast metoder för att mata in i databasen.
        //----------------------------------------------------------//
        #region Metoder för att uppdatera data i databasen
        /// <summary>
        /// Metod för att uppdatera en systamanvändare i databasen
        /// </summary>
        /// <param name="systemuser">Den systemanvändare som skall uppdateras</param>
        public void UpdateSystemuser(Systemuser systemuser)
        {
            Postgres db = new Postgres(connection);
            string sql = "UPDATE systemusers SET (username,password,firstname,lastname,personnumber,adress,email,authorisation) =(@Username, @Password,@Firstname,@Lastname,@Personnumber,@Adress,@Email,@Auth) WHERE id=@Id";
            db.AddParameter("@Id", systemuser.Id);
            db.AddParameter("@Username", systemuser.Username);//Inparametrar.
            db.AddParameter("@Password", systemuser.Password);
            db.AddParameter("@Firstname", systemuser.Firstname);
            db.AddParameter("@Lastname", systemuser.Lastname);
            db.AddParameter("@Personnumber", systemuser.Personnumber);
            db.AddParameter("@Adress", systemuser.Adress);
            db.AddParameter("@Email", systemuser.Email);
            if (systemuser.Auth != null)
            {
                db.AddParameter("@Auth", systemuser.Auth.Id);
            }
            else
            {
                db.AddParameter("@Auth", DBNull.Value);
            }
            db.NonQuery(sql);
        }
        /// <summary>
        /// Metod för att uppdatera befintliga reservationsuppgifter
        /// </summary>
        /// <param name="customer">Customern som skall uppdateras</param>
        public void UpdateCustomers(Customer customer)
        {
            Postgres db = new Postgres(connection);
            string sql = "UPDATE customers SET (firstname, lastname, phonenumber) = (@Fname, @Lname, @Phonenmr) WHERE customers.id =@Id; ";
            db.AddParameter("@Id", customer.Id);
            db.AddParameter("@Fname", customer.Firstname);
            db.AddParameter("@Lname", customer.Lastname);
            db.AddParameter("@Phonenmr", customer.Phonenumber);
            db.NonQuery(sql);
        }
        /// <summary>
        /// Metod för att uppdatera en föreställning
        /// </summary>
        /// <param name="show">Den föreställningen som finns att uppdatera</param>
        public void UpdateShow(Show show)
        {
            RemoveAllActsFromShow(show);
            Postgres db = new Postgres(connection);
            string sql = "UPDATE shows SET (name, date, summary, salestartdate, visible, circustent) = (@Name, @Date, @Summary, @SalestartDate, @Visible, @Tent) WHERE shows.id =@Id;";
            db.AddParameter("@Id", show.Id);
            db.AddParameter("@Name", show.Name);
            db.AddParameter("@Date", show.Date);
            db.AddParameter("@Summary", show.Summary);
            db.AddParameter("@SalestartDate", show.SalestartDate);
            db.AddParameter("@Visible", show.Visible);
            db.AddParameter("@Tent", show.Tent.Id);
            db.NonQuery(sql);
            foreach (var act in show.Acts)
            {
                InsertShowAct(show.Id, act);
            }
        }
        /// <summary>
        /// Uppdaterar om en show är aktiv eller inte
        /// </summary>
        /// <param name="show">Den föreställningen som skall ändras</param>
        public void UpdateVisibility(Show show)
        {
            Postgres db = new Postgres(connection);
            string sql = "UPDATE shows SET visible = (@Visible) WHERE shows.id = @Id; ";
            db.AddParameter("@Id", show.Id);
            db.AddParameter("@Visible", show.Visible);
            db.NonQuery(sql);
        }
        /// <summary>
        /// Metod för att uppdatera en befintlig act i databasen
        /// </summary>
        /// <param name="act">Den acten som skall uppdateras</param>
        public void UpdateAct(Act act)
        {
            Postgres db = new Postgres(connection);
            string sql = "UPDATE acts SET (name,starttime,endtime,description,price) =(@Name,@Starttime,@Endtime,@Description,@Price) WHERE acts.id=@Id";
            db.AddParameter("@Id", act.Id);
            db.AddParameter("@Name", act.Name);
            db.AddParameter("@Starttime", act.Starttime);
            db.AddParameter("@Endtime", act.Endtime);
            db.AddParameter("@Description", act.Description);
            db.AddParameter("@Price", act.Price);
            db.NonQuery(sql);
        }
        public void UpdateTicketTypePriceANDSectionPrice(int adultprice, int youthprice, int kidprice, int sitdownprice, int standprice)
        {
            Postgres db = new Postgres(connection);
            string sqltickettypes = "UPDATE Tickettypes SET price = @Kidprice WHERE id = 1; UPDATE Tickettypes SET price = @Youthprice WHERE id = 2; UPDATE Tickettypes SET price = @Adultprice WHERE id = 3; " 
             + "UPDATE sections SET price = @Sitdownprice WHERE id IN(1, 4, 5, 6, 7, 8, 9, 10); UPDATE sections SET price = @Standprice WHERE id = 11";
            db.AddParameter("@Adultprice", adultprice);
            db.AddParameter("@Youthprice", youthprice);
            db.AddParameter("@Kidprice", kidprice);
            db.AddParameter("@Sitdownprice", sitdownprice);
            db.AddParameter("@Standprice", standprice);
            db.NonQuery(sqltickettypes);
            
        }
        #endregion Endast metoder för att mata in i databasen.
        //----------------------------------------------------------//
        #region Endast metoder för att ta bort ur databasen
        public void RemoveReservation(Cart remove)
        {
            Postgres db = new Postgres(connection);
            string sql = " DELETE from actseats USING tickets WHERE actseats.ticket = tickets.id AND tickets.cart = @Id;" +
           "DELETE from tickets WHERE tickets.cart = @Id;" +
           "DELETE from carts WHERE carts.id = @Id;";
            db.AddParameter("@Id", remove.Id);
            db.NonQuery(sql);
        }//Metod för att tabort en reservation i databasen. 
        public void RemoveCustomers(Customer customer)
        {
            Postgres db = new Postgres(connection);
            string sql = "DELETE from customers WHERE customers.id = @Id";
            db.AddParameter("@Id", customer.Id);
            db.NonQuery(sql);
        }// Metod för att ta bort kund från databasen
        public void RemoveCustomerOnCart(Cart reservedcart)//Metod för att ta bort kund från cart. 
        {
            Postgres db = new Postgres(connection);
            string sql = "UPDATE CARTS SET customer = NULL WHERE customer = @Id;";
            db.AddParameter("@Id", reservedcart.TicketBuyer.Id);
            db.NonQuery(sql);
        }
        private void RemoveAllActsFromShow(Show show)
        {
            Postgres db = new Postgres(connection);
            string sql = "DELETE FROM showacts WHERE showacts.show = @ShowId;";
            db.AddParameter("@ShowId", show.Id);
            db.NonQuery(sql);
        }
        public void RemoveAct(Act act)
        {
            Postgres db = new Postgres(connection);
            string sql = "DELETE FROM acts WHERE acts.id = @actId;";
            db.AddParameter("@actId", act.Id);
            db.NonQuery(sql);
        }
        #endregion Endast Metoder för att ta bort ur databasen
        //----------------------------------------------------------//
    } //class
}
