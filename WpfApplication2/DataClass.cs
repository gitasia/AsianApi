using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Collections.ObjectModel;

namespace WpfApplication2
{
    //класс категории (IN Running, ..
    public class CategoryClass
    {
        //id категории
        private int categoryId;

        public int CategoryId
        {
            get { return categoryId; }
            set { categoryId = value; }
        }

        //имя категории
        private string categoryName;

        public string CategoryName
        {
            get { return categoryName; }
            set { categoryName = value; }
        }

        //список входящих в категорию лиг
        private ObservableCollection<ProductClass> productsList;

        public ObservableCollection<ProductClass> ProductsList
        {
            get { return productsList; }
            set { productsList = value; }
        }

        //класс лиги


        public CategoryClass(int _categoryId, string _categoryName)
        {
            CategoryId = _categoryId;
            CategoryName = _categoryName;

            ProductsList = new ObservableCollection<ProductClass>();

           
        }
    }
    public class ProductClass
    {
        //id лиги
        private int productId;

        public int ProductId
        {
            get { return productId; }
            set { productId = value; }
        }

        //имя лиги
        private string productName;

        public string ProductName
        {
            get { return productName; }
            set { productName = value; }
        }

        public ProductClass(int _productId, string _productName)
        {
            ProductId = _productId;
            ProductName = _productName;
        }
    }

   
}

