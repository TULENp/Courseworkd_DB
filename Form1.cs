using System;
using System.Windows.Forms;

namespace Courseworkd_DB
{
    public partial class Form1 : Form
    {
        DB_Tools DB = new DB_Tools();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AddSaleTab.Parent = null;
            UpdateTables();
        }
        private void UpdateTables()
        {
            dataSet.Clear();
            Add_Sale_Del_Table.Rows.Clear();

            DB.Fill("Select * from Products", dataSet.Products);
            DB.Fill("Select * from Categories", dataSet.Categories);

            DB.Fill("Select * from Suppliers", dataSet.Suppliers);
            DB.Fill("SELECT d.ID, d.ID_supplier, s.Name AS SupplierName, d.DATE, D.Cost " +
                "FROM Deliveries d JOIN Suppliers s ON d.ID_supplier = s.ID ORDER BY d.Date DESC, d.ID DESC", dataSet.Deliveries);
            DB.Fill("SELECT dp.DeliveryID, dp.ProductID, p.Name AS ProductName, dp.Quantity, dp.Cost " +
                "FROM DeliveryProduct dp JOIN Products p ON dp.ProductID = p.ID", dataSet.DeliveryProduct);

            DB.Fill("Select * from Sales order by Date desc,ID desc", dataSet.Sales);
            DB.Fill("SELECT sp.SaleID, sp.ProductID, P.Name as ProductName, sp.Quantity, sp.Cost " +
                "FROM SaleProduct sp JOIN Products p ON sp.ProductID = p.ID", dataSet.SaleProduct);

        }
       
        //todo maybe add addsaletable clear in UpdateTable()
        private void AddSaleB_Click(object sender, EventArgs e)
        {
            AddSaleTab.Parent = tabControl1;
            tabControl1.SelectTab("AddSaleTab");
            Add_Sale_Del_Table.Rows.Clear();
            AddDeliveryID_CB.Visible = false;
        }

        private void AddDeliveryB_Click(object sender, EventArgs e)
        {
            AddSaleTab.Parent = tabControl1;
            tabControl1.SelectTab("AddSaleTab");
            Add_Sale_Del_Table.Rows.Clear();
            AddDeliveryID_CB.Visible = true;
        }
        private void SaleFillB_Click(object sender, EventArgs e)
        {
            DB.Query("INSERT INTO Sales(Date) VALUES(GETDATE())");
            UpdateTables();
            string query = "";
            for (int i = 0; i < Add_Sale_Del_Table.Rows.Count - 1; i++)
            {
                query += $"UPDATE Products SET Quantity = Quantity-{Add_Sale_Del_Table[1, i].Value} " +
                    $"WHERE ID={Add_Sale_Del_Table[0, i].Value}; " +
                    $"INSERT INTO SaleProduct(SaleID, ProductID, Quantity) " +
                    $"VALUES({SalesTable[0, 0].Value}, {Add_Sale_Del_Table[0, i].Value}, {Add_Sale_Del_Table[1, i].Value});";
            }
            DB.Query(query);
            UpdateTables();


        }
        private void DeliveryFillB_Click(object sender, EventArgs e)
        {
            DB.Query($"INSERT INTO Deliveries (ID_supplier, Date) VALUES({AddDeliveryID_CB.SelectedValue},GETDATE());");
            UpdateTables();
            string query = "";
            for (int i = 0; i < Add_Sale_Del_Table.Rows.Count - 1; i++)
            {
                query += $"UPDATE Products SET Quantity = Quantity+{Add_Sale_Del_Table[1, i].Value} " +
                     $"WHERE ID={Add_Sale_Del_Table[0, i].Value}; " +
                     $"INSERT INTO DeliveryProduct(DeliveryID, ProductID, Quantity) " +
                     $"VALUES({DeliviriesTable[0, 0].Value}, {Add_Sale_Del_Table[0, i].Value}, {Add_Sale_Del_Table[1, i].Value});";
            }
            DB.Query(query);
            UpdateTables();

        }











        //todo сделать фильтр только цифр для таблицы AddSaleTable

        //private void AddSaleTable_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e) // setting the filter to the table 
        //{
        //    TextBox tb = (TextBox)e.Control;
        //    if (AddSaleTable.CurrentCell.ColumnIndex == 1) // if current column is "Quantity"
        //    {
        //        tb.KeyPress += new KeyPressEventHandler(Table_KeyPress); // invoke filter method 
        //    }


        //}
        //void Table_KeyPress(object sender, KeyPressEventArgs e) // filter method 
        //{
        //    if (!(Char.IsDigit(e.KeyChar) || (e.KeyChar == (char)Keys.Back))) // only allow numbers and backspace
        //    {
        //        e.Handled = true;
        //    }
        //}

    }
}
