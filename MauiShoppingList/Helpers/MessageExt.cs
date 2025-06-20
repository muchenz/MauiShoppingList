using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui;
using System;
using System.Collections.Generic;
using System.Text;
using Test_MauiApp1.Models;
using Test_MauiApp1.ViewModels.Messages;


namespace Test_MauiApp1.Helpers
{
    public static class MessageExt
    {

        public static void DontHavePermissionMessage(this IMessenger messager)
        {
            var message = new DisplayAlert();
            message.Title = "Message";
            message.Message = $"You don't have perrmision\n to do this operation.";
            message.Cancel = "Ok";

            messager.Send(new DisplayAlertMessage(message));

        }


        public static void SimpleInformationMessage(this IMessenger messager, string txt)
        {
            var message = new DisplayAlert();

            message.Title = "Information";
            message.Message = txt;
            message.Cancel = "Ok";

            messager.Send(new DisplayAlertMessage(message));


        }
    }
}
