using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Http;
namespace DiceBot
{
    public partial class SimpleSwap : Form
    {
        Uri baseAddress = new Uri("http://www.simpleswap.me/api/");
        HttpClient Client;
        ExchangeType Type;
        public delegate void dWithdraw(string Address, decimal Amount);
        public event dWithdraw Withdraw;
        string[] Currencies = new string[] { "btc","doge", "clam", "dash", "ltc" };

        public SimpleSwap()
        {
            InitializeComponent();
            GetCurrencies();
            Client = new HttpClient { BaseAddress = baseAddress };
        }

        void GetCurrencies()
        {
            Currencies = new string[] { "btc", "doge", "clam", "dash", "ltc" }; ;
            cbTo.Items.Clear();
            cbFrom.Items.Clear();
            foreach(string s in Currencies)
            {
                cbFrom.Items.Add(s);
                cbTo.Items.Add(s);
            }
        }

        public SimpleSwap(ExchangeType Type, string LockedCurrency)
        {
            InitializeComponent();
            GetCurrencies();
            this.Type = Type;
            Client = new HttpClient { BaseAddress = baseAddress };
            if (Type == ExchangeType.deposit)
            {
                cbTo.Enabled = false;
                cbTo.SelectedIndex = Array.IndexOf(Currencies, LockedCurrency);
                if (cbTo.SelectedIndex==-1)
                {
                    MessageBox.Show("Oops! It seems we can't swap to the currency you want!");
                    Type = ExchangeType.free;
                    cbTo.Enabled = true;
                }
            }
            else if (Type == ExchangeType.withdraw)
            {
                cbFrom.Enabled = false;
                cbFrom.SelectedIndex = Array.IndexOf(Currencies, LockedCurrency);
                if (cbFrom.SelectedIndex == -1)
                {
                    MessageBox.Show("Oops! It seems we can't swap from the currency you want!");
                    Type = ExchangeType.free;
                    cbFrom.Enabled = true;
                }
            }
        }
        int GetIndex(string Currency)
        {
            switch (Currency.ToLower())
            {
                case "btc": return 0;
                case "doge": return 1;
                case "clam": return 2;
                case "ltc": return 3;
                case "dash": return 4;
                default: return 0;
            }
        }
        public SimpleSwap(ExchangeType Type, string LockedCurrency, string Deposit)
        {
            InitializeComponent();
            GetCurrencies();
            Client = new HttpClient { BaseAddress = baseAddress };
            if (Type == ExchangeType.deposit)
            {
                cbTo.Enabled = false;
                
                txtReceiving.Text = Deposit;
                txtReceiving.ReadOnly = true;
                cbTo.SelectedIndex = Array.IndexOf(Currencies, LockedCurrency);
                if (cbTo.SelectedIndex == -1)
                {
                    MessageBox.Show("Oops! It seems we can't swap to the currency you want!");
                    Type = ExchangeType.free;
                    cbTo.Enabled = true;
                    txtReceiving.ReadOnly = false;
                    txtReceiving.Text = "";
                }
            }
            else if (Type == ExchangeType.withdraw)
            {
                cbFrom.Enabled = false;
                cbFrom.SelectedIndex = Array.IndexOf(Currencies, LockedCurrency);
                if (cbFrom.SelectedIndex == -1)
                {
                    MessageBox.Show("Oops! It seems we can't swap from the currency you want!");
                    Type = ExchangeType.free;
                    cbFrom.Enabled = true;
                }
            }
        }


        private void btnProgress_Click(object sender, EventArgs e)
        {
            if (last_swap!="")
            System.Diagnostics.Process.Start("http://www.simpleswap.me/exchange/" + last_swap);
        }

