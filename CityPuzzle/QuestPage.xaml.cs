﻿using CityPuzzle.Classes;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace CityPuzzle
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuestPage : ContentPage
    {
        private double UserLat;
        private double UserLng;
        private double QuestLat;
        private double QuestLng;
        private Puzzle[] Target;
        public static Puzzle QuestInProgress;

        public QuestPage()
        {
            InitializeComponent();
            using (SQLiteConnection conn = new SQLiteConnection(App.ObjectPath))
            {
                conn.CreateTable<Puzzle>();
                var obj = conn.Table<Puzzle>().ToArray();
                Target = obj;
            }
            
            if (Target.Length == 0)
            {
                Navigation.PushAsync(new AddObjectPage());
            }
            else
            {
                ShowQuest();
            }
        }

        async private void ShowQuest()
        {
            await UpdateCurrentLocation();
            Puzzle target = GetQuest(Target);

            if (target == null)      // when no nearby quests are found. Suggest creating a new one and exit to meniu
            {
                await DisplayAlert("No destinations in " + App.CurrentUser.maxQuestDistance + " km radius", "Consider creating a nearby destination yourself.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                QuestLat = target.Latitude;
                QuestLng = target.Longitude;
                QuestInProgress = target;
                var questImg = target.ImgAdress;
                objimg.Source = questImg;

                MissionLabel.Text = "Tavo uzduotis- surasti mane!";
                QuestField.Text = target.Quest;

                await RevealImg();    // Start the quest completion loop
                App.CurrentUser.QuestComlited.Add(target.Name);            // TO DO: save user data to database after finishing quest or loging out
                await DisplayAlert("Congratulations", "You have reached the destination", "OK");
            }
        }

        // Get a random quest that is within given distance and is not already completed by current user
        private Puzzle GetQuest(Puzzle[] puzzles)
        {
            bool InRange(Puzzle puzzle)
            {
                double dist = DistanceToPoint(puzzle.Latitude, puzzle.Longitude);
                if (dist <= App.CurrentUser.maxQuestDistance)
                    return true;
                return false;
            }

            //Linq query
            //List<Puzzle> inRange = puzzles.Where(puzzle => InRange(puzzle) && !App.CurrentUser.QuestComlited.Contains(puzzle.Name)).ToList();
            var inRange =
                (from puzzle in puzzles
                where InRange(puzzle)
                where !App.CurrentUser.QuestComlited.Contains(puzzle.Name)
                select puzzle)
                .ToList();

            if (inRange.Count != 0)
            {
                var random = new Random();
                int index = random.Next(inRange.Count);

                var target = inRange[index];
                return target;
            }
            else
            {
                return null;
            }
        }

        // When called show all img masks and then reveal random masks depending on distance left
        // (when mask amount increases new random masks will be hiden)
        async private Task RevealImg()
        {
            double distLeft = DistanceToPoint(QuestLat, QuestLng);
            double distStep = distLeft / 9F;

            List<Image> masks = new List<Image>() { mask1, mask2, mask3, mask4, mask5, mask6, mask7, mask8, mask9 };
            var random = new Random();

            int maskCount = 0;      // Count hiden masks

            while (distLeft > 0.01)        // Quest completion loop that reveals parts of image depending on distance left
            {
                await UpdateCurrentLocation();
                distLeft = DistanceToPoint(QuestLat, QuestLng);

                int newMaskCount = 9 - (int)(distLeft / distStep);      // How many masks should be hiden (9 - (mask count until finish. pvz 1.9km / 0.333 = 5.7 = 5 masks left) = 3 hiden)

                int count = newMaskCount - maskCount;       // If newMaskCount increased then hide more masks. Else if newMaskCount decreased then "wrong direction"
                if (count >= 1)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        maskCount += 1;
                        int index = random.Next(masks.Count);
                        masks[index].IsVisible = false;

                        masks.Remove(masks[index]);
                    }
                }
                else if (newMaskCount == 9)
                {
                    distLeft = 0;
                }
            }

            await Navigation.PushAsync(new ComplitedPage(QuestInProgress));     //When loop ends go to quest completed page
        }

        //Thread that updates current user location
        async Task UpdateCurrentLocation()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(60));
                var cts = new CancellationTokenSource();
                var location = await Geolocation.GetLocationAsync(request, cts.Token);

                if (location != null)
                {
                    string x = " " + location.Latitude + " " + location.Longitude + " " + location.Altitude;
                    UserLat = location.Latitude;
                    UserLng = location.Longitude;
                }
                else CurrentLocationError();
            }
            catch (Exception ex)
            {
                CurrentLocationError();     // Unable to get location
            }
        }

        async void CurrentLocationError()
        {
            await DisplayAlert("Error", "Nepavyksta aptikti jusu buvimo vietos.", "OK");
        }

        //Distance from user to some location. MUST CALL await UpdateCurrentLocation() before to work correctly
        private double DistanceToPoint(double pLat, double pLng)
        {
            Location start = new Location(UserLat, UserLng);
            Location end = new Location(pLat, pLng);
            return Location.CalculateDistance(start, end, 0);
        }

        void check_Click(object sender, EventArgs e)
        {
            PrintDistance();
        }

        void help_Click(object sender, EventArgs e)
        {
            Navigation.PushAsync(new GamePage(QuestLat, QuestLng));
        }

        //Displays distance to quest
        async void PrintDistance()
        {
            await UpdateCurrentLocation();
            double dist = DistanceToPoint(QuestLat, QuestLng);

            string vienetai = "km";
            if (dist < 1)
            {
                vienetai = "metrai";
                dist = dist * 1000;
            }
            await DisplayAlert("Tau liko:", " " + dist + " " + vienetai, "OK");
        }
    }
}