using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace insertstock
{
    class Program
    {
        static void Main()
        {
            DataTable table = DBConnection.GetData("SELECT @@VERSION");



            DateTime myDate = DateTime.Now;
            //string myDateString = myDate.ToString("yyyyMMdd");
            string myDateString = "20190222";
            //string Today = DateTime.Now.ToString("yyyy/MM/dd");
            string Today = "2019/02/22";
            
            //string TodayEn = String.Empty;
            //if (DateTime.Now.ToString("tt").ToString() == "上午")
            //{
            //    TodayEn = Today.Replace("上午", "AM");
            //}
            //else if (DateTime.Now.ToString("tt").ToString() == "下午")
            //{
            //    TodayEn = Today.Replace("下午", "PM");
            //}

            string csvName = "C:\\Users\\user\\TWSE\\" + myDateString + "stock.csv";
            string byline = String.Empty;
            string line = String.Empty;
            string NumberOf_T;
            string[] stockinfo;


            using (StreamReader fileReader = new StreamReader(csvName))
            {
                if (null != (byline = fileReader.ReadLine())) // skip header
                {
                    while ((byline = fileReader.ReadLine()) != null)
                    {
                        line = byline.Replace("--", "0");
                        stockinfo = line.Split('|');
                        NumberOf_T = stockinfo[4].ToString().Replace(",", "");

                        DBConnection.GetData("insert into Stock (Stock_Id, Stock_Name, NumberOf_T, Opening_P, Highest_P, Lowest_P, Closing_P, Range, Difference, DateTime) values ('" + stockinfo[1] + "',N'" + stockinfo[2] + "'," + NumberOf_T + "," + stockinfo[6] + "," + stockinfo[7] + "," + stockinfo[8] + "," + stockinfo[9] + ",'" + stockinfo[10] + "'," + stockinfo[11] + ",'" + Today + "')");

                    }
                }
                fileReader.Close();
            }
        }
    }
}
