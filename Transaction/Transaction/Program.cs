using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data.SqlClient;
using System.Data;


namespace Transaction
{
    class Program
    {
        static void Main(string[] args)
        {

                String Date = DateTime.Now.ToString("yyyy/MM/dd");
                String YDate = DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd");
                float Yesterday = 0;

            try
                {
                    //    Session["time"] = DateTime.Now.ToString();

                    //連資料庫 抓資料 
                    string cs = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                    string qs = "SELECT Stock.Closing_P, [MemberMaster].Balance, UnsuccessfulRecord.Account,UnsuccessfulRecord.Stock_Id,UnsuccessfulRecord.Order_P,UnsuccessfulRecord.Amount,UnsuccessfulRecord.Action,UnsuccessfulRecord.Order_DT FROM [MemberMaster] CROSS JOIN Stock CROSS JOIN UnsuccessfulRecord WHERE ([MemberMaster].Account=UnsuccessfulRecord.Account) AND (Stock.Stock_Id=UnsuccessfulRecord.Stock_Id) AND (Stock.DateTime=@DateTime) ORDER BY Action DESC,Order_DT ";
                    string qs1 = "SELECT Stock.Closing_P FROM Stock WHERE (Stock.Stock_Id=@Stock_Id) AND (Stock.DateTime=@YDateTime)";
                //1.引用SqlConnection物件連接資料庫        
                using (SqlConnection cn = new SqlConnection(cs))
                {
                        //2.開啟資料庫
                        cn.Open();
                    //3.引用SqlCommand物件
                    using (SqlCommand command1 = new SqlCommand(qs1, cn))
                    {
                        command1.Parameters.AddWithValue("@DateTime", YDate);

                        using (SqlDataReader dr1 = command1.ExecuteReader())
                        {
                            while ((dr1.Read()))
                            {
                                if (!dr1[0].Equals(DBNull.Value))    //不為空
                                {
                                    string temp1 = dr1[0].ToString().Trim();   //  昨收
                                    Yesterday = Convert.ToSingle(temp1);
                                }
                            }
                        }
                    }
                    using (SqlCommand command = new SqlCommand(qs, cn))
                    {
                            command.Parameters.AddWithValue("@DateTime", Date);
                            
                            //4.搭配SqlCommand物件使用SqlDataReader
                        using (SqlDataReader dr = command.ExecuteReader())
                            {
                                while ((dr.Read()))
                                {
                                    //5.判斷資料列是否為空    
                                    if (!dr[0].Equals(DBNull.Value))    //不為空
                                    {
                                        string temp = dr[0].ToString().Trim();  //把收盤放進temp
                                        float price = Convert.ToSingle(temp);    //變成float型態

                                        temp = dr[4].ToString().Trim();   //把出價放進temp
                                        float BuyorSell = Convert.ToSingle(temp);

                                        temp = dr[5].ToString().Trim();  //把買賣數量放進temp
                                        float amount = Convert.ToSingle(temp);

                                        string Do = dr[6].ToString().Trim();   //把買或賣存進Do     

                                        string OrderTime = dr[7].ToString().Trim();   //把下單時間存進 OrderTime

                                        //temp = dr[8].ToString().Trim();   //  昨收
                                        //float Yesterday = Convert.ToSingle(temp);

                                        temp = dr[1].ToString().Trim();  //把餘額放進temp
                                        float Money = Convert.ToSingle(temp);  //交易前餘額  (每筆交易都要重查 在裡面查 因為這個迴圈不會邊查邊輸出)
                                        float NewMoney = Convert.ToSingle(temp);  //交易後餘額 (會用來計算)

                                        float Own = 0; //擁有股票的數量
                                        int Charge = Convert.ToInt32(Math.Floor(price * amount * 1000 * 1.425 / 1000));   //手續費  
                                        int Tax = Convert.ToInt32(Math.Floor(price * amount * 1000 * 3 / 1000));    //證交稅
                                        int TransactionPrice = Convert.ToInt32(price * 1000 * amount);  //交易金額
                                                                                                        /*
                                                                                                       if (Charge < 20)
                                                                                                       {
                                                                                                           Charge = 20;     //如果手續費小於20元 則用20計算
                                                                                                       }
                                                                                                         */
                                        String t1 = DateTime.Now.ToString("yyyy/MM/dd") + " PM 01:30:00";
                                        String t2 = DateTime.Now.ToString("yyyy/MM/dd") + " PM 12:00:00";

                                        if (string.Compare(t1, OrderTime) >= 0 || (string.Compare(OrderTime, t2) >= 0))   //前面大   因 PM 12:00~1:00 會不做 所以增加後面判斷式
                                        {       //下單時間在1:30前 可以做   12:00~1:00 可以做


                                            if (price != 0)   //如果當天收盤價不是抓到0 就做
                                            {


                                                if (Equals(Do, "Buy"))                              //判斷買賣
                                                {
                                                    Tax = 0;   //買不用收證交稅

                                                    if (price >= Yesterday * 1.1)
                                                    {        //今天股價 漲停板

                                                        //新增至失敗紀錄                              
                                                        string cs2 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                        string qs2 = "INSERT INTO FailRecord (Account, Stock_Id,Order_P,Daily_P,Amount,Action,Before_TB,Order_DT,Process_DT,Remark) VALUES (@帳號, @股票號碼,@下單價格,@當日股價,@數量,@買賣,@交易前餘額,@下單時間,@處理時間,@備註) ";
                                                        using (SqlConnection cn2 = new SqlConnection(cs2))
                                                        {
                                                            cn2.Open();
                                                            using (SqlCommand command2 = new SqlCommand(qs2, cn2))
                                                            {
                                                                command2.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                command2.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());
                                                                command2.Parameters.AddWithValue("@Order_P", dr[4].ToString().Trim());
                                                                command2.Parameters.AddWithValue("@Daily_P", dr[0].ToString().Trim());
                                                                command2.Parameters.AddWithValue("@Amount", dr[5].ToString().Trim());
                                                                command2.Parameters.AddWithValue("@Action", dr[6].ToString().Trim());
                                                                command2.Parameters.AddWithValue("@Before_TB", Money);
                                                                command2.Parameters.AddWithValue("@Order_DT", dr[7].ToString().Trim());
                                                                command2.Parameters.AddWithValue("@Process_DT", DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss"));
                                                                command2.Parameters.AddWithValue("@Remark", "漲停買不到");
                                                                command2.ExecuteNonQuery();
                                                            }
                                                            cn2.Close();
                                                        }

                                                        Console.WriteLine(dr[2].ToString().Trim() + " 漲停買不到");


                                                    }
                                                    else
                                                    {
                                                        //查帳戶餘額 (因當場更新所以每次都需重查  一開始的查完會一直迴圈輸出資料 並不是會一邊查一邊輸出)
                                                        SqlDataAdapter adapter = new SqlDataAdapter();
                                                        string cs5 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                        string qs5 = "SELECT Balance FROM [MemberMaster] WHERE (Account=@Account)";
                                                        using (SqlConnection cn5 = new SqlConnection(cs5))
                                                        {
                                                            cn5.Open();
                                                            using (SqlCommand command5 = new SqlCommand(qs5, cn5))
                                                            {
                                                                command5.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());

                                                                DataSet dataset = new DataSet();
                                                                adapter.SelectCommand = command5;
                                                                adapter.Fill(dataset);
                                                                Money = (float)Convert.ToDouble(dataset.Tables[0].Rows[0][0]); //更新後的餘額
                                                            }

                                                            cn5.Close();
                                                        }


                                                        NewMoney = Money - TransactionPrice - Tax - Charge;   //判斷餘額(有扣手續費.證交稅 3+1)
                                                        if (NewMoney >= 0)  //餘額夠
                                                        {
                                                            if (BuyorSell >= price)  //買價夠高 可以買
                                                            {
                                                                //新增至成功紀錄                              
                                                                string cs2 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                                string qs2 = "INSERT INTO Record (Account, Stock_Id,Order_P,Trans_P,Amount,Action,Before_TB,Trans_SumP,Fee,Tax,After_TB,Order_DT,Process_DT) VALUES (@Account,@Stock_Id,@Order_P,@Trans_P,@Amount,@Action,@Before_TB,@Trans_SumP,@Fee,@Tax@,After_TB,@Order_DT,@Process_DT)";
                                                                using (SqlConnection cn2 = new SqlConnection(cs2))
                                                                {
                                                                    cn2.Open();
                                                                    using (SqlCommand command2 = new SqlCommand(qs2, cn2))
                                                                    {
                                                                        command2.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Order_P", dr[4].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Trans_P", dr[0].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Amount", dr[5].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Action", dr[6].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Before_TB", Money);
                                                                        command2.Parameters.AddWithValue("@Trans_SumP", TransactionPrice);
                                                                        command2.Parameters.AddWithValue("@Fee", Charge);
                                                                        command2.Parameters.AddWithValue("@Tax", Tax);
                                                                        command2.Parameters.AddWithValue("@After_TB", NewMoney);
                                                                        command2.Parameters.AddWithValue("@Order_DT", dr[7].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Process_DT", DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss"));
                                                                        command2.ExecuteNonQuery();
                                                                    }
                                                                    cn2.Close();
                                                                }

                                                                //算扣款後的帳戶金額
                                                                //       NewMoney = Money-TransactionPrice-Tax-Charge; 

                                                                //扣款
                                                                string cs3 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                                string qs3 = "UPDATE [MemberMaster] SET Balance = @Balance WHERE Account = @Account ";
                                                                using (SqlConnection cn3 = new SqlConnection(cs3))
                                                                {
                                                                    cn3.Open();
                                                                    using (SqlCommand command3 = new SqlCommand(qs3, cn3))
                                                                    {
                                                                        command3.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                        command3.Parameters.AddWithValue("@Balance", NewMoney);
                                                                        command3.ExecuteNonQuery();
                                                                    }
                                                                    cn3.Close();


                                                                }

                                                                //查有沒有擁有這項股票  (決定要insert 或 update至Own)
                                                                SqlDataAdapter adapter6 = new SqlDataAdapter();
                                                                string cs6 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                                string qs6 = "SELECT Amount FROM [Own] WHERE (Account=@Account) AND (Stock_Id=@Stock_Id)";
                                                                using (SqlConnection cn6 = new SqlConnection(cs6))
                                                                {
                                                                    cn6.Open();
                                                                    using (SqlCommand command6 = new SqlCommand(qs6, cn6))
                                                                    {
                                                                        command6.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                        command6.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());

                                                                        DataSet dataset = new DataSet();
                                                                        adapter6.SelectCommand = command6;
                                                                        adapter6.Fill(dataset);
                                                                        if (dataset.Tables[0].Rows.Count != 0)   //判斷dataset有沒有值  (如果有值就做UPDATE)
                                                                        {
                                                                            Own = (float)Convert.ToDouble(dataset.Tables[0].Rows[0][0]);   //這項股票所擁有的數量
                                                                            Own = Own + amount;

                                                                            //加到Own 擁有的股票
                                                                            string cs7 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                                            string qs7 = "UPDATE [Own] SET Amount=@Amount WHERE (Account=@Account) AND (Stock_Id=@Stock_Id)";
                                                                            using (SqlConnection cn7 = new SqlConnection(cs7))
                                                                            {
                                                                                cn7.Open();
                                                                                using (SqlCommand command7 = new SqlCommand(qs7, cn7))
                                                                                {
                                                                                    command7.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                                    command7.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());
                                                                                    command7.Parameters.AddWithValue("@Amount", Own);
                                                                                    command7.ExecuteNonQuery();
                                                                                }
                                                                                cn7.Close();
                                                                            }
                                                                        }
                                                                        else if (dataset.Tables[0].Rows.Count == 0)  //沒值 用insert的
                                                                        {
                                                                            //新增至Own                            
                                                                            string cs7 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                                            string qs7 = "INSERT INTO Own (Account, Stock_Id,Amount) VALUES (@Account, @Stock_Id,@Amount) ";
                                                                            using (SqlConnection cn7 = new SqlConnection(cs7))
                                                                            {
                                                                                cn7.Open();
                                                                                using (SqlCommand command7 = new SqlCommand(qs7, cn7))
                                                                                {
                                                                                    command7.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                                    command7.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());
                                                                                    command7.Parameters.AddWithValue("@Amount", amount);
                                                                                    command7.ExecuteNonQuery();
                                                                                }
                                                                                cn7.Close();
                                                                            }
                                                                        }
                                                                    }

                                                                    cn6.Close();
                                                                }       //insert 或 UPDATE 至 Own 的結尾


                                                                Console.WriteLine(dr[2].ToString().Trim() + " 成功買進");


                                                            }
                                                            else if (BuyorSell < price)   //買價太低 不能買
                                                            {
                                                                //新增至失敗紀錄                              
                                                                string cs2 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                                string qs2 = "INSERT INTO FailRecord (Account, Stock_Id,Order_P,Daily_P,Amount,Action,Before_TB,Order_DT,Process_DT,Remark) VALUES (@Account, @Stock_Id,@Order_P,@Daily_P,@Amount,@Action,@Before_TB,@Order_DT,@Process_DT,@Remark) ";
                                                                using (SqlConnection cn2 = new SqlConnection(cs2))
                                                                {
                                                                    cn2.Open();
                                                                    using (SqlCommand command2 = new SqlCommand(qs2, cn2))
                                                                    {
                                                                        command2.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Order_P", dr[4].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Daily_P", dr[0].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Amount", dr[5].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Action", dr[6].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Before_TB", Money);
                                                                        command2.Parameters.AddWithValue("@Order_DT", dr[7].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Process_DT", DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss"));
                                                                        command2.Parameters.AddWithValue("@Remark", "買價太低");
                                                                        command2.ExecuteNonQuery();
                                                                    }
                                                                    cn2.Close();
                                                                }

                                                                Console.WriteLine(dr[2].ToString().Trim() + " 買太低 未成功買進");

                                                            }
                                                        }   //if (NewMoney >= 0)的後括
                                                        else if (NewMoney < 0)  //當餘額不足購買
                                                        {

                                                            //新增至失敗紀錄                              
                                                            string cs2 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                            string qs2 = "INSERT INTO FailRecord (Account, Stock_Id,Order_P,Daily_P,Amount,Action,Before_TB,Order_DT,Process_DT,Remark) VALUES (@Account, @Stock_Id,@Order_P,@Daily_P,@Amount,@Action,@Before_TB,@Order_DT,@Process_DT,@Remark) ";
                                                            using (SqlConnection cn2 = new SqlConnection(cs2))
                                                            {
                                                                cn2.Open();
                                                                using (SqlCommand command2 = new SqlCommand(qs2, cn2))
                                                                {
                                                                    command2.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                    command2.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());
                                                                    command2.Parameters.AddWithValue("@Order_P", dr[4].ToString().Trim());
                                                                    command2.Parameters.AddWithValue("@Daily_P", dr[0].ToString().Trim());
                                                                    command2.Parameters.AddWithValue("@Amount", dr[5].ToString().Trim());
                                                                    command2.Parameters.AddWithValue("@Action", dr[6].ToString().Trim());
                                                                    command2.Parameters.AddWithValue("@Before_TB", Money);
                                                                    command2.Parameters.AddWithValue("@Order_DT", dr[7].ToString().Trim());
                                                                    command2.Parameters.AddWithValue("@Process_DT", DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss"));
                                                                    command2.Parameters.AddWithValue("@Remark", "餘額不足");
                                                                    command2.ExecuteNonQuery();
                                                                }
                                                                cn2.Close();
                                                            }

                                                            Console.WriteLine(dr[2].ToString().Trim() + " 餘額不足購買");
                                                        }

                                                    }  //判斷漲停的後括

                                                    //刪掉在Unsuccessful的資料   (成功買進,價格買不到,餘額不夠買 都用這個)
                                                    string cs4 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                    string qs4 = "DELETE FROM UnsuccessfulRecord WHERE Account=@Account AND Order_DT=@Order_DT";
                                                    using (SqlConnection cn4 = new SqlConnection(cs4))
                                                    {
                                                        cn4.Open();
                                                        using (SqlCommand command4 = new SqlCommand(qs4, cn4))
                                                        {
                                                            command4.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                            command4.Parameters.AddWithValue("@Order_DT", dr[7].ToString().Trim());

                                                            command4.ExecuteNonQuery();
                                                        }
                                                        cn4.Close();
                                                    }

                                                }  //買的後括


                                                else if (Equals(Do, "賣"))
                                                {
                                                    if (price <= Yesterday * 0.9)
                                                    {

                                                        //新增至失敗紀錄                              
                                                        string cs2 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                        string qs2 = "INSERT INTO FailRecord (Account, Stock_Id,Order_P,Daily_P,Amount,Action,Before_TB,Order_DT,Process_DT,Remark) VALUES (@Account, @Stock_Id,@Order_P,@Daily_P,@Amount,@Action,@Before_TB,@Order_DT,@Process_DT,@Remark) ";
                                                        using (SqlConnection cn2 = new SqlConnection(cs2))
                                                        {
                                                            cn2.Open();
                                                            using (SqlCommand command2 = new SqlCommand(qs2, cn2))
                                                            {
                                                                command2.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                command2.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());
                                                                command2.Parameters.AddWithValue("@Order_P", dr[4].ToString().Trim());
                                                                command2.Parameters.AddWithValue("@Daily_P", dr[0].ToString().Trim());
                                                                command2.Parameters.AddWithValue("@Amount", dr[5].ToString().Trim());
                                                                command2.Parameters.AddWithValue("@Action", dr[6].ToString().Trim());
                                                                command2.Parameters.AddWithValue("@Before_TB", Money);
                                                                command2.Parameters.AddWithValue("@Order_DT", dr[7].ToString().Trim());
                                                                command2.Parameters.AddWithValue("@Process_DT", DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss"));
                                                                command2.Parameters.AddWithValue("@Remark", "跌停賣不出");
                                                                command2.ExecuteNonQuery();
                                                            }
                                                            cn2.Close();
                                                        }

                                                        Console.WriteLine(dr[2].ToString().Trim() + " 跌停賣不出");
                                                    }
                                                    else
                                                    {

                                                        //查有沒有 擁有這項股票
                                                        SqlDataAdapter adapter6 = new SqlDataAdapter();
                                                        string cs6 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                        string qs6 = "SELECT Amount FROM [Own] WHERE (Account=@Account) AND (Stock_Id=@Stock_Id)";
                                                        using (SqlConnection cn6 = new SqlConnection(cs6))
                                                        {
                                                            cn6.Open();
                                                            using (SqlCommand command6 = new SqlCommand(qs6, cn6))
                                                            {
                                                                command6.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                command6.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());

                                                                DataSet dataset = new DataSet();
                                                                adapter6.SelectCommand = command6;
                                                                adapter6.Fill(dataset);
                                                                if (dataset.Tables[0].Rows.Count != 0)   //判斷dataset有沒有值  (如果有值就做)
                                                                {
                                                                    Own = (float)Convert.ToDouble(dataset.Tables[0].Rows[0][0]);   //這項股票所擁有的數量
                                                                }
                                                            }

                                                            cn6.Close();
                                                        }

                                                        //查帳戶餘額 (因當場更新所以每次都需重查)
                                                        SqlDataAdapter adapter = new SqlDataAdapter();
                                                        string cs5 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                        string qs5 = "SELECT Balance FROM [User] WHERE (Account=@Account)";
                                                        using (SqlConnection cn5 = new SqlConnection(cs5))
                                                        {
                                                            cn5.Open();
                                                            using (SqlCommand command5 = new SqlCommand(qs5, cn5))
                                                            {
                                                                command5.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());

                                                                DataSet dataset = new DataSet();
                                                                adapter.SelectCommand = command5;
                                                                adapter.Fill(dataset);
                                                                Money = (float)Convert.ToDouble(dataset.Tables[0].Rows[0][0]); //更新後的餘額
                                                            }

                                                            cn5.Close();
                                                        }


                                                        if (Own >= amount)
                                                        { //如果有這項股票且數量夠
                                                            if (BuyorSell <= price)
                                                            {
                                                                NewMoney = Money + TransactionPrice - Tax - Charge;

                                                                //新增至成功紀錄                              
                                                                string cs2 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=stock_manager";
                                                                string qs2 = "INSERT INTO Record (Account, Stock_Id,Order_P,Trans_P,Amount,Action,Before_TB,Trans_SumP,Fee,Tax,After_TB,Order_DT,Process_DT) VALUES (@Account,@Stock_Id,@Order_P,@Trans_P,@Amount,@Action,@Before_TB,@Trans_SumP,@Fee,@Tax@,After_TB,@Order_DT,@Process_DT)";
                                                                using (SqlConnection cn2 = new SqlConnection(cs2))
                                                                {
                                                                    cn2.Open();
                                                                    using (SqlCommand command2 = new SqlCommand(qs2, cn2))
                                                                    {
                                                                        command2.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Order_P", dr[4].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Trans_P", dr[0].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Amount", dr[5].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Action", dr[6].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Before_TB", Money);
                                                                        command2.Parameters.AddWithValue("@Trans_SumP", TransactionPrice);
                                                                        command2.Parameters.AddWithValue("@Fee", Charge);
                                                                        command2.Parameters.AddWithValue("@Tax", Tax);
                                                                        command2.Parameters.AddWithValue("@After_TB", NewMoney);
                                                                        command2.Parameters.AddWithValue("@Order_DT", dr[7].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Process_DT", DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss"));
                                                                        command2.ExecuteNonQuery();
                                                                    }
                                                                    cn2.Close();
                                                                }


                                                                //算進款後的帳戶餘額
                                                                NewMoney = Money + TransactionPrice - Charge - Tax;
                                                                //進款



                                                                string cs3 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                                string qs3 = "UPDATE [MemberMaster] SET Balance = @Balance WHERE Account = @Account ";
                                                                using (SqlConnection cn3 = new SqlConnection(cs3))
                                                                {
                                                                    cn3.Open();
                                                                    using (SqlCommand command3 = new SqlCommand(qs3, cn3))
                                                                    {
                                                                        command3.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                        command3.Parameters.AddWithValue("@Balance", NewMoney);
                                                                        command3.ExecuteNonQuery();
                                                                    }
                                                                    cn3.Close();


                                                                }

                                                                //查這項股票的數量  (決定要DELETE 或 update至Own)
                                                                SqlDataAdapter adapter8 = new SqlDataAdapter();
                                                                string cs8 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                                string qs8 = "SELECT Amount FROM [Own] WHERE (Account=@Account) AND (Stock_Id=@Stock_Id)";
                                                                using (SqlConnection cn8 = new SqlConnection(cs8))
                                                                {
                                                                    cn8.Open();
                                                                    using (SqlCommand command8 = new SqlCommand(qs8, cn8))
                                                                    {
                                                                        command8.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                        command8.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());

                                                                        DataSet dataset = new DataSet();
                                                                        adapter8.SelectCommand = command8;
                                                                        adapter8.Fill(dataset);
                                                                        Own = (float)Convert.ToDouble(dataset.Tables[0].Rows[0][0]);   //這項股票所擁有的數量
                                                                        if (Own - amount > 0)   //判斷賣掉後還有沒有剩  (如果有剩就做UPDATE)
                                                                        {

                                                                            Own = Own - amount;

                                                                            //更新Own 擁有的股票
                                                                            string cs7 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                                            string qs7 = "UPDATE [Own] SET Amount=@Amount WHERE (Account=@Account) AND (Stock_Id=@Stock_Id)";
                                                                            using (SqlConnection cn7 = new SqlConnection(cs7))
                                                                            {
                                                                                cn7.Open();
                                                                                using (SqlCommand command7 = new SqlCommand(qs7, cn7))
                                                                                {
                                                                                    command7.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                                    command7.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());
                                                                                    command7.Parameters.AddWithValue("@Amount", Own);
                                                                                    command7.ExecuteNonQuery();
                                                                                }
                                                                                cn7.Close();
                                                                            }
                                                                        }
                                                                        else if (Own - amount == 0)  //賣完後沒剩 刪除
                                                                        {
                                                                            //刪除                            
                                                                            string cs7 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                                            string qs7 = "DELETE FROM Own WHERE (Account=@Account) AND (Stock_Id=@Stock_Id) ";
                                                                            using (SqlConnection cn7 = new SqlConnection(cs7))
                                                                            {
                                                                                cn7.Open();
                                                                                using (SqlCommand command7 = new SqlCommand(qs7, cn7))
                                                                                {
                                                                                    command7.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                                    command7.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());
                                                                                    command7.ExecuteNonQuery();
                                                                                }
                                                                                cn7.Close();
                                                                            }
                                                                        }
                                                                    }

                                                                    cn8.Close();
                                                                }       //DELETE 或 UPDATE 至 Own 的結尾



                                                                Console.WriteLine(dr[2].ToString().Trim() + " 成功賣出");
                                                            }


