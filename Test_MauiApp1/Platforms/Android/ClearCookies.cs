using Android.Webkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test_MauiApp1.Services;

namespace Test_MauiApp1.Platforms.Android;

    public class ClearCookies : IClearCookies
    {
        public void ClearAllCookies()
        {
            var cookieManager = CookieManager.Instance;

            cookieManager.RemoveAllCookies(null);

        }
    }
