using System;
using System.Windows.Forms;

namespace Courseworkd_DB
{
    public partial class MainForm : Form
    {
        DB_Tools DB = new DB_Tools(); // database tool object
        bool add; // add or change product check
        bool sale; // sale or deliverry check

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ReportTab.Parent = null;
            UpdateTables();
        }

        private void UpdateTables() // fill all tables 
        {
            dataSet.Clear();

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

        private void AddSaleB_Click(object sender, EventArgs e) //open "Report" tab for Sales
        {
            sale = true;
            ReportTab.Parent = tabControl1;
            tabControl1.SelectTab(ReportTab);

            ReportTable.Rows.Clear();
            SupID_CB.Visible = false;
            supL.Visible = false;
            NameCB.SelectedIndex = -1;
            QuantityTB.Clear();
        }

        private void AddDeliveryB_Click(object sender, EventArgs e) //open "Report" tab for Deliveries
        {
            sale = false;
            ReportTab.Parent = tabControl1;
            tabControl1.SelectTab(ReportTab);

            ReportTable.Rows.Clear();
            SupID_CB.Visible = true;
            supL.Visible = true;
            NameCB.SelectedIndex = -1;
            QuantityTB.Clear();
        }

        private void ReportB_Click(object sender, EventArgs e) //add report
        {
            if (ReportTable.Rows.Count > 1)
            {
                string query = "";

                if (sale) // work with Sales
                {
                    DB.Query("INSERT INTO Sales(Date) VALUES(GETDATE())"); // adding sale
                    UpdateTables();

                    for (int i = 0; i < ReportTable.Rows.Count - 1; i++)
                    {
                        query += $"UPDATE Products SET Quantity = Quantity-{ReportTable[2, i].Value} " + // changing quantity of products 
                            $"WHERE ID={ReportTable[0, i].Value}; " +
                            $"INSERT INTO SaleProduct(SaleID, ProductID, Quantity) " + // adding products into SaleProduct
                            $"VALUES({SalesTable[2, 0].Value}, {ReportTable[0, i].Value}, {ReportTable[2, i].Value});";
                    }
                    tabControl1.SelectedTab = SalesTab;
                }
                else //work with Deliveries
                {
                    DB.Query($"INSERT INTO Deliveries (ID_supplier, Date) VALUES({SupID_CB.SelectedValue},GETDATE());"); // adding delivery 
                    UpdateTables();
                    for (int i = 0; i < ReportTable.Rows.Count - 1; i++)
                    {
                        query += $"UPDATE Products SET Quantity = Quantity+{ReportTable[2, i].Value} " + // changing quantity of products 
                             $"WHERE ID={ReportTable[0, i].Value}; " +
                             $"INSERT INTO DeliveryProduct(DeliveryID, ProductID, Quantity) " + // adding products into DeliveryProduct
                             $"VALUES({DeliviriesTable[3, 0].Value}, {ReportTable[0, i].Value}, {ReportTable[2, i].Value});";
                    }
                    tabControl1.SelectedTab = DeliveriesTab;
                }
                DB.Query(query);
                UpdateTables();
                ReportTab.Parent = null;
            }
        }

        private void ReportCancelB_Click(object sender, EventArgs e)
        {
            ReportTab.Parent = null;
            if (sale)
            {
                tabControl1.SelectedTab = SalesTab;
            }
            else
            {
                tabControl1.SelectedTab = DeliveriesTab;
            }
        }

        //todo prohibit adding identical products 
        //todo prohibit writing in combobox 
        //todo delete afterspaces (find sql function like Trim)
        private void AddProductB_Click(object sender, EventArgs e) // add row into Report table
        {
            if (NameCB.Text != "" && QuantityTB.Text != "")
            {
                ReportTable.Rows.Add(NameCB.SelectedValue, NameCB.Text, QuantityTB.Text);
            }
            NameCB.SelectedIndex = -1; QuantityTB.Text = "";
        }

        private void AddProdB_Click(object sender, EventArgs e) // open add product panel
        {
            add = true;
            ControlP_Open();
        }

        private void ChangeProdB_Click(object sender, EventArgs e) // open change product panel
        {
            expdateTP.Format = DateTimePickerFormat.Short;
            add = false;
            ControlP_Open();

            nameProdTB.Text = ProductsTable.CurrentRow.Cells[2].Value.ToString().Trim();
            priceProdTB.Text = ProductsTable.CurrentRow.Cells[3].Value.ToString().Replace(",", ".");
            unitsProdCB.Text = ProductsTable.CurrentRow.Cells[4].Value.ToString();
            quantProdTB.Text = ProductsTable.CurrentRow.Cells[5].Value.ToString();
            expdateTP.Text = ProductsTable.CurrentRow.Cells[6].Value.ToString();
        }

        private void ControlProdOK_Click(object sender, EventArgs e) // adding or changing product 
        {
            if (nameProdTB.Text != "" && priceProdTB.Text != "" && quantProdTB.Text != "" && expdateTP.Text != " " && unitsProdCB.Text != "")
            {
                if (add) // adding 
                {
                    DB.Query($"INSERT into Products (ID_category, Name, Units, Quantity, Price, Exp_Date) VALUES " +
                        $"({catidProdTB.SelectedValue},'{nameProdTB.Text}','{unitsProdCB.SelectedItem}',{quantProdTB.Text},{priceProdTB.Text}," +
                        $"'{expdateTP.Value.Year}-{expdateTP.Value.Month}-{expdateTP.Value.Day}');");
                }
                else // changing 
                {
                    DB.Query($"UPDATE Products SET ID_category ={catidProdTB.SelectedValue}, Name ='{nameProdTB.Text}', " +
                    $"Units ='{unitsProdCB.SelectedItem}', Quantity ={quantProdTB.Text}, Price ={priceProdTB.Text}, " +
                        $"Exp_Date = '{expdateTP.Value.Year}-{expdateTP.Value.Month}-{expdateTP.Value.Day}' where id ={ProductsTable.CurrentRow.Cells[0].Value};");
                }
                dataSet.Products.Clear();
                DB.Fill("Select * from Products", dataSet.Products);
                ControlP_Cancel();
                EntryClear();
            }
        }

        private void ControlProdCancel_Click(object sender, EventArgs e) // cancel changing or adding a product  
        {
            ControlP_Cancel();
            EntryClear();
        }

        private void RemProdB_Click(object sender, EventArgs e) // remove product
        {
            DB.Query($"DELETE Products WHERE ID = {ProductsTable.CurrentRow.Cells[0].Value};");
            dataSet.Products.Clear();
            DB.Fill("Select * from Products", dataSet.Products);
        }

        private void AddSupB_Click(object sender, EventArgs e)
        {
            SupCtrlP_Open();
        }

        private void RemSupB_Click(object sender, EventArgs e)
        {
            DB.Query($"DELETE Suppliers WHERE ID = {SuppliersTable.CurrentRow.Cells[3].Value};");
            dataSet.Suppliers.Clear();
            DB.Fill("Select * from Suppliers", dataSet.Suppliers);
        }

        private void SupOkB_Click(object sender, EventArgs e)
        {
            if (SupNameTB.Text != "" && SupAddressTB.Text != "" && SupNumTB.Text != "")
            {
                DB.Query($"INSERT into Suppliers(Name, Address, PhoneNumber) VALUES('{SupNameTB.Text}','{SupAddressTB.Text}','{SupNumTB.Text}');");

                dataSet.Suppliers.Clear();
                DB.Fill("Select * from Suppliers", dataSet.Suppliers);
                SupCtrlP_Cancel();
                SupEnrtyClear();
            }
        }

        private void SupCancelB_Click(object sender, EventArgs e)
        {
            SupCtrlP_Cancel();
            SupEnrtyClear();
        }



        // Addition methods 
        private void ControlP_Open()
        {
            EntryP.Visible = true;
            Ok_CancelP.Visible = true;
            ControlP.Visible = false;
        }

        private void ControlP_Cancel()
        {
            EntryP.Visible = false;
            Ok_CancelP.Visible = false;
            ControlP.Visible = true;
        }

        private void SupCtrlP_Open()
        {
            SupEntryP.Visible = true;
            Sup_OK_CancelP.Visible = true;
            SupCtrlP.Visible = false;
        }

        private void SupCtrlP_Cancel()
        {
            SupEntryP.Visible = false;
            Sup_OK_CancelP.Visible = false;
            SupCtrlP.Visible = true;
        }

        private void expdateTP_Enter(object sender, EventArgs e)
        {
            expdateTP.Format = DateTimePickerFormat.Short;
        }

        private void EntryClear()
        {
            nameProdTB.Text = "";
            priceProdTB.Text = "";
            unitsProdCB.Text = "";
            quantProdTB.Text = "";
            expdateTP.Format = DateTimePickerFormat.Custom;
        }

        private void SupEnrtyClear()
        {
            SupNameTB.Text = "";
            SupAddressTB.Text = "";
            SupNumTB.Text = "";
        }


        // Filters 
        private void quantProdTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) || (e.KeyChar == (char)Keys.Back))) // only numbers and backspace
            {
                e.Handled = true;
            }
        }

        private void priceProdTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) || (e.KeyChar == (char)Keys.Back) || (e.KeyChar == '.'))) // only number, bacspace and "."
            {
                e.Handled = true;
            }
        }

        private void QuantityTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) || (e.KeyChar == (char)Keys.Back))) // only numbers and backspace
            {
                e.Handled = true;
            }
        }

        private void SupNumTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) || (e.KeyChar == (char)Keys.Back))) // only numbers and backspace
            {
                e.Handled = true;
            }
        }
    }
}