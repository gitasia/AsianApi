using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Data;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using MySql.Data;

namespace AsianApi
{
    public class BaseUp
    {
        public BaseUp()
        {
           // _template_ = _template;
        }

        public String ConBase()
        {
            String err = "";
            connect_base(err);
            return err;
        }

        public Int32 read_user_credit(Int32 id_user)
        {
           // Int32 credit = 0;
            read_credits(id_user);
            return credit;
        }

        public List<user_bet_table> read_Base(Int32 user_id)
        {
            read_table(user_id);
            return Base;
        }

        public String write_Base(List<user_bet_table> Base, Int32 id_user, Int32 credit_user)
        {
            String err = "";
            write_table(Base, id_user, credit_user);
            return err;
        }

        public class user_bet_table
        {
            public user_bet_table(Int32 user_id, string liga_id, string match_id, string game_id, string home_command, string away_command, 
                                  string event_, string liga_name, string my_odds, string bet, Boolean betted, string kef, string data_bet,
                                  string result_first, string result_full, Boolean on_off, string mani, string data_game, string diff, 
                                  string sport, string live, Boolean checked_)
            {
                this.user_id = user_id;
                this.liga_id = liga_id;
                this.match_id = match_id;
                this.game_id = game_id;
                this.home_command = home_command;
                this.away_command = away_command;
                this.event_ = event_;
                this.liga_name = liga_name;
                this.my_odds = my_odds;
                this.bet = bet;
                this.betted = betted;
                this.kef = kef;
                this.data_bet = data_bet;
                this.result_first = result_first;
                this.result_full = result_full;
                this.on_off = on_off;
                this.mani = mani;
                this.data_game = data_game;
                this.diff = diff;
                this.sport = sport;
                this.live = live;
                this.checked_ = checked_;
                
            }
            public Int32 user_id;
            public String liga_id;
            public String match_id;
            public String game_id;
            public String home_command;
            public String away_command;
            public String event_; // first, full + *_command or draw
            public String liga_name;
            public String my_odds;
            public String bet;
            public Boolean betted;
            public String kef;
            public String data_bet; // timestamp
            public String result_first; // score
            public String result_full;
            public Boolean on_off; // itog
            public String mani; // in this bet
            public String data_game; // + time
            public String diff; // under, above 
            public String sport; // football, basketball, ..
            public String live; // in running, today, early, ..
            public Boolean checked_;
        }

        public void connect_base(String err)
        {
          try
            {
                conn = new MySqlConnection(connParams);
                conn.Open();
                conn.Close();

            }
            catch (MySqlException ex)
            {
                err = ex.Message;
                throw;
            }
        }
        public void read_table(Int32 user_id)
        {
            Int32 rows;
            IDataReader r;
            object rr;
            try
            {
                conn = new MySqlConnection(connParams);
                conn.Open();
                cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Count(*) as row_count FROM `user_bet` where `user_id`=" + user_id + ";";
                rr = cmd.ExecuteScalar();
                rows = Convert.ToInt32(rr.ToString());
                Base = new List<user_bet_table>();
                cmd.CommandText = "select * from `user_bet`where `user_id`=" + user_id + ";";
                r = cmd.ExecuteReader();
                while (r.Read())
                {
                   Base.Add(new user_bet_table(Convert.ToInt32(r["user_id"].ToString()), r["liga_id"].ToString(), r["match_id"].ToString(), r["game_id"].ToString(), r["home_command"].ToString(), r["away_command"].ToString(), 
                                               r["event"].ToString(), r["liga_name"].ToString(), r["my_odds"].ToString(), r["bet"].ToString(), Convert.ToBoolean(r["betted"]), r["kef"].ToString(),
                                               r["data_bet"].ToString(), r["result_first"].ToString(), r["result_full"].ToString(), Convert.ToBoolean(r["on_off"]), r["mani"].ToString(), 
                                               r["data_game"].ToString(), r["diff"].ToString(), r["sport"].ToString(), r["live"].ToString(), Convert.ToBoolean(r["checked_"])));
                    //Convert.ToDateTime(
                }
                r.Close();
                if (rows > 0)
                {
                    cmd.CommandText = "delete LOW_PRIORITY from `user_bet`";
                    cmd.CommandText += " where `user_id`=" + user_id;
                    cmd.CommandText += " limit " + rows + ";";

                    int d = cmd.ExecuteNonQuery();

                }
                conn.Close();
            }
            catch(MySqlException ex)
            {
               
                MessageBox.Show(ex.Message);
                Base.Clear();
                throw;
             //   return;
            }
        }

