using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gecko;
using Gecko.DOM;
using System.Security.Cryptography;
using System.Globalization;
using System.Web;
using System.Net;
using System.IO;
namespace DiceBot
{
    public abstract class DiceSite
    {
        public bool AutoWithdraw { get; set; }
        public bool AutoInvest { get; set; }
        public bool ChangeSeed {get;set;}
        public bool AutoLogin { get; set; }
        public string Name { get; protected set; }
        public abstract void PlaceBet(bool High, Gecko.GeckoWebBrowser gckBrowser);
        public abstract void SetChance(string Chance, Gecko.GeckoWebBrowser gckBrowser);
        public abstract void SetAmount(double Amount, Gecko.GeckoWebBrowser gckBrowser);
        public abstract void ResetSeed(Gecko.GeckoWebBrowser gckBrowser);
        public abstract void SetClientSeed(string Seed, Gecko.GeckoWebBrowser gckBrowser);
        public virtual bool Invest(double Amount, int Counter, string secretURL, GeckoWebBrowser gckBrowser)
        {
            return true;

        }
        public virtual bool Withdraw(double Amount, string Address, int Counter, string secretURL, GeckoWebBrowser gckBrowser)
        {
            return true;
        }
        public virtual bool Login(string Username, string Password, GeckoWebBrowser gckBrowser)
        {
            return true;

        }
        public abstract string GetbalanceValue(Gecko.GeckoWebBrowser gckBrowser);
        public abstract string GetSiteProfitValue(Gecko.GeckoWebBrowser gckBrowser);
        public abstract string GetTotalBets(Gecko.GeckoWebBrowser gckBrowser);
        public abstract string GetMyProfit(Gecko.GeckoWebBrowser gckBrowser);
        public abstract bool ReadyToBet(Gecko.GeckoWebBrowser gckBrowser);
        public virtual double GetLucky(string server, string client, int nonce)
        {
            HMACSHA512 betgenerator = new HMACSHA512();
            
            int charstouse = 5;
            List<byte> serverb = new List<byte>();

            for (int i = 0; i < server.Length; i++)
            {
                serverb.Add(Convert.ToByte(server[i]));
            }

            betgenerator.Key = serverb.ToArray();

            List<byte> buffer = new List<byte>();
            string msg = /*nonce.ToString() + ":" + */client + ":" + nonce.ToString();
            foreach (char c in msg)
            {
                buffer.Add(Convert.ToByte(c));
            }
            
            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);


            for (int i = 0; i < hex.Length; i+=charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);
                
