using Microsoft.AspNetCore.Mvc;
using Mobile_Web_Api.Model;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using WEBAPITESTPROJECT.Services;
using Microsoft.AspNetCore.Cors;



namespace Mobile_Web_Api.Controllers
{
    [ApiController]
    [EnableCors("AllowCors"), Route("[controller]")]
    public class ApiController : Controller
    {
        private readonly IConfiguration _configuration;

        public ApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public static List<Helmet> Helmets = new List<Helmet>()
        {
            
        };
        [HttpPost]
        [Route("DisconnectingAlert")]
        public IActionResult DisconnectingAlert(List<Helmet> helmet)
        {
            string connection = _configuration.GetConnectionString("DefaultConnection");
            Logging logging = new Logging();
            using (SqlConnection connect = new SqlConnection(connection))
            {
                try
                {
                    connect.Open();
                    foreach (var item in helmet)
                    {
                        var qryInsert = "Insert into Disconnecting_Alert(Device_Id,USER_ID,Disconect_Time," +
                        "Disconnect_Reason_Code,Latitude,Longitude,Vehicle_Type,Created_On,Created_By," +
                        "Updated_On,Updated_By) Values( '" + item.Helmet_ID + "'," +
                        "(SELECT User_Id FROM User_Job_details WHERE Device_Id='" + item.Helmet_ID + "')," +
                        "GETDATE(), '" + item.Disconnect_Reason_Code + "'," +
                        "'" + item.Latitude + "','" + item.Longitude + "'," +
                        "'" + item.Vehicle_Type + "',GETDATE(),'" + item.Created_By + "'," +
                        "GETDATE(),'" + item.Updated_By + "')";
                        SqlCommand cmdInsert = new SqlCommand(qryInsert, connect);
                        cmdInsert.ExecuteNonQuery();

                    }
                    connect.Close();

                }
                catch (Exception ex)
                {
                    logging.logger(ex.Message + " Disconnecting_Alert-Sql-Error", 1);
                    return BadRequest(ex.Message);

                }
                finally
                {
                    connect.Close();
                }

                /*                using (var mysqlConnection = new MySqlConnection(oldDB_Connection))
                                {
                                    try
                                    {
                                        mysqlConnection.Open();
                                        foreach (var item in cat)
                                        {
                                            string date_from = item.Disconnect_Time.ToString("yyyy/MM/dd HH:mm");
                                            var mySQLQuery = $"INSERT INTO nohademo.disconnectingalert" +
                                                $"(DeviceId, extra1," +
                                                $"Reason,DisconnectionTime," +
                                                $"Latitude,Longitude,extra2," +
                                                $"Version,IsNew,IsDeleted,Attitude)" +
                                                $"Values ('{item.Helmet_ID}','{item.Helmet_ID}'," +
                                                $"{item.Disconnect_Reason_Code},'{date_from}'," +
                                                $"{item.Latitude},{item.Longitude},'{item.Vehicle_Type}'," +
                                                $"0,0,0,0.0)";

                                            MySqlCommand mySqlCommand = new MySqlCommand(mySQLQuery, mysqlConnection);
                                            mySqlCommand.ExecuteNonQuery();

                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        logging.logger(ex.Message + " Disconnecting_Alert-Mysql-Error", 2);
                                        return BadRequest(ex.Message);
                                    }
                                    finally
                                    {
                                        mysqlConnection.Close();
                                    }

                                }*/

            }
            Enumerable.Concat(Helmets,helmet);
            return Ok("Data has been inserted");
        }