        public void read_credits(Int32 id_user)
        {
        //    IDataReader r;
            object rr;
            try
            {
                conn = new MySqlConnection(connParams);
                conn.Open();
                cmd = conn.CreateCommand();
            //    cmd.CommandText = "SELECT Count(*) as row_count FROM `user_bet` where `user_id`=" + user_id + ";";
            //    rr = cmd.ExecuteScalar();
                cmd.CommandText = "select `credit` from `fos_user` where `Id`=" + id_user + ";";
                //     r = cmd.ExecuteReader();
                rr = cmd.ExecuteScalar();
                credit = Int32.Parse(rr.ToString());
                
                              
                conn.Close();
            }
            catch (MySqlException ex)
            {

                MessageBox.Show(ex.Message);
                credit=0;
                throw;
                //   return;
            }
        }

        public void write_table(List<user_bet_table> Base, Int32 id_user, Int32 credit_user)
        {
          try
            {
                conn = new MySqlConnection(connParams);
                conn.Open();
                IDbCommand cmd = conn.CreateCommand();
                if (Base.Count != 0)
                {
                    for (int ll = 0; ll <= Base.Count - 1; ll++)
                    {
                        cmd.CommandText = "insert LOW_PRIORITY `user_bet` set ";

                        cmd.CommandText += " `live`=\"" +         Base[ll].live + "\",";
                        cmd.CommandText += " `liga_id`='" +       Base[ll].liga_id + "',";
                        cmd.CommandText += " `match_id`='" +      Base[ll].match_id + "',";
                        cmd.CommandText += " `game_id`='" +       Base[ll].game_id + "',";
                        cmd.CommandText += " `home_command`='" +  Base[ll].home_command + "',";
                        cmd.CommandText += " `away_command`='" +  Base[ll].away_command + "',";

                        cmd.CommandText += " `event`='" +         Base[ll].event_ + "',";
                        cmd.CommandText += " `liga_name`='" +     Base[ll].liga_name + "',";
                        cmd.CommandText += " `my_odds`='" +       Base[ll].my_odds + "',";
                        cmd.CommandText += " `bet`='" +           Base[ll].bet + "',";

                        cmd.CommandText += " `betted`=" +         Base[ll].betted + ",";
                        cmd.CommandText += " `kef`='" +           Base[ll].kef + "',";
                        cmd.CommandText += " `data_bet`='" +      Base[ll].data_bet + "',";
                        cmd.CommandText += " `result_first`='" +  Base[ll].result_first + "',";

                        cmd.CommandText += " `result_full`='" +   Base[ll].result_full + "',";
                        cmd.CommandText += " `on_off`=" +         Base[ll].on_off + ",";
                        cmd.CommandText += " `mani`='" +          Base[ll].mani + "',";
                        cmd.CommandText += " `data_game`='" +     Base[ll].data_game + "',";

                        cmd.CommandText += " `diff`='" +          Base[ll].diff + "',";
                        cmd.CommandText += " `sport`='" +         Base[ll].sport + "',";
                        cmd.CommandText += " `checked_`=" +       Base[ll].checked_ + ",";

                        //             if (Base[ll].data_bet != null) cmd.CommandText += " `data_bet`=" + Base[ll].data_bet + ",";
                        //              if (Base[ll].data_game != null) cmd.CommandText += " `data_game`=" + Base[ll].data_game + ",";

                        cmd.CommandText += " `user_id`=" + Base[ll].user_id + ";";


                        int w = cmd.ExecuteNonQuery();
                    }
                }
                cmd = conn.CreateCommand();
                cmd.CommandText = "update LOW_PRIORITY `fos_user` set  `credit`=" + credit_user + " where `Id`=" + id_user + ";";
               int ww = cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (MySqlException ex)
            {
                err = ex.Message;
                throw;
            }

        }
        private List<user_bet_table> Base;
        private String err;
        private String connParams = "Server=192.168.1.16; Port=3306; Database=symfony; User ID=root; Password=db-4dm1n; Pooling=false;";
        private IDbConnection conn;
        private IDbCommand cmd;
      //  private Int32 id_user;
        private Int32 credit;
    }
}
