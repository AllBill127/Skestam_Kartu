﻿using CityPuzzle.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CityPuzzle.Game_Room.Join_GameRoom
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SeeEnteredRooms : ContentPage
    {
        public static List<Room> AllRooms = new List<Room>();
        public static List<User> AllUsers = new List<User>();
        public static Task tReadRooms = new Task(() => { AllRooms = Sql.ReadRooms(); });
        public static Task tReadUsers = new Task(() => { AllUsers = Sql.ReadUsers(); });
        public SeeEnteredRooms()
        {
            InitializeComponent();
            if(!tReadRooms.IsCompleted) tReadRooms.Start();
            if (!tReadUsers.IsCompleted) tReadUsers.Start();
            Task<List<Room>> treadFindRooms = new Task<List<Room>>(() => { return GetData(); });
            treadFindRooms.Start();
            Task.WaitAll();
            LoadingGrid.IsVisible = false;
            MainStack.IsVisible = true;
            MyListView.ItemsSource = treadFindRooms.Result;
        }
        public static List<Room> GetData()
        {
            List<Room> findedRooms = new List<Room>();
            Task<List<string>> treadFind = new Task<List<string>>(
              () =>
              {
                  return Sql.FindParticipantRoomsIDs(App.CurrentUser.ID);
              });
            treadFind.Start();
            Task.WaitAll();
            foreach (string pin in treadFind.Result)
            {
                findedRooms.Add(AllRooms.SingleOrDefault(x => x.ID == pin));
            }
            return findedRooms;
        }
        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
            {
                return;
            }
            ((ListView)sender).SelectedItem = null;
            Console.WriteLine(" " + e.Item);
        }

        void Sign_Click(object sender, EventArgs e)
        {
            string gamePin = GamePin.Text;
            Navigation.PushAsync(new EntryGameRoomPage(gamePin));

        }




    }
}