                                                            else if (BuyorSell > price)
                                                            {

                                                                //新增至失敗紀錄                              
                                                                string cs2 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                                string qs2 = "INSERT INTO FailRecord (Account, Stock_Id,Order_P,Daily_P,Amount,Action,Before_TB,Order_DT,Process_DT,Remark) VALUES (@Account, @Stock_Id,@Order_P,@Daily_P,@Amount,@Action,@Before_TB,@Order_DT,@Process_DT,@Remark) ";
                                                                using (SqlConnection cn2 = new SqlConnection(cs2))
                                                                {
                                                                    cn2.Open();
                                                                    using (SqlCommand command2 = new SqlCommand(qs2, cn2))
                                                                    {
                                                                        command2.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Order_P", dr[4].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Daily_P", dr[0].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Amount", dr[5].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Action", dr[6].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Before_TB", Money);
                                                                        command2.Parameters.AddWithValue("@Order_DT", dr[7].ToString().Trim());
                                                                        command2.Parameters.AddWithValue("@Process_DT", DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss"));
                                                                        command2.Parameters.AddWithValue("@Remark", "賣價太高");
                                                                        command2.ExecuteNonQuery();
                                                                    }
                                                                    cn2.Close();
                                                                }

                                                                Console.WriteLine(dr[2].ToString().Trim() + " 賣太高 未成功賣出");


                                                            }
                                                        } //如果有這項股票且數量夠 的後括