        [HttpPost]
        [Route("DisconnectingReason")]
        public IActionResult DisconnectingReason(List<Helmet> helmet)
        {
            string connection = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connect = new SqlConnection(connection))
            {
                Logging logging = new Logging();
                try
                {
                    connect.Open();
                    foreach (var item in helmet)
                    {
                        var query = $@"Select  COUNT,DATE from APPLICATION_DISCONNECTING_REASON  where HELMET_ID='" + item.Helmet_ID + "' AND  REASON='" + item.REASON + "' AND STATUS_DESC='" + item.STATUS_DESC + "' AND DATE='" + item.DATE + "' ";
                        SqlCommand cmd = new SqlCommand(query, connect);
                        int count = 0;
                        {
                            SqlDataReader dreader = cmd.ExecuteReader();
                            if (dreader.Read())
                            {
                                DateTime LastDate = Convert.ToDateTime(dreader[1].ToString());
                                TimeSpan ts = Convert.ToDateTime(item.DATE) - LastDate;
                                if (ts.TotalMinutes < 60 * 60)
                                {
                                    if (Convert.ToInt32(dreader[0].ToString()) == 0)
                                    {
                                        count = 1;
                                    }
                                    else
                                    {
                                        count = Convert.ToInt32(dreader[0].ToString()) + 1;
                                    }
                                }
                            }
                            else if (!dreader.Read())
                            {
                                count = 1;
                            }
                            dreader.Close();

                            if (count <= 1)
                            {
                                var qryInsert = @"Insert into APPLICATION_DISCONNECTING_REASON  
                                    values ('" + item.Helmet_ID + "',(select USER_ID from User_Job_details where Device_Id='" + item.Helmet_ID + "'),'" + item.REASON + "'," +
                                    "'" + item.STATUS_DESC + "','" + count + "','" + item.DATE + "')";
                                SqlCommand cmdInsert = new SqlCommand(qryInsert, connect);
                                cmdInsert.ExecuteNonQuery();
                            }
                            else if (count > 1)
                            {
                                var qryInsert = @"update APPLICATION_DISCONNECTING_REASON  
                                    set COUNT='" + count + "' where HELMET_ID='" + item.Helmet_ID + "' AND USER_ID='" + item.User_Id + "' ";
                                SqlCommand cmdInsert = new SqlCommand(qryInsert, connect);
                                cmdInsert.ExecuteNonQuery();

                            }

                        }

                    }
                    connect.Close();
                }
                catch (Exception ex)
                {
                    logging.logger(ex.Message + " DisconnectingReason-Sql_Error", 1);
                    return BadRequest(ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            }
            Enumerable.Concat(Helmets, helmet);
            //Category.(cat);
            return Ok("Data has been inserted");
        }

        [HttpPost]
        [Route("TrJourney")]
        public async Task<IActionResult> TrJourney(List<Helmet> cat)
        {
            string connection = _configuration.GetConnectionString("DefaultConnection");
            Logging logging = new Logging();
            string q = "";
            using (SqlConnection connect = new SqlConnection(connection))
            {
                try
                {
                    int configjourney = 0;
                    int JourneyID = 1;
                    connect.Open();

                    foreach (var item in cat)
                    {
                        DateTime Date = DateTime.Parse(item.API_DateTime.ToString());
                        string Apidate = Date.ToString("yyyy-MM-dd");
                        var query = "Select  top 1 API_Date,Journey_ID from Tracking_details where API_Date>=DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) " +
                            "and user_Id=(SELECT User_Id FROM User_Job_details WHERE Device_Id='" + item.Helmet_ID + "')" +
                            "order by API_Date desc";
                        SqlCommand cmd = new SqlCommand(query, connect);
                        var queryjourney = "Select Config_Value from Application_Configuration where Config_id=2";
                        SqlCommand cmdjourney = new SqlCommand(queryjourney, connect);
                        SqlDataReader dreaderjourney = await cmdjourney.ExecuteReaderAsync();
                        if (dreaderjourney.Read())
                        {
                            configjourney = Convert.ToInt32(dreaderjourney[0]);
                        }
                        dreaderjourney.Close();
                        {
                            SqlDataReader dreader = await cmd.ExecuteReaderAsync();
                            if (dreader.Read())
                            {
                                JourneyID = Convert.ToInt32(dreader[1].ToString());
                                DateTime LastApiDate = Convert.ToDateTime(dreader[0].ToString());
                                TimeSpan ts = item.API_DateTime - LastApiDate;
                                if (ts.TotalMinutes > configjourney)
                                    JourneyID = JourneyID + 1;
                            }
                            dreader.Close();
                            {
                                int configspeed = 0;
                                var queryconfig = "Select Config_Value from Application_Configuration where Config_id=1";
                                SqlCommand cmdcongif = new SqlCommand(queryconfig, connect);
                                SqlDataReader dreaderconfig = await cmdcongif.ExecuteReaderAsync();
                                if (dreaderconfig.Read())
                                {
                                    configspeed = Convert.ToInt32(dreaderconfig[0]);
                                }
                                dreaderconfig.Close();
                                var queryspeed = $@"select top 1 * from Tracking_Details where User_Id = '{item.User_Id}'
                                                and Api_Date >= '{Apidate}' order by id desc";
                                SqlCommand cmdspeed = new SqlCommand(queryspeed, connect);
                                SqlDataReader drspeed = await cmdspeed.ExecuteReaderAsync();
                                double? res_Speed = 0;
                                double? res_Break = 0;

                                if (drspeed.Read())
                                {
                                    string sp = Convert.ToString(drspeed[4]);
                                    double? oldSpeed = Convert.ToDouble(sp.ToString());
                                    {
                                        res_Speed = item.Speed - oldSpeed;
                                        res_Break = oldSpeed - item.Speed;

                                    }
                                }
                                drspeed.Close();
                                int Harsh_Break = 0;
                                int Harsh_Speed = 0;
                                if (res_Speed > Convert.ToInt32(configspeed))
                                {
                                    Harsh_Speed = 1;
                                }
                                else if (res_Break > Convert.ToInt32(configspeed))
                                {
                                    Harsh_Break = 1;
                                }

                                if (item.Speed != 0)

                                {
                                    var qryInsert = "Insert into Tracking_Details(Device_Id,USER_ID,Api_Date,JOURNEY_ID,Created_On,Created_By,Updated_On,Updated_By,Speed,Is_Wrong_Way,Is_Wear_Helmet,Latitude,Longitude,Harsh_Speed,HARSH_BREAK) " +
                               "Values( '" + item.Helmet_ID + "',(SELECT User_Id FROM User_Job_details WHERE Device_Id='" + item.Helmet_ID + "'),'" + item.API_DateTime + "','" + JourneyID + "',GETDATE(),'" + item.Created_By + "',GETDATE(),'" + item.Updated_By + "','" +
                               item.Speed + "','" + item.Is_Wrong_Way + "','" + item.IS_Wear_Helmet + "','" + item.Latitude + "','" + item.Longitude + "'," + Harsh_Speed + "," + Harsh_Break + ")";
                                    SqlCommand cmdInsert = new SqlCommand(qryInsert, connect);
                                    await cmdInsert.ExecuteNonQueryAsync();
                                    q = qryInsert;
                                }


                            }

                        }

                    }
                    connect.Close();
                    logging.logger(" TrJourney-Sql", 1);
                }
                catch (Exception ex)
                {
                    logging.logger(ex.Message + " TrJourney-Sql-Error" + "{ " + q + " }", 1);
                    return BadRequest(ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            }
            /*            using (var mysqlConnection = new MySqlConnection(oldDB_Connection))
                        {
                            try
                            {
                                mysqlConnection.Open();
                                foreach (var item in cat)
                                {
                                    if (item.IS_Wear_Helmet == "0")
                                    {
                                        item.IS_Wear_Helmet = "Not Weared";
                                    }
                                    else
                                    {
                                        item.IS_Wear_Helmet = "Weared";
                                    }

                                    int Is_Wrong_Way = Convert.ToInt32(item.Is_Wrong_Way);
                                    string date_from = item.API_DateTime.ToString("yyyy/MM/dd HH:mm");
                                    string date = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
                                    var query = $"Select  APIDate,extra10 from nohademo.trackingdetails where APIDate>=CURDATE() and DeviceId='{item.Helmet_ID}';";
                                    MySqlCommand cmd = new MySqlCommand(query, mysqlConnection);
                                    int JourneyID = 1;

                                    {
                                        MySqlDataReader dreader = cmd.ExecuteReader();
                                        if (dreader.Read())
                                        {
                                            int JID = Convert.ToInt32(dreader[1].ToString());
                                            DateTime LastApiDate = Convert.ToDateTime(dreader[0].ToString());
                                            TimeSpan ts = item.API_DateTime - LastApiDate;
                                            if (ts.TotalMinutes > 30)
                                                JourneyID = JID + 1;
                                        }
                                        else
                                        {
                                            JourneyID = 1;
                                        }
                                        dreader.Close();

                                        var qryInsert = $@"Insert into nohademo.trackingdetails 
                                            (Version,IsNew,IsDeleted,DeviceId,username,Latitude,Longitude,ApiDate,extra10,SpeedStatus,WrongWay,VehicleType,
                                            CreatedDateTime,CreatedUser,ModifiedUser,ModifiedDateTime)
                                            Values( 0,0,0,'{item.Helmet_ID }','{item.User_Id}','{item.Latitude}','{item.Longitude}',
                                            '{date_from}','{JourneyID}',{item.Speed},{Is_Wrong_Way},'{item.IS_Wear_Helmet}',
                                            '{date}','{item.Created_By}','{item.Updated_By}','{date}')";
                                        MySqlCommand mySqlCommand = new MySqlCommand(qryInsert, mysqlConnection);
                                        mySqlCommand.ExecuteNonQuery();
                                    }


                                }
                            }
                            catch (Exception ex)
                            {
                                logging.logger(ex.Message + " TrJourney-MySql-Error", 2);
                                return BadRequest(ex.Message);
                            }

                            finally
                            {
                                mysqlConnection.Close();
                            }
                        }*/
            Enumerable.Concat(Helmets, cat);
            return Ok("Data has been inserted");
        }

        [HttpGet]
        [Route("GetMobileAlarmTime")]
        public string GetMobileAlarmTime()
        {
            string connection = _configuration.GetConnectionString("DefaultConnection");
            Logging logging = new Logging();
            using (SqlConnection connect = new SqlConnection(connection))
            {
                try
                {
                    connect.Open();
                    var queryjourney = "Select Config_Value from Application_Configuration where Config_Parameter='Mobile_Alaram_Time' ";
                    SqlCommand cmdjourney = new SqlCommand(queryjourney, connect);
                    SqlDataReader dreaderjourney = cmdjourney.ExecuteReader();
                    if (dreaderjourney.Read())
                    {

                        DateTime dateTimeValue = Convert.ToDateTime(dreaderjourney[0]);
                        string formattedTime = dateTimeValue.ToString("HH:mm:ss");
                        return formattedTime;
                    }
                    connect.Close();
                }
                catch (Exception ex)
                {
                    return ex.Message.ToString();
                    logging.logger(ex.Message + " MobileAlarm-Sql-Error", 1);
                }
            }
            return null;
        }
    }


}
