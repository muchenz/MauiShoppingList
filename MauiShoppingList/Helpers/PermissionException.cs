using System;
using System.Collections.Generic;
using System.Text;

namespace Test_MauiApp1.Helpers
{
    public class WebPermissionException: Exception
    {
        private readonly string _message;

        public WebPermissionException(string message)
        {
            _message = message;
        }

        public override string Message => _message;
    }
}
