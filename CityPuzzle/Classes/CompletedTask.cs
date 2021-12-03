﻿using CityPuzzle.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace CityPuzzle.Classes
{
    [DataContract]
    public partial class CompletedTask
    {
        [Key]
        [DataMember]
        public int UserId { get; set; }
        [Key]
        [DataMember]
        public int PuzzleId { get; set; }

        public async void Save()
        {
            try
            {
                var response = await App.WebServices.SaveObject(this);
                Console.WriteLine("Saving is working");
            }
            catch (APIFailedSaveException ex) //reikia pagalvot kaip handlinti(galima mesti toliau ir try kur skaitoma(throw)) 
            {
                Console.WriteLine("APIFailedSaveException Error" + ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: else " + ex);
            }
        }
        public void Delete()
        {
            string json = Rest_Services.Client.APICommands.Serialize(this);
            string adress = "Tasks/"+json;

            try
            {
                App.WebServices.DeleteObject(adress);
                Console.WriteLine("Delete is working");
            }
            catch (APIFailedDeleteException ex)
            {
                Console.WriteLine("APIFailedSaveException Error" + ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: else " + ex);
            }
        }
    }
}
