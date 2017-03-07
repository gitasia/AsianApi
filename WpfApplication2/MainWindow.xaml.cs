using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

using System.Collections.ObjectModel;

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<CategoryClass> marketList;

        public MainWindow()
        {
            InitializeComponent();
        }

    private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            marketList = new ObservableCollection<CategoryClass>();
            var cat1 = new CategoryClass(1, "In Running ()");
            cat1.ProductsList = new ObservableCollection<ProductClass>();
            // Здесь нужно заполнить  лигами из запроса
            cat1.ProductsList.Add(new ProductClass(1, "AUSTRALIA HYUNDAI A LEAGUE"));
            cat1.ProductsList.Add(new ProductClass(1, "AUSTRALIA HYUNDAI A LEAGUE - NO OF CORNERS"));
            cat1.ProductsList.Add(new ProductClass(1, "CHONBURI INVTATION TOURNAMENT"));
            cat1.ProductsList.Add(new ProductClass(1, "CLUB FRIENDLY"));
            marketList.Add(cat1);
            marketList.Add(new CategoryClass(2, "Today ()"));
            marketList.Add(new CategoryClass(3, "Early ()"));

            treeView1.ItemsSource = marketList; // обновление списка лиг ( 2 иерархии : текущие (In Running) --> лиги; )
        }
        //Загрузка содержимого таблицы
        private void grid_Loaded(object sender, RoutedEventArgs e)
        {

            Liga.Text = "AUSTRALIA HYUNDAI A LEAGUE"; // Имя лиги для которой заполняется таблица.
            List<MyTable> result = new List<MyTable>();
            // Тут нужны переменные, которые заполняются из пришедшего запроса..
            // в данном примере строки таблицы заполнены конкретными данными
            result.Add(new MyTable("0:0", "Sydnay FC", "1.98", "0.5", "2.000", "1.5", "1.990","","","","",""));
            result.Add(new MyTable("HT", "Adelaide United", "5.71", "", "1.917", "", "1.909", "", "", "", "", ""));
            result.Add(new MyTable("*", "Draw", "2.77", "", "", "", "", "", "", "", "", ""));
            result.Add(new MyTable("", "", "", "0-0.5", "1.641", "1-1.5", "1.680", "", "", "", "", ""));
            result.Add(new MyTable("", "", "", "", "2.389", "", "2.290", "", "", "", "", ""));
            result.Add(new MyTable("", "", "", "0.5-1", "2.389", "1.5-2", "2.340", "", "", "", "", ""));
            result.Add(new MyTable("", "", "", "", "1.641", "", "1.645", "", "", "", "", ""));

           
            grid.ItemsSource = result; // обновление таблицы
            grid.Columns[0].Width = 71;
            grid.Columns[1].Width = 181;
            grid.Columns[2].Width = 48;
            grid.Columns[3].Width = 47;
            grid.Columns[4].Width = 47;
            grid.Columns[5].Width = 53;
            grid.Columns[6].Width = 53;
            grid.Columns[7].Width = 48;
            grid.Columns[8].Width = 47;
            grid.Columns[9].Width = 47;
            grid.Columns[10].Width = 53;
            grid.Columns[11].Width = 53;
        }

        //Получаем данные из таблицы по клику на строке
        private void grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MyTable path = grid.SelectedItem as MyTable;
            MessageBox.Show(" TIME: " + path.TIME + "\n EVENT: " + path.EVENT);
        }
    }

    
}
