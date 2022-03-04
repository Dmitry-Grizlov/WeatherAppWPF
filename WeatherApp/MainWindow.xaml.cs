using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace _01._02._2022
{
    public partial class MainWindow : Window
    {
        int dayCounter;
        DetectIp detect;
        MasterWeather master;
        bool isFarengheitPressed = false;
        readonly Dictionary<int, string> dayOfWeek = new Dictionary<int, string>()
        {
            {1, "Monday" },
            {2, "Tuesday" },
            {3, "Wednesday" },
            {4, "Thursday" },
            {5, "Friday" },
            {6, "Saturday" },
            {7, "Sunday" }
        };

        string feelsLike;
        string currentTemperature;

        public MainWindow()
        {
            InitializeComponent();
            menuButton.Click += BtnOpen_Click;
            GetCurrentCity();
            DayOfWeekDisplay();
            GetWeather();
        }

        // To use register api from https://extreme-ip-lookup.com/ , using Newtonsoft.Json;
        void GetCurrentCity()
        {
            string url = "https://extreme-ip-lookup.com/json/?key=VwRYE70EOyh8yDGe9oWC";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string response;
            using (StreamReader sr = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = sr.ReadToEnd();
            }
          
            detect = JsonConvert.DeserializeObject<DetectIp>(response);
            currCity.Text =  detect.city;
        }

        void GetWeather()
        {
            // Get information from API
            string url = $"https://api.openweathermap.org/data/2.5/onecall?lat={detect.lat}&lon={detect.lon}&units=metric&exclude=minutely,alerts&appid=e3b5657a21f2f4078e6830c572c2992a";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string response;
            using (StreamReader sr = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = sr.ReadToEnd();
            }

            master = JsonConvert.DeserializeObject<MasterWeather>(response);

            FillFields(master);
        }

        private void FillFields(MasterWeather master)
        {
            try
            {
                currentTemperature = Convert.ToInt32((float)Convert.ToDouble(master.current.temp.Replace('.', ','))).ToString();
                feelsLike = Convert.ToInt32((float)Convert.ToDouble(master.current.feels_like.Replace('.', ','))).ToString();

                currTime.Text = TimeStampToDateTime(Convert.ToDouble(master.current.dt)).ToShortTimeString();
                currTemp.Text = currentTemperature + "°";
                currentWeatherState.Text = master.current.weather[0].main;
                currentWeatherConditionImage.Source = WeatherState(master.current.weather[0].id);

                feels_like.Text = "feels like " + feelsLike + "°";
                
                for (int i = 0; i < parentForHourly.Children.Count; i++)
                {
                    StackPanel sp = parentForHourly.Children[i] as StackPanel;
                    TextBlock temperature = sp.Children[2] as TextBlock;
                    TextBlock time = sp.Children[0] as TextBlock;
                    Image hourlyImg = sp.Children[1] as Image;
                    temperature.Text = Convert.ToInt32((float)Convert.ToDouble(master.hourly[i].temp.Replace('.', ','))).ToString() + "°";
                    time.Text = TimeStampToDateTime(Convert.ToDouble(master.hourly[i].dt)).ToShortTimeString();
                    hourlyImg.Source = MiniatureWeatherState(master.hourly[i].weather[0].id);
                }

                int counter = 0;

                for (int i = 0; i < parentForDaily.Children.Count; i++)
                {
                    if (parentForDaily.Children[i].GetType().ToString() != "System.Windows.Controls.GridSplitter")
                    {
                        StackPanel outer = parentForDaily.Children[i] as StackPanel;
                        StackPanel inner = outer.Children[0] as StackPanel;
                        Image weatherImage = inner.Children[1] as Image;
                        TextBlock dayTemp = inner.Children[2] as TextBlock;
                        TextBlock nightTemp = inner.Children[3] as TextBlock;
                        TextBlock weekDay = inner.Children[0] as TextBlock;

                        dayTemp.Text = Convert.ToInt32((float)Convert.ToDouble(master.daily[counter].temp.day.Replace('.', ','))).ToString() + "°"; ;
                        weatherImage.Source = MiniatureWeatherState(master.daily[counter].weather[0].id);
                        nightTemp.Text = Convert.ToInt32((float)Convert.ToDouble(master.daily[counter].temp.night.Replace('.', ','))).ToString() + "°";
                        counter++;

                        if (weekDay.Text == "Monday" || weekDay.Text == "Tuesday")
                            weatherImage.Margin = new Thickness(60, 0, 0, 0);
                        else if (weekDay.Text == "Wednesday")
                            weatherImage.Margin = new Thickness(15, 0, 0, 0);
                        else if (weekDay.Text == "Thursday")
                            weatherImage.Margin = new Thickness(48, 0, 0, 0);
                        else if (weekDay.Text == "Friday")
                            weatherImage.Margin = new Thickness(89, 0, 0, 0);
                        else if (weekDay.Text == "Saturday")
                            weatherImage.Margin = new Thickness(52, 0, 0, 0);
                        else if (weekDay.Text == "Sunday")
                            weatherImage.Margin = new Thickness(71, 0, 0, 0);
                    }
                }

                isFarengheitPressed = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public ImageSource WeatherState(string weatherCond)
        {
            if (Convert.ToInt32(weatherCond) >= 200 && Convert.ToInt32(weatherCond) <= 232 ||
               Convert.ToInt32(weatherCond) >= 700 && Convert.ToInt32(weatherCond) <= 781 ||
               Convert.ToInt32(weatherCond) > 800 && Convert.ToInt32(weatherCond) <= 804)
                return cloudyImage.Source;
            else if(Convert.ToInt32(weatherCond) >= 300 && Convert.ToInt32(weatherCond) <= 321 ||
               Convert.ToInt32(weatherCond) >= 500 && Convert.ToInt32(weatherCond) <= 531)
                return rainyImage.Source;
            else if (Convert.ToInt32(weatherCond) >= 600 && Convert.ToInt32(weatherCond) <= 622)
                return snowyImage.Source;
            else
                return sunnyImage.Source;
        }

        public ImageSource MiniatureWeatherState(string weatherCond)
        {
            if (Convert.ToInt32(weatherCond) >= 200 && Convert.ToInt32(weatherCond) <= 232 ||
               Convert.ToInt32(weatherCond) >= 700 && Convert.ToInt32(weatherCond) <= 781 ||
               Convert.ToInt32(weatherCond) > 800 && Convert.ToInt32(weatherCond) <= 804)
                return cloudyMiniature.Source;
            else if (Convert.ToInt32(weatherCond) >= 300 && Convert.ToInt32(weatherCond) <= 321 ||
               Convert.ToInt32(weatherCond) >= 500 && Convert.ToInt32(weatherCond) <= 531)
                return rainyMiniature.Source;
            else if (Convert.ToInt32(weatherCond) >= 600 && Convert.ToInt32(weatherCond) <= 622)
                return snowyMiniature.Source;
            else
                return sunnyMiniature.Source;
        }

        private void DayOfWeekDisplay()
        {
            foreach (var day in dayOfWeek)
                if(day.Value == DateTime.Now.DayOfWeek.ToString())
                    dayCounter = day.Key;
            
            for (int i = 0; i < parentForDaily.Children.Count; i++)
            {
                if (parentForDaily.Children[i].GetType().ToString() != "System.Windows.Controls.GridSplitter")
                {
                    StackPanel outer = parentForDaily.Children[i] as StackPanel;
                    StackPanel inner = outer.Children[0] as StackPanel;
                    TextBlock weekDay = inner.Children[0] as TextBlock;
                    Image img = inner.Children[1] as Image;
                    TextBlock dayTemp = inner.Children[2] as TextBlock;
                    TextBlock nightTemp = inner.Children[3] as TextBlock;
                    weekDay.Text = dayOfWeek[dayCounter];
                    dayCounter++;
                    if (dayCounter == 7)
                        dayCounter = 1;
                    dayTemp.TextAlignment = TextAlignment.Center;
                    nightTemp.TextAlignment = TextAlignment.Center;
                    img.Width = 45;
                }
            }
        }

        DateTime TimeStampToDateTime(double TimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(TimeStamp).ToLocalTime();
            return dateTime;
        }

        void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = Resources["openMenu"] as Storyboard;
            sb.Begin(leftMenu);
        }

        void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            StoryboardClose();
        }

        void StoryboardClose()
        {
            Storyboard sb = Resources["closeMenu"] as Storyboard;
            sb.Begin(leftMenu);
        }

        void GetWeather(DetectIp detect)
        {
            // Get information from API
            string url = $"https://api.openweathermap.org/data/2.5/onecall?lat={detect.lat}&lon={detect.lon}&units=metric&exclude=minutely,alerts&appid=e3b5657a21f2f4078e6830c572c2992a";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string response;
            using (StreamReader sr = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = sr.ReadToEnd();
            }

            master = JsonConvert.DeserializeObject<MasterWeather>(response);

            FillFields(master);
        }

        public void SearchCity()
        {
            if (string.IsNullOrEmpty(cityToSearch.Text))
            {
                MessageBox.Show("Строка пуста");
                return;
            }

            try
            {
                string url = $"https://eu1.locationiq.com/v1/search.php?key=pk.c3f6087c389649f6b1b01a3fc541215a&q={cityToSearch.Text}&format=json&country=Russia";
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                string response;
                using (StreamReader sr = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    response = sr.ReadToEnd();
                }

                List<MasterSearchCityCoord> searchMaster = new List<MasterSearchCityCoord>();
                var data = JsonConvert.DeserializeObject<List<MasterSearchCityCoord>>(response);

                DetectIp searchDetect = new DetectIp
                {
                    lat = data[0].lat.ToString(),
                    lon = data[0].lon.ToString(),
                    city = data[0].display_name.Split(',')[0]
                };
                currCity.Text = searchDetect.city;
                cityToSearch.Text = null;
                GetWeather(searchDetect);

                StoryboardClose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LocalData(object sender, RoutedEventArgs e)
        {
            GetCurrentCity();
            GetWeather();
        }

        void ConvertToFarengheit()
        {
            currTemp.Text = FarengheitConverter(currentTemperature) + "°";
            feels_like.Text = "feels like " + FarengheitConverter(feelsLike) + "°";

            for (int i = 0; i < parentForHourly.Children.Count; i++)
            {
                StackPanel sp = parentForHourly.Children[i] as StackPanel;
                TextBlock temperature = sp.Children[2] as TextBlock;
                temperature.Text = FarengheitConverter(Convert.ToInt32((float)Convert.ToDouble(master.hourly[i].temp.Replace('.', ','))).ToString()) + "°";
            }

            int counter = 0;
            for (int i = 0; i < parentForDaily.Children.Count; i++)
            {
                if (parentForDaily.Children[i].GetType().ToString() != "System.Windows.Controls.GridSplitter")
                {
                    StackPanel outer = parentForDaily.Children[i] as StackPanel;
                    StackPanel inner = outer.Children[0] as StackPanel;
                    TextBlock dayTemp = inner.Children[2] as TextBlock;
                    TextBlock nightTemp = inner.Children[3] as TextBlock;
                    dayTemp.Text = FarengheitConverter(Convert.ToInt32((float)Convert.ToDouble(master.daily[counter].temp.day.Replace('.', ','))).ToString()) + "°";
                    nightTemp.Text = FarengheitConverter(Convert.ToInt32((float)Convert.ToDouble(master.daily[counter].temp.night.Replace('.', ','))).ToString()) + "°";
                    counter++;
                }
            }
            StoryboardClose();
            isFarengheitPressed = true;
        }

        string FarengheitConverter(string data)
        {
            try
            {
                return (Convert.ToInt32(data) * 9 / 5 + 32).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return data;
            }
            
        }

        private void ConvertCelsius(object sender, RoutedEventArgs e)
        {
            if (isFarengheitPressed == false)
            {
                StoryboardClose();
                return;
            }
            cityToSearch.Text = currCity.Text;
            SearchCity();
            isFarengheitPressed = false;
        }

        private void Search_Click(object sender, MouseButtonEventArgs e)
        {
            SearchCity();
        }

        private void Enter_Detect(object sender, KeyEventArgs e)
        { 
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(cityToSearch.Text))
                SearchCity();
        }

        private void ConvertFarengheit(object sender, RoutedEventArgs e)
        {
            if (isFarengheitPressed)
                return;
            ConvertToFarengheit();
        }
    }

    public class DetectIp
    {
        public string city;

        // Needs for weather api request
        public string lat;
        public string lon;
    }

    public class MasterWeather
    {
        public CurrentWeather current;
        public HourlyWeather[] hourly;
        public DailyWeather[] daily;
        public string lat;
        public string lon;
        public string timezone;
        public string timezone_offset;
    }

    public class CurrentWeather
    {
        public string temp;
        public string feels_like;
        public string dt;
        public Weather[] weather;
    }

    public class Weather
    {
        public string main;
        public string id;
    }

    public class HourlyWeather
    {
        public string temp;
        public string dt;
        public Weather[] weather;
    }


    public class DailyWeather
    {
        public string dt;
        public string humidity;
        public DailyTemp temp;
        public Weather[] weather;
    }

    public class DailyTemp
    {
        public string day;
        public string night;
    }


    public class MasterSearchCityCoord
    {
        public double lat;
        public double lon;
        public string display_name;
    }
}
