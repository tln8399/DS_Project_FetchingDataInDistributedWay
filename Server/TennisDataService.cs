using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
//using System.Threading.Tasks;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Global;

namespace Server
{
    class TennisDataService
    {
        public static Dictionary<string, int> attributes = new Dictionary<string, int>();

        public static void Main(string[] args)
        {
            IPAddress[] ips = Dns.GetHostAddresses("192.168.1.108");
            TcpListener listner = new TcpListener( ips[0],1313);
            listner.Start();
            Console.WriteLine(  "Server started.. Listening for requests" );
            TcpClient client = listner.AcceptTcpClient();
           NetworkStream nwStream = client.GetStream();
            StreamReader sReader = new StreamReader(nwStream);
            StreamWriter sWriter = new StreamWriter(nwStream);
            String request = sReader.ReadLine();
            Console.WriteLine("A client connected. Message from client:" + request);
            //Console.ReadKey();
            while (true)
            {

                IFormatter formatter = new BinaryFormatter();
                URL url = (URL)formatter.Deserialize(nwStream);
                
                //URL 
                if (url != null)
                {
                    Console.WriteLine("" + url.Url);
                    //Server has got the url
                    Output result = fetchATPdata(url);
                    if (result != null)
                    {
                        // send the data to client
                        formatter.Serialize(nwStream, result);
                    }

                }

                
                
            }
        }

