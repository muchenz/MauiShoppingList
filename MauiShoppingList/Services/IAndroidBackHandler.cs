using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_MauiApp1.Services;
public interface IAndroidBackHandler
{
    void EnableMinimizeOnBack();
    void DisableMinimizeOnBack();
}