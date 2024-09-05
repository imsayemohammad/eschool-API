﻿using ChalkboardAPI.Helpers;
using ChalkboardAPI.Models;
using ESCHOOL.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ESCHOOL.Services
{
    public interface IStdAttendanceServices
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        IEnumerable<StdAttendance> GetAll();
        List<StdAttendance> GetById(int id);
    }
    public class StdAttendanceServices : IStdAttendanceServices
    {
        private List<StdAttendance> _students = new List<StdAttendance>
        {
            new StdAttendance { StdAttendanceId = 1,StdId="1",StdAttClassId="1",
                StdAttSectionId="1", StdAttDate = DateTime.Now, StdStatus = "test", StdAttEntryBy = "test",
                MonthName = "test" ,SchoolId=1 }
        };

        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;

        public StdAttendanceServices(IOptions<AppSettings> appSettings, IConfiguration configuration)
        {
            _configuration = configuration;
            _appSettings = appSettings.Value;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _students.SingleOrDefault(x => x.MonthName == model.Email && x.StdAttEntryBy == model.Password);

            // return null if user not found
            if (user == null) return null;

            // authentication successful so generate jwt token
            var token = generateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }

        public IEnumerable<StdAttendance> GetAll()
        {
            List<StdAttendance> studentProfileViews = new List<StdAttendance>();

            string connectionString = _configuration.GetConnectionString("StudentDB");
            SqlConnection connection = new SqlConnection(connectionString);
            string query = "Select * FROM StdAttendance";
            SqlCommand com = new SqlCommand(query, connection);
            connection.Open();
            SqlDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                StdAttendance studentProfileView = new StdAttendance();
                studentProfileView.StdAttendanceId = Convert.ToInt32(reader["StdAttendanceId"]);
                studentProfileView.StdId = reader["StdId"].ToString();
                studentProfileView.StdAttClassId = reader["StdAttClassId"].ToString();
                studentProfileView.StdAttSectionId = reader["StdAttSectionId"].ToString();
                studentProfileView.StdAttDate = Convert.ToDateTime(reader["StdAttDate"]);
                studentProfileView.StdStatus = reader["StdStatus"].ToString();
                studentProfileView.StdAttEntryBy = reader["StdAttEntryBy"].ToString();
                studentProfileView.MonthName = reader["MonthName"].ToString();
                studentProfileView.SchoolId = Convert.ToInt32(reader["SchoolId"]);
                studentProfileViews.Add(studentProfileView);
            }
            reader.Close();
            connection.Close();
            return studentProfileViews;
            //return _students;
        }

        public List<StdAttendance> GetById(int id)
        {
            List<StdAttendance> stdAttendances = new List<StdAttendance>();
            string connectionString = _configuration.GetConnectionString("StudentDB");
            SqlConnection connection = new SqlConnection(connectionString);
            string query = "PrcStdAttendance ";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@stdid", id);
            connection.Open();
            SqlDataReader reader = com.ExecuteReader();
            while (reader.Read())
            {
                StdAttendance stdAttendance = new StdAttendance();
                stdAttendance.StdId = reader["StdId"].ToString();
                stdAttendance.StdAttClassId = reader["StdAttClassId"].ToString();
                stdAttendance.StdAttSectionId = reader["StdAttSectionId"].ToString();
                stdAttendance.January = reader["January"].ToString();
                stdAttendance.February = reader["February"].ToString();
                stdAttendance.March = reader["March"].ToString();
                stdAttendance.April = reader["April"].ToString();
                stdAttendance.May = reader["May"].ToString();
                stdAttendance.June = reader["June"].ToString();
                stdAttendance.July = reader["July"].ToString();
                stdAttendance.August = reader["August"].ToString();
                stdAttendance.September = reader["September"].ToString();
                stdAttendance.October = reader["October"].ToString();
                stdAttendance.November = reader["November"].ToString();
                stdAttendance.December = reader["December"].ToString();

                stdAttendances.Add(stdAttendance);
            }

            return stdAttendances;
        }

        #region
        //public List<StdAttendance> GetById(int id)
        //{
        //    List<StdAttendance> stdAttendances = new List<StdAttendance>();
        //    string connectionString = _configuration.GetConnectionString("StudentDB");
        //    SqlConnection connection = new SqlConnection(connectionString);
        //    string query = "Select * FROM StdAttendance where StdId=" + id + "";
        //    SqlCommand com = new SqlCommand(query, connection);
        //    connection.Open();
        //    SqlDataReader reader = com.ExecuteReader();
        //    while (reader.Read())
        //    {
        //        StdAttendance stdAttendance = new StdAttendance();
        //        stdAttendance.StdAttendanceId = Convert.ToInt32(reader["StdAttendanceId"]);
        //        stdAttendance.StdId = reader["StdId"].ToString();
        //        stdAttendance.StdAttClassId = reader["StdAttClassId"].ToString();
        //        stdAttendance.StdAttSectionId = reader["StdAttSectionId"].ToString();
        //        stdAttendance.StdAttDate = Convert.ToDateTime(reader["StdAttDate"]);
        //        stdAttendance.StdStatus = reader["StdStatus"].ToString();
        //        stdAttendance.StdAttEntryBy = reader["StdAttEntryBy"].ToString();
        //        stdAttendance.MonthName = reader["MonthName"].ToString();
        //        stdAttendance.SchoolId = Convert.ToInt32(reader["SchoolId"]);
        //        stdAttendances.Add(stdAttendance);

        //    }
        //    return stdAttendances;

        //    //return _students.FirstOrDefault(x => x.StdAttendanceId == id);
        //}


        // helper methods
        #endregion

        private string generateJwtToken(StdAttendance user)
        {
            // generate token that is valid for 3 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("StdId", user.StdId.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
