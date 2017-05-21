using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Data;
using AsianApi.Model;
using AsianApi.Model.Worker;
using System.Globalization;

namespace AsianApi
{
    /// <summary>
    /// Interaction logic for UCTable.xaml
    /// </summary>
    public partial class UCTable : UserControl
    {
        protected AccountApi account;
        private List<string> Ligs; // pool ligs
        public static ObservableCollection<MyStr> Ligas; // view table
        public static List<string> Select_Ligs; //кликнутые
        private List<string> Ligass; // for view ligs

        private List<MyTable> result; //pool kef
        public static List<limit> limit2; // edit limit kef
        private ObservableCollection<MyTable> Itog_result; // view table

        private string path = @"Test.txt";

        private DispatcherTimer timer;
        public delegate void EventHandler(object sender, object e); //tick timer

        private MyTable path_cell;
        private string data; // readln file
        public static Int32 user_id;
        public static Int32 credit;
        public static Int32 yesterday, today, outstanding, openbet;
        public static ObservableCollection<BaseUp.user_bet_table> Base;

        public UCTable(AccountApi accountApi)
        {
            InitializeComponent();
            account = accountApi;
            result = new List<MyTable>(); // приемник данных
            limit2 = new List<limit>();
            Itog_result = new ObservableCollection<MyTable>(); // сгруппированная таблица для отображения
            Ligas = new ObservableCollection<MyStr>();
            Select_Ligs = new List<string>(); //КЛИКНУТЫЕ ЛИГИ
            Ligs = new List<string>();// work list, to do sort ...
            Ligass = new List<string>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(path)) File.Delete(path); //старый, если есть
            //binding view data collection
            Football_Ligas.ItemsSource = Ligas;
            grid.ItemsSource = Itog_result;

            Listener listener = new Listener(account);

            listener.initThread();

