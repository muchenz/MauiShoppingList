using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test_MauiApp1.Services;

namespace Test_MauiApp1;
public class AndroidBackHandler : IAndroidBackHandler
{
    public void EnableMinimizeOnBack()
    {
        MainActivity.EnableMinimizeOnBack();
    }

    public void DisableMinimizeOnBack()
    {
        MainActivity.DisableMinimizeOnBack();
    }
}