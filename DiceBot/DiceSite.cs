using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gecko;
using Gecko.DOM;
namespace DiceBot
{
    public abstract class DiceSite
    {
        public bool AutoWithdraw { get; set; }
        public bool AutoInvest { get; set; }
        public bool ChangeSeed {get;set;}
        public bool AutoLogin { get; set; }

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
    }

    public class PRC : DiceSite
    {
        public PRC()
        {
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = true;
            AutoLogin = false;
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
                return true;
            }
            else
                return false;
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
    public class PD : DiceSite
    {
        public PD()
        {
            AutoInvest = false;
            AutoWithdraw = false;
            ChangeSeed = false;
        }

        public override void PlaceBet(bool High, GeckoWebBrowser gckBrowser)
        {

            GeckoNodeCollection ctrls = gckBrowser.Document.GetElementsByClassName("btn btn--primary btn--huge btn--limited btn--block text--center");
            GeckoInputElement gie = null;
            foreach (GeckoNode gn in ctrls)
            {
                gie = new GeckoInputElement(gn.DomObject);
            }
            gie.Click();
            
        }

        public override void SetChance(string Chance, GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gieChance = new GeckoInputElement(gckBrowser.Document.GetElementById("ember2802").DomObject);
            gieChance.Value = Chance;
        }

        public override void SetAmount(double Amount, GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gieAmount = new GeckoInputElement(gckBrowser.Document.GetElementById("ember768").DomObject);
            gieAmount.Value = Amount.ToString("0.00000000");

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
            throw new NotImplementedException();
        }

        public override string GetSiteProfitValue(GeckoWebBrowser gckBrowser)
        {
            throw new NotImplementedException();
        }

        public override string GetTotalBets(GeckoWebBrowser gckBrowser)
        {
            throw new NotImplementedException();
        }

        public override string GetMyProfit(GeckoWebBrowser gckBrowser)
        {
            throw new NotImplementedException();
        }

        public override bool ReadyToBet(GeckoWebBrowser gckBrowser)
        {
            throw new NotImplementedException();
        }
    }

    public class D999 : DiceSite
    {
        public D999()
        {
            AutoInvest = false;
            AutoWithdraw = true;
            ChangeSeed = false;
            AutoLogin = true;
        }

        public override void PlaceBet(bool High, GeckoWebBrowser gckBrowser)
        {
            GeckoInputElement gie = new GeckoInputElement(gckBrowser.Document.GetElementById("BetLowButton").DomObject);
            gie.Click();
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
    }
}
