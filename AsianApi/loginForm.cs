﻿using AsianApi.Api;
using AsianApi.Model;
using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
namespace AsianApi
{
    public partial class loginForm : Form
    {
        private ApiAsian api;
        protected AccountApi account;

        public loginForm()
        {
            InitializeComponent();
        }

        private void loginForm_Load(object sender, EventArgs e)
        {
            account = AccountApi.Instance();
            api = new ApiAsian(account);

            userName.Text = "main_bettor";
            password.Text = "192shilo291";
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            try
            {
               this.Cursor = Cursors.WaitCursor;
               api.Login(userName.Text, password.Text);
            }
            catch (Exception exep)
            {
                userName.Text = "";
                password.Text = "";
                this.Cursor = Cursors.Default;
                MessageBox.Show(exep.Message);
                return;
            }

            //       this.Hide(); // не наддо скрывать а раскрыть для вывода таблицы wpf
            this.loginButton.Hide();
            this.userNameLabel.Hide();
            this.userName.Hide();
            this.password.Hide(); 
            this.userNameLabel.Hide(); 
            this.passwordLabel.Hide();
            this.ClientSize = new System.Drawing.Size(1320, 640);
            this.Text = "sianODDS";
            

            //       MainWindow dashboard = new MainWindow(account);
            //      dashboard.Show();
            // запуск wpf user control
            ElementHost TableHost = new ElementHost();
            TableHost.Dock = DockStyle.Fill;
            
            UCTable ucobj = new UCTable(account);
            TableHost.BringToFront();
            TableHost.Child = ucobj;
            this.Controls.Add(TableHost);
            TableHost.Show();

            return;
        }
        // закрываем форму
        private void loginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var err = new BaseUp().ConBase();
            if (err == "")
            {
                //   Base = new BaseUp().read_Base(Base, 3);
                if (UCTable.Base != null)
                {
                    err = new BaseUp().write_Base(UCTable.Base, UCTable.user_id, UCTable.credit);
                    if (err != "") MessageBox.Show(err);
                }
            }
            else
            {
                MessageBox.Show(err);
            }
            Environment.Exit(0);
            return;
        }
    }
}