            timer = new DispatcherTimer();
            timer.Tick += new System.EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 20); //20 sec for cycle to read file apidata
            Base = new ObservableCollection<BaseUp.user_bet_table>();
            var err = new BaseUp().ConBase();
            if (err == "")
            {
                user_id = account.Id;
                credit = new BaseUp().read_user_credit(user_id); // for view /100
                yesterday = 0; today = 0; outstanding = 0; openbet = 0;
                Account_Summary.Content = Account_Summary.Content + ": " + account.Username; // who is user
                Base = new BaseUp().read_Base(user_id);
                bets.ItemsSource = Base;
                if (Base != null)
                {
                    DateTime parsedDate;
                    DateTime date_t = DateTime.UtcNow;
                    for (int ll = 0; ll <= Base.Count - 1; ll++)
                    {
                        if (DateTime.TryParse(Base[ll].data_game, out parsedDate))
                        {
                            var delta = date_t.Subtract(parsedDate); // period
                            if (!Base[ll].checked_ && Base[ll].betted) // only betted
                            {
                    //            check(ll);
                                if (delta.Hours < 2 && delta.Days == 0) 
                                {
                                    outstanding = outstanding + (Int32)(Single.Parse(Base[ll].bet.Replace(".", ",")) * 100); // continue for checked
                                    openbet++;
                                }
                                if (delta.Hours >2 || delta.Days != 0)
                                {
                            //        credit = credit + (Int32)(Single.Parse(Base[ll].bet.Replace(".", ",")) * 100); // hole, may be checked in server by cron 
                            //        Base.RemoveAt(ll);
                            //        ll--;
                            //        continue;
                                }
                            }
                            if (Base[ll].checked_ && Base[ll].betted)
                            {
                                if (parsedDate.Day != date_t.Day && delta.Days <= 1)
                                {
                                    yesterday = yesterday + (Int32)(Single.Parse(Base[ll].mani.Replace(".", ",")) * 100);
                                }
                                if (parsedDate.Day == date_t.Day)
                                {
                                    today = today + (Int32)(Single.Parse(Base[ll].mani.Replace(".", ",")) * 100);
                                }
                                if (parsedDate.Day != date_t.Day && delta.Days > 7) // del old betted & checked
                                {
                                    Base.RemoveAt(ll);
                                    ll--;
                                }
                            }
                            if (!Base[ll].betted && (delta.Hours > 3 || delta.Days != 0))
                            {
                                if (Base[ll].bet != "") credit = credit + (Int32)(Single.Parse(Base[ll].bet.Replace(".", ",")) * 100);
                                Base.RemoveAt(ll);
                                ll--;
                            }
                        }
                    }
                }
            }
           else
            {
                MessageBox.Show(err);
            }
            timer.Start();
            while (true)
            {
               if (File.Exists(path)) // дождались свежеих данных с кеша 
                {
                    table();
                    break;
                }
            }
        }

        private void timer_Tick(object sender, object e)
        {
            timer.Stop();
            table();
            timer.Start();
        }

        private void grid_MouseUp(object sender, MouseButtonEventArgs e) //Получаем данные из таблицы по клику на строке
        {
            //     path_cell = grid.SelectedItem as MyTable;
            //    MessageBox.Show(" TIME: " + path.TIME + "\n EVENT: " + path.FULL_TIME_HDP_2);
        }

        private void Check_Account(object sender, MouseButtonEventArgs e) 
        {
            //    
        }

        private void view_bets(object sender, MouseButtonEventArgs e) //Green - bets on, LightBlue - bets in waiting, Yellow - bets pending, White - bets off 
        {
            if (bets.Visibility != Visibility.Visible)
            {
                bets.Visibility = Visibility.Visible;
                L_bets.Background = Brushes.White;
                bets.ItemsSource = null;
                bets.ItemsSource = Base;
            }
            else
            {
                bets.Visibility = Visibility.Hidden;
                L_bets.Background = Brushes.Gray;
            }
        }

        private void bets_MouseUp(object sender, MouseButtonEventArgs e) //View to select Liga from table_bets
        {
            BaseUp.user_bet_table path = bets.SelectedItem as BaseUp.user_bet_table;
            Select_Ligs.Clear();
            Select_Ligs.Add(path.liga_name);
            if (Ligas.Count != 0) // brush
            {
                MyStr path1 = Ligas[0]; Ligas.RemoveAt(0); Ligas.Insert(0, path1);
            }
            set1();
        }

        private void Edit_begin(object sender, DataGridBeginningEditEventArgs e) //Начали ввод границы коэффициента
        {
            timer.Stop();
            path_cell = grid.SelectedItem as MyTable;
            string bet_column = e.Column.SortMemberPath.ToString();
            if (path_cell.EVENT == " " || path_cell.IsActiv == " False" || path_cell.WillBeRemoved == " True") // not bet if game off or wii be off
            {
                e.Cancel = true;
                timer.Start();
                return;
            }
            switch (bet_column)
            {
                case "Bet_FULL_TIME_1X2":
                    if (path_cell.FULL_TIME_1X2 == "") //path_cell.Bet_FIRST_HALF_1X2 != "" ||  - must be kef booka
                    {
                         e.Cancel = true;
                         timer.Start();
                         return;
                    }
                     break;
                case "Bet_FIRST_HALF_1X2":
                    if (path_cell.FIRST_HALF_1X2 == "") //path_cell.Bet_FULL_TIME_1X2 != "" ||  
                    {
                        e.Cancel = true;
                        timer.Start();
                        return;
                    }
                     break;
                case "Bet_FULL_1X2":
                    if (path_cell.Bet_FULL_TIME_1X2 == "" && path_cell.Bet_FULL_1X2 =="")
                    {
                        e.Cancel = true;
                        timer.Start();
                        return;
                    }
                     break;
                case "Bet_FIRST_1X2":
                    if (path_cell.Bet_FIRST_HALF_1X2 == "")
                    {
                        e.Cancel = true;
                        timer.Start();
                        return;
                    }
                     break;
               default:
                   timer.Start();
                    break;
              }

            }

        private void Edit_cell(object sender, DataGridCellEditEndingEventArgs e) //Закончили ввод коэффициента
        {
            int index_row = grid.SelectedIndex; // id row table where wii be edit cell
            var editedTextbox = e.EditingElement as TextBox;
            string edit_cell = editedTextbox.Text.ToString();
            string bet_column = e.Column.SortMemberPath.ToString(); //SortMemberPath;DisplayIndex
            path_cell = grid.SelectedItem as MyTable;
            double bet;
            bool bett = double.TryParse(edit_cell.Replace(".", ","), out bet);
            bet = Math.Abs(bet);
            double t = 0;
            bool tt;
            string mybet = "";
            string kef ="";
            string event_ = "";
            switch (bet_column)
            {
                case "Bet_FULL_TIME_1X2":
                    tt = double.TryParse(path_cell.FULL_TIME_1X2.Replace(".", ","), out t);
                    path_cell.Bet_FULL_TIME_1X2 = edit_cell;
                    kef = path_cell.FULL_TIME_1X2;
                    event_ = "full,";

                    break;
                case "Bet_FIRST_HALF_1X2":
                    tt = double.TryParse(path_cell.FIRST_HALF_1X2.Replace(".", ","), out t);
                    path_cell.Bet_FIRST_HALF_1X2 = edit_cell;
                    kef = path_cell.FIRST_HALF_1X2;
                    event_ = "first,";

                    break;
                case "Bet_FULL_1X2":
                    path_cell.Bet_FULL_1X2 = edit_cell;
                    mybet = "FULL";
                    break;
                case "Bet_FIRST_1X2":
                    path_cell.Bet_FIRST_1X2 = edit_cell;
                    mybet = "FIRST";
                    break;
                default:
                    break;
            }
            
            int z = 0;
            string away_command = "";
            string home_command = "";
            string liganame = "";
            if (path_cell.TIME == "*")
            {
                away_command = Itog_result[index_row - 1].EVENT;
                home_command = Itog_result[index_row - 2].EVENT;
            }
            if (path_cell.TIME.IndexOf(":") !=-1)
            {
                away_command = Itog_result[index_row + 1].EVENT;
                home_command = Itog_result[index_row].EVENT;
            }
            if (path_cell.TIME.IndexOf(" ") != -1)
            {
                away_command = Itog_result[index_row].EVENT;
                home_command = Itog_result[index_row-1].EVENT;
            }
            int index_row_liganame = index_row; // up to liganame where TIME =""
            while (Itog_result[index_row_liganame].TIME != "")
                index_row_liganame--;
            liganame = Itog_result[index_row_liganame].EVENT;


            if (Base.Count == 0)
            {
                if (edit_cell != "")
                {
                    string diff = "under"; if (t < bet) diff = "above";
                    Base.Add(new BaseUp.user_bet_table(user_id, path_cell.LeagueId, path_cell.MathcId, path_cell.GameId, 
                                                       home_command, away_command, event_ + path_cell.EVENT, liganame, 
                                                       edit_cell, "", false, kef, "", "", "", false, "", path_cell.DataGame,
                                                       diff, "football", "in running", false));
                }
            }
            else
            {
               for (int ll = 0; ll <= Base.Count - 1; ll++)
                {
                  if (Base[ll].event_.Split(',')[1] + Base[ll].liga_id + Base[ll].match_id + Base[ll].game_id 
                      == path_cell.EVENT + path_cell.LeagueId + path_cell.MathcId + path_cell.GameId && Base[ll].event_.Split(',')[0] == "full" && (event_ == "full," || mybet == "FULL"))
                        {                                                                            // record new data in bet
                        if (edit_cell != "" && !Base[ll].betted)
                        {
                            switch (mybet)
                            {
                                case "":
                                    string diff = "under"; if (t < bet) diff = "above";
                                    Base[ll].my_odds = edit_cell; Base[ll].event_ = event_ + path_cell.EVENT; Base[ll].diff = diff; Base[ll].betted = false;
                                    break;
                                case "FULL":
                     //           case "FIRST":
                                    if (credit >= (Int32)bet * 100)
                                    {
                                        Base[ll].bet = bet.ToString();
                                        credit = credit - (Int32)bet * 100;
                                    }
                                    else
                                    {
                                        Base[ll].bet = (credit / 100).ToString();
                                        credit = 0; 
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            if (!Base[ll].betted) // del bet and myodds before bettind
                            {
                                switch (mybet)
                                {
                                    case "":
                                        Base[ll].my_odds = ""; Base[ll].diff = ""; Base[ll].betted = false;
                                        break;
                                    case "FULL":
                       //             case "FIRST":
                                        if (Base[ll].bet != "") credit = credit + (Int32)(Single.Parse(Base[ll].bet.Replace(".", ",")) * 100);
                                        Base[ll].bet = "";
                                        break;
                                    default:
                                        break;
                                }
                                if (Base[ll].bet == "" && Base[ll].my_odds == "")
                                {
                                    Base.RemoveAt(ll);
                                    ll--;
                                }
                            }
                        }
                        z = 1; break;
                    }

                    if (Base[ll].event_.Split(',')[1] + Base[ll].liga_id + Base[ll].match_id + Base[ll].game_id
                                          == path_cell.EVENT + path_cell.LeagueId + path_cell.MathcId + path_cell.GameId && Base[ll].event_.Split(',')[0] == "first" && (event_ == "first," || mybet == "FIRST"))
                    {                                                                            // record new data in bet
                        if (edit_cell != "" && !Base[ll].betted)
                        {
                            switch (mybet)
                            {
                                case "":
                                    string diff = "under"; if (t < bet) diff = "above";
                                    Base[ll].my_odds = edit_cell; Base[ll].event_ = event_ + path_cell.EVENT; Base[ll].diff = diff; Base[ll].betted = false;
                                    break;
                     //           case "FULL":
                                case "FIRST":
                                    if (credit >= (Int32)bet * 100)
                                    {
                                        Base[ll].bet = bet.ToString();
                                        credit = credit - (Int32)bet * 100;
                                    }
                                    else
                                    {
                                        Base[ll].bet = (credit / 100).ToString();
                                        credit = 0;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            if (!Base[ll].betted) // del bet and myodds before bettind
                            {
                                switch (mybet)
                                {
                                    case "":
                                        Base[ll].my_odds = ""; Base[ll].diff = ""; Base[ll].betted = false;
                                        break;
                 //                   case "FULL":
                                    case "FIRST":
                                        if (Base[ll].bet != "") credit = credit + (Int32)(Single.Parse(Base[ll].bet.Replace(".", ",")) * 100);
                                        Base[ll].bet = "";
                                        break;
                                    default:
                                        break;
                                }
                                if (Base[ll].bet == "" && Base[ll].my_odds == "")
                                {
                                    Base.RemoveAt(ll);
                                    ll--;
                                }
                            }
                        }
                        z = 1; break;
                    }
                }
                if (z == 0)
                {
                    if (edit_cell != "")
                    {
                        string diff = "under"; if (t < bet) diff = "above";
                        Base.Add(new BaseUp.user_bet_table(user_id, path_cell.LeagueId, path_cell.MathcId, path_cell.GameId,
                                                           home_command, away_command, event_ + path_cell.EVENT, liganame, 
                                                           edit_cell, "", false, kef, "", "", "", false, "", path_cell.DataGame,
                                                           diff, "football", "in running", false));
                    }
                }
            }
            timer.Start();
        }

        private void table()
        {
            if (File.Exists(path))
            {
                int i = 0; int j = 0;
                try
                {
                    using (StreamReader sw = new StreamReader(path))
                    {
                        while ((data = sw.ReadLine()) != null)
                        {
                            if (data.Split(',')[0] == "")
                            {
                                if (Ligs.Count <= j) Ligs.Add(data.Split(',')[1]);
                                else Ligs[j] = data.Split(',')[1];
                                j++;
                            }
                            if (result.Count <= i) result.Add(new MyTable(data.Split(',')[0], data.Split(',')[1], data.Split(',')[2], data.Split(',')[3], data.Split(',')[4], data.Split(',')[5], data.Split(',')[6], "", "", data.Split(',')[7], data.Split(',')[8], data.Split(',')[9], data.Split(',')[10], data.Split(',')[11], "", "", data.Split(',')[12], data.Split(',')[13], data.Split(',')[14], data.Split(',')[15], data.Split(',')[16], data.Split(',')[17]));
                            else
                            {
                                int z = -1;
                                //   if (result[i].Bet_FULL_TIME_1X2 != "")
                                {
                                    for (int ll = 0; ll <= Base.Count - 1; ll++)
                                    {
                                        if (Base[ll].event_.Split(',')[1] + Base[ll].liga_id + Base[ll].match_id + Base[ll].game_id == data.Split(',')[1] + data.Split(',')[12] + data.Split(',')[13] + data.Split(',')[14])
                                        {
                                            z = ll; //break; (if one event in row) we have max two events when odds pointed on 1st time & game
                                        }
                                        //}
                                        if (z == -1)
                                        {
                                            result[i].Bet_FULL_TIME_1X2 = "";
                                            result[i].Bet_FIRST_HALF_1X2 = "";
                                            result[i].Bet_FULL_1X2 = "";
                                            result[i].Bet_FIRST_1X2 = "";
                                        }
                                        else
                                        {
                                            switch (Base[z].event_.Split(',')[0])
                                            {
                                                case "full":
                                                    result[i].Bet_FULL_TIME_1X2 = Base[z].my_odds;
                                                    result[i].Bet_FULL_1X2 = Base[z].bet;
                                                    break;
                                                case "first":
                                                    result[i].Bet_FIRST_HALF_1X2 = Base[z].my_odds;
                                                    result[i].Bet_FIRST_1X2 = Base[z].bet;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }// for
                                }
                                result[i] = new MyTable(data.Split(',')[0], data.Split(',')[1], data.Split(',')[2], data.Split(',')[3], data.Split(',')[4], data.Split(',')[5], data.Split(',')[6], result[i].Bet_FULL_TIME_1X2, result[i].Bet_FULL_1X2, data.Split(',')[7], data.Split(',')[8], data.Split(',')[9], data.Split(',')[10], data.Split(',')[11], result[i].Bet_FIRST_HALF_1X2, result[i].Bet_FIRST_1X2, data.Split(',')[12], data.Split(',')[13], data.Split(',')[14], data.Split(',')[15], data.Split(',')[16], data.Split(',')[17]);
                            }
                            i++;
                        }
                    }
                    File.Delete(path);
                    for (int ii = Ligs.Count - 1; ii >= j - 1; ii--) Ligs.RemoveAt(ii);
                    Lab.Content = "In Running" + " (" + Ligs.Count.ToString() + ")";
                    for (int ii = result.Count - 1; ii >= i; ii--) result.RemoveAt(ii); // лишние для отображения
                    // check bettig
                    for (int ll = 0; ll <= Base.Count - 1; ll++)
                    {
                        int z = -1;
                        for (int ii = 0; ii < result.Count-1; ii++)
                        {
                            if ((Base[ll].event_.Split(',')[1] + Base[ll].liga_id + Base[ll].match_id + Base[ll].game_id == result[ii].EVENT + result[ii].LeagueId + result[ii].MathcId + result[ii].GameId) && !Base[ll].checked_) // && Base[ll].betted)
                            {
                                DateTime parsedDate; DateTime date_t = DateTime.UtcNow;
                                if (DateTime.TryParse(Base[ll].data_game, out parsedDate))
                                  {
                                    var delta = date_t.Subtract(parsedDate); // period
                                    if (result[ii].WillBeRemoved.ToString() == " False" && result[ii].IsActiv.ToString() == " True") // game over 
                                     {
                                         if (result[ii].TIME == "*") Base[ll].result_full = result[ii - 2].TIME;
                                         if (result[ii].TIME.IndexOf(":") != -1) Base[ll].result_full = result[ii].TIME;
                                         if (result[ii].TIME.IndexOf(" ") != -1) Base[ll].result_full = result[ii - 1].TIME;
                                         if(Base[ll].event_.Split(',')[0] == "full" && result[ii].FULL_TIME_1X2 != "") Base[ll].kef = result[ii].FULL_TIME_1X2;
                                         if (parsedDate.Day == date_t.Day && delta.Hours < 1 && delta.Minutes < 50) // for fiks result first time
                                            {
                                               if (result[ii].TIME == "*") Base[ll].result_first = result[ii - 2].TIME;
                                               if (result[ii].TIME.IndexOf(":") != -1) Base[ll].result_first = result[ii].TIME;
                                               if (result[ii].TIME.IndexOf(" ") != -1) Base[ll].result_first = result[ii - 1].TIME;
                                            if (Base[ll].event_.Split(',')[0] == "first" && result[ii].FIRST_HALF_1X2 != "") Base[ll].kef = result[ii].FIRST_HALF_1X2;
                                        }
                                         if (parsedDate.Day == date_t.Day && delta.Hours >=1 && !Base[ll].checked_ && Base[ll].event_.Split(',')[0] == "first") // check after first time
                                        {
                                            if (Base[ll].betted) check(ll);
                                            else
                                            {
                                                credit = credit + (Int32)(Single.Parse(Base[ll].bet.Replace(".", ",")) * 100);
                                                Base.RemoveAt(ll);
                                                ll--;
                                            }
                                        }
                                     }
                                    if (Base[ll].checked_ && Base[ll].betted)
                                       {
                                         if (parsedDate.Day != date_t.Day && delta.Days <= 1)
                                          {
                                            yesterday = yesterday + (Int32)(Single.Parse(Base[ll].mani.Replace(".", ",")) * 100);
                                            today = today - (Int32)(Single.Parse(Base[ll].mani.Replace(".", ",")) * 100);
                                          }
                                         if (parsedDate.Day == date_t.Day)
                                          {
                                 //           today = today + (Int32)(Single.Parse(Base[ll].mani.Replace(".", ",")) * 100);
                                 //           outstanding = outstanding - (Int32)(Single.Parse(Base[ll].mani.Replace(".", ",")) * 100);
                                 //           openbet--;
                                          }
                                      }
                                  }
                                z = ll; break;
                            }
                        }
                        if (z == -1) // to be removed
                        {
                            if (Base[ll].betted && !Base[ll].checked_)
                            {  
                                check(ll);
                            }
                            if(!Base[ll].betted)
                            {
                                credit = credit + (Int32)(Single.Parse(Base[ll].bet.Replace(".", ",")) * 100);
                                Base.RemoveAt(ll);
                                ll--;
                            }
                        }
                    }
                    set1();
                }
                catch (Exception e)
                {
 //                   MessageBox.Show(e.Message);
                    return;
                }
            }
        }

        private void set1()
        {
            // refresh table ligas
            
            Credit.Content = (credit / 100).ToString() + "." + (Math.Abs(credit % 100)).ToString();
            YesterdayPL.Content = (yesterday / 100).ToString() + "." + (Math.Abs(yesterday % 100)).ToString();
            TodayPL.Content = (today / 100).ToString() + "." + (Math.Abs(today % 100)).ToString();
            Outstanding.Content = (outstanding / 100).ToString() + "." + (Math.Abs(outstanding % 100)).ToString();
            Openbets.Content = openbet;
            int l = 0;
            for (int g = 0; g <= Ligs.Count - 1; g++)
            {
                if (Ligas.Count <= l) Ligas.Add(new MyStr(Ligs[g]));
                else Ligas[l] = new MyStr(Ligs[g]);
                l++;
            }
            for (int ii = Ligas.Count - 1; ii >= l; ii--) Ligas.RemoveAt(ii);

            if (Ligs.Count != 0) //принятые лиги
            {
                Ligass.Clear(); for (int g = 0; g <= Ligs.Count - 1; g++) Ligass.Add(Ligs[g]);
            }

            if (Select_Ligs.Count != 0) //кликнутые лиги
            {
                Ligass.Clear(); Select_Ligs.Sort(); for (int g = 0; g <= Select_Ligs.Count - 1; g++) Ligass.Add(Select_Ligs[g]);
            }

            int i = 0; int j = 0; int k = 0;
            for (int lg = 0; lg <= Ligass.Count - 1; lg++)
            {
                i = 0;
                while (i < result.Count - 1)
                {
                    if (result[i].EVENT == Ligass[lg])
                    {
                        if (Itog_result.Count <= j) { Itog_result.Add(result[i]); j++; }
                        else { Itog_result[j] = result[i]; j++; }
                        i++;
                        while (result[i].TIME != "" && i < result.Count - 3)
                        {
                            if (Itog_result.Count <= j) { Itog_result.Add(result[i]); j++; Itog_result.Add(result[i + 1]); j++; Itog_result.Add(result[i + 2]); j++; }
                            else
                            {
                                k = 0;
                                while (Itog_result.Count > j && k < 3) { Itog_result[j] = (result[i + k]); j++; k++; }
                                while (k < 3) { Itog_result.Add(result[i + k]); j++; k++; }
                            }
                            i = i + 3;
                        }
                        i--;
                    }
                    i++;
                }
            }
            for (int ii = Itog_result.Count - 1; ii >= j; ii--) Itog_result.RemoveAt(ii); // лишние для отображения
        }

        private void Football_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Select_Ligs.Clear();  // выбранных лиг нет
            if (Ligas.Count != 0) // brush
            {
                MyStr path = Ligas[0]; Ligas.RemoveAt(0); Ligas.Insert(0, path);
            }
            set1();
        }

        public void Football_Ligas_MouseUp(object sender, MouseButtonEventArgs e) //Получаем данные из таблицы по клику на строке
        {
            MyStr path = Football_Ligas.SelectedItem as MyStr;
            int i = Football_Ligas.SelectedIndex;
            if (path == null) return;
            string name = ((MyStr)path).LigaName;
            if (Select_Ligs == null || Select_Ligs.IndexOf(name) < 0) Select_Ligs.Add(name);
            else Select_Ligs.Remove(name);
            // brush
            Ligas.RemoveAt(i);
            Ligas.Insert(i, path);
            set1();
        }

        private void check(int ll)
        {
            switch (Base[ll].event_.Split(',')[0])
            {
                case "full":
                    if (Base[ll].result_full == "") break;
                    if (Base[ll].checked_) break;
                    if (Base[ll].home_command == Base[ll].event_.Split(',')[1])
                    {
                        if (Int32.Parse(Base[ll].result_full.Split(':')[0]) > Int32.Parse(Base[ll].result_full.Split(':')[1])) raise_credit(ll);
                        else fold_bet(ll);
                    }
                    if (Base[ll].away_command == Base[ll].event_.Split(',')[1])
                    {
                        if (Int32.Parse(Base[ll].result_full.Split(':')[0]) < Int32.Parse(Base[ll].result_full.Split(':')[1])) raise_credit(ll);
                        else fold_bet(ll);
                    }
                    if ("Draw" == Base[ll].event_.Split(',')[1])
                    {
                        if (Int32.Parse(Base[ll].result_full.Split(':')[0]) == Int32.Parse(Base[ll].result_full.Split(':')[1])) raise_credit(ll);
                        else fold_bet(ll);
                    }
                    Base[ll].checked_ = true;
                    today = today + (Int32)(Single.Parse(Base[ll].mani.Replace(".", ",")) * 100);
                    outstanding = outstanding - (Int32)(Single.Parse(Base[ll].bet.Replace(".", ",")) * 100);
                    openbet--;
                    break;
                case "first":
                    if (Base[ll].result_first == "") break;
                    if (Base[ll].checked_) break;
                    if (Base[ll].home_command == Base[ll].event_.Split(',')[1])
                    {
                        if (Int32.Parse(Base[ll].result_first.Split(':')[0]) > Int32.Parse(Base[ll].result_first.Split(':')[1])) raise_credit(ll);
                        else fold_bet(ll);
                    }
                    if (Base[ll].away_command == Base[ll].event_.Split(',')[1])
                    {
                        if (Int32.Parse(Base[ll].result_first.Split(':')[0]) < Int32.Parse(Base[ll].result_first.Split(':')[1])) raise_credit(ll);
                        else fold_bet(ll);
                    }
                    if ("Draw" == Base[ll].event_.Split(',')[1])
                    {
                        if (Int32.Parse(Base[ll].result_first.Split(':')[0]) == Int32.Parse(Base[ll].result_first.Split(':')[1])) raise_credit(ll);
                        else fold_bet(ll);
                    }
                    Base[ll].checked_ = true;
                    today = today + (Int32)(Single.Parse(Base[ll].mani.Replace(".", ",")) * 100);
                    outstanding = outstanding - (Int32)(Single.Parse(Base[ll].bet.Replace(".", ",")) * 100);
                    openbet--;
                    break;
                  
                default:
                    break;
            }
        }

        private void raise_credit(int ll)
        {
            Base[ll].on_off = true;
            Base[ll].mani = ((Single.Parse(Base[ll].my_odds.Replace(".", ",")) * Single.Parse(Base[ll].bet.Replace(".", ",")))).ToString();
            credit = credit + (Int32)(Single.Parse(Base[ll].mani.Replace(".", ",")) * 100);
        }

        private void fold_bet(int ll)
        {
            Base[ll].on_off = false;
            Base[ll].mani = (-Single.Parse(Base[ll].bet.Replace(".", ","))).ToString();
     //       credit = credit + (Int32)(Single.Parse(Base[ll].mani.Replace(".", ",")) * 100);
        }
    }

    class MultiBindingConverter_myConv : IMultiValueConverter
    {
        private Int32 bets;
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush r_brush = Brushes.Honeydew;
            object obj = values[0];  // тут получили значение поля TIME
            object obj1 = values[1]; // тут получили значение поля EVENT
            object match = values[5];
            if (obj.ToString() == "" && match.ToString() == "") //liga_name
            {
                if (obj1.ToString() != "")
                {
                    foreach (MyStr Lig in UCTable.Ligas)
                    {
                        if ((obj1.ToString()) == ((MyStr)Lig).LigaName)   return Brushes.LightGray; //строка color
                    }
                }
            }
            else
            {
                object F1x2 = values[2];
                object Bet_F1x2 = values[3];
                object H1x2 = values[7];
                object Bet_H1x2 = values[8];
                object meni_full = values[9];
                object meni_first = values[10];
                object IsActive = values[11]; //IsActive.ToString() == " True" &&
                if (IsActive.ToString() == " True" && ((Bet_F1x2.ToString() != "" && F1x2.ToString() != "") || (Bet_H1x2.ToString() != "" && H1x2.ToString() != ""))) // имеются границы и кэфы не пустые
                {
                    object liga = values[4];

                    object game = values[6];
                    int z = -1;
                    for (int ll = 0; ll <= UCTable.Base.Count - 1; ll++) //odds
                    {
                        if (UCTable.Base[ll].event_.Split(',')[1] + UCTable.Base[ll].liga_id + UCTable.Base[ll].match_id + UCTable.Base[ll].game_id == obj1.ToString() + liga.ToString() + match.ToString() + game.ToString())
                        {
                            z = ll; //break;
                        }
                        //}
                        if (z != -1)
                        {
                            if ((Bet_F1x2.ToString() != "" && F1x2.ToString() != "" && meni_full.ToString() != "") && UCTable.Base[z].event_.Split(',')[0] == "full")
                            {
                                double tconv;
                                double betconv;
                                bool ttconv = double.TryParse(F1x2.ToString().Replace(".", ","), out tconv);
                                bool bettconv = double.TryParse(Bet_F1x2.ToString().Replace(".", ","), out betconv);
                                if (UCTable.Base[z].diff == "under")
                                {
                                    if (UCTable.Base[z].betted) r_brush = Brushes.LightBlue; // brush forever
                                    else
                                    if (betconv >= tconv) return betting(z, tconv, Brushes.LightBlue);
                                }
                                else
                                {
                                    if (UCTable.Base[z].betted) r_brush = Brushes.LightCoral; // brush forever
                                    else
                                    if (betconv <= tconv) return betting(z, tconv, Brushes.LightCoral);
                                }
                            }
                            if ((Bet_H1x2.ToString() != "" && H1x2.ToString() != "" && meni_first.ToString() != "") && UCTable.Base[z].event_.Split(',')[0] == "first")
                            {
                                double tconv;
                                double betconv;
                                bool ttconv = double.TryParse(H1x2.ToString().Replace(".", ","), out tconv);
                                bool bettconv = double.TryParse(Bet_H1x2.ToString().Replace(".", ","), out betconv);
                                if (UCTable.Base[z].diff == "under")
                                {
                                    if (UCTable.Base[z].betted) r_brush = Brushes.LightSalmon; // brush forever
                                    else
                                    if (betconv >= tconv) return betting(z, tconv, Brushes.LightSalmon);
                                }
                                else
                                {
                                    if (UCTable.Base[z].betted) r_brush = Brushes.LightPink; // brush forever
                                    else
                                    if (betconv <= tconv) return betting(z, tconv, Brushes.LightPink);
                                }
                            }
                        }
                    } // for
                }
            }
            return r_brush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

       private SolidColorBrush betting(int z, double tconv, SolidColorBrush color)
        {
            // betting   for example : string kef_betting = new Listener(..).betting(....)
            //  if (kef_betting != "")
            // { UcTable.Base[z].kef=kef_betting;
            UCTable.Base[z].betted = true; // fiks bet
            UCTable.Base[z].data_bet = DateTime.UtcNow.ToString();
            bets = (Int32)(Single.Parse(UCTable.Base[z].bet.Replace(".", ",")) * 100);
            UCTable.outstanding = UCTable.outstanding + bets;
            UCTable.Base[z].my_odds = tconv.ToString();
            UCTable.openbet = UCTable.openbet + 1;
            return color;
        }
    }
    class MultiBindingConverter_ : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object obj = values[0];  // тут получили значение поля LigaName в таблице с лигами
            SolidColorBrush r_brush = Brushes.Black;
            if (UCTable.Select_Ligs.Count() != 0)
            {
                foreach (string Lig in UCTable.Select_Ligs) // по всем лигам заполняем таблицу
                {
                    if ((obj.ToString()) == Lig)
                    {
                        r_brush = Brushes.DarkGreen;
                        return r_brush;
                    }
                }
            }

            return r_brush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class MultiBindingConverter_Base : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object obj = values[0];  // тут получили значение поля LigaName в таблице с лигами
            object bet = values[1];
            object betted = values[2];
            SolidColorBrush r_brush = Brushes.Snow;
            string ss = obj.ToString();
            if ((Boolean)obj)
                    {
                        r_brush = Brushes.LightGreen;
                        return r_brush;
                    }
            if (bet.ToString() == "" && (Boolean)betted)
            {
                r_brush = Brushes.Yellow;
                return r_brush;
            }
            if (!(Boolean)betted)
            {
                r_brush = Brushes.LightBlue;
                return r_brush;
            }
            return r_brush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
