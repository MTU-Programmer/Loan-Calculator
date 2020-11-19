using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Loan
{
    public partial class Form1 : Form
    {
        Formulas formulas;

        private void listView1_Resize(object sender, EventArgs e)
        {
            SizeLastColumn((ListView)sender);
        }

        /*---------------------------------------------
         * Set the column widths for both ListView controls.
         */
        private void styleListViewControls()
        {
            listView1.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.None);
            listView1.Columns[0].Width = 80;
            //Interest
            listView1.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.None);
            listView1.Columns[1].Width = 100;
            //Principal
            listView1.Columns[2].AutoResize(ColumnHeaderAutoResizeStyle.None);
            listView1.Columns[2].Width = 100;
            //Outstanding Balance
            listView1.Columns[3].AutoResize(ColumnHeaderAutoResizeStyle.None);
            listView1.Columns[3].Width = 150;

            //--------------------------
            listView2.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.None);
            listView2.Columns[0].Width = 80;
            //Interest
            listView2.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.None);
            listView2.Columns[1].Width = 100;
            //Principal
            listView2.Columns[2].AutoResize(ColumnHeaderAutoResizeStyle.None);
            listView2.Columns[2].Width = 100;
            //Outstanding Balance
            listView2.Columns[3].AutoResize(ColumnHeaderAutoResizeStyle.None);
            listView2.Columns[3].Width = 150;

            //listView1.Columns[listView1.Columns.Count - 1].Width = -2;

        }
        private void SizeLastColumn(ListView lv)
        {
            lv.Columns[lv.Columns.Count - 1].Width = -2;
        }

        /*---------------------------------------------
         * Try to parse the textBox string into a double.
         * If unsuccessful display the errorMsg in a dialog box and return false,
         * else return true and out double v = the string converted to a double.
         */
        private bool tryParseDouble(string errMsg, TextBox tb, out double v)
        {
            bool result = double.TryParse(tb.Text, out v);
            if (!result)
            {
                MessageBox.Show(errMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return result;
        }
        /*---------------------------------------------
         */
        private void infoToTextBoxes()
        {
            int sf = formulas.info.SolveFor;
            double dpa, ti, tip;
            DeferredPaymentBox.Text = "";
             
            textBox1.Text = String.Format("{0:0.0}", formulas.info.Terms);
            textBox2.Text = String.Format("{0:0}", formulas.info.X12);
            textBox3.Text = String.Format("{0:0.00}", formulas.info.Apr);
            textBox4.Text = String.Format("{0:0.00}", formulas.info.Eff);
            textBox5.Text = String.Format("{0:0,0.00}", formulas.info.Principal);
            textBox6.Text = String.Format("{0:0,0.00}", formulas.info.Payment);
            
            //First, turn off all 6 radio buttons
            radioPanel.Controls.OfType<RadioButton>().ToList().ForEach(r => r.Checked = false);
            //Next, turn on the radio button corresponding to SolveFor
            radioPanel.Controls.OfType<RadioButton>().Where(r => r.Tag.ToString() == sf.ToString()).First(r => r.Checked = true);

            // Set the values for Deferred payment amount, Total interest and Total interest per cent.
            dpa = formulas.info.Payment * formulas.info.Terms;
            ti = dpa - formulas.info.Principal;
            tip = 1 - (formulas.info.Principal / dpa);
            DeferredPaymentBox.Text = String.Format("{0:0,0.00}", dpa);
            TotalInterestBox.Text = String.Format("{0:0,0.00}", ti);
            TotalInterestPercentBox.Text = String.Format("{0:0.##%}", tip);



        }
        //---------------------------------------------
        private bool textBoxesToInfo()
        {
            string s;
            double v;
            bool result = false;
            int sf = formulas.info.SolveFor;

            if (sf != 1)
            {
                result = tryParseDouble("Enter a valid number for Terms", textBox1, out v);
                if (!result)
                {
                    this.ActiveControl = textBox1;
                    return result;
                }
                formulas.info.Terms = v;
            }

            if (sf != 2)
            {
                result = tryParseDouble("Invalid number for Compounding per year", textBox2, out v);
                if (!result)
                {
                    this.ActiveControl = textBox2;
                    return result;
                }
                formulas.info.X12 = v;
            }

            if (sf != 3)
            {
                result = tryParseDouble("Invalid number for APR", textBox3, out v);
                if (!result)
                {
                    this.ActiveControl = textBox3;
                    return result;
                }
                formulas.info.Apr = v;
            }

            if (sf != 4)
            {
                result = tryParseDouble("Invalid number for EFF", textBox4, out v);
                if (!result)
                {
                    this.ActiveControl = textBox4;
                    return result;
                }
                formulas.info.Eff = v;
            }

            if (sf != 5)
            {
                result = tryParseDouble("Invalid number for Principal", textBox5, out v);
                if (!result)
                {
                    this.ActiveControl = textBox5;
                    return result;
                }
                formulas.info.Principal = v;
            }

            if (sf != 6)
            {
                result = tryParseDouble("Invalid number for Payment", textBox6, out v);
                if (!result)
                {
                    this.ActiveControl = textBox6;
                    return result;
                }
                formulas.info.Payment = v;
            }

            // If result is false the Amortization table cannot be generated.
            return result;
        }

        //---------------------------------------------
        public Form1()
        {
            InitializeComponent();
            //Period
            styleListViewControls();
            formulas = new Formulas();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SizeLastColumn(listView1);
            infoToTextBoxes();
            TabPage t = tabControl1.TabPages[1];
            
            pseudoBtnClick();
            tabControl1.SelectedTab = t; //go to tab
            tabPage2.Select();


        }
        // When the user clicks on a radio button, set the corresponding text box to "0"
        private void radioButton_Click(object sender, EventArgs e)
        {
            string rb = ((RadioButton)sender).Tag.ToString();
            TextBox tBox = panel2.Controls.OfType<TextBox>().Where(t => t.Tag.ToString() == rb).First();
            tBox.Text = "0";
            //If the user clicked on the Eff radio button then reset the Apr text box to zero as well.
            if (tBox.Tag.ToString() == "4" || tBox.Tag.ToString() == "3")
            {
                textBox3.Text = "0";
                textBox4.Text = "0";
            }

        }
        /*---------------------------------------------
            There are two ListView grids inside a tab control: one for monthly data
            and the other for yearly data.
            First copy all of the monthly financial data to the monthly ListView.
            Period, Interest, Principal, Balance
         */
        private void sendDataToListView()
        {
            int i, t = (int) formulas.info.Terms;
            double principal = formulas.info.Principal;
            ListViewItem item;
            //Display the monthly data
            listView1.Items.Clear();
            for (i = 0; i < t; i +=1)
            {
                item = new ListViewItem(String.Format("{0:0}", (i+1)));
                item.SubItems.Add(String.Format("{0:0,0.00}", (formulas.mi[i])));
                item.SubItems.Add(String.Format("{0:0,0.00}", (formulas.mp[i])));
                item.SubItems.Add(String.Format("{0:0,0.00}", (formulas.mb[i])));
                listView1.Items.Add(item);

            }
            //Display the yearly data
            listView2.Items.Clear();
            t = formulas.tmi.Length;
            for (i = 0; i < t; i += 1)
            {
                item = new ListViewItem(String.Format("{0:0}", (i + 1)));
                item.SubItems.Add(String.Format("{0:0,0.00}", (formulas.tmi[i])));
                item.SubItems.Add(String.Format("{0:0,0.00}", (formulas.tmp[i])));
                principal -= formulas.tmp[i];
                item.SubItems.Add(String.Format("{0:0,0.00}", principal));
                listView2.Items.Add(item);
            }
        }

        //---------------------------------------------
        private void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
            if ((e.ItemIndex % 2) == 1)
            {
                //e.Item.BackColor = Color.FromArgb(230, 230, 255);
                e.Item.BackColor = Color.Azure;
                e.Item.UseItemStyleForSubItems = true;
            }
        }

        private void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            e.DrawText();
        }

        private void pseudoBtnClick()
        {
            // copy the values from the 6 text boxes into info.
            // if an error occurred don't continue with the Amortization.
            var rBtn = radioPanel.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked);
            formulas.info.SolveFor = int.Parse(rBtn.Tag.ToString());
            bool b = textBoxesToInfo();
            if (b)
            {
                formulas.generateAmortization();
                infoToTextBoxes();
                sendDataToListView();
            }
        }
        private void calcBtn_Click(object sender, EventArgs e)
        {
            pseudoBtnClick();
        }

    }//End of Form1
}
