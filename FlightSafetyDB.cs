using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Dapper;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Cms;

namespace Flygsäkerhetsanalysverktyg
{
    public class FlightSafetyDB
    {
        private MySqlConnection cnn;
        public FlightSafetyDB()
        {
            //Lägg in ditt egna lösenord här
            string connectionString = "server=localhost;database=flightsafetydb;uid=root;pwd=Bananer!!!;";
            cnn = new MySqlConnection(connectionString);
        }

        
        public List<T> getDataFromDB<T>(string query, List<T> listName)
        {
            // öppnar databas, och via en string query anropa databasen och skriva ner svaret i en lista.
            //Returnerar en lista med objekt av typ beroende på vad som kallade funktioenen.
            listName = new List<T>();
            try 
            {
                cnn.Open();
                listName = cnn.Query<T>(query).ToList();
                cnn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return listName;
        }

        public int storeItemInDbGetId(string query)
        {
            //Skickar en query till databas och ger tillbaka en int. Används vid ny post i databas där int smo kommer tillbaka är id som kan användas lokalt i applikationen
            int id = 0;
            try
            {
                cnn.Open();
                id = cnn.QuerySingle<int>(query);
                cnn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return id;
        }

        public void updateItemInDB(string query)
        {
            //Skickar en query till databas som inte ger tillbaka nåt värde. T ex vid uppdateringar
            try
            {
                cnn.Open();
                cnn.Execute(query);
                cnn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public DataView fillDataTable(string query)
        {
            //Funktion för att använda en join-sats över junction-tables i en stored procedure...
            DataTable dt = new DataTable();
            try
            {
                MySqlCommand cmd = new MySqlCommand(query, cnn);

                cnn.Open();
                MySqlDataAdapter sdr = new MySqlDataAdapter(cmd);
                sdr.Fill(dt);
                DataGrid dataGrid = new DataGrid();
                dataGrid.ItemsSource = dt.DefaultView;
                cnn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return dt.DefaultView;
        }
    }
}
