﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;
using System.Web.UI.WebControls;

namespace SuperSecureBank
{
	public class AccountMgmt
	{
		public static DataTable GetAccounts(int UserID)
		{
            
			string selectAccounts = @"SELECT * FROM FriendlyAccounts
									WHERE userID = {0}";

			DataTable dt = new DataTable();
            try
            {
                dt.Columns.Add("accountID");
                dt.Columns.Add("balance");
                dt.Columns.Add("LevelName");
                dt.Columns.Add("LevelDescription");
                dt.Columns.Add("TypeName");
                dt.Columns.Add("TypeDescription");
                dt.Columns.Add("Status");

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ssbcon"].ConnectionString))
                {
                    conn.Open();
                    selectAccounts = String.Format(selectAccounts, UserID);
                    SqlCommand command = new SqlCommand(selectAccounts, conn);
                    SqlDataReader reader = command.ExecuteReader();
                    int totalValue = 0;
                    while (reader.Read())
                    {
                        DataRow dr = dt.NewRow();
                        dr["accountID"] = reader.GetInt32(1);
                        dr["balance"] = string.Format("{0:C}", reader.GetInt32(2));
                        dr["LevelName"] = reader.GetString(3);
                        dr["LevelDescription"] = reader.GetString(4);
                        dr["TypeName"] = reader.GetString(5);
                        dr["TypeDescription"] = reader.GetString(6);
                        dr["Status"] = reader.GetString(7);

                        totalValue += reader.GetInt32(2);

                        dt.Rows.Add(dr);
                    }

                    DataRow footer = dt.NewRow();
                    footer["accountID"] = "<strong>Total:</strong>";
                    footer["balance"] = string.Format("<strong>{0:C}</strong>", totalValue);
                    footer["LevelName"] = "";
                    footer["LevelDescription"] = "";
                    footer["TypeName"] = "";
                    footer["TypeDescription"] = "";
                    footer["Status"] = "";
                    dt.Rows.Add(footer);
                }
            }
            catch
            {
                throw;
            }
            return dt;
		}

		public static int GetBalance(int accountID)
		{
			string selectAccount = @"SELECT balance FROM Accounts WHERE accountID = {0}";
			int balance = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ssbcon"].ConnectionString))
                {
                    conn.Open();
                    selectAccount = String.Format(selectAccount, accountID);
                    SqlCommand command = new SqlCommand(selectAccount, conn);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        balance = reader.GetInt32(0);
                    }
                }
            }
            catch
            {
                throw;
            }
			return balance;
		}

		public static void Transfer(int FromAccount, int ToAccount, int Amount)
		{
            try
            {
                int FromAccountBalance = GetBalance(FromAccount);
                int ToAccountBalance = GetBalance(ToAccount);

                UpdateBalance(ToAccount, ToAccountBalance + Amount);
                Thread.Sleep(new Random().Next(1000, 5000)); //simulate a long account wire transfer that takes 1-5 seconds to happen
                UpdateBalance(FromAccount, FromAccountBalance - Amount);
            }
            catch
            {
                throw;
            }
		}

		public static void UpdateBalance(int Account, int NewAmount)
		{
            try
            {
                string updateAmount = @"UPDATE Accounts SET balance={0} WHERE accountID = {1}";
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ssbcon"].ConnectionString))
                {
                    conn.Open();
                    updateAmount = String.Format(updateAmount, NewAmount, Account);
                    SqlCommand command = new SqlCommand(updateAmount, conn);
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                throw;
            }
		}

		internal static ListItem[] GetAccountList(int userID)
		{
			List<ListItem> items = new List<ListItem>();
            try
            {
                string selectAccounts = @"SELECT * FROM FriendlyAccounts WHERE userID = {0}";
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ssbcon"].ConnectionString))
                {
                    conn.Open();
                    selectAccounts = String.Format(selectAccounts, userID);
                    SqlCommand command = new SqlCommand(selectAccounts, conn);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int accountID = reader.GetInt32(1);
                        int balance = reader.GetInt32(2);
                        items.Add(new ListItem(string.Format("{0} : {1:C}", accountID, balance), accountID.ToString(), balance > 0));
                    }
                }
            }
            catch
            {
                throw;
            }
			return items.ToArray();
		}

		internal static void CreateAccount(int userID, string accountType, string balance, string accountLevel, int status)
		{
            try
            {
                string insertNewAccount = "INSERT INTO Accounts VALUES ({0}, {1}, {2}, {3}, {4})";
                insertNewAccount = string.Format(insertNewAccount, userID, accountType, balance, accountLevel, status);
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ssbcon"].ConnectionString))
                {
                    conn.Open();
                    SqlCommand command = new SqlCommand(insertNewAccount, conn);
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                throw;
            }
		}
	}
}