        bool GetRates()
        {
            try
            {
                using (var response = Client.GetStringAsync("rate/" + cbFrom.Items[cbFrom.SelectedIndex].ToString() + "-" + cbTo.Items[cbTo.SelectedIndex].ToString()))
                {
                    string responseData = response.Result;
                    SSRate Rate = json.JsonDeserialize<SSRate>(responseData);
                    if (!Rate.status)
                    {
                        MessageBox.Show("Failed to get rates! Please try again in a few minutes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        txtRate.Text = Rate.rate;
                        return true;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Failed to get rates! Please try again in a few minutes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        bool GetQuote()
        {
            try
            {
                using (var response = Client.GetStringAsync("quote/" + cbFrom.Items[cbFrom.SelectedIndex].ToString() + "-" + cbTo.Items[cbTo.SelectedIndex].ToString() + "/"+nudSending.Value))
                {
                    string responseData = response.Result;
                    SSRate Rate = json.JsonDeserialize<SSRate>(responseData);
                    if (!Rate.status)
                    {
                        MessageBox.Show("Failed to get quote! Please try again in a few minutes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        txtRate.Text = Rate.rate;
                        decimal amount = decimal.Parse(Rate.rate, System.Globalization.NumberFormatInfo.InvariantInfo)* (decimal)nudSending.Value;
                        txtReveice.Text = amount.ToString();
                        return true;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Failed to get quote! Please try again in a few minutes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private void btnExchange_Click(object sender, EventArgs e)
        {
            
            if (!CheckedRate)
            {
                if (GetQuote())
                {
                    DialogResult dr = MessageBox.Show(string.Format("You will get {0} {3} for your {2} {1}", txtReveice.Text, cbFrom.Items[cbFrom.SelectedIndex].ToString(), nudSending.Value, cbTo.Items[cbTo.SelectedIndex].ToString()), "Confirm", MessageBoxButtons.YesNo);
                    CheckedRate = dr == DialogResult.Yes;
                }
                
            }
            if (CheckedRate)
            {
                if (!Swap())
                {
                    MessageBox.Show("Failed exchange");
                }
            }

        }

        string last_swap = "";
        bool Swap()
        {
            try
            {
                string _Post = string.Format("pair={0}-{1}&address={2}&amount={3}&apikey={4}", cbFrom.Items[cbFrom.SelectedIndex].ToString(), cbTo.Items[cbTo.SelectedIndex].ToString(), txtReceiving.Text, nudSending.Value, "8368efc68d72206949bbb43e0f96fc1f26baf47b3a1f49f2");
                StringContent Post = new StringContent(_Post);
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("pair", string.Format("{0}-{1}", cbFrom.Items[cbFrom.SelectedIndex].ToString(), cbTo.Items[cbTo.SelectedIndex].ToString())));
                pairs.Add(new KeyValuePair<string, string>("address", txtReceiving.Text));
                pairs.Add(new KeyValuePair<string, string>("amount", nudSending.Value.ToString()));
                pairs.Add(new KeyValuePair<string, string>("apikey", "7ff5666f5b869cfa407ea69bc8c64ef1e9efb709b8cbb32e34"));
                FormUrlEncodedContent Content = new FormUrlEncodedContent(pairs);
                using (var response = Client.PostAsync("swap",Content))
                {
                    string responseData = response.Result.Content.ReadAsStringAsync().Result;
                    ssSwap Rate = json.JsonDeserialize<ssSwap>(responseData);
                    if (!Rate.status)
                    {
                        MessageBox.Show("Failed to get quote! Please try again in a few minutes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        txtDeposit.Text = Rate.deposit_address;
                        last_swap = Rate.swap_id;
                        panel1.Visible = true;
                        if (Type == ExchangeType.withdraw)
                        {
                            if (Withdraw!=null)
                            { Withdraw(Rate.deposit_address, (decimal)nudSending.Value); }
                        }
                        //System.Diagnostics.Process.Start("http://www.simpleswap.me/swap/"+Rate.swap_id);
                        return true;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Failed to get quote! Please try again in a few minutes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        bool CheckedRate = false;
        private void cbTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckedRate = false;
            if (cbTo.SelectedIndex == cbFrom.SelectedIndex)
            {
                cbTo.SelectedIndex = -1;
            }
        }

        private void cbFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckedRate = false;
            if (cbTo.SelectedIndex == cbFrom.SelectedIndex)
            {
                cbFrom.SelectedIndex = -1;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetRates();            
        }

        private void btnQuote_Click(object sender, EventArgs e)
        {
            CheckedRate = GetQuote();
        }

        private void nudSending_ValueChanged(object sender, EventArgs e)
        {

        }
    }
    public enum ExchangeType
    {
        deposit,withdraw,free
    }
    public class SSRate
    {
        public bool status { get; set; }
        public string pair { get; set; }
        public string rate { get; set; }
    }
    public class ssSwap
    {
        public bool status { get; set; }
        public string swap_id { get; set; }
        public string deposit_address { get; set; }
        
        public string error { get; set; }
    }

    public class ssSwapStatus
    {
        public bool status { get; set; }
        public string swap_id { get; set; }
        public string pair { get; set; }
        public string depost_address { get; set; }
        public decimal depost_amount { get; set; }
        public decimal exchanged_amount { get; set; }
        public string send_to_address { get; set; }
        public string send_trx { get; set; }
        public string swap_status { get; set; }
        public string error { get; set; }
    }
}
