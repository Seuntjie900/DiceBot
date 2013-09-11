using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Web;
using System.IO;
using System.Windows.Forms;

namespace DiceBot
{
    public partial class cLogin : Form
    {
        public string user = "";
        public string pass = "";
        public bool auto = false;
        public cLogin()
        {
            InitializeComponent();
            txtUser.Focus();
            loadsettings();
            txtPass.Text = pass;
            txtUser.Text = user;
            
        }

        void loadsettings()
        {
            if (File.Exists(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\Psettings"))
            {
                using (StreamReader sr = new StreamReader(Environment.GetEnvironmentVariable("APPDATA") + "\\DiceBot2\\Psettings"))
                {
                    try
                    {
                        string info = sr.ReadLine();
                        string[] chars = info.Split(' ');
                        string suser = "";
                        string spass = "";
                        bool login = false;
                        
                        int word = 0;
                        //NCPuser,ncppass,autologin
                        for (int i = 0; i < chars.Length; i++)
                        {
                            int num = 0;
                            if (int.TryParse(chars[i], out num))
                            {
                                if ((char)num == ',')
                                    word++;
                                else
                                switch (word)
                                {
                                    case 0: suser += (char)num; break;
                                    case 1: spass += (char)num; break;
                                    case 2: if ((char)num == '1') login = true; else login = false; break;
                                }
                            }
                        }
                       
                        user = suser;
                        pass = spass;
                        auto = login;
                    }
                    catch
                    {

                    }
                }
            }
        }

        void login()
        {
            try
            {
                if (Login("http://forum.netcodepool.org/forum/", txtUser.Text, txtPass.Text) == true)
                {
                    this.Hide();
                    new cDiceBot().Show();

                }
                else
                {
                    MessageBox.Show("Could Not log in.");
                    //Me.Close()
                }
            }
            catch
            {
                MessageBox.Show("Could not connect to NCP server");
            }
        }

        private void Button1_Click(System.Object sender, System.EventArgs e)
        {
            login();
            
        }

        
        string Login_ContentType = "application/x-www-form-urlencoded";
        public string Login_Benutzername;
        public string Login_Passwort;
        string Quelltext;
        public bool Login(string url, string Login_Benutzername, string Login_Passwort)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes("action=do_login&url=" + url + "index.php&quick_login=1&quick_username=" + Login_Benutzername + "&quick_password=" + Login_Passwort + "&submit=Login&quick_remember=yes");
            System.Net.WebRequest req = System.Net.HttpWebRequest.Create(url + "member.php");
            req.Method = ("POST");
            req.ContentType = Login_ContentType;
            req.ContentLength = data.Length;
            //req.UserAgent = Login_UserAgent;
            Stream dataStream = req.GetRequestStream();
            dataStream.Write(data, 0, data.Length);
            System.Net.WebResponse res = req.GetResponse();
            System.IO.Stream resStream = res.GetResponseStream();
            System.IO.StreamReader sr = new System.IO.StreamReader(resStream);
            Quelltext = (sr.ReadToEnd());

            if (Quelltext.Contains("You have successfully been logged in") | Quelltext.Contains("erfolgreich eingeloggt") == true)
            {
               
                sr.Close();
                string str = null;
                string[] str2 = null;
                string strValue = null;
                req = System.Net.HttpWebRequest.Create(url + "member.php?action=profile");
                foreach (string str_loopVariable in res.Headers)
                {
                    str = str_loopVariable;
                    str2 = res.Headers.GetValues(str);
                    if (str == "Set-Cookie")
                    {
                        strValue = res.Headers.Get(str);
                        strValue = strValue.Replace(",", "; ");
                        req.Headers.Add("Cookie", strValue);
                    }
                }
                res = req.GetResponse();
                resStream = res.GetResponseStream();
                sr = new System.IO.StreamReader(resStream);
                Quelltext = (sr.ReadToEnd());
                sr.Close();
                
                return true;
            }
            else
            {
                
                return (false);
            }
            
        }

        private void cLogin_Load(object sender, EventArgs e)
        {
           
        }

        private void cLogin_Paint(object sender, PaintEventArgs e)
        {
            if (auto)
                login();
        }
    }
}
