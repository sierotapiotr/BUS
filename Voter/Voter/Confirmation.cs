using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Voter
{
    /// <summary>
    /// Klasa dot. potwierdzenia dla Votera - otrzymuje on od Proxy potwierdzenie swojego wyboru
    /// </summary>
    class Confirmation
    {   
        /// <summary>
        /// Numer wybranej kolumny
        /// </summary>
        private int numColumn;
        /// <summary>
        /// Część potwierzdenia - zapis głosu jako string
        /// </summary>
        private string column;

        /// <summary>
        /// Część potwierzdenia - token odpowiadający wybranej kolumnie
        /// </summary>
        private BigInteger token;

        /// <summary>
        /// Część potwierzdenia - wybrana kolumna podpisana przez EA 
        /// </summary>
        private BigInteger signedColumn;

        /// <summary>
        /// ListView wyświetlający potwierdzenie
        /// </summary>
        private ListView confirmationListView;

        /// <summary>
        /// Konstruktor klasy dot. potwierdzenia wybory głosowania
        /// </summary>
        /// <param name="confirmationListView"></param>
        public Confirmation(ListView confirmationListView)
        {
            this.confirmationListView = confirmationListView;
        }

        public int ColumnNumber
        {
            set { numColumn = value; }
        }


        public string Column
        {
            set { column = value; }
            get { return column; }
        }

        public BigInteger Token
        {
            set { token = value; }
            get { return token; }
        }

        public BigInteger SignedColumn
        {
            set { signedColumn = value; }
            get { return signedColumn; }
        }

        public int Index
        {
            get { return numColumn; }
        }


        /// <summary>
        /// Metoda dodająca oraz wyświetlająca potwierdzenie.
        /// </summary>
        /// <param name="anotherThread"></param>
        public void addConfirm(bool anotherThread = false)
        {
            ListViewItem item1 = new ListViewItem();
            ListViewItem item2 = new ListViewItem();
            ListViewItem item3 = new ListViewItem();
            ListViewItem item4 = new ListViewItem();
            item1.Text = "Kolumna: " + this.numColumn;
            item2.Text = "Kolumna (twój głos): " + this.column;
            item3.Text = "Token: " + this.token;
            item4.Text = "Podpisana kolumna: " + this.signedColumn;

            if (!anotherThread)
            {
                confirmationListView.Items.Add(item1);
                confirmationListView.Items[confirmationListView.Items.Count - 1].EnsureVisible();
                confirmationListView.Items.Add(item2);
                confirmationListView.Items[confirmationListView.Items.Count - 1].EnsureVisible();
                confirmationListView.Items.Add(item3);
                confirmationListView.Items[confirmationListView.Items.Count - 1].EnsureVisible();
                confirmationListView.Items.Add(item4);
                confirmationListView.Items[confirmationListView.Items.Count - 1].EnsureVisible();

            }
            else
            {
                confirmationListView.Invoke(new MethodInvoker(delegate ()
                {
                    confirmationListView.Items.Add(item1);
                    confirmationListView.Items[confirmationListView.Items.Count - 1].EnsureVisible();
                    confirmationListView.Items.Add(item2);
                    confirmationListView.Items[confirmationListView.Items.Count - 1].EnsureVisible();
                    confirmationListView.Items.Add(item3);
                    confirmationListView.Items[confirmationListView.Items.Count - 1].EnsureVisible();
                    confirmationListView.Items.Add(item4);
                    confirmationListView.Items[confirmationListView.Items.Count - 1].EnsureVisible();
                })
                    );
            }
        }

    }
}
