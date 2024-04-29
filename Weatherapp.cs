using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WeatherLogApplication
{
    public class WeatherEntry
    {
        public string Date { get; set; }
        public double Temperature { get; set; }
    }

    class Program
    {
        static string weatherLogFilePath = "Weatherlog.json";
        static string YRApiUrl = "https://api.met.no/weatherapi/locationforecast/2.0/compact?lat=60&lon=11";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Weather Log Application");

            while (true)
            {
                DisplayMenu();
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await LogWeatherData();
                        break;
                    case "2":
                        ViewReports();
                        break;
                    case "3":
                        ViewLogHistory();
                        break;
                    case "4":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        static void DisplayMenu()
        {
            Console.WriteLine("\nMenu:");
            Console.WriteLine("1. Log weather data for today");
            Console.WriteLine("2. View reports");
            Console.WriteLine("3. View log history");
            Console.WriteLine("4. Exit");
            Console.Write("Enter your choice: ");
        }

        static async Task LogWeatherData()
        {
            try
            {
                Console.WriteLine("\nEnter weather measurements for today:");
                Console.Write("Temperature (°C): ");
                string temperatureInput = Console.ReadLine();
                if (!double.TryParse(temperatureInput, out double temperature))
                {
                    Console.WriteLine("Invalid temperature input. Please enter a valid number.");
                    return;
                }

                var newEntry = new WeatherEntry
                {
                    Date = DateTime.Now.ToString("yyyy-MM-dd"),
                    Temperature = temperature
                };

                List<WeatherEntry> weatherLog = new List<WeatherEntry>();
                if (File.Exists(weatherLogFilePath))
                {
                    string jsonData = File.ReadAllText(weatherLogFilePath);
                    weatherLog = JsonSerializer.Deserialize<List<WeatherEntry>>(jsonData);
                }

                weatherLog.Add(newEntry);

                string jsonString = JsonSerializer.Serialize(weatherLog);
                File.WriteAllText(weatherLogFilePath, jsonString);

                Console.WriteLine("Weather data logged successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging weather data: {ex.Message}");
            }
        }

        static async Task<dynamic> FetchWeatherDataAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Weatherapp", "ssdiab@uia.no");

                    string response = await client.GetStringAsync(YRApiUrl);
                    dynamic jsonData = JsonSerializer.Deserialize<dynamic>(response);
                    return jsonData;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fetch weather data from the API.", ex);
            }
        }

        static void ViewReports()
        {
            Console.WriteLine("\nView Reports:");
            Console.WriteLine("1. Day report");
            Console.WriteLine("2. Week report");
            Console.WriteLine("3. Month report");
            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    DayReport();
                    break;
                case "2":
                    WeekReport();
                    break;
                case "3":
                    MonthReport();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }

        private static void WeekReport()
        {
            throw new NotImplementedException();
        }

        private static void MonthReport()
        {
            throw new NotImplementedException();
        }

        static void DayReport()
        {
            Console.WriteLine("\nEnter date for the report (yyyy-MM-dd): ");
            string dateStr = Console.ReadLine();
            DateTime date = DateTime.Parse(dateStr);

            dynamic weatherData = FetchWeatherDataForDate(date).Result;

            if (weatherData != null)
            {
                double temperature = (double)weatherData["properties"]["timeseries"][0]["data"]["instant"]["details"]["air_temperature"];

                Console.WriteLine($"\nDay Report for {date.ToShortDateString()}:");
                Console.WriteLine($"Temperature: {temperature} °C");
            }
            else
            {
                Console.WriteLine("No weather data available for the specified date.");
            }
        }

        static async Task<dynamic> FetchWeatherDataForDate(DateTime date)
        {
            try
            {
                string apiUrl = $"https://api.met.no/weatherapi/locationforecast/2.0/compact?lat=59.93&lon=10.72&altitude=90&time={date:yyyy-MM-dd}T12:00:00Z";

                using (HttpClient client = new HttpClient())
                {
                    string response = await client.GetStringAsync(apiUrl);
                    dynamic jsonData = JsonSerializer.Deserialize<dynamic>(response);
                    return jsonData;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch weather data from the API: {ex.Message}");
                return null;
            }
        }

        static async Task<dynamic> FetchWeatherDataForPeriod(DateTime startDate, DateTime endDate)
        {
            try
            {
                string apiUrl = $"https://api.met.no/weatherapi/locationforecast/2.0/compact?lat=59.93&lon=10.72&altitude=90&start={startDate:yyyy-MM-dd}T00:00:00Z&end={endDate:yyyy-MM-dd}T23:59:59Z";

                using (HttpClient client = new HttpClient())
                {
                    string response = await client.GetStringAsync(apiUrl);
                    dynamic jsonData = JsonSerializer.Deserialize<dynamic>(response);
                    return jsonData;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch weather data from the API: {ex.Message}");
                return null;
            }
        }

       static void ViewLogHistory()
{
    try
    {
        if (File.Exists(weatherLogFilePath))
        {
            string jsonData = File.ReadAllText(weatherLogFilePath);
            List<WeatherEntry> weatherLog = JsonSerializer.Deserialize<List<WeatherEntry>>(jsonData);

            Console.WriteLine("\nLog History:");
            Console.ForegroundColor = ConsoleColor.Blue; // Set text color to blue

            foreach (var entry in weatherLog)
            {
                Console.WriteLine($"Date: {entry.Date}, Temperature: {entry.Temperature} °C");
            }

            Console.ResetColor(); // Reset text color to default
        }
        else
        {
            Console.WriteLine("No log data available.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error viewing log history: {ex.Message}");
    }
}


        private static double CalculateAverageTemperature(dynamic weatherData)
        {
            double sum = 0.0;
            int count = 0;

            foreach (var dataPoint in weatherData["timeseries"])
            {
                double temperature = (double)dataPoint["data"]["instant"]["details"]["air_temperature"];
                sum += temperature;
                count++;
            }

            return count > 0 ? sum / count : 0.0;
        }

        private static double CalculateMaxTemperature(dynamic weatherData)
        {
            double maxTemperature = double.MinValue;

            foreach (var dataPoint in weatherData["timeseries"])
            {
                double temperature = (double)dataPoint["data"]["instant"]["details"]["air_temperature"];
                if (temperature > maxTemperature)
                {
                    maxTemperature = temperature;
                }
            }

            return maxTemperature != double.MinValue ? maxTemperature : 0.0;
        }

        private static double CalculateMinTemperature(dynamic weatherData)
        {
            double minTemperature = double.MaxValue;

            foreach (var dataPoint in weatherData["timeseries"])
            {
                double temperature = (double)dataPoint["data"]["instant"]["details"]["air_temperature"];
                if (temperature < minTemperature)
                {
                    minTemperature = temperature;
                }
            }

            return minTemperature != double.MaxValue ? minTemperature : 0.0;
        }
    }
}
