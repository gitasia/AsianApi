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

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic 
    /// </summary>

    public partial class MainWindow : Window
    {

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
     
        public MainWindow()
        {
            InitializeComponent();
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

            Process proc = Process.Start(@"Resources\AsianApi.exe");//api forever
        
            timer = new DispatcherTimer();
            timer.Tick += new System.EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1); //1 sec for cycle to read file apidata
            timer.Start();
        }

        private void timer_Tick(object sender, object e)
        {
            timer.Stop();
            table();    
            timer.Start();
        }
        //shutdown
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
          Process[] ps= System.Diagnostics.Process.GetProcessesByName("AsianApi");
            foreach(Process p1 in ps)
            {
                if (p1.ProcessName == "AsianApi")
                {
                    p1.Kill();
                    p1.WaitForExit();
                }
            }
         Environment.Exit(0);
         return;
         }

        private void _Loaded(MyStr name_liga)
        {
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
                   if(path_cell.Bet_FIRST_HALF_1X2 != "")
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
                default:
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
                    default:
                        break;
                }
                string diff = "under";
                if (t < bet) diff = "above";
                int z = 0;
                if (limit2.Count == 0) limit2.Add(new limit(path_cell.EVENT + path_cell.LeagueId + path_cell.MathcId + path_cell.GameId, edit_cell, bet_column, diff, false));
                else
                {
                    for (int ll = 0; ll <= limit2.Count - 1; ll++)
                    {
                        if (limit2[ll].Id == path_cell.EVENT + path_cell.LeagueId + path_cell.MathcId + path_cell.GameId)
                        {
                            if (edit_cell != "")
                            {
                                limit2[ll].v = edit_cell;
                                limit2[ll].bet_column = bet_column;
                                limit2[ll].diff = diff;
                                limit2[ll].betted = false;
                            }
                            else limit2.RemoveAt(ll);
                            z = 1; break;
                        }
                    }
                    if (z == 0) limit2.Add(new limit(path_cell.EVENT + path_cell.LeagueId + path_cell.MathcId + path_cell.GameId, edit_cell, bet_column, diff, false));
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
                            if (result.Count <= i) result.Add(new MyTable(data.Split(',')[0], data.Split(',')[1], data.Split(',')[2], data.Split(',')[3], data.Split(',')[4], data.Split(',')[5], data.Split(',')[6], data.Split(',')[7], data.Split(',')[8], data.Split(',')[9], data.Split(',')[10], data.Split(',')[11], "", "", "", "", "", "", "", "", "", "", data.Split(',')[12], data.Split(',')[13], data.Split(',')[14]));
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
                                    }
                                    else
                                    {
                                        switch (limit2[z].bet_column)
                                        {
                                            case "Bet_FULL_TIME_1X2":
                                                result[i].Bet_FULL_TIME_1X2 = limit2[z].v;
                                                break;
                                            case "Bet_FIRST_HALF_1X2":
                                                result[i].Bet_FIRST_HALF_1X2 = limit2[z].v;
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                                result[i] = new MyTable(data.Split(',')[0], data.Split(',')[1], data.Split(',')[2], data.Split(',')[3], data.Split(',')[4], data.Split(',')[5], data.Split(',')[6], data.Split(',')[7], data.Split(',')[8], data.Split(',')[9], data.Split(',')[10], data.Split(',')[11], result[i].Bet_FULL_TIME_1X2, "", result[i].Bet_FULL_TIME_HDP_2, "", result[i].Bet_FULL_TIME_OU_2, result[i].Bet_FIRST_HALF_1X2, "", result[i].Bet_FIRST_HALF_HDP_2, "", result[i].Bet_FIRST_HALF_OU_2, data.Split(',')[12], data.Split(',')[13], data.Split(',')[14]);
                            }
                            i++;
                        }
                    }
                    File.Delete(path);
                    for (int ii = Ligs.Count-1; ii >= j-1; ii--) Ligs.RemoveAt(ii);
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
            for (int lg = 0; lg <= Ligass.Count-1; lg++)
            {
                i = 0;
                while (i < result.Count - 1)
                {
                    if (result[i].EVENT == Ligass[lg])
                    {
                        if (Itog_result.Count <= j) {Itog_result.Add(result[i]); j++;}
                        else { Itog_result[j] = result[i]; j++;}
                        i++;
                        while (result[i].TIME != "" && i < result.Count - 3)
                        {
                            if (Itog_result.Count <= j) {Itog_result.Add(result[i]); j++; Itog_result.Add(result[i + 1]); j++; Itog_result.Add(result[i + 2]); j++;}
                            else
                            {   k = 0;
                                while (Itog_result.Count > j && k < 3) {Itog_result[j] = (result[i + k]); j++; k++;}
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
            SolidColorBrush r_brush = Brushes.White;
            object obj = values[0];  // тут получили значение поля TIME
            object obj1 = values[1]; // тут получили значение поля EVENT
            object match = values[5];
            if (obj.ToString() == "" && match.ToString() == "")
            {
                if (obj1.ToString() != "")
                {
                    foreach (MyStr Lig in MainWindow.Ligas)
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
                if ((Bet.ToString() !="" && F1x2.ToString() != "") || (Bet_H1x2.ToString() != "" && H1x2.ToString() != "")) // имеются границы и кэфы не пустые
                {
                    object liga = values[4];
                    
                    object game = values[6];
                    int z = -1;
                    for (int ll = 0; ll <= MainWindow.limit2.Count - 1; ll++)
                    {
                        if (MainWindow.limit2[ll].Id == obj1.ToString() + liga.ToString() + match.ToString() + game.ToString())
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

                        if (MainWindow.limit2[z].diff == "under")
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

                            if (MainWindow.limit2[z].diff == "under")
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
                if (MainWindow.Select_Ligs.Count() != 0)
                {
                    foreach (string Lig in MainWindow.Select_Ligs) // по всем лигам заполняем таблицу
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
//   private void Datagrid_ItemCreated(object sender, System.Web.UI.WebControls.DataGridItemEventArgs e)
//   {
//If Header item

//        if (e.Item == "")
//   {
//           e.Item.Cells.RemoveAt(2);
//           e.Item.Cells(1).ColumnSpan = 2;
//Insert the table shown in the diagram 3
// to the Text property of the Cell
//            e.Item.Cells(1).Text = "<table style='FONT-WEIGHT: bold; WIDTH:" +
//                 " 100%; COLOR: white; TEXT-ALIGN: center'><tr align" +
//                  " =center><td colspan = 2 style='BORDER-BOTTOM:" +
//                  " cccccc 1pt solid'>Name</td></tr>" +
//                  "<tr align =center ><td style ='BORDER-RIGHT:" +
//                  " cccccc 1pt solid'>F Name</td><td>L" +
//                 " Name</td></tr></table>";
//        }
//    }
//   protected void DataGrid_RowDataBound(object sender, GridViewRowEventArgs e)
//    {
//        if (e.Row.RowType == DataControlRowType.DataRow
//            && ((e.Row.DataItem as System.Data.DataRowView).Row.IsNull("SmthDate")
//                || Convert.ToDateTime((e.Row.DataItem as System.Data.DataRowView).Row["SmthDate"]) < System.DateTime.Now.Date))
//            e.Row.Style["color"] = "blue";
//    }


//         MessageBox.Show(grid.R ToString());

//      DataGridCell cell = new DataGridCell();
//       cell = grid. .CurrentCell;
//       int x = cell.RowNumber;
//        int y = cell.ColumnNumber;
//        DataGrid[x, y] = "NewText";
//          for (int i = 0; i <= result.Count-1; i++)
//          {
//   if(grid.Items.GetItemAt(i) == Brushes.Gray)
//              {
//                  try
//                 {
//                     string d = grid.RowBackground.ToString();
//                     MessageBox.Show(grid.RowBackground.ToString());
//                 }
//                catch (Exception ex)
//                {
//                    MessageBox.Show("Ошибка: " + ex.Message);
//                }
//     grid.CurrentCell.Column
//        grid.Cells.RemoveAt(2);
//        grid.Item.Cells(1).ColumnSpan = 2;
//            }
//           Console.WriteLine(i);
//      }
//         TextBlock text1 = new TextBlock();

//   < TextBlock x: Name = "Liga" HorizontalAlignment = "Left" DockPanel.Dock = "Top"  Foreground = "White" Background = "Black" Width = "750" ></ TextBlock >
//      text1.HorizontalAlignment = HorizontalAlignment.Left;
//        DockPanel.SetDock(text1, Dock.Top);
//    text1.VerticalAlignment = VerticalAlignment.Top;
//       text1.Width = 750;
//        text1.Text = Liga.Text;
//       Pages_.Children.Add(text1);

//       DataGrid grid1 = new DataGrid();

//      InitializeComponent();
//< DataGrid  x: Name = "grid" HorizontalAlignment = "Left" VerticalAlignment = "Top" Width = "750"
//       Loaded = "grid_Loaded" MouseUp = "grid_MouseUp" GridLinesVisibility = "Horizontal" HeadersVisibility = "None" />

//     grid1.HorizontalAlignment = HorizontalAlignment.Left;
//      DockPanel.SetDock(grid1, Dock.Top);
//      grid1.VerticalAlignment = VerticalAlignment.Top;
//      grid1.Width = 750;
//      grid1.HeadersVisibility = (DataGridHeadersVisibility)0;
//       grid1.GridLinesVisibility = (DataGridGridLinesVisibility)1;
//       grid1.AutoGenerateColumns = true;
//grid1.Visibility = false;
//      grid1.Loaded += grid1_Loaded;
//        Pages_.Children.Add(grid1);
//        grid1.ItemsSource = result;

//        MessageBox.Show("Загрузить?");
//     Pages_.Children.Add(grid1);
//        grid1.Columns[0].Width = 71;
//        grid1.Columns[1].Width = 181;
//        grid1.Columns[2].Width = 48;
//        grid1.Columns[3].Width = 47;
//        grid1.Columns[4].Width = 47;
//        grid1.Columns[5].Width = 53;
//        grid1.Columns[6].Width = 53;
//         grid1.Columns[7].Width = 48;
//        grid1.Columns[8].Width = 47;
//        grid1.Columns[9].Width = 47;
//        grid1.Columns[10].Width = 53;
//        grid1.Columns[11].Width = 53;
//             if ((obj1.ToString()) == "CLUB FRIENDLY") // если значение поля Наименование больше 500 то задаем цвет
//              {
//      BrushConverter converter = new BrushConverter();
//      SolidColorBrush brush = converter.ConvertFromString("#e32636") as SolidColorBrush;
//            r_brush = Brushes.Gray;
//        var cell = (DataGridCell)values[1];
//        cell.
//                 r_brush = Brushes.Gray; 
//       if (values[1] is DataRow)
//       {
//Change the background of any cell with 1.0 to light red.
//            var cell = (DataGridCell)values[0];
//           var row = (DataRow)values[1];
//      }
//   SolidColorBrush r_brush = Brushes.Black;
// e.Item.Cells.RemoveAt(2);
// e.Item.Cells(1).ColumnSpan = 2;
//               }
//    if (obj1.ToString() != "")
//   {
//         if (System.Convert.ToDecimal(obj1.ToString()) > 800) // если значение поля Обозначение больше 800 то задаем цвет
//         {
//             r_brush = Brushes.GreenYellow;
//         }
//         else
//          {

//          }
//      }
//
/*
     if (Itog_result[j].Bet_FULL_TIME_1X2 !="" )
                                    {
                                        if (limit2.Count == 0)
                                        {
                                            limit2.Add(new limit(Itog_result[j].EVENT + Itog_result[j].LeagueId + Itog_result[j].MathcId + Itog_result[j].GameId, Itog_result[j].Bet_FULL_TIME_1X2));
                                            lim++;
                                        }
                                        else
                                        {
                                            for( int ll = 0; ll <= limit2.Count-1; ll++)
                                            {
                                                if (limit2[ll].Id == Itog_result[j].EVENT + Itog_result[j].LeagueId + Itog_result[j].MathcId + Itog_result[j].GameId)
                                                {
                                                    z = 1; break;
                                                }
                                            }
                                            if(z==0)
                                            {
                                                limit2.Add(new limit(Itog_result[j].EVENT + Itog_result[j].LeagueId + Itog_result[j].MathcId + Itog_result[j].GameId, Itog_result[j].Bet_FULL_TIME_1X2));
                                                lim++;
                                                
                                            }
                                            z = 0;
                                        }
                                    }
                                    */