                                                        //沒這項股票或數量不夠
                                                        else if (Own < amount)
                                                        {

                                                            //新增至失敗紀錄                              
                                                            string cs2 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                            string qs2 = "INSERT INTO FailRecord (Account, Stock_Id,Order_P,Daily_P,Amount,Action,Before_TB,Order_DT,Process_DT,Remark) VALUES (@Account, @Stock_Id,@Order_P,@Daily_P,@Amount,@Action,@Before_TB,@Order_DT,@Process_DT,@Remark) ";
                                                            using (SqlConnection cn2 = new SqlConnection(cs2))
                                                            {
                                                                cn2.Open();
                                                                using (SqlCommand command2 = new SqlCommand(qs2, cn2))
                                                                {
                                                                    command2.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                                    command2.Parameters.AddWithValue("@Stock_Id", dr[3].ToString().Trim());
                                                                    command2.Parameters.AddWithValue("@Order_P", dr[4].ToString().Trim());
                                                                    command2.Parameters.AddWithValue("@Daily_P", dr[0].ToString().Trim());
                                                                    command2.Parameters.AddWithValue("@Amount", dr[5].ToString().Trim());
                                                                    command2.Parameters.AddWithValue("@Action", dr[6].ToString().Trim());
                                                                    command2.Parameters.AddWithValue("@Before_TB", Money);
                                                                    command2.Parameters.AddWithValue("@Order_DT", dr[7].ToString().Trim());
                                                                    command2.Parameters.AddWithValue("@Process_DT", DateTime.Now.ToString("yyyy/MM/dd tt hh:mm:ss"));
                                                                    command2.Parameters.AddWithValue("@Remark", "股票數量不足");
                                                                    command2.ExecuteNonQuery();
                                                                }
                                                                cn2.Close();
                                                            }


                                                            Console.WriteLine(dr[2].ToString().Trim() + " 股票數量不足");
                                                        }
                                                    }  //判斷跌停 的後括
                                                       //刪掉在Unsuccessful的資料 (成功賣出,賣太高,沒有或數量不夠賣 都用這個)
                                                    string cs4 = "Data Source=140.120.53.200;User Id=ClassManager; Password=12345678; Initial Catalog=StockManage_2018";
                                                    string qs4 = "DELETE FROM UnsuccessfulRecord WHERE Account=@Account AND Order_DT=@Order_DT";
                                                    using (SqlConnection cn4 = new SqlConnection(cs4))
                                                    {
                                                        cn4.Open();
                                                        using (SqlCommand command4 = new SqlCommand(qs4, cn4))
                                                        {
                                                            command4.Parameters.AddWithValue("@Account", dr[2].ToString().Trim());
                                                            command4.Parameters.AddWithValue("@Order_DT", dr[7].ToString().Trim());

                                                            command4.ExecuteNonQuery();
                                                        }
                                                        cn4.Close();
                                                    }

                                                } //賣的後括  


                                            }  //  判斷當天是不是都抓到0的if後括
                                            else
                                            {
                                                Console.WriteLine(dr[2].ToString().Trim() + " 收盤價有錯誤!!!");
                                            }   //判斷當天是不是都抓到0的else



                                        }//判斷是不是在1:30~2:40間的 if後括
                                        else
                                        {   //下單時間在1:30~2:40間 要隔天才能做
                                            Console.WriteLine(dr[2].ToString().Trim() + " 超過下午1:30 不做");
                                        }




                                    }  //if 資料列不為空的後括     
                                }
                                dr.Close();      //關閉Reader
                            }

                       }
                        cn.Close();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                }

                Console.ReadLine();
           
        }
    }
}
