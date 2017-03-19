using AsianApi.Api;
using AsianApi.Api.Model;
using AsianApi.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AsianApi
{
    /// <summary>
    /// Логика взаимодействия для Dashboard.xaml
    /// </summary>
    /// 
       public partial class Dashboard : Window
    {
        public System.Windows.Threading.DispatcherTimer timer;
        public ObservableCollection<CategoryClass> marketList;
        protected AccountApi account;
        private ApiAsian api;
        //private   ApiAsian api;

        // лист лиг в которых идут игры (пока Live)
        private List<LeagueModel> leaguesList;
        // лист лиг для отображения
        public static List<string> Select_Ligs; //кликнутые
        private List<string> Ligs; // буфер для сортировки и обработки перед выводом в табл
        public static ObservableCollection<MyStr> Ligas; // табл с лигами
        private ObservableCollection<MyStr> Ligass;

        // лист и сгруппированный лист для отображения
        private List<MyTable> result; // очищается полностью перед приемом новых данных
   //     private List<MyTable> Itog_result; // держит текущее значение

  //      private ObservableCollection<MyTable> result;
        private ObservableCollection<MyTable> Itog_result;


        public string Liga_name_in_Table; // надпись лиги в таблице
        public string Home_Team; // команда в матче

        public string game_minuts;
        //static datagrid FootBall_Ligas; 
        private Task<ObservableCollection<MyStr>> task;
        private string path = @"Test.txt";

        public delegate void EventHandler(object sender, object e); //tick timer

        public Dashboard(AccountApi accountApi)
        {
            InitializeComponent();

            account = accountApi;
            account.MarketTypeId = 0;
            account.sportsType = 1;
            api = new ApiAsian(account);
            leaguesList = new List<LeagueModel>();
            result = new List<MyTable>(); // приемник данных
            Itog_result = new ObservableCollection<MyTable>(); // сгруппированная таблица для отображения
            Ligas = new ObservableCollection<MyStr>();
            Ligass = new ObservableCollection<MyStr>();
            Select_Ligs = new List<string>(); //КЛИКНУТЫЕ ЛИГИ
            Ligs = new List<string>();// work list, to do sort ...
            while (true)
            {
                if (!File.Exists(path)) // если файл есть , то не нужно скачивать с азии 
                {
                    Get_Ligas(); Get_Matches();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
              
    //        grid.ItemsSource = Itog_result; // обновление таблицЫ
            
   //         Football_Ligas.ItemsSource = Ligas; // обновление списка лиг 
            // получить текущие лиги
  //          Get_Ligas();
            // получить текущие матчи, распределить их по лигам
  //          Get_Matches();
            // on timer 3 sec
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new System.EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
    //        timer.Start();
    //        while (true)
   //         {
    //            task = new Task<ObservableCollection<MyStr>>(() => Get_Ligas());
                //     task = new Task<ObservableCollection<MyStr>>(() => Get_Ligas());
    //            task.Start();
    //            Ligas = task.Result;
    //            task.Dispose();
    //            Football_Ligas.ItemsSource = Ligas; // обновление списка лиг 
    //        }
            
        }
 
        private void timer_Tick(object sender, object e)
        {
            timer.Stop();
            Get_Ligas(); Get_Matches();
            timer.Start();
        }

        private void Get_Ligas()
        {
            // получить текущие лиги
            JToken leaguesJson = api.GetLeagues();
            List<JToken> leagues = ApiModel.Parse(leaguesJson, "Sports League");
            // записать их в свойство объекта LeaguesList
            foreach (JToken league in leagues)
            {
                LeagueModel leagueModel = new LeagueModel();
                leagueModel.LeagueId = (long)league.SelectToken("LeagueId");
                leagueModel.LeagueName = (string)league.SelectToken("LeagueName");
                leagueModel.MarketTypeId = (int)league.SelectToken("MarketTypeId");
                leagueModel.Since = (ulong)league.SelectToken("Since");
                leagueModel.ListGames = new List<Game>();

                leaguesList.Add(leagueModel);
                if (Ligs.IndexOf(leagueModel.LeagueName) < 0) // только новое имя лиги
                {
                    Ligs.Add(leagueModel.LeagueName);
                }
            }
         //   leaguesList.Clear(); // if read only ligas
            leaguesList.Sort((a,b) => a.LeagueName.CompareTo(b.LeagueName));
            Ligs.Sort();
            Lab.Content = "In Running" +" ("+Ligs.Count.ToString()+")";

            Ligas.Clear(); // new ? impossible rewrite
             
            for (int g = 0; g <= Ligs.Count - 1; g++) Ligas.Add(new MyStr(Ligs[g]));
        //    return Ligass;
        }

        public void Get_Matches()
        {
            JToken feedsJson = api.GetFeeds();
            List<JToken> games = ApiModel.Parse(feedsJson, "Sports MatchGames"); // сбоит когда нет лиг
            Home_Team = "";
            result.Clear();
            foreach (JToken game in games)
            {
                if (!(bool)game.SelectToken("IsActive"))   // Live ? 
                {
                    continue;
                }
                if ((bool)game.SelectToken("WillBeRemoved")) // ?
                {
                    continue;
                }
                var league = leaguesList.Find(x => x.LeagueId == (long)game.SelectToken("LeagueId"));
                Game gameLine = null;

                if (league != null && league.ListGames.Count > 0)
                {
                    gameLine = league.ListGames.Find(x => x.MatchId == (long)game.SelectToken("MatchId"));
                }
                EventModel events = new EventModel();

                if (gameLine == null)
                {
                    gameLine = new Game();
                    Team awayTeam = new Team();
                    Team homeTeam = new Team();
                    gameLine.EventsList = new List<EventModel>();

                    ApiModel.setTeam(awayTeam, game, "AwayTeam");
                    ApiModel.setTeam(homeTeam, game, "HomeTeam");

                    gameLine.AwayTeam = awayTeam;
                    gameLine.HomeTeam = homeTeam;
                    gameLine.ExpectedLength = (int)game.SelectToken("ExpectedLength");      // пока как продолжительность игры
                    gameLine.Favoured = (int)game.SelectToken("Favoured");                  // где применяется? это указывается  фаворит типа. Красным красят что ли в таблице.
                    gameLine.InGameinutes = (int)game.SelectToken("InGameMinutes");         //  -  ?
                    gameLine.isActive = (bool)game.SelectToken("IsActive");                 //   проверяется в момент ставки
                    gameLine.isLive = (int)game.SelectToken("IsLive");
                    gameLine.LeagueId = (long)game.SelectToken("LeagueId");                 // -
                    gameLine.LeagueName = (string)game.SelectToken("LeagueName");
                    gameLine.MarketType = (string)game.SelectToken("MarketType");           // -
                    gameLine.MarketTypeId = (int)game.SelectToken("MarketTypeId");          // -
                    gameLine.MatchId = (long)game.SelectToken("MatchId");                   // -
                    gameLine.StartTime = (ulong)game.SelectToken("StartTime");              // -
                    gameLine.StartsOn = (string)game.SelectToken("StartsOn");               // -
                    gameLine.ToBeRemovedOn = (long)game.SelectToken("ToBeRemovedOn");       // -
                    gameLine.UpdatedDateTime = (ulong)game.SelectToken("UpdatedDateTime");  // -
                    gameLine.WillBeRemoved = (bool)game.SelectToken("WillBeRemoved");       // -

                }
                events.GameId = (long)game.SelectToken("GameId");
                ApiModel.setEventModel(events, game);
                gameLine.EventsList.Add(events);
                if (league != null)
                league.ListGames.Add(gameLine);
            }
            Home_Team = "";
            for (int i = 0; i < leaguesList.Count; i++)
            {
                result.Add(new MyTable("", leaguesList[i].LeagueName, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", leaguesList[i].LeagueId.ToString(), "", ""));
                int j = 0;
                int k = 0;
                leaguesList[i].ListGames.Sort((a, b) => a.HomeTeam.Name.CompareTo(b.HomeTeam.Name));
                while (j < leaguesList[i].ListGames.Count) // && leaguesList[i].ListGames[j] != null)
                                                           //       if (leaguesList[i].ListGames[j] != null)
                {

                    game_minuts = " ";
                    if (leaguesList[i].ListGames[j].InGameinutes >= 60) game_minuts = "Live";
                    if (leaguesList[i].ListGames[j].InGameinutes >= 120 && leaguesList[i].ListGames[j].InGameinutes < 180) game_minuts = "2H " + (leaguesList[i].ListGames[j].InGameinutes - 120).ToString() + "'";
                    if (game_minuts == " ") game_minuts = leaguesList[i].ListGames[j].InGameinutes.ToString(); // конкреное число, если что
                                                                                                               // int k = 0;
                                                                                                               //       for (int k = 0; k < leaguesList[i].ListGames[j].EventsList.Count; k++)
                    if (leaguesList[i].ListGames[j].HomeTeam.Name != Home_Team)
                    {
                        //       if (k == 0)
                        k = 0;
                        result.Add(new MyTable(leaguesList[i].ListGames[j].HomeTeam.Score.ToString() + ":" + leaguesList[i].ListGames[j].AwayTeam.Score.ToString(), leaguesList[i].ListGames[j].HomeTeam.Name, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOneXTwo.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.BookieOdds, 1, 0), Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOneXTwo.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.BookieOdds, 1, 0), "", "", "", "", "", "", "", "", "", "", leaguesList[i].LeagueId.ToString(), leaguesList[i].ListGames[j].MatchId.ToString(), leaguesList[i].ListGames[j].EventsList[k].GameId.ToString())); //первая строка
                        result.Add(new MyTable(game_minuts, leaguesList[i].ListGames[j].AwayTeam.Name, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOneXTwo.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.BookieOdds, 2, 0), Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOneXTwo.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.BookieOdds, 2, 0), "", "", "", "", "", "", "", "", "", "", leaguesList[i].LeagueId.ToString(), leaguesList[i].ListGames[j].MatchId.ToString(), leaguesList[i].ListGames[j].EventsList[k].GameId.ToString()));
                        result.Add(new MyTable("*", "Draw", WinX(leaguesList[i].ListGames[j].EventsList[k].FullTimeOneXTwo.BookieOdds), "", "", "", "", WinX(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOneXTwo.BookieOdds), "", "", "", "", "", "", "", "", "", "", "", "", "", "", leaguesList[i].LeagueId.ToString(), leaguesList[i].ListGames[j].MatchId.ToString(), leaguesList[i].ListGames[j].EventsList[k].GameId.ToString()));
                    }

                    else
                    {
                        result.Add(new MyTable(" ", " ", Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOneXTwo.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.BookieOdds, 1, 0), Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOneXTwo.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.BookieOdds, 1, 0), "", "", "", "", "", "", "", "", "", "", leaguesList[i].LeagueId.ToString(), leaguesList[i].ListGames[j].MatchId.ToString(), leaguesList[i].ListGames[j].EventsList[k].GameId.ToString())); //первая строка
                        result.Add(new MyTable(" ", " ", Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOneXTwo.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.BookieOdds, 2, 0), Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOneXTwo.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.BookieOdds, 2, 0), "", "", "", "", "", "", "", "", "", "", leaguesList[i].LeagueId.ToString(), leaguesList[i].ListGames[j].MatchId.ToString(), leaguesList[i].ListGames[j].EventsList[k].GameId.ToString()));
                        result.Add(new MyTable(" ", " ", WinX(leaguesList[i].ListGames[j].EventsList[k].FullTimeOneXTwo.BookieOdds), "", "", "", "", WinX(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOneXTwo.BookieOdds), "", "", "", "", "", "", "", "", "", "", "", "", "", "", leaguesList[i].LeagueId.ToString(), leaguesList[i].ListGames[j].MatchId.ToString(), leaguesList[i].ListGames[j].EventsList[k].GameId.ToString()));
                    }
                    //j++;
                    k++;
                    Home_Team = leaguesList[i].ListGames[j].HomeTeam.Name;
                    j++;
                }
            }
            result.Add(new MyTable("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "")); // пустая строка - как конец 
            leaguesList.Clear();

                    string data = "";
          //          if (!File.Exists(path))   это проверяется ранее..но нужно для отладки
                    {
                        using (StreamWriter sw = File.CreateText(path))
                        {
                            for (int fi = 0; fi < result.Count; fi++)
                            {
                                data = string.Join(",", result[fi]);
                                sw.WriteLine(data);
                            }
                        }
                    }
            //set1();   // нужен для отладки в монопольном режиме
        }
             

        private void set1()
        {
            if (Select_Ligs.Count != 0) //кликнутые лиги
            {
                Ligs.Clear(); Select_Ligs.Sort();
                for (int g = 0; g <= Select_Ligs.Count - 1; g++) Ligs.Add(Select_Ligs[g]);
            }
            //Itog_result.Clear();
            int i = 0;
            int j = 0;
            int k = 0;
            for (int lg = 0; lg <= Ligs.Count - 1; lg++)
            {
                i = 0;
                while (i < result.Count - 0)
                {
                    if (result[i].EVENT == Ligs[lg])
                    {                        
                            if (Itog_result.Count <= j)
                            {
                                Itog_result.Add(result[i]); j++;
                            }
                            else
                            {
                                Itog_result[j] = result[i]; j++;
                            }
                        i++;
                        while (result[i].TIME != "" && i < result.Count - 3)
                        {
                           if (Itog_result.Count <= j)
                                {
                                    Itog_result.Add(result[i]); j++; Itog_result.Add(result[i + 1]); j++; Itog_result.Add(result[i + 2]); j++;
                                }
                                else
                                {
                                    k = 0;
                                    while (Itog_result.Count > j && k < 3)
                                    { Itog_result[j] = (result[i + k]); j++; k++; }
                                    while (k < 3) { Itog_result.Add(result[i + k]); j++; k++; }
                                }
                           i = i + 3;
                        }
                        i--;
                    }
                    i++;
                }
            }
            for (int ii = Itog_result.Count - 1; ii >= j; ii--)
            {
                Itog_result.RemoveAt(ii); // лишние для отображения
            }
            Ligs.Clear();
        }

        // разбор BookieOdds       
        private string Win(string str,int index1, int index2)
        {
            string retstr;
            if (str != "")
               try { retstr = str.Split(' ')[index1].Split(',')[index2]; }
               catch { retstr = ""; }
            else
             retstr = "";
            return retstr;
        }
        private string WinX(string str)
        {
            string retstr;
            if (str != "")
                try { retstr = str.Split(' ')[3]; }
                catch { retstr = ""; }
            else
                retstr = "";
            return retstr;
        }

        //Получаем данные из таблицы по клику на строке
        private void grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
  //          MyTable path = grid.SelectedItem as MyTable;
 //           System.Windows.MessageBox.Show(" TIME: " + path.TIME + "\n EVENT: " + path.EVENT);
        }

        private void Football_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Select_Ligs.Clear();  // выбранных лиг нет
            if (Ligas.Count != 0) // refresh
            { 
            MyStr path = Ligas[0];
            Ligas.RemoveAt(0);
            Ligas.Insert(0, path);
            }
        //    set1();
        }

        public void Football_Ligas_MouseUp(object sender, MouseButtonEventArgs e) //Получаем данные из таблицы по клику на строке
        {
            MyStr path = Football_Ligas.SelectedItem as MyStr;
            int i = Football_Ligas.SelectedIndex;
            if (path == null ) return;
            string name = ((MyStr)path).LigaName;
            if (Select_Ligs == null || Select_Ligs.IndexOf(name) < 0)
            {
                Select_Ligs.Add(name);
            }
            else
            {
                Select_Ligs.Remove(name);
            }
            // refresh
            Ligas.RemoveAt(i);
            Ligas.Insert(i, path);
            // если не было старта таймера (для отладки) - раскомментировать две нижние строки
            //  Get_Ligas();
            //  Get_Matches();
            set1();
        }

        private void Dashboard_Closing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
            return;
        }
    }
    class MultiBindingConverter_myConv : IMultiValueConverter
    {
        
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object obj1 = values[1]; // тут получили значение поля EVENT
            SolidColorBrush r_brush = Brushes.White;
            if (obj1.ToString() != "")
            {
                foreach (MyStr Lig in Dashboard.Ligas) // по всем лигам заполняем таблицу
                {
                    if ((obj1.ToString()) == ((MyStr)Lig).LigaName)
                    {
                        r_brush = Brushes.LightGray; // красим серым выбранную лигу 
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

    class MultiBindingConverter_ : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object obj = values[0];  // тут получили значение поля LigaName в таблице с лигами
            SolidColorBrush r_brush = Brushes.White;
            if (Dashboard.Select_Ligs.Count() != 0)
            {
                foreach (string Lig in Dashboard.Select_Ligs) // по всем лигам заполняем таблицу
                {
                    if ((obj.ToString()) == Lig)
                    {
                        r_brush = Brushes.LightGray;
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
}
//        string path = @"Test.txt";
//        string data = "";
//        if (!File.Exists(path))
//        {
//            using (StreamWriter sw = File.CreateText(path))
//            {
//                for (int fi = 0; fi < result.Count; fi++)
//                {
//                    data = string.Join(",", result[fi]);
//                    sw.WriteLine(data);
//                }
//            }
//        }
/*
private void set()
        {
            if (Select_Ligs.Count != 0) //кликнутые лиги
            {
                Ligs.Clear(); Select_Ligs.Sort();
                for (int g = 0; g <= Select_Ligs.Count - 1; g++) Ligs.Add(Select_Ligs[g]);
            }
            //Itog_result.Clear();
            int i = 0;
            int j = 0;
            int k = 0;
            int jj = 0;
            for (int lg = 0; lg <= Ligs.Count - 1; lg++)
            {
                i = 0;
                jj = 0;
                while (i < result.Count - 0)
                {
                    if (result[i].EVENT == Ligs[lg])
                    {
                        if (jj == 0) // один раз лигу пишем и к ней все собираем
                        {
                            if (Itog_result.Count <= j)
                            {
                                Itog_result.Add(result[i]); j++;
                            }
                            else
                            {
                                Itog_result[j] = result[i]; j++;
                            }
                        }
                        jj++;
                        i++;
                        while (result[i].TIME != "" && i < result.Count - 3)
                        {
                            if (result[i].EVENT != Home_Team)  // новый матч в лиге
                            {
                                if (Itog_result.Count <= j)
                                {
                                    Itog_result.Add(result[i]); j++; Itog_result.Add(result[i + 1]); j++; Itog_result.Add(result[i + 2]); j++;
                      //              Home_Team = result[i].EVENT; // запомнили матч по домашней команде
                                }
                                else
                                {
                                    k = 0;
                                    while (Itog_result.Count > j && k < 3)
                                    { Itog_result[j] = (result[i + k]); j++; k++; }
                                    while (k < 3) { Itog_result.Add(result[i + k]); j++; k++; }
                      //              Home_Team = result[i].EVENT;
                                }
                            }
                            else
                            {
                                result[i].TIME = " "; result[i + 1].TIME = " "; result[i + 2].TIME = " "; result[i].EVENT = " "; result[i + 1].EVENT = " "; result[i + 2].EVENT = " ";
                                if (Itog_result.Count <= j)
                                {
                                    Itog_result.Add(result[i]); j++; Itog_result.Add(result[i + 1]); j++; Itog_result.Add(result[i + 2]); j++;
                                }
                                else
                                {
                                    k = 0;
                                    while (Itog_result.Count > j && k < 3) { Itog_result[j] = (result[i + k]); j++; k++; }
                                    while (k < 3) { Itog_result.Add(result[i + k]); j++; k++; }
                                }
                                // стерли совпадающие данные по матчу пробелом
                              //  Itog_result[j - 1].EVENT = " "; Itog_result[j - 2].EVENT = " "; Itog_result[j - 3].EVENT = " "; Itog_result[j - 1].TIME = " "; Itog_result[j - 2].TIME = " "; Itog_result[j - 3].TIME = " ";
                            }
                            // result.RemoveAt(i); result.RemoveAt(i); result.RemoveAt(i); // убрали из таблицы
                            i = i + 3;
                        }
                        i--;
                    }
                    i++;
                }
            }

            for (int ii = Itog_result.Count - 1; ii >= j; ii--)
            {
                Itog_result.RemoveAt(ii); // лишние для отображения
            }
            Ligs.Clear();
           // result.Clear();
     //       grid.ItemsSource = null;
     //       grid.ItemsSource = Itog_result; // обновление таблицы
            //Football_Ligas.ItemsSource = null;
            //Football_Ligas.ItemsSource = Ligas;
        }
*/