        public static Output fetchATPdata(URL url)
        {
            try
            {
                var webGet = new HtmlWeb();
                Output output = new Output();
                output.tournament = url.TournamentName;
                output.gender = url.Gender;
                var doc = webGet.Load(url.Url);

                if (doc != null)
                {
                    HtmlNode summary = doc.DocumentNode.SelectSingleNode("//div[@id='head2HeadMain']");
                    if (summary != null)
                    {
                        HtmlNodeCollection label = summary.SelectNodes("//table[@id='head2HeadMainTable']//tr[@class='labelRow']//td");
                        HtmlNodeCollection info = summary.SelectNodes("//table[@id='head2HeadMainTable']//tr[@class='infoRow']//td");

                        using (var e1 = label.ToList().GetEnumerator())
                        using (var e2 = info.ToList().GetEnumerator())
                        {
                            string SSP1, SSP2 = "";
                            string player1, player2 = "";
                            while (e1.MoveNext() && e2.MoveNext())
                            {
                                switch (e1.Current.InnerText)
                                {
                                    case "Aces":
                                        {
                                            output.attributes["ACE1"] = Convert.ToInt16(e2.Current.InnerText);
                                            e2.MoveNext();
                                            output.attributes["ACE2"] = Convert.ToInt16(e2.Current.InnerText);
                                            break;
                                        }
                                    case "Double Faults":
                                        {
                                            output.attributes["DBF1"] = Convert.ToInt16(e2.Current.InnerText);
                                            e2.MoveNext();
                                            output.attributes["DBF2"] = Convert.ToInt16(e2.Current.InnerText);
                                            break;
                                        }
                                    case "Total points won":
                                        {
                                            output.attributes["TPW1"] = Convert.ToInt16(e2.Current.InnerText);
                                            e2.MoveNext();
                                            output.attributes["TPW2"] = Convert.ToInt16(e2.Current.InnerText);
                                            break;
                                        }
                                    case "Players":
                                        {
                                            output.Player1 = e2.Current.InnerText;
                                            e2.MoveNext();
                                            output.Player2 = e2.Current.InnerText;

                                            e2.MoveNext(); //e2.MoveNext();
                                            break;
                                        }
                                    case "1st Serve":
                                        {
                                            player1 = e2.Current.InnerText;
                                            e2.MoveNext();
                                            player2 = e2.Current.InnerText;
                                            int start1 = player1.IndexOf('%');
                                            int start2 = player2.IndexOf('%');
                                            player1 = player1.Substring(0, start1);
                                            player2 = player2.Substring(0, start2);
                                            SSP1 = (100 - Convert.ToInt16(player1)).ToString();
                                            SSP2 = (100 - Convert.ToInt16(player2)).ToString();

                                            output.attributes["FSP1"] = Convert.ToInt16(player1);
                                            output.attributes["FSP2"] = Convert.ToInt16(player2);
                                            output.attributes["SSP1"] = Convert.ToInt16(SSP1);
                                            output.attributes["SSP2"] = Convert.ToInt16(SSP2);
                                            break;
                                        }
                                    case "1st Serve Points Won":
                                        {
                                            player1 = e2.Current.InnerText;
                                            e2.MoveNext();
                                            player2 = e2.Current.InnerText;
                                            int start1 = player1.IndexOf('(') + 1;
                                            int start2 = player2.IndexOf('(') + 1;

                                            int end1 = player1.Length - (player1.Length - player1.IndexOf('/')) - start1;
                                            int end2 = player2.Length - (player2.Length - player2.IndexOf('/')) - start2;

                                            output.attributes["FSW1"] = Convert.ToInt16(player1.Substring(start1, end1));
                                            output.attributes["FSW2"] = Convert.ToInt16(player2.Substring(start2, end2));
                                            break;
                                        }
                                    case "2nd Serve Points Won":
                                        {
                                            player1 = e2.Current.InnerText;
                                            e2.MoveNext();
                                            player2 = e2.Current.InnerText;
                                            int start1 = player1.IndexOf('(') + 1;
                                            int start2 = player2.IndexOf('(') + 1;

                                            int end1 = player1.Length - (player1.Length - player1.IndexOf('/')) - start1;
                                            int end2 = player2.Length - (player2.Length - player2.IndexOf('/')) - start2;

                                            output.attributes["SSW1"] = Convert.ToInt16(player1.Substring(start1, end1));
                                            output.attributes["SSW2"] = Convert.ToInt16(player2.Substring(start2, end2));
                                            break;
                                        }
                                    case "Break Points Saved":
                                        {
                                            player1 = e2.Current.InnerText;
                                            e2.MoveNext();
                                            player2 = e2.Current.InnerText;
                                            int start1 = player1.IndexOf('/') + 1;
                                            int start2 = player2.IndexOf('/') + 1;

                                            int end1 = player1.Length - (player1.Length - player1.IndexOf(')')) - start1;
                                            int end2 = player2.Length - (player2.Length - player2.IndexOf(')')) - start2;

                                            output.attributes["BPW1"] = Convert.ToInt16(player1.Substring(start1, end1));
                                            output.attributes["BPW2"] = Convert.ToInt16(player2.Substring(start2, end2));

                                            start1 = player1.IndexOf('(') + 1;
                                            start2 = player2.IndexOf('(') + 1;

                                            end1 = player1.Length - (player1.Length - player1.IndexOf('/')) - start1;
                                            end2 = player2.Length - (player2.Length - player2.IndexOf('/')) - start2;

                                            output.attributes["BPC1"] = Convert.ToInt16(player1.Substring(start1, end1));
                                            output.attributes["BPC2"] = Convert.ToInt16(player2.Substring(start2, end2));
                                            break;
                                        }
                                    default:
                                        break;
                                }                                
                                //Console.WriteLine(e1.Current.InnerText + " " + e2.Current.InnerText);
                            }
                        }
                    }
                }
                return output;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static int IsNull(string value)
        {
            return (value == "&nbsp;" ? -1 : Convert.ToInt16((value.Length > 1 ? value.Substring(0,1) : value)));
        }

        public static Output fetchUSOpendata(URL url)
        {
            try
            {
                var webGet = new HtmlWeb();
                Output output = new Output();
                output.tournament = url.TournamentName;
                output.gender = url.Gender;
                var doc = webGet.Load(url.Url);
                int fnl1 = 0, fnl2 = 0;
                if (doc != null)
                {
                    HtmlNode summary = doc.DocumentNode.SelectSingleNode("//div[@id='summary']");
                    HtmlNode matchStats = summary.SelectSingleNode(".//div[@id='match-stats']");
                    HtmlNodeCollection team1 = matchStats.SelectNodes(".//div[@class='team team1']");
                    HtmlNodeCollection label = matchStats.SelectNodes(".//div[@class='statlabel bg']");
                    HtmlNodeCollection team2 = matchStats.SelectNodes(".//div[@class='team team2']");

                    ////////////////////////////////////////////////////////////////////////////
                    //Get set data from the website
                    HtmlNode summary1 = doc.DocumentNode.SelectSingleNode("//div[@id='team1info']");
                    HtmlNode ts11 = summary1.SelectSingleNode(".//div[@class='set1']");
                    HtmlNode ts12 = summary1.SelectSingleNode(".//div[@class='set2']");
                    HtmlNode ts13 = summary1.SelectSingleNode(".//div[@class='set3']");
                    HtmlNode ts14 = summary1.SelectSingleNode(".//div[@class='set4']");
                    HtmlNode ts15 = summary1.SelectSingleNode(".//div[@class='set5']");
                    HtmlNode summary2 = doc.DocumentNode.SelectSingleNode("//div[@id='team2info']");
                    HtmlNode ts21 = summary2.SelectSingleNode(".//div[@class='set1']");
                    //Console.WriteLine("Set 1 :"+ set1.InnerText);
                    HtmlNode ts22 = summary2.SelectSingleNode(".//div[@class='set2']");
                    //Console.WriteLine(set2.InnerText);
                    HtmlNode ts23 = summary2.SelectSingleNode(".//div[@class='set3']");
                    //Console.WriteLine(set3.InnerText);
                    HtmlNode ts24 = summary2.SelectSingleNode(".//div[@class='set4']");
                    //Console.WriteLine(set4.InnerText);
                    HtmlNode ts25 = summary2.SelectSingleNode(".//div[@class='set5']");
                    //Console.WriteLine(set5.InnerText);

                    using (var e1 = team1.ToList().GetEnumerator())
                    using (var e2 = label.ToList().GetEnumerator())
                    using (var e3 = team2.ToList().GetEnumerator())
                    {
                        e1.MoveNext();
                        e3.MoveNext();
                        String player1 = e1.Current.InnerText;
                        String player2 = e3.Current.InnerText;
                        string SSP1, SSP2 = "";
                        output.Player1 = player1;
                        output.Player2 = player2;

                        while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext())
                        {
                            switch (e2.Current.InnerText)
                            {
                                case "Aces":
                                    {
                                        output.attributes["ACE1"] = Convert.ToInt16(e1.Current.InnerText);
                                        output.attributes["ACE2"] = Convert.ToInt16(e3.Current.InnerText);
                                        break;
                                    }
                                case "Double faults":
                                    {
                                        output.attributes["DBF1"] = Convert.ToInt16(e1.Current.InnerText);
                                        output.attributes["DBF2"] = Convert.ToInt16(e3.Current.InnerText);
                                        break;
                                    }
                                case "Players":
                                    {
                                        break;
                                    }
                                case "Total points won":
                                    {
                                        output.attributes["TPW1"] = Convert.ToInt16(e1.Current.InnerText);
                                        output.attributes["TPW2"] = Convert.ToInt16(e3.Current.InnerText);
                                        break;
                                    }
                                case "1st serves in":
                                    {
                                        int start1 = e1.Current.InnerText.IndexOf('(') + 1;
                                        int start2 = e3.Current.InnerText.IndexOf('(') + 1;
                                        player1 = e1.Current.InnerText.Substring(start1, 2);
                                        player2 = e3.Current.InnerText.Substring(start2, 2);
                                        SSP1 = (100 - Convert.ToInt16(player1)).ToString();
                                        SSP2 = (100 - Convert.ToInt16(player2)).ToString();
                                        output.attributes["FSP1"] = Convert.ToInt16(player1);
                                        output.attributes["FSP2"] = Convert.ToInt16(player2);
                                        output.attributes["SSP1"] = Convert.ToInt16(SSP1);
                                        output.attributes["SSP2"] = Convert.ToInt16(SSP2);
                                        break;
                                    }
                                case "1st serve points won":
                                    {
                                        int start1 = e1.Current.InnerText.IndexOf('/');
                                        int start2 = e3.Current.InnerText.IndexOf('/');
                                        output.attributes["FSW1"] = Convert.ToInt16(e1.Current.InnerText.Substring(0, start1));
                                        output.attributes["FSW2"] = Convert.ToInt16(e3.Current.InnerText.Substring(0, start2));
                                        break;
                                    }
                                case "2nd serve points won":
                                    {
                                        int start1 = e1.Current.InnerText.IndexOf('/');
                                        int start2 = e3.Current.InnerText.IndexOf('/');
                                        output.attributes["SSW1"] = Convert.ToInt16(e1.Current.InnerText.Substring(0, start1));
                                        output.attributes["SSW2"] = Convert.ToInt16(e3.Current.InnerText.Substring(0, start2));
                                        break;
                                    }
                                case "Net points won":
                                    {
                                        //5/8 (63 %)
                                        int start1 = e1.Current.InnerText.IndexOf('/') + 1;
                                        int start2 = e3.Current.InnerText.IndexOf('/') + 1;
                                        int end1 = e1.Current.InnerText.Length - (e1.Current.InnerText.Length - e1.Current.InnerText.IndexOf('(')) - start1 - 1;
                                        int end2 = e3.Current.InnerText.Length - (e3.Current.InnerText.Length - e3.Current.InnerText.IndexOf('(')) - start2 - 1;
                                        output.attributes["NPW1"] = Convert.ToInt16(e1.Current.InnerText.Substring(start1, end1));
                                        output.attributes["NPW2"] = Convert.ToInt16(e3.Current.InnerText.Substring(start2, end2));

                                        output.attributes["NPA1"] = Convert.ToInt16(e1.Current.InnerText.Substring(0, --start1));
                                        output.attributes["NPA2"] = Convert.ToInt16(e3.Current.InnerText.Substring(0, --start2));
                                        break;
                                    }
                                case "Break points won":
                                    {
                                        //5/8 (63 %)
                                        int start1 = e1.Current.InnerText.IndexOf('/') + 1;
                                        int start2 = e3.Current.InnerText.IndexOf('/') + 1;
                                        int end1 = e1.Current.InnerText.Length - (e1.Current.InnerText.Length - e1.Current.InnerText.IndexOf('(')) - start1 - 1;
                                        int end2 = e3.Current.InnerText.Length - (e3.Current.InnerText.Length - e3.Current.InnerText.IndexOf('(')) - start2 - 1;
                                        output.attributes["BPW1"] = Convert.ToInt16(e1.Current.InnerText.Substring(start1, end1));
                                        output.attributes["BPW2"] = Convert.ToInt16(e3.Current.InnerText.Substring(start2, end2));

                                        output.attributes["BPA1"] = Convert.ToInt16(e1.Current.InnerText.Substring(0, --start1));
                                        output.attributes["BPA2"] = Convert.ToInt16(e3.Current.InnerText.Substring(0, --start2));


                                        output.attributes["ST11"] = IsNull(ts11.InnerText) == -1 ? 0 : IsNull(ts11.InnerText);
                                        output.attributes["ST12"] = IsNull(ts21.InnerText) == -1 ? 0 : IsNull(ts21.InnerText);

                                        if (IsNull(ts11.InnerText) != -1)
                                        {
                                            if (Convert.ToInt16(IsNull(ts11.InnerText)) > Convert.ToInt16(IsNull(ts21.InnerText)))
                                            {
                                                fnl1++;
                                            }
                                            else
                                            {
                                                fnl2++;
                                            }
                                        }
                                        output.attributes["ST21"] = IsNull(ts12.InnerText) == -1 ? 0 : IsNull(ts12.InnerText);
                                        output.attributes["ST22"] = IsNull(ts22.InnerText) == -1 ? 0 : IsNull(ts22.InnerText);

                                        if (IsNull(ts12.InnerText) != -1)
                                        {
                                            if (Convert.ToInt16(IsNull(ts12.InnerText)) > Convert.ToInt16(IsNull(ts22.InnerText)))
                                            {
                                                fnl1++;
                                            }
                                            else
                                            {
                                                fnl2++;
                                            }
                                        }

                                        output.attributes["ST31"] = IsNull(ts13.InnerText) == -1 ? 0 : IsNull(ts13.InnerText);
                                        output.attributes["ST32"] = IsNull(ts23.InnerText) == -1 ? 0 : IsNull(ts23.InnerText);

                                        if (IsNull(ts13.InnerText) != -1)
                                        {
                                            if (Convert.ToInt16(IsNull(ts13.InnerText)) > Convert.ToInt16(IsNull(ts23.InnerText)))
                                            {
                                                fnl1++;
                                            }
                                            else
                                            {
                                                fnl2++;
                                            }
                                        }

                                        output.attributes["ST41"] = IsNull(ts14.InnerText) == -1 ? 0 : IsNull(ts14.InnerText);
                                        output.attributes["ST42"] = IsNull(ts24.InnerText) == -1 ? 0 : IsNull(ts24.InnerText);

                                        if (IsNull(ts14.InnerText) != -1)
                                        {
                                            if (Convert.ToInt16(IsNull(ts14.InnerText)) > Convert.ToInt16(IsNull(ts24.InnerText)))
                                            {
                                                fnl1++;
                                            }
                                            else
                                            {
                                                fnl2++;
                                            }
                                        }

                                        output.attributes["ST51"] = IsNull(ts15.InnerText) == -1 ? 0 : IsNull(ts15.InnerText);
                                        output.attributes["ST52"] = IsNull(ts25.InnerText) == -1 ? 0 : IsNull(ts25.InnerText);

                                        if (IsNull(ts15.InnerText) != -1)
                                        {
                                            if (Convert.ToInt16(IsNull(ts15.InnerText)) > Convert.ToInt16(IsNull(ts25.InnerText)))
                                            {
                                                fnl1++;
                                            }
                                            else
                                            {
                                                fnl2++;
                                            }
                                        }

                                        output.attributes["FNL1"] = fnl1;
                                        output.attributes["FNL2"] = fnl2;
                                        break;
                                    }

                                default:
                                    {
                                        break;
                                    }
                            }
                        }//WHILE
                    }//USING
                }//IF
                return output;
            }//TRY
            catch (Exception e)
            {
                return null;
            }
            
        }

    }
}
