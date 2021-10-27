﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;



namespace CityPuzzle.Classes
{
    class Sql
    {
        public static string ConnStr = "Server = tcp:citypuzzledb.database.windows.net,1433;Initial Catalog = citypuzzledb; Persist Security Info=False;User ID = citypuzzleuser; Password=User123*; MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30;";

        public static void SaveUser(User user)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                var command = new SqlCommand("INSERT INTO Users (UserName,FirstName,LastName,Pass,Email) VALUES (@UserName,@FirstName,@LastName,@Pass,@Email)", conn);
                command.Parameters.AddWithValue("@UserName", user.UserName);
                command.Parameters.AddWithValue("@FirstName", user.Name);
                command.Parameters.AddWithValue("@LastName", user.LastName);
                command.Parameters.AddWithValue("@Pass", user.Pass);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static String ReadUsers()
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                SqlCommand command;
                SqlDataReader dataReader;
                String sql, Output = "";

                sql = "Select ID,UserName,FirstName,LastName,Pass,Email from Users";
                conn.Open();
                command = new SqlCommand(sql, conn);
                dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    Output = Output + dataReader.GetValue(0) + "-" + dataReader.GetValue(1) + "-" + dataReader.GetValue(3) + "-" + dataReader.GetValue(4) + "-" + dataReader.GetValue(5);
                }
                conn.Close();
                return Output;

            }
        }



    }
}
