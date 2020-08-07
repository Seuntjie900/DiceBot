using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using Noesis.Javascript;

namespace DiceBot.Common
{
    public class Cloudflare
    {
        public static bool doCFThing(string Response, HttpClient Client, HttpClientHandler ClientHandlr, int cflevel, string URI)
        {
            Thread.Sleep(4000);
            var JSC = new JavascriptContext();

            var s1 = Response; //new StreamReader(Response.GetResponseStream()).ReadToEnd();
            var Script = "";
            var jschl_tk = s1.Substring(s1.IndexOf("cf_chl_jschl_tk__=") + "cf_chl_jschl_tk__=".Length);
            jschl_tk = jschl_tk.Substring(0, jschl_tk.IndexOf("\""));
            var r = s1.Substring(s1.IndexOf("name=\"r\" value=\"") + "name=\"r\" value=\"".Length);
            r = r.Substring(0, r.IndexOf("\""));
            var jschl_vc = s1.Substring(s1.IndexOf("jschl_vc"));

            jschl_vc = jschl_vc.Substring(jschl_vc.IndexOf("value=\"") + "value=\"".Length);
            jschl_vc = jschl_vc.Substring(0, jschl_vc.IndexOf("\""));
            var pass = s1.Substring(s1.IndexOf("\"pass\""));
            pass = pass.Substring(pass.IndexOf("value=\"") + "value=\"".Length);
            pass = pass.Substring(0, pass.IndexOf("\""));

            //do the CF bypass thing and get the headers
            Script = s1.Substring(s1.IndexOf("var s,t,o,p,b,r,e,a,k,i,n,g,f,"));
            var Script1 = Script.Substring(0, Script.IndexOf(";") + 1);
            Script = Script.Substring("var s,t,o,p,b,r,e,a,k,i,n,g,f, ".Length);
            var varName = Script.Substring(0, Script.IndexOf("="));
            var varNamep2 = Script.Substring(Script.IndexOf("\"") + 1);
            var Script3 = Script.Substring(Script.IndexOf(";") + 1);
            Script3 = Script3.Substring(0, Script3.IndexOf("t = document"));
            Script1 += Script3;
            var kkkk = Script.Substring(Script.IndexOf("k = '") + "k = '".Length);
            kkkk = kkkk.Substring(0, kkkk.IndexOf("';"));
            var kvalue = s1.Substring(s1.IndexOf($"id=\"{kkkk}\">") + $"id=\"{kkkk}\">".Length);
            kvalue = "var innr = " + kvalue.Substring(0, kvalue.IndexOf("<")) + ";";
            Script1 += kvalue;
            varName += "." + varNamep2.Substring(0, varNamep2.IndexOf("\""));
            Script1 += Script.Substring(Script.IndexOf(varName));
            Script1 = Script1.Substring(0, Script1.IndexOf("f.submit()"));
            Script1 = Script1.Replace("t.length", URI.Length + "");
            Script1 = Script1.Replace("a.value", "var answer");

            if (Script1.Contains("f.action += location.hash;") || Script1.Contains("f.action+=location.hash;"))
                Script1 = Script1.Replace("f.action += location.hash;", "").Replace("f.action+=location.hash;", "");

            Script1 =
                Script1.Replace("(function(p){return eval((true+\"\")[0]+\".ch\"+(false+\"\")[1]+(true+\"\")[1]+Function(\"return escape\")()((\"\")[\"italics\"]())[2]+\"o\"+(undefined+\"\")[2]+(true+\"\")[3]+\"A\"+(true+\"\")[0]+\"(\"+p+\")\")}",
                                $"(function(p){{ return eval(\"{URI}\".charCodeAt(p)) }}");

            Script1 =
                Script1.Replace("function(p){var p = eval(eval(e(\"ZG9jdW1l\")+(undefined+\"\")[1]+(true+\"\")[0]+(+(+!+[]+[+!+[]]+(!![]+[])[!+[]+!+[]+!+[]]+[!+[]+!+[]]+[+[]])+[])[+!+[]]+g(103)+(true+\"\")[3]+(true+\"\")[0]+\"Element\"+g(66)+(NaN+[Infinity])[10]+\"Id(\"+g(107)+\").\"+e(\"aW5uZXJIVE1M\"))); return +(p)}();",
                                "function(p){var p = eval(eval(innr));return +(p)}();");

            JSC.Run(Script1);
            var answer = JSC.GetParameter("answer").ToString();

            try
            {
                var pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("r", r));
                pairs.Add(new KeyValuePair<string, string>("jschl_vc", jschl_vc));
                pairs.Add(new KeyValuePair<string, string>("pass", pass));
                pairs.Add(new KeyValuePair<string, string>("jschl_answer", answer));
                var Content = new FormUrlEncodedContent(pairs);
                var url = $"https://{URI}/?__cf_chl_jschl_tk__={jschl_tk}";
                Content.Headers.Add("Origin", "https://wolf.bet");

                //Content.Headers.Add("Referer", "https://wolf.bet/");
                Content.Headers.Add("sec-fetch-mode", "navigate");
                Content.Headers.Add("sec-fetch-site", "same-origin");

                //Content.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                Content.Headers.Remove("content-type");
                Content.Headers.Add("content-type", "application/x-www-form-urlencoded");
                var Resp = Client.PostAsync(url, Content).Result;

                var Found = false;

                foreach (Cookie c in ClientHandlr.CookieContainer.GetCookies(new Uri("https://" + URI)))
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
                if (!Found && cflevel++ < 5)
                {
                    Found = doCFThing(Resp.Content.ReadAsStringAsync().Result, Client, ClientHandlr, cflevel, URI);
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
