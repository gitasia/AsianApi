using AsianApi.Api;
using AsianApi.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

            userName.Text = "victor";
            password.Text = "kuznechnaya";
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

            this.Hide();

            Dashboard dashboard = new Dashboard(account);

            dashboard.Show();
            return;
        }
    }
}
