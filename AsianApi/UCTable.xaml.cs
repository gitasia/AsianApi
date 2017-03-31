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
            while (true)
            {
               if (File.Exists(path)) // дождались свежеих данных с кеша 
                {
                    table();
                    break;
                }
            }
            timer.Start();
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

        private void Edit_begin(object sender, DataGridBeginningEditEventArgs e) //Начали ввод границы коэффициента
        {
            timer.Stop();
            path_cell = grid.SelectedItem as MyTable;
            string bet_column = e.Column.SortMemberPath.ToString();
            if (path_cell.EVENT == " ")
            {
                e.Cancel = true;
                timer.Start();
                return;
            }
            switch (bet_column)
            {
                case "Bet_FULL_TIME_1X2":
                    if (path_cell.Bet_FIRST_HALF_1X2 != "")
                    {
                        e.Cancel = true;
                        timer.Start();
                        return;
                    }
                    break;
                case "Bet_FIRST_HALF_1X2":
                    if (path_cell.Bet_FULL_TIME_1X2 != "")
                    {
                        e.Cancel = true;
                        timer.Start();
                        return;
                    }
                    break;
                case "Bet_FULL_1X2":
                    if (path_cell.Bet_FULL_TIME_1X2 == "")
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
              //      timer.Start();
                    break;
            }

        }

        private void Edit_cell(object sender, DataGridCellEditEndingEventArgs e) //Закончили ввод коэффициента
        {
            var editedTextbox = e.EditingElement as TextBox;
            string edit_cell = editedTextbox.Text.ToString();
            string bet_column = e.Column.SortMemberPath.ToString(); //SortMemberPath;DisplayIndex
            path_cell = grid.SelectedItem as MyTable;
            double bet;
            bool bett = double.TryParse(edit_cell.Replace(".", ","), out bet);
            double t = 0;
            bool tt;
            string mybet = "";
            switch (bet_column)
            {
                case "Bet_FULL_TIME_1X2":
                    tt = double.TryParse(path_cell.FULL_TIME_1X2.Replace(".", ","), out t);
                    path_cell.Bet_FULL_TIME_1X2 = edit_cell;

                    break;
                case "Bet_FIRST_HALF_1X2":
                    tt = double.TryParse(path_cell.FIRST_HALF_1X2.Replace(".", ","), out t);
                    path_cell.Bet_FIRST_HALF_1X2 = edit_cell;

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
            if (limit2.Count == 0)
            {
                string diff = "under";
                if (t < bet) diff = "above";
                limit2.Add(new limit(path_cell.EVENT + path_cell.LeagueId + path_cell.MathcId + path_cell.GameId, edit_cell, bet_column, diff, "", false));
            }

            else
            {
                for (int ll = 0; ll <= limit2.Count - 1; ll++)
                {
                    if (limit2[ll].Id == path_cell.EVENT + path_cell.LeagueId + path_cell.MathcId + path_cell.GameId)
                    {
                        if (edit_cell != "")
                        {
                            switch (mybet)
                            {
                                case "":
                                    string diff = "under";
                                    if (t < bet) diff = "above";
                                    limit2[ll].v = edit_cell;
                                    limit2[ll].bet_column = bet_column;
                                    limit2[ll].diff = diff;
                                    limit2[ll].betted = false;
                                    break;
                                case "FULL":
                                case "FIRST":
                                    limit2[ll].bet = bet.ToString();
                                    break;
                            }
                        }
                        else limit2.RemoveAt(ll);
                        z = 1; break;
                    }
                }
                if (z == 0)
                {
                    string diff = "under";
                    if (t < bet) diff = "above";
                    limit2.Add(new limit(path_cell.EVENT + path_cell.LeagueId + path_cell.MathcId + path_cell.GameId, edit_cell, bet_column, diff, "", false));
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
                            if (result.Count <= i) result.Add(new MyTable(data.Split(',')[0], data.Split(',')[1], data.Split(',')[2], data.Split(',')[3], data.Split(',')[4], data.Split(',')[5], data.Split(',')[6], "", "", data.Split(',')[7], data.Split(',')[8], data.Split(',')[9], data.Split(',')[10], data.Split(',')[11], "", "", data.Split(',')[12], data.Split(',')[13], data.Split(',')[14]));
                            else
                            {
                                int z = -1;
                                //   if (result[i].Bet_FULL_TIME_1X2 != "")
                                {
                                    for (int ll = 0; ll <= limit2.Count - 1; ll++)
                                    {
                                        if (limit2[ll].Id == data.Split(',')[1] + data.Split(',')[12] + data.Split(',')[13] + data.Split(',')[14])
                                        {
                                            z = ll; break;
                                        }
                                    }
                                    if (z == -1)
                                    {
                                        result[i].Bet_FULL_TIME_1X2 = "";
                                        result[i].Bet_FIRST_HALF_1X2 = "";
                                        result[i].Bet_FULL_1X2 = "";
                                        result[i].Bet_FIRST_1X2 = "";
                                    }
                                    else
                                    {
                                        switch (limit2[z].bet_column)
                                        {
                                            case "Bet_FULL_TIME_1X2":
                                                result[i].Bet_FULL_TIME_1X2 = limit2[z].v;
                                                result[i].Bet_FULL_1X2 = limit2[z].bet;
                                                break;
                                            case "Bet_FIRST_HALF_1X2":
                                                result[i].Bet_FIRST_HALF_1X2 = limit2[z].v;
                                                result[i].Bet_FIRST_1X2 = limit2[z].bet;
                                                break;
                                            default:
                                                break;
                                        }

                                    }
                                }
                                result[i] = new MyTable(data.Split(',')[0], data.Split(',')[1], data.Split(',')[2], data.Split(',')[3], data.Split(',')[4], data.Split(',')[5], data.Split(',')[6], result[i].Bet_FULL_TIME_1X2, result[i].Bet_FULL_1X2, data.Split(',')[7], data.Split(',')[8], data.Split(',')[9], data.Split(',')[10], data.Split(',')[11], result[i].Bet_FIRST_HALF_1X2, result[i].Bet_FIRST_1X2, data.Split(',')[12], data.Split(',')[13], data.Split(',')[14]);
                            }
                            i++;
                        }
                    }
                    File.Delete(path);
                    for (int ii = Ligs.Count - 1; ii >= j - 1; ii--) Ligs.RemoveAt(ii);
                    Lab.Content = "In Running" + " (" + Ligs.Count.ToString() + ")";
                    for (int ii = result.Count - 1; ii >= i; ii--) result.RemoveAt(ii); // лишние для отображения
                    set1();
                }
                catch
                {
                    return;
                }
            }
        }

        private void set1()
        {
            // refresh table ligas
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
    }

    class MultiBindingConverter_myConv : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush r_brush = Brushes.Honeydew;
            object obj = values[0];  // тут получили значение поля TIME
            object obj1 = values[1]; // тут получили значение поля EVENT
            object match = values[5];
            if (obj.ToString() == "" && match.ToString() == "")
            {
                if (obj1.ToString() != "")
                {
                    foreach (MyStr Lig in UCTable.Ligas)
                    {
                        if ((obj1.ToString()) == ((MyStr)Lig).LigaName)
                        {
                            r_brush = Brushes.LightGray; //строка color
                            return r_brush;
                        }
                    }
                }
            }
            else
            {
                object F1x2 = values[2];
                object Bet = values[3];
                object H1x2 = values[7];
                object Bet_H1x2 = values[8];
                if ((Bet.ToString() != "" && F1x2.ToString() != "") || (Bet_H1x2.ToString() != "" && H1x2.ToString() != "")) // имеются границы и кэфы не пустые
                {
                    object liga = values[4];

                    object game = values[6];
                    int z = -1;
                    for (int ll = 0; ll <= UCTable.limit2.Count - 1; ll++)
                    {
                        if (UCTable.limit2[ll].Id == obj1.ToString() + liga.ToString() + match.ToString() + game.ToString())
                        {
                            z = ll; break;
                        }
                    }
                    if (z != -1)
                    {
                        if ((Bet.ToString() != "" && F1x2.ToString() != ""))
                        {
                            double tconv;
                            double betconv;
                            bool ttconv = double.TryParse(F1x2.ToString().Replace(".", ","), out tconv);
                            bool bettconv = double.TryParse(Bet.ToString().Replace(".", ","), out betconv);

                            if (UCTable.limit2[z].diff == "under")
                            {
                                if (betconv >= tconv)
                                {
                                    r_brush = Brushes.LightBlue; return r_brush;
                                }
                            }
                            else
                            {
                                if (betconv <= tconv)
                                {
                                    r_brush = Brushes.LightCoral; return r_brush;
                                }
                            }
                        }
                        if ((Bet_H1x2.ToString() != "" && H1x2.ToString() != ""))
                        {
                            double tconv;
                            double betconv;
                            bool ttconv = double.TryParse(H1x2.ToString().Replace(".", ","), out tconv);
                            bool bettconv = double.TryParse(Bet_H1x2.ToString().Replace(".", ","), out betconv);

                            if (UCTable.limit2[z].diff == "under")
                            {
                                if (betconv >= tconv)
                                {
                                    r_brush = Brushes.LightSalmon; return r_brush;
                                }
                            }
                            else
                            {
                                if (betconv <= tconv)
                                {
                                    r_brush = Brushes.LightPink; return r_brush;
                                }
                            }
                        }
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

}
