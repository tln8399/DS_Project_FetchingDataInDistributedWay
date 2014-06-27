using GemBox.Spreadsheet;
using Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
//using System.Threading.Tasks;

namespace DSProject
{
    class DataManager
    {
        public List<URL> urlTray;
        public static Boolean accessTray = true;
        public static int sharedCount = 0;
        public static Mutex urlMutex = new Mutex();
        public static Mutex outMutex = new Mutex();
        IEnumerator<URL> cur;
        public static ExcelFile xclFile;
        public static ExcelWorksheet xclWSheet;
        public static Stopwatch watch;
        static int rowATP = 0, rowUSM = -1;
        public List<Output> outputTray; 

        public DataManager()
        {
            urlTray = new List<URL>();
            outputTray = new List<Output>();
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
            xclFile = new ExcelFile();
        }

        public static void Main(string[] args)
        {
            watch = Stopwatch.StartNew();
            //Console.WriteLine("" +  );
            DataManager dm = new DataManager();
            dm.urlGenerator();
            dm.cur = dm.urlTray.GetEnumerator();

            NodeHandler node1 = new NodeHandler("Thread1","localhost", 1313);
            Thread thread1 = new Thread(new ParameterizedThreadStart(node1.execute));

            thread1.Start(dm);

            NodeHandler node2 = new NodeHandler("Thread2", "localhost", 2313);
            Thread thread2 = new Thread(node2.execute);

            thread2.Start(dm);

            NodeHandler node3 = new NodeHandler("Thread3", "localhost", 3313);
            Thread thread3 = new Thread(node3.execute);

            thread3.Start(dm);

            //watch.Stop();
            //var timetaken = watch.ElapsedMilliseconds;
            //Console.WriteLine("Time taken : " + timetaken);
            Console.ReadKey();
            watch.Stop();
        }
        public URL getURL()
        {
            URL curURL = null;

            if (cur.MoveNext())
            {
                curURL = cur.Current;
            }
            
            return curURL;
        }

        /// <summary>
        /// 
        /// </summary>
        public void urlGenerator()
        {
            URL url;
            string[] lines = System.IO.File.ReadAllLines(@"C:\Stats\ATP.txt");
            string tournament = "ATP";
            string gender = "Men Singles";
            string path = "";
            int i = 0;

            foreach (string line in lines)
            {
                path = "http://www.atpworldtour.com/Share/" + line;
                url = new URL(path, tournament, gender);
                Console.WriteLine((i++) + " " + line);
                urlTray.Add(url);
            }


            lines = System.IO.File.ReadAllLines(@"C:\Stats\USOpenMen.txt");
            tournament = "US Open";
            gender = "Men Singles";
            path = "";
            i = 0;

            //foreach (string line in lines)
            //{
            //    path = line;
            //    url = new URL(path, tournament, gender);
            //    Console.WriteLine((i++) + " " + line);
            //    urlTray.Add(url);
            //}

            //path = "http://www.atpworldtour.com/Share/Match-Facts-Pop-Up.aspx?t=0404&y=2013&r=1&p=A596";
            //url = new URL(path, tournament, gender);
            //urlTray.Add(url);
        }

        
        public void writeInFile(Output output)
        {
            string worksheetName = output.tournament + " " + output.gender;
            int row = 0;
            //TODO switch case to wite into correct sheet
            //switch (worksheetName)
            //{
            //    case "ATP Men Singles":
            //        {
            //            rowATP++;
            //            row = rowATP;
            //            break;
            //        }
            //    case "US Open Men Singles":
            //        {
            //            rowUSM++;
            //            row = rowUSM;
            //            break;
            //        }
            //    default:
            //        {
            //            break;
            //        }
            //}

            //check if worksheet exists in Excel file
            if (!xclFile.Worksheets.Contains(worksheetName))
            {
                //else create a worksheet and add row header
                xclWSheet = xclFile.Worksheets.Add(worksheetName);
                addRowHeader(output);
            }
            //Add the record in the sheet
            rowATP++;
            createRows(output, rowATP);

            xclFile.SaveXls("C:\\R stuff\\TD1.xls");

            watch.Stop();
            var timetaken = watch.ElapsedMilliseconds;
            Console.WriteLine("Time taken : " + timetaken);
        }

        private static void createRows(Output output, int row)
        {
            int count = 0;
            xclWSheet.Rows[row].Cells[count++].Value = output.Player1;
            xclWSheet.Rows[row].Cells[count++].Value = output.Player2;

            xclWSheet.Rows[row].Cells[count++].Value = output.Round;
            xclWSheet.Rows[row].Cells[count++].Value = output.Result;

            foreach (int value in output.attributes.Values)
            {
                xclWSheet.Rows[row].Cells[count++].Value = value;
            }
        }

        private static void addRowHeader(Output output)
        {
            int count = 0;

            //for each column
            for (int c = 0; c < (output.attributes.Count + 4); c++)
            {
                xclWSheet.Columns[c].Width = 25 * 256; //set the width to each column                
            }

            xclWSheet.Rows[0].Cells[count].Value = "Player1";
            xclWSheet.Rows[0].Cells[++count].Value = "Player2";

            xclWSheet.Rows[0].Cells[++count].Value = "Round";
            xclWSheet.Rows[0].Cells[++count].Value = "Result";

            foreach (string key in output.attributes.Keys)
            {
                xclWSheet.Rows[0].Cells[++count].Value = key;
            }
        }

    }
}
