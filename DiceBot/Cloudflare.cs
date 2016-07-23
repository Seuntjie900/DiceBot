using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noesis.Javascript;
using System.Threading;
using System.Net.Http;
using System.Net;

namespace DiceBot
{
    class Cloudflare
    {
        public static bool doCFThing(string Response, HttpClient Client, HttpClientHandler ClientHandlr, int cflevel, string URI)
        {
            Thread.Sleep(4000);
            JavascriptContext JSC = new JavascriptContext();

            string s1 = Response;//new StreamReader(Response.GetResponseStream()).ReadToEnd();
            string Script = "";
            string jschl_vc = s1.Substring(s1.IndexOf("jschl_vc"));
            jschl_vc = jschl_vc.Substring(jschl_vc.IndexOf("value=\"") + "value=\"".Length);
            jschl_vc = jschl_vc.Substring(0, jschl_vc.IndexOf("\""));
            string pass = s1.Substring(s1.IndexOf("pass"));
            pass = pass.Substring(pass.IndexOf("value=\"") + "value=\"".Length);
            pass = pass.Substring(0, pass.IndexOf("\""));

            //do the CF bypass thing and get the headers
            Script = s1.Substring(s1.IndexOf("var s,t,o,p,b,r,e,a,k,i,n,g,f,") + "var s,t,o,p,b,r,e,a,k,i,n,g,f, ".Length);
            string Script1 = "var " + Script.Substring(0, Script.IndexOf(";") + 1);
            string varName = Script.Substring(0, Script.IndexOf("="));
            string varNamep2 = Script.Substring(Script.IndexOf("\"") + 1);
            varName += "." + varNamep2.Substring(0, varNamep2.IndexOf("\""));
            Script1 += Script.Substring(Script.IndexOf(varName));
            Script1 = Script1.Substring(0, Script1.IndexOf("f.submit()"));
            Script1 = Script1.Replace("t.length", URI.Length + "");
            Script1 = Script1.Replace("a.value", "var answer");
            JSC.Run(Script1);
            string answer = JSC.GetParameter("answer").ToString();

            try
            {
                HttpResponseMessage Resp = Client.GetAsync("cdn-cgi/l/chk_jschl?jschl_vc=" + jschl_vc + "&pass=" + pass.Replace("+", "%2B").Replace("-", "%2D") + "&jschl_answer=" + answer).Result;
                
                bool Found = false;

                foreach (Cookie c in ClientHandlr.CookieContainer.GetCookies(new Uri("https://"+URI)))
                {
                    if (c.Name == "cf_clearance")
                    {
                        Found = true;
                        break;
                    }
                }
                /*if (ClientHandlr.CookieContainer.Count==3)
                {
                    Thread.Sleep(2000);
                }*/
                if ((!Found /*|| Resp.StatusCode== HttpStatusCode.Forbidden*/ )&& cflevel++ < 5)
                {
                    Found = doCFThing(Resp.Content.ReadAsStringAsync().Result, Client, ClientHandlr, cflevel, URI);
                }
                else
                {
                    
                }
                return Found;

            }
            catch (AggregateException e)
            {
                /*Parent.DumpLog(e.InnerException.Message, 3);
                Parent.DumpLog(e.InnerException.StackTrace, 4);*/
            }
            return false;
        }
    }
}
