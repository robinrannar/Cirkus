using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace CirkusExplorer
{
    public partial class FormAll : Form
    {
        private List<Button> ShowButtons { get; set; }
        private List<Button> ActButtons { get; set; }
        private List<PictureBox> Pxseat { get; set; }
        private Cart ActiveCart { get; set; }
        private Cirkus Circus { get; set; }
        public FormAll()
        {
            InitializeComponent();
            Circus = new Cirkus();
            ShowButtons = new List<Button>();
            ActButtons = new List<Button>();
            Pxseat = new List<PictureBox>();
            ActiveCart = new Cart();
            LogIn(false);
            try
            {
                Circus.GetDataWhenProgramStart();
                Circus.Timelimitreservation();
                UIUpdateStart();
                PrintShowButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //----------------------------------------------------------//
        #region Login

            #region Knappar
        /// <summary>
        /// Trycker man enter så kallar den på logga in metoden
        /// </summary>
        private void txbLoginPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                LogIn(true);
            }
        }
        /// <summary>
        /// Knapp för att logga in i systemet
        /// </summary>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            LogIn(true);
        }
        /// <summary>
        /// Knapp för att logga ut från systemet
        /// </summary>
        private void btnLogout_Click(object sender, EventArgs e)
        {
            LogIn(false);
        }
        #endregion Knappar

            #region UI
        /// <summary>
        /// Metod för att logga in och ut ur systemet, kallas även vid start för att gömma tabbar.
        /// </summary>
        /// <param name="login">True=Logga in, False=Logga ut</param>
        private void LogIn(bool login)
        {
            try
            {
                btnLogout.Visible = login;
                if (!login)
                {
                    tabControlMaster.TabPages.Clear();
                    tabControlMaster.TabPages.Add(tabLogin);
                    Circus.UserLoggedIn = null;
                    lbloginname.ResetText();
                    txbLoginPassword.ResetText();
                    txbLoginUsername.ResetText();
                    return;
                }
                bool? loggedin = Circus.CheckLogin(txbLoginUsername.Text, txbLoginPassword.Text);
                if (loggedin == true)
                {
                    tabControlMaster.TabPages.Add(tabAdmin);
                }
                if (loggedin == true || loggedin == false)
                {
                    
                    tabControlMaster.TabPages.Add(tabSalesman);
                    tabControlMaster.TabPages.Remove(tabLogin);
                    lbloginname.Text = "Du är nu inloggad nu som: " + Circus.UserLoggedIn.Firstname + " " + Circus.UserLoggedIn.Lastname;
                }
                else
                {
                    MessageBox.Show("De uppgifter du anget stämmer inte. Vänligen och försök igen!", "Fel inloggningsuppgifter");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion UI   

        #endregion Login
        //----------------------------------------------------------//
        #region Biljettförsäljning

            #region Knappar
        /// <summary>
        /// Knappen för att reservera som finns i vyn med platskartan.
        /// </summary>
        private void btnReserv_Click(object sender, EventArgs e)
        {
            BuyOrReservCart(false);
        }
        /// <summary>
        /// Knappen för att köpa de ihop plockade biljetter och platser
        /// </summary>
        private void btnBuy_Click(object sender, EventArgs e)
        {
            BuyOrReservCart(true);
        }
        /// <summary>
        /// Knapphändelse för att ta bort vuxenbiljett.
        /// </summary>
        private void btnMinusAdult_Click(object sender, EventArgs e)
        {
            MinusTicket("Vuxen");
        }
        /// <summary>
        /// Knapphändelse för att ta bort ungdomsbiljett.
        /// </summary>
        private void btnMinusYouth_Click(object sender, EventArgs e)
        {
            MinusTicket("Ungdom");
        }
        /// <summary>
        /// Knapphändelse för att ta bort barnbiljett.
        /// </summary>
        private void btnMinusKid_Click(object sender, EventArgs e)
        {
            MinusTicket("Barn");
        }
        #endregion Knappar

            #region Listboxhändelser
        /// <summary>
        /// Sökfunktion för att hitta tidigare registrerade kunder som skall användas för att reservera biljetter
        /// </summary>
        private void txbSearchCustomerInfo_TextChanged(object sender, EventArgs e)
        {
            try
            {
                lbxOldCustomers.Items.Clear();
                foreach (var customer in Circus.Customers)
                {
                    if (txbSearchCustomerInfo.Text != "" && 
                        (customer.Firstname.ToLower().Contains(txbSearchCustomerInfo.Text.ToLower()) || 
                        customer.Lastname.ToLower().Contains(txbSearchCustomerInfo.Text.ToLower()) || 
                        customer.Phonenumber.Contains(txbSearchCustomerInfo.Text)))
                    {
                        lbxOldCustomers.Items.Add(customer);
                    }
                    if (txbSearchCustomerInfo.Text == "")
                    {
                        lbxOldCustomers.Items.Add(customer);
                    }
                }
                lbxOldCustomers.DisplayMember = "PrintCustomer";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion Listboxhändelser

            #region UI
        /// <summary>
        /// Metod för att ta bort en biljett ifrån biljettförsäljningen
        /// </summary>
        /// <param name="tickettype">Den typ av biljett som skall tas bort</param>
        private void MinusTicket(string tickettype)
        {
            try
            {
                List<Seat> rmseatId = ActiveCart.RemoveTicket(tickettype);
                foreach (var seat in rmseatId)
                {
                    foreach (var px in Pxseat)
                    {
                        if (px.Tag.ToString() == seat.Name || px.Tag.ToString() == seat.Id.ToString())
                        {
                            px.BackColor = Color.FromName(px.Name);
                            continue;
                        }
                    }
                }
                UpdateTicketSaleList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Metod för att köpa det som finns i kundvagnen eller föra det vidare till reservationsrutan
        /// </summary>
        /// <param name="buy">True=Biljettköp, False=Biljettreservering</param>
        private void BuyOrReservCart(bool buy)
        {
            try
            {
                if (ActiveCart.Tickets.Count == 0 || !(ActiveCart.AllTicketsHaveSeats()))
                {
                    MessageBox.Show("Du måste först välja biljetter till en föreställning och platser till alla biljetter.", "Felmeddelande biljettköp");
                    return;
                }
                else if (buy)
                {
                    Circus.InsertCart(ActiveCart);
                    PrintingTickets(lbxSelectedTickets);
                    ActiveCart = new Cart();
                    UpdateTicketSaleList();
                    Circus.GetCartsTicketActSeat();
                    PrintShowButtons();
                }
                else if (!buy)
                {
                    panelreserved.Visible = true;
                    panelreserved.Dock = DockStyle.Fill;
                    UIReservWindowUpdate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Metod för att skriva ut de aktiva föreställningarna som buttons
        /// följt av föreställningens acter. Knapparna blir tilldelade metoder
        /// för att dessa knappar skall få någon funktion.
        /// </summary>
        private void PrintShowButtons()
        {
            try
            {
                gbTicketinfo.Enabled = false;
                panelShowAct.Controls.Clear();
                Pxseat.Clear();
                foreach (var panel in panel_seats.Controls.OfType<FlowLayoutPanel>())
                {
                    panel.Controls.Clear();
                }
                foreach (var show in Circus.Shows)
                {
                    if (show.Visible && show.NotOldShow())
                    {
                        Button ShowButton = new Button
                        {
                            Height = 50,
                            Width = 200,
                            Text = show.Name + "\r Datum: " + show.Date,
                            Tag = show.Id
                        };
                        ShowButton.Click += delegate
                        {
                            if (ActiveCart.Tickets.Count != 0 && ActiveCart.Tickets[0].ActSeats.Count != 0 && !(ActiveCart.Tickets[0].ActSeats[0].TheShow == show))
                            {
                                ActiveCart = new Cart();
                                UpdateTicketSaleList();
                            }
                            RemoveClickEvent(btnPlusAdult);
                            RemoveClickEvent(btnPlusYouth);
                            RemoveClickEvent(btnPlusKid);
                            ShowSeatmap(show, null);
                            gbTicketinfo.Enabled = true;
                            btnPlusAdult.Click += delegate { AddTicketButtons(show, "Vuxen"); };
                            btnPlusYouth.Click += delegate { AddTicketButtons(show, "Ungdom"); };
                            btnPlusKid.Click += delegate { AddTicketButtons(show, "Barn"); };
                        };
                        ShowButtons.Add(ShowButton);
                        panelShowAct.Controls.Add(ShowButton);
                        foreach (var act in show.Acts)
                        {
                            Button newActButton = new Button
                            {
                                Height = 50,
                                Width = 150,
                                Text = act.Name,
                                Tag = act.Id,
                                Name = show.Name,
                                Visible = false
                            };
                            ActButtons.Add(newActButton);
                            newActButton.Click += delegate { ShowSeatmap(show, act); };
                            panelShowAct.Controls.Add(newActButton);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Metod för att lägga till biljett i varukorgen. Kollar så det finns lediga platser i den valda föreställningen
        /// </summary>
        /// <param name="show">Den valda föreställningen</param>
        /// <param name="tickettype">Den typen av biljett som skall läggas till</param>
        private void AddTicketButtons(Show show, string tickettype)
        {
            try
            {
                int AddedTicketToCart = Convert.ToInt32(ActiveCart.NumberOfTicketsOfType("Vuxen")) +
                                    Convert.ToInt32(ActiveCart.NumberOfTicketsOfType("Ungdom")) +
                                    Convert.ToInt32(ActiveCart.NumberOfTicketsOfType("Barn"));
                int[] takenSeatsInAct = new int[show.Acts.Count];
                int seats = Circus.NumberOfSeatsInShow(show, ActiveCart, takenSeatsInAct);
                if (takenSeatsInAct.Sum() + AddedTicketToCart == seats * show.Acts.Count)
                {
                    MessageBox.Show("Det finns inte tillräckligt många lediga platser i denna föreställning!");
                    return;
                }
                for (int i = 0; i < takenSeatsInAct.Length; i++)
                {
                    if (takenSeatsInAct[i] + AddedTicketToCart == seats)
                    {
                        MessageBox.Show("Det saknas platser för att se hela föreställningen.\nDet finns inte tillräcklig många lediga platser i Akt" + (i + 1) + ".", "Platsbrist");
                    }
                }
                Ticket ticket = ActiveCart.AddTicket(Circus.GetTicketType(tickettype));
                UpdateTicketSaleList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Metod för att rensa händelser(Events) på en knapp
        /// </summary>
        /// <param name="button">Knappen som skall rensas</param>
        private void RemoveClickEvent(Button button)
        {
            try
            {
                FieldInfo fieldInfo = typeof(Control).GetField("EventClick", BindingFlags.Static | BindingFlags.NonPublic);
                object obj = fieldInfo.GetValue(button);
                PropertyInfo propertyInfo = button.GetType().GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);
                EventHandlerList list = (EventHandlerList)propertyInfo.GetValue(button, null);
                list.RemoveHandler(obj, list[obj]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Metod för att kolla vilja platser som är bokade på en viss föreställning
        /// och act. Denna metod kallar på en annan metod för att se om platsen är ledig.
        /// </summary>
        /// <param name="act">Om det är en specifik act skall den matas in här(Hela föreställningen så ska de vara NULL)</param>
        private void ShowSeatmap(Show show ,Act act)
        {
            try
            {
                lbstandMinus.Text = "Ta bort läktarplats";
                lbstandPlus.Text = "Lägg till läktarplats";
                Pxseat.Clear();
                if (act == null)
                {
                    foreach (var btn in ActButtons)
                    {
                        if (btn.Name == show.Name ? btn.Visible = true : btn.Visible = false) { };
                    }
                }
                foreach (var panel in panel_seats.Controls.OfType<FlowLayoutPanel>())
                {
                    panel.Controls.Clear();
                }
                foreach (var section in show.Tent.Sections)
                {
                    if (section.Name == "Stands")
                    {
                        PictureBox standimgminus = new PictureBox
                        {
                            Height = 65,
                            Width = 279,
                            BackColor = Color.DarkRed,
                            Tag = "StandMinus",
                        };
                        Pxseat.Add(standimgminus);
                        PictureBox standimg = new PictureBox
                        {
                            Height = 65,
                            Width = 279,
                            BackColor = Color.DarkRed,
                            Tag = "Stand",
                            Name = "Green"
                        };
                        Pxseat.Add(standimg);

                        if (Circus.FreeStandInShow(show, section, act, ActiveCart) == null)
                        {
                            standimg.BackColor = Color.Yellow;
                        }
                        else if (Circus.FreeStandInShow(show, section, act, ActiveCart) == true)
                        {
                            standimg.BackColor = Color.Green;
                            standimg.Click += delegate
                            {
                                ActiveCart.AddSeatToCart(show, section.Seats[Circus.NextFreeStandId(show, ActiveCart)], standimg.BackColor.Name, act);
                                bool? free = Circus.FreeStandInShow(show, section, act, ActiveCart);
                                if (free == null)
                                {
                                    standimg.BackColor = Color.Yellow;
                                }
                                else if (free == true)
                                {
                                    standimg.BackColor = Color.Green;
                                }
                                else if (free == false)
                                {
                                    standimg.BackColor = Color.Gray;
                                }
                                UpdateTicketSaleList();
                            };
                        }
                        standimgminus.Click += delegate
                        {
                            ActiveCart.RemoveStandFromCart(act, section, show);
                            standimg.BackColor = Color.Green;
                            UpdateTicketSaleList();
                        };
                        panelStand.Controls.Add(standimgminus);
                        panelStand.Controls.Add(standimg);
                    }
                    else
                    {
                        foreach (var panel in panel_seats.Controls.OfType<FlowLayoutPanel>())
                        {
                            if ((string)panel.Tag == section.Name)
                            {
                                foreach (var seat in section.Seats)
                                {
                                    PictureBox img = new PictureBox
                                    {
                                        Height = 25,
                                        Width = 25,
                                        BackColor = Color.DarkRed,
                                        Tag = seat.Id,
                                        Name = "White"
                                    };
                                    Label label = new Label() { Text = seat.Name };
                                    img.Controls.Add(label);
                                    if (Circus.CheckForBusySeatsIn(show, seat, act, ActiveCart) == 0)
                                    {
                                        img.BackColor = Color.White;
                                        img.Click += delegate
                                        {
                                            string color = ActiveCart.AddSeatToCart(show, seat, img.BackColor.Name, act);
                                            if (img.BackColor == Color.White && color == "White")
                                            {
                                                MessageBox.Show("Lägg till biljetter innan ni lägger till platser.");
                                            }
                                            img.BackColor = Color.FromName(color);
                                            UpdateTicketSaleList();
                                        };
                                        label.Click += delegate
                                        {
                                            string color = ActiveCart.AddSeatToCart(show, seat, img.BackColor.Name, act);
                                            if (img.BackColor == Color.White && color == "White")
                                            {
                                                MessageBox.Show("Lägg till biljetter innan ni lägger till platser.");
                                            }
                                            img.BackColor = Color.FromName(color);
                                            UpdateTicketSaleList();
                                        };
                                    }
                                    else if (Circus.CheckForBusySeatsIn(show, seat, act, ActiveCart) < show.Acts.Count && act == null)
                                    {
                                        img.BackColor = Color.Yellow;
                                    }
                                    Pxseat.Add(img);
                                    panel.Controls.Add(img);
                                    if (section.Name == "H" || section.Name == "F" || section.Name == "D")
                                    {
                                        panel.Controls.SetChildIndex(img, 0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        /// <summary>
        /// Metod för att uppdatera listbox och totalsumman i biljettförsäljning vid platskartan.
        /// </summary>
        private void UpdateTicketSaleList()
        {
            lbxSelectedTickets.Items.Clear();
            lbxSelectedTickets.Items.AddRange(ActiveCart.GetTicketAndActSeats().ToArray());
            txbAdult.Text = ActiveCart.NumberOfTicketsOfType("Vuxen");
            txbYouth.Text = ActiveCart.NumberOfTicketsOfType("Ungdom");
            txbKid.Text = ActiveCart.NumberOfTicketsOfType("Barn");
            lblsum.Text = ActiveCart.GetCartPrice.ToString() + "kr";
        }
        /// <summary>
        /// Metod för att skriva ut biljetter
        /// </summary>
        /// <param name="listbox">Den listboxen biljetterna ligger i</param>
        private void PrintingTickets(ListBox listbox)
        {
            try
            {
                int originalheight = listbox.Height;
                int width = listbox.Size.Width;
                int height = listbox.Size.Height + (listbox.Height = listbox.PreferredHeight);
                Rectangle rect = new Rectangle(0, 0, width, height);
                string pdfName = DateTime.Today.ToShortDateString() + "biljetter.pdf";
                FileStream fs = new FileStream(pdfName, FileMode.Create, FileAccess.Write, FileShare.None);
                var doc = new iTextSharp.text.Document();
                doc.SetMargins(0f, 0f, 20f, 0f);
                doc.SetPageSize(new iTextSharp.text.Rectangle((float)width, (float)height));
                iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, fs);
                doc.Open();

                Bitmap bm = new Bitmap(width, height);
                listbox.DrawToBitmap(bm, new Rectangle(0, 0, width, height));
                System.Drawing.Image image = bm;
                iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(image,
                System.Drawing.Imaging.ImageFormat.Jpeg);
                doc.Add(pdfImage);
                doc.Close();
                System.Diagnostics.Process.Start(@pdfName);
                listbox.Height = originalheight;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion UI

        #endregion Biljettförsäljning
        //----------------------------------------------------------//
        #region Skapa akter

            #region Knappar
        /// <summary>
        /// Knapp för att skapa ny act
        /// </summary>
        private void btnCreateAct_Click(object sender, EventArgs e)
        {
            InsertOrUpdateAct(true);
        }
        /// <summary>
        /// Knappför att uppdatera befintlig act
        /// </summary>
        private void btnUpdateAct_Click(object sender, EventArgs e)
        {
            InsertOrUpdateAct(false);
        }
        /// <summary>
        /// Knapp för att rensa alla textboxar och ladda om listboxar
        /// </summary>
        private void btnClearAct_Click(object sender, EventArgs e)
        {
            UpdateActs();
        }
        /// <summary>
        /// Knapp för att avbryta och gå tillbaka till föreställningshanteringen
        /// </summary>
        private void btnCancelAct_Click(object sender, EventArgs e)
        {
            UpdateActs();
            tabControlAdmin.SelectedIndex = 0;
        }
        #endregion Knappar

            #region Listboxhändelser
        /// <summary>
        /// Knapptryckhändelse som kallar på en metod som inte tillåter annat än siffor
        /// </summary>
        private void txbActprice_KeyPress(object sender, KeyPressEventArgs e)
        {
            DigitsOnly(e);
        }
        #endregion Listboxhändelser

            #region UI
        /// <summary>
        /// Metod för att lägga in eller uppdatera en akt
        /// </summary>
        /// <param name="insert">True=Lägga in ny akt, False=Uppdatera akt</param>
        private void InsertOrUpdateAct(bool insert)
        {
            try
            {
                bool error = false;
                string name = txbActname.Text;
                string starttime = txbActStarttime.Text;
                string endttime = txbActstoptime.Text;
                string description = txbActDescription.Text;
                TextBox[] textboxs = { txbActname, txbActStarttime, txbActstoptime, txbActDescription, txbActprice };
                foreach (var textbox in textboxs)
                {
                    if (CheckIfTextBoxIsEmpty(textbox))
                    {
                        error = true;
                    }
                }
                if (error) { return; }
                int price = Convert.ToInt32(txbActprice.Text);
                if (insert)
                {
                    Circus.InsertUpdateAct(name, starttime, endttime, description, price, null);
                }
                else if (!insert && lBoxActInfo.Items[0] != null)
                {
                    Act act = (Act)lBoxActInfo.SelectedItem;
                    Circus.InsertUpdateAct(name, starttime, endttime, description, price, act);
                }
                else if (!insert && lBoxActInfo.Items[0] == null)
                {
                    MessageBox.Show("Du måste välja en akt på föreställnings/akt fliken."); return;
                }
                UpdateActs();
                UICreateShowUpdate();
                tabControlAdmin.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Metod som uppdaterar textfält och listbox i aktfliken
        /// </summary>
        private void UpdateActs()
        {
            lBoxActInfo.Items.Clear();
            txbActname.Clear();
            txbActprice.Clear();
            txbActStarttime.Clear();
            txbActstoptime.Clear();
            txbActDescription.Clear();
            lxbUpdateActs.Items.Clear();
            foreach (Act act in Circus.Acts)
            {
                lxbUpdateActs.Items.Add(act);
            }
            lxbUpdateActs.DisplayMember = "PrintActs";
        }
        /// <summary>
        /// Metod för att skriva ut info om en act
        /// </summary>
        /// <param name="act">Acten som skall skrivas ut</param>
        private void PrintActInfo(Act act)
        {
            lBoxActInfo.Items.Add(act);    
            txbActname.Text = act.Name;
            txbActStarttime.Text = act.Starttime;
            txbActstoptime.Text = act.Endtime;
            txbActDescription.Text = act.Description;
            txbActprice.Text = act.Price.ToString();
            lBoxActInfo.DisplayMember = "PrintActs";
        }
        #endregion UI

        #endregion Skapa akter
        //----------------------------------------------------------//
        #region Hantera föreställningar

            #region Knappar
        /// <summary>
        /// Knapp som tar användaren till fliken för att skapa ny föreställning
        /// </summary>
        private void btnGoToCreateShow_Click(object sender, EventArgs e)
        {
            tabControlAdmin.SelectedIndex = 1;
        }
        /// <summary>
        /// Knapp som tar användare till fliken för att skapa ny akt
        /// </summary>
        private void btnGoToCreateAct_Click(object sender, EventArgs e)
        {
            tabControlAdmin.SelectedIndex = 2;
        }
        #endregion Knappar

            #region Listboxhändelser
        /// <summary>
        /// När man klickar på ett object i listboxen kommer två knappar upp med alternativ
        /// </summary>
        private void lxbUpdateActs_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (lxbUpdateActs.SelectedItem != null && lxbUpdateActs.SelectedItem.GetType() == typeof(Act))
            {
                Act act = (Act)lxbUpdateActs.SelectedItem;
                AddEventButtons(delegate { tabControlAdmin.SelectedIndex = 2; PrintActInfo(act); },
                                delegate 
                                {
                                    try
                                    {
                                        Circus.RemoveAct(act);
                                        Circus.GetCartsTicketActSeat();
                                        UpdateActs();
                                        ShowAdminUpdate();
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }
                                },
                                "Redigera", "Radera");
            }
        }
        /// <summary>
        /// När man klickar på ett object i listboxen kommer två knappar upp med alternativ
        /// </summary>
        private void lbxActiveShows_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(lbxActiveShows.SelectedIndex == -1))
            {
                lbxInactiveShows.SelectedItem = null;
            }
            if (lbxActiveShows.SelectedItem != null && lbxActiveShows.SelectedItem.GetType() == typeof(Show))
            {
                Show show = (Show)lbxActiveShows.SelectedItem;
                AddEventButtons(delegate { tabControlAdmin.SelectedIndex = 1; PrintShowInfo(show); },
                                delegate { ActivateOrInactiveShow(lbxActiveShows, false); },
                                "Redigera", "Inaktivera");
            }
        }
        /// <summary>
        /// När man klickar på ett object i listboxen kommer två knappar upp med alternativ
        /// </summary>
        private void lbxInactiveShows_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(lbxInactiveShows.SelectedIndex == -1))
            {
                lbxActiveShows.SelectedItem = null;
            }
            if (lbxInactiveShows.SelectedItem != null && lbxInactiveShows.SelectedItem.GetType() == typeof(Show))
            {
                Show show = (Show)lbxInactiveShows.SelectedItem;
                AddEventButtons(delegate { tabControlAdmin.SelectedIndex = 1; PrintShowInfo(show); },
                                delegate { ActivateOrInactiveShow(lbxInactiveShows, true); },
                                "Redigera", "Aktivera");
            }
        }
        #endregion Listboxhändelser

            #region UI
        /// <summary>
        /// Metod för att aktivera eller inaktivera en föreställning
        /// </summary>
        /// <param name="list">Den listboxen föreställningen ligger i</param>
        /// <param name="activate">True=Aktivera föreställning, False=Inaktivera föreställning</param>
        private void ActivateOrInactiveShow(ListBox list, bool activate)
        {
            try
            {
                if (list.SelectedItem != null)
                {
                    Show show = (Show)list.SelectedItem;
                    Circus.UpdateShowVisibility(show, activate);
                    ShowAdminUpdate();
                }
                else
                {
                    MessageBox.Show("Du har inte markerat en föreställning.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Metod för att uppdatera listboxarna med föreställningar på aktiveringssidan.
        /// </summary>
        private void ShowAdminUpdate()
        {
            lbxActiveShows.Items.Clear();
            lbxInactiveShows.Items.Clear();
            foreach (Show s in Circus.Shows)
            {
                if (s.Visible == false)
                {
                    lbxInactiveShows.Items.Add(s);
                }
                else if (s.Visible == true)
                {
                    lbxActiveShows.Items.Add(s);
                }
                lbxInactiveShows.DisplayMember = "Name";
                lbxActiveShows.DisplayMember = "Name";
            }
        }
        #endregion UI

        #endregion Hantera föreställningar
        //----------------------------------------------------------//
        #region Skapa föreställningar

            #region Knappar
        /// <summary>
        /// Knapp för att spara ny föreställning
        /// </summary>
        private void btnSaveShow_Click(object sender, EventArgs e)
        {
            InsertOrUpdateShow(true);
        }
        /// <summary>
        /// Knapp för att uppdatera befintlig föreställning
        /// </summary>
        private void btnUpdateShow_Click(object sender, EventArgs e)
        {
            InsertOrUpdateShow(false);
        }
        /// <summary>
        /// Knapp för att rensa textfält och uppdatera listboxar
        /// </summary>
        private void btnClearShow_Click(object sender, EventArgs e)
        {
            UICreateShowUpdate();
        }
        /// <summary>
        /// Knapp för att lägga till nya akter, tar en vidare till akt fliken
        /// </summary>
        private void btnAddAct_Click(object sender, EventArgs e)
        {
            UpdateActs();
            tabControlAdmin.SelectedIndex = 2;
        }
        /// <summary>
        /// Knapp för att avbryta och gå tillbaka till föregående sida.
        /// </summary>
        private void btn_AbortAddShow_Click(object sender, EventArgs e)
        {
            UICreateShowUpdate();
            tabControlAdmin.SelectedIndex = 0;
        }
        #endregion Knappar

            #region UI
        private void PrintShowInfo(Show show)
        {
            lBoxShowInfo.Items.Add(show);
            lBoxShowInfo.DisplayMember = "Name";
            txbShowName.Text = show.Name;
            txbShowSummary.Text = show.Summary;
            dtPickShowDate.Value = Convert.ToDateTime(show.Date);
            dtSaleStart.Value = Convert.ToDateTime(show.SalestartDate);
            cbxTents.Text = show.Tent.Name;
            for (int i = 0; i < cbListChooseActs.Items.Count; i++)
            {
                foreach (var act in show.Acts)
                {
                    if (act == cbListChooseActs.Items[i])
                    {
                        cbListChooseActs.SetItemChecked(i, true);
                        break;
                    }
                    else
                    {
                        cbListChooseActs.SetItemChecked(i, false);
                    }
                }
            }
            cbListChooseActs.DisplayMember = "PrintActs";
        }
        /// <summary>
        /// Metod för att lägga in eller uppdatera en föreställning
        /// </summary>
        /// <param name="insert">True=Lägga till ny föreställning,False=Uppdatera befintlig</param>
        private void InsertOrUpdateShow(bool insert)
        {
            try
            {
                bool error = false;
                string name = txbShowName.Text;
                string summary = txbShowSummary.Text;
                string date = dtPickShowDate.Value.ToShortDateString();
                string salestartdate = dtSaleStart.Value.ToShortDateString();
                TextBox[] texts = { txbShowName, txbShowSummary };
                foreach (var text in texts)
                {
                    if (CheckIfTextBoxIsEmpty(text))
                    {
                        error = true;
                    }
                }
                if (error) { return; }
                if (cbxTents.SelectedItem == null)
                {
                    MessageBox.Show("Välj ett tält föreställningen skall hållas"); return;
                }
                if (cbListChooseActs.CheckedItems.Count == 0)
                {
                    MessageBox.Show("Du behöver lägga till akter till föreställningen"); return;
                }
                Cirkustent tent = (Cirkustent)cbxTents.SelectedItem;
                List<Act> acts = new List<Act>();
                foreach (Act act in cbListChooseActs.CheckedItems)
                {
                    acts.Add(act);
                };
                if (insert)
                {
                    Circus.InsertOrUpdateShow(null, name, date, summary, salestartdate, tent, acts);
                }
                else if (lBoxShowInfo.Items[0] == null)
                {
                    MessageBox.Show("Välj en föreställning från föreställnings/akt fliken att uppdatera.");
                    return;
                }
                else if (!insert)
                {
                    Show show = (Show)lBoxShowInfo.Items[0];
                    Circus.InsertOrUpdateShow(show, name, date, summary, salestartdate, tent, acts);
                }
                UICreateShowUpdate();
                tabControlAdmin.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Metod som rensar textfält och uppdaterar listbox i skapaföreställnings fliken
        /// </summary>
        private void UICreateShowUpdate()
        {
            txbShowName.Clear();
            txbShowSummary.Clear();
            cbxTents.SelectedItem = null;
            lBoxShowInfo.Items.Clear();
            cbListChooseActs.Items.Clear();
            foreach (Act act in Circus.Acts)
            {
                cbListChooseActs.Items.Add(act);
            }
            cbListChooseActs.DisplayMember = "PrintActs";
        }
        #endregion UI

        #endregion Skapa föreställningar
        //----------------------------------------------------------//
        #region Reservering

            #region Knappar
        /// <summary>
        /// Knapp för att lägga till nya reservationsuppgifter
        /// </summary>
        private void btnAddReservCustomer_Click(object sender, EventArgs e)
        {
            InsertOrUpdateReservedCustomer(true);
        }
        /// <summary>
        /// Knapp för att uppdatera befintliga reservationsuppgifter
        /// </summary>
        private void btnUpdateReservCustomer_Click(object sender, EventArgs e)
        {
            InsertOrUpdateReservedCustomer(false);
        }
        /// <summary>
        /// Knapp för att lägga till reservation
        /// </summary>
        private void btnMakeReservation_Click(object sender, EventArgs e)
        {
            AddOrRemoveReservation(true);
        }
        /// <summary>
        /// Knapp för att rensa textfälten för reservationsuppgifter
        /// </summary>
        private void btnRensaTextReservation_Click(object sender, EventArgs e)
        {
            UIReservWindowUpdate();
        }
        /// <summary>
        /// Knapp för att återgå till platskartan
        /// </summary>
        private void btnReservationGoBack_Click(object sender, EventArgs e)
        {
            panelreserved.Visible = false;
        }
        #endregion Knappar

            #region Listboxhändelser
        /// <summary>
        /// Listboxhändelse som rensar textfält och skriver ut info om den valda kunden
        /// </summary>
        private void lbxOldCustomers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxOldCustomers.SelectedItem != null)
            {
                Customer customer = (Customer)lbxOldCustomers.SelectedItem;
                txbCFname.Text = customer.Firstname;
                txbCLname.Text = customer.Lastname;
                txbPhonenumber.Text = customer.Phonenumber;
            }
        }
        /// <summary>
        /// Textfälthändelse som förhindrar att skriva annat än siffror
        /// </summary>
        private void txbPhonenumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            DigitsOnly(e);
        }
        #endregion Listboxhändelser

            #region UI
        /// <summary>
        /// Metod för att uppdatera eller lägga in nya reservationsuppgifter
        /// </summary>
        /// <param name="insert">True=Lägg in nya, False=Uppdatera befintliga</param>
        private void InsertOrUpdateReservedCustomer(bool insert)
        {
            try
            {
                bool error = false;
                string fname = txbCFname.Text;
                string lname = txbCLname.Text;
                string phonenumb = txbPhonenumber.Text;
                Customer customer = (Customer)lbxOldCustomers.SelectedItem;
                TextBox[] texts = { txbCFname, txbCLname, txbPhonenumber };
                foreach (var text in texts)
                {
                    if (CheckIfTextBoxIsEmpty(text))
                    {
                        error = true;
                    }
                }
                if (error) { return; }

                if (insert)
                {
                    Circus.InsertOrUpdateCustomer(null, fname, lname, phonenumb);
                }
                else if (!insert && lbxOldCustomers.SelectedItem != null)
                {
                    Circus.InsertOrUpdateCustomer(customer, fname, lname, phonenumb);
                }
                else if (lbxOldCustomers.SelectedItem == null)
                {
                    MessageBox.Show("Välj en kund att uppdatera"); return;
                }
                UIReservWindowUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Metod för att uppdater UI i reservationsfönstret
        /// </summary>
        private void UIReservWindowUpdate()
        {
            txbCFname.Clear();
            txbCLname.Clear();
            txbPhonenumber.Clear();
            Circus.GetCartsTicketActSeat();
            lbxOldCustomers.Items.Clear();
            lbxOldCustomers.Items.AddRange(Circus.Customers.ToArray());
            lbxOldCustomers.DisplayMember = "PrintCustomer";
            lbxCart.Items.Clear();
            lbxCart.Items.AddRange(ActiveCart.GetTicketAndActSeats().ToArray());
        }
        /// <summary>
        /// Metod för att lägga till en reservation eller ta bort
        /// "Hämta ut" fliken kallar på denna vid borttagning
        /// </summary>
        /// <param name="add">True=lägga till, False=ta bort</param>
        private void AddOrRemoveReservation(bool add)
        {
            try
            {
                if (add && ActiveCart.Tickets.Count != 0 && lbxOldCustomers.SelectedItem != null)
                {
                    ActiveCart.TicketBuyer = (Customer)lbxOldCustomers.SelectedItem;
                    Circus.InsertCart(ActiveCart);
                    ActiveCart = new Cart();
                    panelreserved.Visible = false;
                    UpdateTicketSaleList();
                    MessageBox.Show("Bokningen genomförd");
                    PrintShowButtons();
                }
                else if (!add && lbxOldCustomers.SelectedItem != null)
                {
                    Cart cart = (Cart)lbxCustomers.SelectedItem;
                    string errormsg = Circus.RemoveReservation(cart);
                    ReservedUpdate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion UI

        #endregion Reservering
        //----------------------------------------------------------//
        #region Hämta ut reservation

            #region Knappar
        /// <summary>
        /// Knapphändelser för att ta bort en reservation,
        /// kallar på en metod under Reserverings regionen
        /// </summary>
        private void btnRemoveReservation_Click(object sender, EventArgs e)
        {
            DialogResult remove = MessageBox.Show("Är du säker på att du vill ta bort reservationen?", "Säkerhetsfråga!", MessageBoxButtons.YesNo);
            if (remove == DialogResult.Yes)
            {
                AddOrRemoveReservation(false);
            }
        }
        /// <summary>
        /// Knapphändelse för att hämta ut sin reservation.
        /// </summary>
        private void btnGetReservedTickets_Click(object sender, EventArgs e)
        {
            GetReservedTickets();
        }
        #endregion Knappar

            #region Listboxhändelser
        /// <summary>
        /// Listboxhändelse som visar information om reservationen
        /// </summary>
        private void lbxCustomers_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbxReservedTickets.Items.Clear();
            Cart cart = (Cart)lbxCustomers.SelectedItem;
            if (cart != null)
            {
                lbCartnmr.Text = "Varukorgensid: " + cart.Id.ToString();

                foreach (var ticket in cart.Tickets)
                {
                    lbxReservedTickets.Items.Add(ticket.TicketInfo);
                    foreach (var actseat in ticket.ActSeats)
                    {
                        if (actseat.TheSeat != null)
                        {
                            lbxReservedTickets.Items.Add(actseat.SeatName);
                        }
                    }
                }
            }
        }
        #endregion Listboxhändelser

            #region UI
        /// <summary>
        /// Metod för att hämta ut sina reserverade biljetter
        /// </summary>
        private void GetReservedTickets()
        {
            try
            {
                Cart cart = (Cart)lbxCustomers.SelectedItem;
                if (cart == null)
                {
                    MessageBox.Show("Du måste välja en kundvagn!"); return;
                }
                else
                {
                    Circus.RemoveCustomerIDFromCart(cart);
                    PrintingTickets(lbxReservedTickets);
                    ReservedUpdate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Metod som uppdaterar Hämta ut fliken och hämtar på nytt från databasen.
        /// </summary>
        private void ReservedUpdate()
        {
            try
            {
                lbxCustomers.Items.Clear();
                lbxReservedTickets.Items.Clear();
                Circus.GetCartsTicketActSeat();
                foreach (Cart cart in Circus.Carts)
                {
                    if (cart.TicketBuyer != null)
                    {
                        lbxCustomers.Items.Add(cart);
                    }
                    lbxCustomers.DisplayMember = "PrintCustomer";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion UI

        #endregion Hämta ut reservation
        //----------------------------------------------------------//
        #region Användarhantering

            #region Knappar
        /// <summary>
        /// Knapp för att lägga till ny systemanvändare
        /// </summary>
        private void btnAddUser_Click(object sender, EventArgs e)
        {
            AddOrUpdateSystemUser(true);
        }
        /// <summary>
        /// Knapp för att uppdatera en systemanvändare
        /// </summary>
        private void btnChangeUser_Click(object sender, EventArgs e)
        {
            AddOrUpdateSystemUser(false);
        }
        /// <summary>
        /// Knapp för att rensa textfält och ladda om listboxar i användarhanteringsfliken
        /// </summary>
        private void btnClear_Click(object sender, EventArgs e)
        {
            btnAddUser.Enabled = true;
            UISystemUserUpdate();
        }
        /// <summary>
        /// Knapp för att inaktivera systemanvändare
        /// </summary>
        private void btnInactiveUser_Click(object sender, EventArgs e)
        {
            if (lbxUsers.SelectedItem != null)
            {
                ActivateOrInactiveSystemuser((Systemuser)lbxUsers.SelectedItem, false);
                MessageBox.Show("Personen är nu inaktiverad");
            }
            else
            {
                MessageBox.Show("Välj en systemanvändare att inaktivera");
            }
        }
        /// <summary>
        /// Knapp för att aktivera en systemanvändare
        /// </summary>
        private void btnActiveUser_Click(object sender, EventArgs e)
        {
            if (lbxInactivateUser.SelectedItem != null)
            {
                ActivateOrInactiveSystemuser((Systemuser)lbxInactivateUser.SelectedItem, true);
                MessageBox.Show("Personen är nu aktiverad");
            }
            else
            {
                MessageBox.Show("Välj en systemanvändare att aktivera.");
            }
        }
        #endregion Knappar

            #region Listboxhändelser
        /// <summary>
        /// Listboxhändelse som skriver ut information om systemanvändaren
        /// </summary>
        private void lbxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnAddUser.Enabled = false;
            Systemuser systemuser = (Systemuser)lbxUsers.SelectedItem;
            if (lbxUsers.SelectedItem != null)
            {
                txbUsername.Text = systemuser.Username;
                txbPassword.Text = systemuser.Password;
                txbFirstname.Text = systemuser.Firstname;
                txbLastname.Text = systemuser.Lastname;
                txbPersonnumber.Text = systemuser.Personnumber;
                txbAdress.Text = systemuser.Adress;
                txbEmail.Text = systemuser.Email;
                cbxAuthorisation.Text = systemuser.Auth.Type;
            }
        }
        #endregion Listboxhändelser

            #region UI
        /// <summary>
        /// Metod för att aktivera eller inaktivera en systemanvändare
        /// </summary>
        /// <param name="systemuser">Den som skall aktiveras/inaktiveras</param>
        /// <param name="activate">True=Aktivera,False=Inaktivera</param>
        private void ActivateOrInactiveSystemuser(Systemuser systemuser, bool activate)
        {
            Systemuser sysuser = (Systemuser)lbxUsers.SelectedItem;
            if (activate)
            {
                Authorisation newauth = null;
                foreach (var auth in Circus.Auths)
                {
                    if (auth.Type == "Biljettförsäljare")
                    {
                        newauth = auth;
                    }
                }
                Circus.UpdateActivateOrInactivateSystemusers(systemuser, newauth);
            }
            else if (!activate)
            {
                Circus.UpdateActivateOrInactivateSystemusers(sysuser, null);
            }
            UISystemUserUpdate();
        }
        /// <summary>
        /// Metod för att lägga till eller uppdatera en systemanvändare
        /// </summary>
        /// <param name="add">True=Lägg till, False=Uppdatera</param>
        private void AddOrUpdateSystemUser(bool add)
        {
            bool error = false;
            string username = txbUsername.Text;
            string password = txbPassword.Text;
            string firstname = txbFirstname.Text;
            string lastname = txbLastname.Text;
            string personnumber = txbPersonnumber.Text;
            string adress = txbAdress.Text;
            string email = txbEmail.Text;
            TextBox[] texts = { txbUsername, txbPassword, txbFirstname, txbLastname, txbPersonnumber, txbAdress, txbEmail };
            Authorisation auth = (Authorisation)cbxAuthorisation.SelectedItem;
            Systemuser systemuser = (Systemuser)lbxUsers.SelectedItem;
            foreach (var text in texts)
            {
                if (CheckIfTextBoxIsEmpty(text))
                {
                    error = true;
                }
            }
            if (error) { return; }
            if (auth == null)
            {
                MessageBox.Show("Välj en behörighet för användaren"); return;
            }
            if (add)
            {
                Circus.InsertOrUpdateSystemusers(null, txbUsername.Text, txbPassword.Text, txbFirstname.Text, txbLastname.Text, txbPersonnumber.Text, txbAdress.Text, txbEmail.Text, auth);
            }
            else if(systemuser == null)
            {
                MessageBox.Show("Välj en systemanvändare att uppdatera."); return;
            }
            else
            {
                Circus.InsertOrUpdateSystemusers(systemuser, username, password, firstname, lastname, personnumber, adress, email, auth);
            }
            UISystemUserUpdate();    
        }
        /// <summary>
        /// Medtod för att uppdatera UI't hos systamanvändare
        /// </summary>
        private void UISystemUserUpdate()
        {
                txbUsername.Clear();
                txbPassword.Clear();
                txbFirstname.Clear();
                txbLastname.Clear();
                txbPersonnumber.Clear();
                txbAdress.Clear();
                txbEmail.Clear();
                Circus.GetSystemusers();
                lbxInactivateUser.Items.Clear();
                lbxUsers.Items.Clear();
                foreach (Systemuser su in Circus.Systemusers)
                {
                    if (su.Auth == null)
                    {
                        lbxInactivateUser.Items.Add(su);
                    }
                    else
                    {
                        lbxUsers.Items.Add(su);
                    }
                }
                lbxUsers.DisplayMember = "PrintSystemuser";
                lbxInactivateUser.DisplayMember = "PrintInactiveSystemuser";
        }
        #endregion UI

        #endregion Usermanagment
        //----------------------------------------------------------//
        #region Rapporter
        
            #region Knappar
        /// <summary>
        /// Knapptryck som skapar rapporten
        /// </summary>
        private void btnCreateReport_Click(object sender, EventArgs e)
        {
            ReportUpdate();
        }
        /// <summary>
        /// Metod för att aktivera/Inaktivera datumfiltrering på rapporterna
        /// </summary>
        private void checkBox_DateRapport_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_DateRapport.Checked)
            {
                dtpStartdate.Enabled = true;
                dtpEndDate.Enabled = true;
            }
            else
            {
                dtpStartdate.Enabled = false;
                dtpEndDate.Enabled = false;
            }
        }
        /// <summary>
        /// Knapp för att skriva ut en rapport.
        /// </summary>
        private void btn_PrintRapport_Click(object sender, EventArgs e)
        {
            PrintReport();
        }
        #endregion Knappar

            #region UI
        /// <summary>
        /// Metod för att skapa rapport(Den behöver garanteras optimeras, tror dock inte tiden finns)
        /// </summary>
        private void ReportUpdate()
        {
            try
            {
                panel_Rapport.Controls.Clear();
                panel_Rapport.Controls.Add(pxRapportLogo);
                Show show = null;
                Rapport rapport = null;
                string fromdate = dtpStartdate.Value.ToShortDateString();
                string todate = dtpEndDate.Value.ToShortDateString();
                if (!dtpStartdate.Enabled && !dtpEndDate.Enabled)
                {
                    fromdate = "";
                    todate = "";
                }
                show = (Show)cbxShows.SelectedItem;
                rapport = Circus.GetRapport(fromdate, todate, show);
                int top = 150;
                foreach (var rapportshow in rapport.RapportShows)
                {
                    int row = 0;
                    TableLayoutPanel tbl = new TableLayoutPanel()
                    {
                        Width = 575,
                        Height = 30,
                        GrowStyle = TableLayoutPanelGrowStyle.AddRows,
                        CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                        AutoSize = true,
                        Top = top,
                        Left = 10
                    };
                    panel_Rapport.Controls.Add(tbl);
                    Label label = new Label()
                    {
                        Text = "Föreställning: \n" + rapportshow.Name + " " + rapportshow.Date,
                        Font = new Font(Label.DefaultFont, FontStyle.Bold),
                        Width = 345,
                        Height = 35
                    };
                    row = tbl.RowCount;
                    tbl.Controls.Add(label, 0, row);
                    tbl.RowCount++;
                    foreach (var rapporticket in rapportshow.RapportTickets)
                    {
                        Label tickettype = new Label() { Text = rapporticket.Tickettypes, Width = 345 };
                        Label ticketcount = new Label() { Text = rapporticket.Count.ToString() + "st" };
                        Label ticketsum = new Label() { Text = rapporticket.Summa.ToString() + "kr" };
                        row = tbl.RowCount;
                        tbl.Controls.Add(tickettype, 0, row);
                        tbl.Controls.Add(ticketcount, 1, row);
                        tbl.Controls.Add(ticketsum, 2, row);
                        tbl.RowCount++;
                    }
                    tbl.RowCount++;
                    row = tbl.RowCount;
                    Label totalcount = new Label() { Text = rapportshow.GetTotalTickets().ToString() + "st", Font = new Font(Label.DefaultFont, FontStyle.Bold) };
                    Label totalsumma = new Label() { Text = rapportshow.GetTotalSumma().ToString() + "kr", Font = new Font(Label.DefaultFont, FontStyle.Bold) };
                    tbl.Controls.Add(totalcount, 1, row);
                    tbl.Controls.Add(totalsumma, 2, row);
                    top += tbl.Height + 20;
                }
                TableLayoutPanel tbltotal = new TableLayoutPanel()
                {
                    Width = 575,
                    Height = 30,
                    GrowStyle = TableLayoutPanelGrowStyle.AddRows,
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                    AutoSize = true,
                    Top = top,
                    Left = 10
                };
                panel_Rapport.Controls.Add(tbltotal);
                Label totaltext = new Label()
                {
                    Text = "Totalt: ",
                    Font = new Font(Label.DefaultFont, FontStyle.Bold),
                    Width = 345,
                    Height = 35
                };
                Label adult = new Label() { Text = "Totalt Vuxen:" };
                Label ungdom = new Label() { Text = "Totalt Ungdom:" };
                Label kid = new Label() { Text = "Totalt Barn:" };
                Label totalAdultcount = new Label() { Text = rapport.GetTotalTickets("Vuxen").ToString() + "st", Font = new Font(Label.DefaultFont, FontStyle.Bold) };
                Label totalAdultSumma = new Label() { Text = rapport.GetTotalSumma("Vuxen").ToString() + "kr", Font = new Font(Label.DefaultFont, FontStyle.Bold) };
                Label totalUngdomcount = new Label() { Text = rapport.GetTotalTickets("Ungdom").ToString() + "st", Font = new Font(Label.DefaultFont, FontStyle.Bold) };
                Label totalUngdomSumma = new Label() { Text = rapport.GetTotalSumma("Ungdom").ToString() + "kr", Font = new Font(Label.DefaultFont, FontStyle.Bold) };
                Label totalKidcount = new Label() { Text = rapport.GetTotalTickets("Barn").ToString() + "st", Font = new Font(Label.DefaultFont, FontStyle.Bold) };
                Label totalKidSumma = new Label() { Text = rapport.GetTotalSumma("Barn").ToString() + "kr", Font = new Font(Label.DefaultFont, FontStyle.Bold) };
                Label tbltotalcountlabel = new Label() { Text = rapport.GetTotalTickets().ToString() + "st", Font = new Font(Label.DefaultFont, FontStyle.Bold) };
                Label tbltotalsummalabel = new Label() { Text = rapport.GetTotalSumma().ToString() + "kr", Font = new Font(Label.DefaultFont, FontStyle.Bold) };

                tbltotal.Controls.Add(kid, 0, 0);
                tbltotal.Controls.Add(totalKidcount, 1, 0);
                tbltotal.Controls.Add(totalKidSumma, 2, 0);

                tbltotal.Controls.Add(ungdom, 0, 1);
                tbltotal.Controls.Add(totalUngdomcount, 1, 1);
                tbltotal.Controls.Add(totalUngdomSumma, 2, 1);

                tbltotal.Controls.Add(adult, 0, 2);
                tbltotal.Controls.Add(totalAdultcount, 1, 2);
                tbltotal.Controls.Add(totalAdultSumma, 2, 2);

                tbltotal.Controls.Add(totaltext, 0, 3);
                tbltotal.Controls.Add(tbltotalcountlabel, 1, 3);
                tbltotal.Controls.Add(tbltotalsummalabel, 2, 3);
                panel_Rapport.Height += 200;
                cbxShows.SelectedItem = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Metod för att skriva ut rapporten.
        /// </summary>
        private void PrintReport()
        {
            try
            {
                int width = panel_Rapport.Size.Width;
                int height = panel_Rapport.Size.Height;
                Rectangle rect = new Rectangle(0, 0, width, height);
                string pdfName = DateTime.Today.ToShortDateString() + "cirkus.pdf";
                FileStream fs = new FileStream(pdfName, FileMode.Create, FileAccess.Write, FileShare.None);
                var doc = new iTextSharp.text.Document();
                doc.SetMargins(0f, 0f, 20f, 0f);
                doc.SetPageSize(new iTextSharp.text.Rectangle((float)width, (float)height));
                iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, fs);
                doc.Open();

                Form temp = new Form();
                temp.Controls.Add(panel_Rapport);
                panel_Rapport.BackColor = Color.White;

                Bitmap bm = new Bitmap(width, height);
                panel_Rapport.DrawToBitmap(bm, new Rectangle(0, 0, width, height));
                panel_RapportWrapper.Controls.Add(panel_Rapport);
                panel_Rapport.BackColor = Color.Transparent;
                System.Drawing.Image image = bm;
                iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(image,
                System.Drawing.Imaging.ImageFormat.Jpeg);
                doc.Add(pdfImage);
                doc.Close();
                System.Diagnostics.Process.Start(@pdfName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion UI

        #endregion Rapporter
        //----------------------------------------------------------//
        #region Kundhantering

            #region Knappar
        /// <summary>
        /// Knapp för att uppdatera reservationsuppgifter till en kund
        /// Ligger under kundhanteringsfliken.
        /// </summary>
        private void btnUpdateCustomerInfo_Click(object sender, EventArgs e)
        {
            UpdateOrRemoveCustomer(false);
        }
        /// <summary>
        /// Knapp för att ta bort en kundsreservationsuppgifter
        /// Går inte ta bort om den har en reserverad varukorg.
        /// </summary>
        private void btnRemoveCustomer_Click(object sender, EventArgs e)
        {
            UpdateOrRemoveCustomer(true);
        }
        #endregion Knappar

            #region Listboxhändelser
        /// <summary>
        /// Listboxhändelse som skriver ut info om reservationsuppgifterna under kundhantering.
        /// </summary>
        private void lbxCustomerManagment_SelectedIndexChanged(object sender, EventArgs e)
        {
            Customer customer = (Customer)lbxCustomerManagment.SelectedItem;
            if (lbxCustomerManagment.SelectedItem != null)
            {
                txbFirstnameCustomer.Text = customer.Firstname;
                txbLastnameCustomer.Text = customer.Lastname;
                txbPhonenumberCustomer.Text = customer.Phonenumber;
            }
        }
        /// <summary>
        /// Sökfunktion som tar fram det man söker efter
        /// </summary>
        private void txbSearchCustomer_TextChanged(object sender, EventArgs e)
        {
            lbxReservedTickets.Items.Clear();
            lbxCustomers.Items.Clear();
            foreach (Cart cart in Circus.Carts)
            {
                if (cart.TicketBuyer != null && !string.IsNullOrEmpty(txbSearchCustomer.Text))
                {
                    if (cart.TicketBuyer.Firstname.ToLower().Contains(txbSearchCustomer.Text.ToLower()) || 
                        cart.TicketBuyer.Lastname.ToLower().Contains(txbSearchCustomer.Text.ToLower()) || 
                        cart.TicketBuyer.Phonenumber.Contains(txbSearchCustomer.Text))
                    {
                        lbxCustomers.Items.Add(cart);
                    }
                }
                if (cart.TicketBuyer != null && string.IsNullOrEmpty(txbSearchCustomer.Text))
                {
                    lbxCustomers.Items.Add(cart);
                }
            }
        }
        #endregion Listboxhändelser

            #region UI
        /// <summary>
        /// Metod för att uppdatera eller ta bort reservationsuppgifter under kundhantering
        /// </summary>
        /// <param name="remove">True=Ta bort,False = uppdatera</param>
        private void UpdateOrRemoveCustomer(bool remove)
        {
            try
            {
                if (lbxCustomerManagment.SelectedItem == null)
                {
                    MessageBox.Show("Du behöver välja en kund."); return;
                }
                bool error = false;
                Customer customer = (Customer)lbxCustomerManagment.SelectedItem;
                string fname = txbFirstnameCustomer.Text;
                string lname = txbLastnameCustomer.Text;
                string phonenumb = txbPhonenumberCustomer.Text;
                TextBox[] texts = { txbFirstnameCustomer, txbLastnameCustomer, txbPhonenumberCustomer };
                foreach (var text in texts)
                {
                    if (CheckIfTextBoxIsEmpty(text))
                    {
                        error = true;
                    }
                }
                if (error) { return; }
                if (!remove)
                {
                    Circus.InsertOrUpdateCustomer(customer, fname, lname, phonenumb);
                }
                else if (remove)
                {
                    bool haveZeroReservation = true;
                    foreach (var cart in Circus.Carts)
                    {
                        if (customer.Id == cart.TicketBuyer.Id)
                        {
                            haveZeroReservation = false;
                            MessageBox.Show("Denna kund har en varukorg!");
                            return;
                        }
                    }
                    if (haveZeroReservation)
                    {
                        Circus.RemoveCustomers(customer);
                    }
                }
                CustomersUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Uppdateringsfunktion för kundhanteringssidan
        /// </summary>
        private void CustomersUpdate()
        {
            try
            {
                txbFirstnameCustomer.Clear();
                txbLastnameCustomer.Clear();
                txbPhonenumberCustomer.Clear();
                lbxCustomerManagment.Items.Clear();
                Circus.GetCartsTicketActSeat();
                foreach (Customer customer in Circus.Customers)//Kör igenom listor som berör tab:en.
                {
                    lbxCustomerManagment.Items.Add(customer);
                }
                lbxCustomerManagment.DisplayMember = "PrintCustomer";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion UI

        #endregion Kundhantering
        //----------------------------------------------------------//
        #region Priser

            #region Knappar
        /// <summary>
        /// Knapp för att uppdatera priserna
        /// </summary>
        private void btnUpdatePrices_Click(object sender, EventArgs e)
        {
            UpdatePrice();
        }
        #endregion Knappar

            #region UI
        /// <summary>
        /// Metod för att uppdatera priserna
        /// </summary>
        private void UpdatePrice()
        {
            try
            {
                int kidprice = Convert.ToInt32(txbKidprice.Text);
                int youthprice = Convert.ToInt32(txbyouthprice.Text);
                int adultprice = Convert.ToInt32(txbadultprice.Text);
                int sitdown = Convert.ToInt32(txbsitdownprice.Text);
                int stand = Convert.ToInt32(txbstandprice.Text);
                DialogResult dialogResult = MessageBox.Show("Är du säker på att du vill uppdatera priserna?\nAlla föreställningar kommer att  påverkas av detta.", "Säker?", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    string errormsg = Circus.UpdateTickettypesANDSectionPrices(adultprice, youthprice, kidprice, sitdown, stand);
                    if (errormsg == "")
                    {
                        MessageBox.Show("Priserna har uppdaterats!");
                    }
                    else
                    {
                        MessageBox.Show(errormsg);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Metod för att uppdatera gränssnittet under Priser
        /// </summary>
        private void UIPriceUpdate()
        {
            try
            {
                txbKidprice.Text = string.Empty;
                txbyouthprice.Text = string.Empty;
                txbadultprice.Text = string.Empty;
                txbsitdownprice.Text = string.Empty;
                txbstandprice.Text = string.Empty;
                foreach (TicketType tt in Circus.Tickettypes)
                {
                    if (tt.Name == "Barn")
                    {
                        txbKidprice.Text = tt.Price.ToString();
                    }
                    if (tt.Name == "Ungdom")
                    {
                        txbyouthprice.Text = tt.Price.ToString();
                    }
                    if (tt.Name == "Vuxen")
                    {
                        txbadultprice.Text = tt.Price.ToString();
                    }
                }
                foreach (Show s in Circus.Shows)
                {
                    foreach (Section section in s.Tent.Sections)
                    {
                        if (section.Id == 11)
                        {
                            txbstandprice.Text = section.Price.ToString();
                        }
                        else
                        {
                            txbsitdownprice.Text = section.Price.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion UI

        #endregion Priser
        //----------------------------------------------------------//
        #region Allmänt Uppdateall, Avsluta-knapp. Samt Login.

            #region Knappar
        /// <summary>
        /// Knapp för att avsluta programmet
        /// </summary>
        private void btnEnd_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion Knappar

            #region Listboxhändelser
        /// <summary>
        /// TabControllen som ligger överst, login, admin, biljättförsäljning
        /// </summary>
        private void tabControlMaster_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabControlMaster.SelectedTab == tabControlMaster.TabPages["tabSalesman"])//Kör igenom listor som berör tab:en.
                {
                    tabControlTicketSeller.SelectedIndex = 0;
                    ActiveCart = new Cart();
                    Circus.GetCartsTicketActSeat();
                    UpdateTicketSaleList();
                    panelreserved.Visible = false;
                    PrintShowButtons();
                    lbstandMinus.Text = string.Empty;
                    lbstandPlus.Text = string.Empty;
                }
                else if (tabControlMaster.SelectedTab == tabControlMaster.TabPages["tabAdmin"])//Kör igenom listor som berör tab:en.
                {
                    
                    tabControlAdmin.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// TabControllen som ligger under administratörsfliken
        /// </summary>
        private void tabControlAdmin_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabControlAdmin.SelectedTab == tabControlAdmin.TabPages["tabShows"])//Om taben föreställningar valts. 
                {
                    ShowAdminUpdate();
                    UpdateActs();
                }
                else if (tabControlAdmin.SelectedTab == tabControlAdmin.TabPages["tabCreateShow"])//Om tab skapa shower valts.
                {
                    UICreateShowUpdate();
                }
                else if (tabControlAdmin.SelectedTab == tabControlAdmin.TabPages["tabUsermanagment"])//Om tab systemanvändare valts.
                {
                    UISystemUserUpdate();
                }
                else if (tabControlAdmin.SelectedTab == tabControlAdmin.TabPages["tabCustomers"])//Om tab kunder valts. 
                {
                    CustomersUpdate();
                }
                else if (tabControlAdmin.SelectedTab == tabControlAdmin.TabPages["tabReport"])// Om tab rapport valts. 
                {
                    cbxShows.Items.Clear();
                    foreach (Show SH in Circus.Shows)//Kör igenom listor som berör tab:en.
                    {
                        cbxShows.Items.Add(SH);
                    }
                    cbxShows.DisplayMember = "Name";
                }
                else if (tabControlAdmin.SelectedTab == tabControlAdmin.TabPages["tabAct"])// Om tab skapa akter har valts.
                {
                    UpdateActs();
                }
                else if (tabControlAdmin.SelectedTab == tabControlAdmin.TabPages["tabPrice"])//Om tab systemanvändare valts.
                {
                    UIPriceUpdate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// TabControllen som ligger under biljettförsäljningen
        /// </summary>
        private void tabControlTicketSeller_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabControlTicketSeller.SelectedTab == tabControlTicketSeller.TabPages["tabGetReserved"])//Kör igenom listor som berör tab:en.
                {
                    ReservedUpdate();
                }
                if (tabControlTicketSeller.SelectedTab == tabControlTicketSeller.TabPages["tabTicketSale"])
                {
                    ActiveCart = new Cart();
                    Circus.GetCartsTicketActSeat();
                    UpdateTicketSaleList();
                    panelreserved.Visible = false;
                    PrintShowButtons();
                    lbstandMinus.Text = string.Empty;
                    lbstandPlus.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion Listboxhändelser

            #region UI
        /// <summary>
        /// Metod för att spawna buttons vid muspekaren (Två knappar)
        /// </summary>
        /// <param name="knapp1">Eventhandlar till knapp 1</param>
        /// <param name="knapp2">Eventhandler til knapp 2</param>
        /// <param name="textknapp1">Text till knapp 1</param>
        /// <param name="textknapp2">Text till knapp 2</param>
        private void AddEventButtons(EventHandler knapp1, EventHandler knapp2, string textknapp1, string textknapp2)
        {
            var relativepos = this.PointToClient(Cursor.Position);
            Button button = new Button() { Text = textknapp1, Top = 1, Left = 1 };
            Button abutton = new Button() { Text = textknapp2, Top = 1, Left = button.Width + 1 };
            Panel panel = new Panel()
            {
                Top = relativepos.Y - 5,
                Left = relativepos.X - 10,
                Width = abutton.Width + button.Width + 4,
                Height = abutton.Height + 4,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(panel);
            panel.BringToFront();
            EventHandler remove = new EventHandler(delegate { this.Controls.Remove(panel); });
            panel.Controls.Add(button); panel.Controls.Add(abutton);
            button.BringToFront(); abutton.BringToFront();
            button.Click += remove + knapp1;
            abutton.Click += remove + knapp2;
            panel.MouseLeave += remove;
        }
        /// <summary>
        /// Felhanteringsmetod, som lägger till en label i en textbox om att den inte är ifylld.
        /// </summary>
        /// <param name="text">Textboxen där label skall läggas till</param>
        /// <returns>True=Textboxen är tom, False=Textboxen är inte tom</returns>
        private bool CheckIfTextBoxIsEmpty(TextBox text)
        {
            text.Controls.Clear();
            Label label = new Label() { Text = "Obligatoriskt fält", ForeColor = Color.Red };
            label.Click += delegate { text.Controls.Remove(label); text.Select(); };
            if (text.Text == "")
            {
                text.Controls.Add(label);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Metod som gör att det endast går att skriva siffror och sudda
        /// </summary>
        /// <param name="e">KeyPressEvent från textfält händelsen</param>
        private void DigitsOnly(KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            if (e.KeyChar == (char)8)
            {
                e.Handled = false;
            }
        }
        /// <summary>
        /// Metod för att uppdatera UI vid start Behöver bara köras vid start
        /// </summary> 
        private void UIUpdateStart()
        {
            try
            {
                cbxAuthorisation.Items.Clear();
                cbxAuthorisation.Items.AddRange(Circus.Auths.ToArray());
                cbxAuthorisation.DisplayMember = "Type";
                cbxTents.Items.Clear();
                cbxTents.Items.AddRange(Circus.Tents.ToArray());
                cbxTents.DisplayMember = "Name";
                ShowAdminUpdate();
                UpdateActs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }






        #endregion UI

        #endregion Allmänt Uppdateall, Avsluta-knapp. Samt Login.
        //----------------------------------------------------------//
    }
}