                double lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000;
            }
            return 0;
        }
    }

    public class PRC_old : DiceSite
    {
        double currentbet = 0;
        double lastbet = 0;
        DateTime dtlastbet = new DateTime();

        public PRC_old()
        {
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = true;
            AutoLogin = false;
            this.Name = "PRC";
            dtlastbet = DateTime.Now;
        }

        public override void PlaceBet(bool high, Gecko.GeckoWebBrowser gckBrowser)
        {
            if (high)
            {
                gckBrowser.Navigate("javascript:var cusid_ele = document.getElementsByClassName('btn btn-primary btn-large diceHighButton'); for (var i = 0; i < cusid_ele.length; ++i) { var item = cusid_ele[i];   item.click();}");

            }
            else
            {
                gckBrowser.Navigate("javascript:var cusid_ele = document.getElementsByClassName('btn btn-primary btn-large diceLoButton'); for (var i = 0; i < cusid_ele.length; ++i) { var item = cusid_ele[i];   item.click();}");
            }
            lastbet = currentbet;
            dtlastbet = DateTime.Now;
        }

        public override void SetChance(string Chance, Gecko.GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("diceChance").DomObject);
            gie.Value = Chance;
        }

        public override void SetAmount(double Amount, Gecko.GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gieBet = new GeckoInputElement(gckBrowser.Document.GetElementById("diceBetAmount").DomObject);
            gieBet.Value = Amount.ToString("0.00000000").Replace(',', '.');
            currentbet = Amount;
            if (lastbet == 0)
                lastbet = Amount;
        }

        public override void ResetSeed(Gecko.GeckoWebBrowser gckBrowser)
        {
            gckBrowser.Navigate("javascript:var cusid_ele = document.getElementsByClassName('btn btn-inverse newSeedButton'); for (var i = 0; i < cusid_ele.length; ++i) { var item = cusid_ele[i];   item.click();}");
        }

        public override void SetClientSeed(string Seed, Gecko.GeckoWebBrowser gckBrowser)
        {
            throw new NotImplementedException();
        }

        public override string GetbalanceValue(Gecko.GeckoWebBrowser gckBrowser)
        {
            GeckoNodeCollection gieBalanceitems = (gckBrowser.Document.GetElementsByClassName("myBalance"));
            GeckoInputElement gieBalance = null;
            foreach (GeckoNode node in gieBalanceitems)
            {
                gieBalance = new GeckoInputElement(node.DomObject);
            }
            string sBalance = gieBalance.InnerHtml;
            sBalance = sBalance.Substring(0, sBalance.IndexOf(" "));
            return sBalance;
        }

        public override string GetSiteProfitValue(Gecko.GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gieBalance = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("diceHouseProfit")[0].DomObject);
            string sBalance = gieBalance.InnerHtml.Replace(",", "");
            sBalance = sBalance.Substring(0, sBalance.Length - 2);
            return sBalance;
        }

        public override string GetTotalBets(Gecko.GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement giebets = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("myBtcBets")[0].DomObject);
            return giebets.InnerHtml.Replace(",", "");
        }

        public override string GetMyProfit(Gecko.GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gieprofit = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("myBtcProfit")[0].DomObject);
            return gieprofit.InnerHtml.Replace(",", "");
        }

        public override bool ReadyToBet(GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gieBet = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("btn btn-primary btn-large diceHighButton")[0].DomObject);
            if (!gieBet.GetAttribute("style").ToLower().Contains("none"))
            {
                System.Threading.Thread.Sleep(100);
                double millis = (DateTime.Now - dtlastbet).TotalMilliseconds;
                bool ready = false;
                if (lastbet < 0.00000010 || currentbet < 0.00000010)
                {
                    ready = millis > 1000;
                }
                else if (lastbet < 0.0000010 || currentbet < 0.0000010)
                {
                    ready = millis > 800;
                }
                else if (lastbet < 0.000010 || currentbet < 0.000010)
                {
                    ready = millis > 600;
                }
                else if (lastbet < 0.00010 || currentbet < 0.00010)
                {
                    ready = millis > 400;
                }
                else if (lastbet < 0.0010 || currentbet < 0.0010)
                {
                    ready = millis > 200;
                }
                else
                {
                    ready = true;
                }

                return ready;
                
            }
            else
                return false;
        }

        public override double GetLucky(string server, string client, int nonce)
        {
            server = nonce + ":" + server + ":" + nonce;
            client = nonce + ":" + client;
            return base.GetLucky(server, client, nonce);
        }
    }

    public class JD : DiceSite
    {
        public JD()
        {
            AutoInvest = true;
            AutoWithdraw = true;
            ChangeSeed = true;
            AutoLogin = true;
            Name = "JD";
        }
        public override void PlaceBet(bool High, GeckoWebBrowser gckBrowser)
        {
            if (High)
                gckBrowser.Navigate("javascript:clicked_action_bet_hi()");
            else
                gckBrowser.Navigate("javascript:clicked_action_bet_lo()");
        }

        public override void SetChance(string Chance, GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("pct_chance").DomObject);
            gie.Value = Chance;
        }

        public override void SetAmount(double Amount, GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gieBet = new GeckoInputElement(gckBrowser.Document.GetElementById("pct_bet").DomObject);
            gieBet.Value = Amount.ToString("0.00000000").Replace(',', '.');
        }

        public override void ResetSeed(GeckoWebBrowser gckBrowser)
        {
            gckBrowser.Navigate("javascript:clicked_action_random()");
        }

        public override void SetClientSeed(string Seed, GeckoWebBrowser gckBrowser)
        {
            throw new NotImplementedException();
        }

        public override string GetbalanceValue(GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gieBalance = new GeckoInputElement(gckBrowser.Document.GetElementById("pct_balance").DomObject);
            return gieBalance.Value;
        }

        public override string GetSiteProfitValue(GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gieBalance = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("sprofitraw")[0].DomObject);
            return gieBalance.InnerHtml;
                
        }

        public override string GetTotalBets(GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement giebets = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("bets")[0].DomObject);
            return giebets.InnerHtml.Replace(",", "");
        }

        public override string GetMyProfit(GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gieprofit = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("myprofit")[0].DomObject);
            return gieprofit.InnerHtml.Replace(",", "");
        }

        public override bool ReadyToBet(GeckoWebBrowser gckBrowser)
        {
            return true;
        }
        public override bool Invest(double Amount, int Counter, string SecretURL, GeckoWebBrowser gckBrowser)
        {
            if (Counter == 0)
            {
                GeckoInputElement gieAmount = new GeckoInputElement(gckBrowser.Document.GetElementById("invest_edit").DomObject);
                gieAmount.Click();

                //gckBrowser.Navigate("javascript:clicked_action_Invest();");
            }
            if (Counter == 11)
            {
                GeckoInputElement gieAmount = new GeckoInputElement(gckBrowser.Document.GetElementById("invest_input").DomObject);
                gieAmount.Value = Amount.ToString("0.00000000");

            }
            if (Counter == 20)
            {
                GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("invest_button").DomObject);
                gie.Click();

            }
            
            if (Counter == 40)
            {
                return true;
                
            }
            return false;
            
        }

        public override bool Withdraw(double Amount, string Address, int Counter, string SecretURL, GeckoWebBrowser gckBrowser)
        {
            if (Counter == 0)
            {
                gckBrowser.Navigate("javascript:clicked_action_withdraw();");
            }
            if (Counter == 11)
            {
                GeckoInputElement gieAddress = new GeckoInputElement(gckBrowser.Document.GetElementById("wd_address").DomObject);
                GeckoInputElement gieAmount = new GeckoInputElement(gckBrowser.Document.GetElementById("wd_amount").DomObject);
                gieAddress.Value = Address;
                gieAmount.Value = Amount.ToString("0.00000000");

            }
            if (Counter == 20)
            {
                GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("wd_button").DomObject);
                gie.Click();
                
            }

            if (Counter == 30)
            {
                GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("Close").DomObject);
                gie.Click();
                
            }
            if (Counter == 40)
            {
                return true;
            }
            return false;
            
        }

        public override bool Login(string Username, string Password, GeckoWebBrowser gckBrowser)
        {
            try
            {
                GeckoInputElement gieUser = new GeckoInputElement(gckBrowser.Document.GetElementById("username").DomObject);
                GeckoInputElement giePass = new GeckoInputElement(gckBrowser.Document.GetElementsByName("password")[0].DomObject);

                GeckoInputElement gieSubmit = null;
                foreach (GeckoElement gie in gckBrowser.Document.GetElementsByTagName("input"))
                {
                    if (gie.GetAttribute("type") == "submit")
                    {
                        gieSubmit = new GeckoInputElement(gie.DomObject);
                        break;
                    }
                }

                gieUser.Value = Username;
                giePass.Value = Password;
                gieSubmit.Click();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }


    //fuck primedice and their stupid ember shit!
   
    public class D999 : DiceSite
    {
        public D999()
        {
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = false;
            AutoLogin = true;
            Name = "D999";
        }

        public override void PlaceBet(bool High, GeckoWebBrowser gckBrowser)
        {
            if (!High)
            {
                GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("BetLowButton").DomObject);
                gie.Click();
            }
            else
            {
                GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("BetHighButton").DomObject);
                gie.Click();
            }
        }

        public override void SetChance(string Chance, GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("BetChanceInput").DomObject);
            gie.Value = Chance;
        }

        public override void SetAmount(double Amount, GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("BetSizeInput").DomObject);
            gie.Value = Amount.ToString("0.00000000").Replace(',', '.'); ;
        }

        public override void ResetSeed(GeckoWebBrowser gckBrowser)
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed, GeckoWebBrowser gckBrowser)
        {
            throw new NotImplementedException();
        }

        public override string GetbalanceValue(GeckoWebBrowser gckBrowser)
        {
            GeckoNodeCollection gieBalanceitems = (gckBrowser.Document.GetElementsByClassName("Numbers HighlightedText UserBalance"));
            GeckoInputElement gieBalance = null;
            foreach (GeckoNode node in gieBalanceitems)
            {
                gieBalance = new GeckoInputElement(node.DomObject);
            }
            string sBalance = gieBalance.InnerHtml;
            
            return sBalance;
        }

        public override string GetSiteProfitValue(GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gieSiteProfit = new GeckoInputElement(gckBrowser.Document.GetElementById("StatsSiteBetProfit").DomObject);
            return gieSiteProfit.InnerHtml;
        }

        public override string GetTotalBets(GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gieSiteProfit = new GeckoInputElement(gckBrowser.Document.GetElementById("StatsUserBetCount").DomObject);
            return gieSiteProfit.InnerHtml;
        }

        public override string GetMyProfit(GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gieSiteProfit = new GeckoInputElement(gckBrowser.Document.GetElementById("StatsUserBetProfit").DomObject);
            return gieSiteProfit.InnerHtml;
        }

        public override bool ReadyToBet(GeckoWebBrowser gckBrowser)
        {
            return true;
        }

        public override bool Withdraw(double Amount, string Address, int Counter, string SecretURL, GeckoWebBrowser gckBrowser)
        {
            if (Counter == 0)
            {
                GeckoNodeCollection gieBalanceitems = (gckBrowser.Document.GetElementsByClassName("TextButton WithdrawButton"));
                GeckoInputElement gieBalance = null;
                foreach (GeckoNode node in gieBalanceitems)
                {
                    gieBalance = new GeckoInputElement(node.DomObject);
                }
                gieBalance.Click();
            }
            if (Counter == 11)
            {
                GeckoInputElement gieAddress = new GeckoInputElement(gckBrowser.Document.GetElementById("WithdrawAddress").DomObject);
                GeckoInputElement gieAmount = new GeckoInputElement(gckBrowser.Document.GetElementById("WithdrawAmount").DomObject);
                gieAddress.Value = Address;
                gieAmount.Value = Amount.ToString("0.00000000");

            }
            if (Counter == 20)
            {
                GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("WithdrawValueButton").DomObject);
                gie.Click();

            }

            
            if (Counter == 40)
            {
                return true;
            }
            return false;


        }

        public override bool Login(string Username, string Password, GeckoWebBrowser gckBrowser)
        {
            try
            {
                GeckoInputElement gieUser = new GeckoInputElement(gckBrowser.Document.GetElementById("Username").DomObject);
                GeckoInputElement giePass = new GeckoInputElement(gckBrowser.Document.GetElementById("Password").DomObject);

                GeckoInputElement gieSubmit = null;
                foreach (GeckoElement gie in gckBrowser.Document.GetElementsByTagName("input"))
                {
                    if (gie.GetAttribute("type") == "submit")
                    {
                        gieSubmit = new GeckoInputElement(gie.DomObject);
                        break;
                    }
                }

                gieUser.Value = Username;
                giePass.Value = Password;
                gieSubmit.Click();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override double GetLucky(string server, string client, int nonce)
        {
            Func<string, byte[]> strtobytes = s => Enumerable
        .Range(0, s.Length / 2)
        .Select(x => byte.Parse(s.Substring(x * 2, 2), NumberStyles.HexNumber))
        .ToArray();
            byte[] bserver = strtobytes(server);
            byte[] bclient = strtobytes(client).Reverse().ToArray();
            byte[] num = BitConverter.GetBytes(nonce).Reverse().ToArray();
            byte[] data = bserver.Concat(bclient).Concat(num).ToArray();
            using (SHA512 sha512 = new SHA512Managed())
            {
               
                byte[] hash = sha512.ComputeHash(sha512.ComputeHash(data));
                while (true)
                {
                    for (int x = 0; x <= 61; x += 3)
                    {
                        long result = (hash[x] << 16) | (hash[x + 1] << 8) | hash[x + 2];
                        if (result < 16000000)
                            return result % 1000000;
                    }
                    hash = sha512.ComputeHash(hash);
                }
            }
        }
    }

    public class SafeDice: DiceSite
    {
        
        double lastamount = 0;
        string lastprofit = "";
        bool checklastprofit = false;

        public SafeDice():base()
        {
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = false;
            AutoLogin = false;
            Name = "SafeD";
        }
        public override void PlaceBet(bool High, GeckoWebBrowser gckBrowser)
        {
            try
            {
                GeckoInputElement gietmp = new GeckoInputElement(gckBrowser.Document.GetElementById("hiloswitch").DomObject);
                   
            }
            catch
            {
                
                new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("ng-switch ng-pristine ng-untouched ng-valid")[0].DomObject).Id="hiloswitch";
                
            }
            if (High)
            {
                try
                {
                    GeckoInputElement gieHigh = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("switch-on switch-animate")[0].DomObject);
                    gieHigh = new GeckoInputElement(gckBrowser.Document.GetElementById("hiloswitch").DomObject);
                    gieHigh.Click();

                }
                catch
                {

                }

            }
            else
            {
                try
                {
                    GeckoInputElement gieLow = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("switch-off switch-animate")[0].DomObject);
                    gieLow = new GeckoInputElement(gckBrowser.Document.GetElementById("hiloswitch").DomObject);
                    gieLow.Click();
                }
                catch
                {

                }
            }
            try
            {
                GeckoInputElement gie_test = new GeckoInputElement(gckBrowser.Document.GetElementById("btn-roll").DomObject);
            }
            catch
            {
                (new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("btn-circle btn-alt m-t-20")[0].DomObject)).Id = "btn-roll";
            }

            
            using (AutoJSContext Context = new AutoJSContext(gckBrowser.Window.JSContext))
            {
                string JSresult = "";
                Context.EvaluateScript("$(\"#btn-roll\").click();", (nsISupports)gckBrowser.Window.DomWindow, out JSresult);
            }
        }

        public override void SetChance(string Chance, GeckoWebBrowser gckBrowser)
        {
            try
            {
                GeckoInputElement gie_old = new GeckoInputElement(gckBrowser.Document.GetElementById("txtChance").DomObject);
            }
            catch
            {
                
                (new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("form-control ng-win-chance ng-pristine ng-untouched ng-valid ng-valid-required ng-valid-min-chance ng-valid-max-chance")[0].DomObject)).Id = "txtChance";
            }
            GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("txtChance").DomObject);
            using (AutoJSContext Context = new AutoJSContext(gckBrowser.Window.JSContext))
            {
                string JSresult = "";
                Context.EvaluateScript("$(\"#txtChance\").val('"+Chance+"').change();", (nsISupports)gckBrowser.Window.DomWindow, out JSresult);
            }
            
        }

        public override void SetAmount(double Amount, GeckoWebBrowser gckBrowser)
        {
            try
            {
                GeckoInputElement gie_tst = new GeckoInputElement(gckBrowser.Document.GetElementById("txtAmount").DomObject);
            }
            catch
            {
                (new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("form-control ng-bet-amount ng-pristine ng-untouched ng-valid ng-valid-min ng-valid-required ng-valid-max-balance")[0].DomObject)).Id = "txtAmount";
            }
            GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("txtAmount").DomObject);
            GeckoInputElement gieProfitOnWin = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("m-l-20")[0].DomObject);
            lastprofit = gieProfitOnWin.TextContent;
            if (Amount!= lastamount)
            {
                
                checklastprofit = true;
                lastamount = Amount;
                using (AutoJSContext Context = new AutoJSContext(gckBrowser.Window.JSContext))
                {
                    string JSresult = "";
                    Context.EvaluateScript("$(\"#txtAmount\").val('" + Amount.ToString("0.00000000") + "').change();", (nsISupports)gckBrowser.Window.DomWindow, out JSresult);
                }
            }
            else
            {
                checklastprofit = false;
            }
            
        }

        public override void ResetSeed(GeckoWebBrowser gckBrowser)
        {
            throw new NotImplementedException();
        }

        public override void SetClientSeed(string Seed, GeckoWebBrowser gckBrowser)
        {
            throw new NotImplementedException();
        }

        public override string GetbalanceValue(GeckoWebBrowser gckBrowser)
        {
            //m-t-5 m-b-0 ng-balance ng-isolate-scope
            try
            {
                GeckoInputElement gie_old = new GeckoInputElement(gckBrowser.Document.GetElementById("txtBalance").DomObject);
            }
            catch
            {
                new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("m-t-5 m-b-0 ng-balance ng-isolate-scope")[0].DomObject).Id = "lblBalance";
            }
            GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("lblBalance").DomObject);
            return gie.TextContent;
            
        }

        public override string GetSiteProfitValue(GeckoWebBrowser gckBrowser)
        {
            GeckoNodeCollection nodes = gckBrowser.Document.GetElementsByClassName("nav navbar-text");
            foreach (GeckoNode Node in nodes)
            {
                if (Node.TextContent.ToLower().StartsWith("profit"))
                {
                    return Node.TextContent.Substring(Node.TextContent.IndexOf(":" + 2));
                }
            }
            throw new IndexOutOfRangeException();
        }

        public override string GetTotalBets(GeckoWebBrowser gckBrowser)
        {
            
            
            GeckoNodeCollection nodes = gckBrowser.Document.GetElementsByClassName("media");
            if (nodes.Length<5)
            {
                try
                {
                    GeckoNodeCollection tmpnodes = gckBrowser.Document.GetElementsByClassName("btn btn-sm btn-alt");
                    foreach (GeckoNode tmpNode in tmpnodes)
                    {
                        if (tmpNode.TextContent=="")
                        {
                            new GeckoInputElement(tmpNode.DomObject).Click();
                            GeckoNode tmpNode2 = gckBrowser.Document.GetElementsByClassName("tab-pane ng-scope")[0];
                            GeckoDivElement gie = new GeckoDivElement(tmpNode2.DomObject);
                            gie.SetAttribute("id","pnlBets");
                            using (AutoJSContext Context = new AutoJSContext(gckBrowser.Window.JSContext))
                            {
                                string JSresult = "";
                                Context.EvaluateScript("$(\"#pnlBets\").addClass(\"active\")", (nsISupports)gckBrowser.Window.DomWindow, out JSresult);
                            }
                            break;
                        }
                    }
                }
                catch
                {

                }
            }
            nodes = gckBrowser.Document.GetElementsByClassName("media");
            foreach (GeckoNode Node in nodes)
            {
                if (Node.TextContent.ToLower().Contains("bets"))
                {
                    return Node.ChildNodes[0].TextContent.Substring(0, Node.TextContent.IndexOf("B"));
                }
            }
            throw new IndexOutOfRangeException();
        }

        public override string GetMyProfit(GeckoWebBrowser gckBrowser)
        {
            GeckoNodeCollection nodes = gckBrowser.Document.GetElementsByClassName("media");
            foreach (GeckoNode Node in nodes)
            {
                if (Node.TextContent.ToLower().Contains("profit"))
                {
                    return Node.ChildNodes[0].TextContent.Substring(0, Node.TextContent.IndexOf("P"));
                }
            }
            throw new IndexOutOfRangeException();
        }

        public override bool Withdraw(double Amount, string Address, int Counter, string secretURL, GeckoWebBrowser gckBrowser)
        {
            if (Counter==0)
            { 
                GeckoNodeCollection tmpnodes = gckBrowser.Document.GetElementsByClassName("btn btn-sm btn-alt");
                foreach (GeckoNode tmpNode in tmpnodes)
                {
                    if (tmpNode.TextContent == "WITHDRAW")
                    {
                        new GeckoInputElement(tmpNode.DomObject).Click();
                    }
                }
            }

            if (Counter==5)
            {
                GeckoInputElement gieAddress = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("form-control ng-pristine ng-invalid ng-invalid-required ng-valid-maxlength ng-untouched")[0].DomObject);
                gieAddress.Value = Address;
                gieAddress.Id = "txtWDAddress";
                using (AutoJSContext Context = new AutoJSContext(gckBrowser.Window.JSContext))
                {
                    string JSresult = "";
                    Context.EvaluateScript("$(\"#txtWDAddress\").change()", (nsISupports)gckBrowser.Window.DomWindow, out JSresult);
                }
                
                
            }
            if (Counter==15)
            {
                GeckoInputElement gieAmount = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("form-control ng-pristine ng-valid ng-valid-min ng-valid-required ng-untouched")[0].DomObject);
                gieAmount.Value = Amount.ToString("0.00000000");
                gieAmount.Id =
                    "txtWDAmount";
                using (AutoJSContext Context = new AutoJSContext(gckBrowser.Window.JSContext))
                {
                    string JSresult = "";
                    Context.EvaluateScript("$(\"#txtWDAmount\").change()", (nsISupports)gckBrowser.Window.DomWindow, out JSresult);
                }
            }
            if (Counter==25)
            {
                GeckoNodeCollection nodes = gckBrowser.Document.GetElementsByClassName("btn btn-alt");
                foreach (GeckoNode node in nodes)
                {
                    try
                    {
                        GeckoInputElement gietmp = new GeckoInputElement(node.DomObject);
                        if (gietmp.Value == "Withdraw")
                        {
                            new GeckoInputElement(node.DomObject).Click();
                        }
                    }
                    catch
                    {

                    }
                }

                
            }
            if (Counter==35)
            {
                GeckoNodeCollection tmpnodes = gckBrowser.Document.GetElementsByClassName("btn btn-sm btn-alt");
                foreach (GeckoNode tmpNode in tmpnodes)
                {
                    if (tmpNode.TextContent == "")
                    {
                        new GeckoInputElement(tmpNode.DomObject).Click();
                    }
                }
            }
            if (Counter==45)
            {
                return true;
            }
            return false;
        }

        public override bool ReadyToBet(GeckoWebBrowser gckBrowser)
        {
            try
            {
                try
                { 
                    
                    (new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("btn-circle btn-alt m-t-20")[0].DomObject)).Id = "btnRoll";
                    
                    if (checklastprofit)
                    {
                        GeckoInputElement gieProfitOnWin = new GeckoInputElement(gckBrowser.Document.GetElementsByClassName("m-l-20")[0].DomObject);
                        string txt = gieProfitOnWin.TextContent;
                        if (txt!=lastprofit)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                    catch
                {

                }
            }
                
            
            catch
            {

            }
            return false;
        }

        public override double GetLucky(string server, string client, int nonce)
        {
            string comb = nonce + ":" + client + server + ":" + nonce;

            SHA512 betgenerator = SHA512.Create();
            

            int charstouse = 5;
            
            List<byte> buffer = new List<byte>();

            foreach (char c in comb)
            {
                buffer.Add(Convert.ToByte(c));
            }

            //compute first hash
            byte[] hash = betgenerator.ComputeHash(buffer.ToArray());

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);

            comb = hex.ToString();
            buffer = new List<byte>();

            //convert hash to new byte array
            foreach (char c in comb)
            {
                buffer.Add(Convert.ToByte(c));
            }

            hash = betgenerator.ComputeHash(buffer.ToArray());

            hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);

            for (int i = 0; i < hex.Length; i += charstouse)
            {

                string s = hex.ToString().Substring(i, charstouse);

                double lucky = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                if (lucky < 1000000)
                    return lucky / 10000;
            }
            return 0;
        }
    }

    public class PRC : DiceSite
    {
        double currentbet = 0;
        double lastbet = 0;
        DateTime dtlastbet = new DateTime();
        string lastablance = "";
        public PRC()
            : base()
        {
            AutoInvest = false;
            AutoWithdraw = false;
            AutoLogin = false;
            ChangeSeed = false;
            Name = "prc2";
            dtlastbet = DateTime.Now;
        }

        public override void PlaceBet(bool High, GeckoWebBrowser gckBrowser)
        {
            if (High)
            {
                GeckoInputElement gieBetHigh = new GeckoInputElement(gckBrowser.Document.GetElementById("betHiButton").DomObject);
                gieBetHigh.Click();

            }
            else
            {
                GeckoInputElement gieBetHigh = new GeckoInputElement(gckBrowser.Document.GetElementById("betLoButton").DomObject);
                gieBetHigh.Click();
            }
            lastbet = currentbet;
            
        }

        public override void SetChance(string Chance, Gecko.GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("betChance").DomObject);
            gie.Value = Chance;
            using (AutoJSContext Context = new AutoJSContext(gckBrowser.Window.JSContext))
            {
                string JSresult = "";
                Context.EvaluateScript("$(\"#betChance\").change();", (nsISupports)gckBrowser.Window.DomWindow, out JSresult);
            }
        }

        public override void SetAmount(double Amount, Gecko.GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gieBet = new GeckoInputElement(gckBrowser.Document.GetElementById("betAmount").DomObject);
            gieBet.Value = Amount.ToString("0.00000000").Replace(',', '.');
            currentbet = Amount;
            if (lastbet == 0)
                lastbet = currentbet;
            using (AutoJSContext Context = new AutoJSContext(gckBrowser.Window.JSContext))
            {
                string JSresult = "";
                Context.EvaluateScript("$(\"#betAmount\").change();", (nsISupports)gckBrowser.Window.DomWindow, out JSresult);
            }
        }

        public override void ResetSeed(GeckoWebBrowser gckBrowser)
        {
            GeckoNodeCollection btns = gckBrowser.Document.GetElementsByClassName("btn btn-default btn-sm btn-block");
            foreach (GeckoNode gn in btns)
            {
                GeckoInputElement gieSeed = new GeckoInputElement(gn.DomObject);
                if (gieSeed.TextContent.ToLower() == "generate new server seed")
                {
                    gieSeed.Click();

                }

            }

        }

        public override void SetClientSeed(string Seed, GeckoWebBrowser gckBrowser)
        {
            throw new NotImplementedException();
        }

        public override string GetbalanceValue(GeckoWebBrowser gckBrowser)
        {
            GeckoDivElement gieBalance = new GeckoDivElement(gckBrowser.Document.GetElementById("userBalance").DomObject);

            string sBalance = gieBalance.InnerHtml;
            
            if (lastablance != sBalance)
            {
                lastablance = sBalance;
                dtlastbet = DateTime.Now;
            }

            return sBalance;
        }

        public override string GetSiteProfitValue(GeckoWebBrowser gckBrowser)
        {
            try
            {
                GeckoDivElement gieBalance = new GeckoDivElement(gckBrowser.Document.GetElementById("siteProfit").DomObject);

                string sBalance = gieBalance.InnerHtml;
                
                return sBalance;
            }
            catch
            {
                return "";
            }
        }

        public override string GetTotalBets(GeckoWebBrowser gckBrowser)
        {
            GeckoDivElement gieBalance = new GeckoDivElement(gckBrowser.Document.GetElementById("myTotalBets").DomObject);

            string sBalance = gieBalance.InnerHtml;
            
            return sBalance;
        }

        public override string GetMyProfit(GeckoWebBrowser gckBrowser)
        {
            GeckoDivElement gieBalance = new GeckoDivElement(gckBrowser.Document.GetElementById("myProfit").DomObject);

            string sBalance = gieBalance.InnerHtml;
            
            return sBalance;
        }

        public override bool ReadyToBet(GeckoWebBrowser gckBrowser)
        {
            double millis = (DateTime.Now - dtlastbet).TotalMilliseconds;
            bool ready = true;

            if (lastbet <= 0.0010 || currentbet <= 0.0010)
            {
                ready = millis > 200;
            } 
            if (lastbet <= 0.00010 || currentbet <= 0.00010)
            {
                ready = millis > 400;
            }
            if (lastbet <= 0.000010 || currentbet <= 0.000010)
            {
                ready = millis > 600;
            }
            
            if (lastbet <= 0.0000010 || currentbet <= 0.0000010)
            {
                ready = millis > 800;
            }
            
            if (lastbet <= 0.00000010 || currentbet <= 0.00000010)
            {
                ready = millis > 1000;
            }
            
            

            return ready;
        }

        public override double GetLucky(string server, string client, int nonce)
        {
            server = nonce + ":" + server + ":" + nonce;
            client = nonce + ":" + client;
            return base.GetLucky(server, client, nonce);
        }
    }

}
