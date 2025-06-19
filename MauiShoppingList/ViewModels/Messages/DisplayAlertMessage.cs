using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_MauiApp1.ViewModels.Messages;

internal class DisplayAlertMessage : ValueChangedMessage<DisplayAlert>
{
    public DisplayAlertMessage(DisplayAlert message) : base(message)
    {
    }
}

internal class DisplayAlertMessage2
{
    public DisplayAlert DisplayAlertMessage { get; set; }
}

internal class DisplayAlert
{
    public DisplayAlert()
    {

    }

    public string Title { get; set; }
    public string Message { get; set; }
    public string Cancel { get; set; }
    public string Accept { get; set; }

    public Action<bool> OnCompleted { get; set; }
}