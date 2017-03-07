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
    public partial class AccountForm : Form
    {
        private static AccountForm accountForm = null;

        public static AccountForm Instance()
        {
            if (accountForm == null)
            {
                accountForm = new AccountForm();
            }

            return accountForm;
        }

        private AccountForm()
        {
            InitializeComponent();
        }

        private void Account_Load(object sender, EventArgs e)
        {
            
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
            accountForm = null;
        }
    }
}
