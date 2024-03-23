using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Test_MauiApp1.Models
{
    public class RegistrationModelError_V0

        : INotifyPropertyChanged
    {
        private readonly RegistrationModel_V0 _registrationModel;

        public RegistrationModelError_V0(RegistrationModel_V0 registrationModel)
        {
            _registrationModel = registrationModel;
            _registrationModel.PropertyChanged += _registrationModel_PropertyChanged;

        }

        private void _registrationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == nameof(_registrationModel.Password))
            {
                _passwordError = OnValidate2(nameof(_registrationModel.Password));
                ValidateEventRise(nameof(_registrationModel.Password) + "Error");
                ValidateEventRise(nameof(IsValid));
                return;
            }

            

            _userNameError = OnValidate2(e.PropertyName);

            ValidateEventRise(e.PropertyName + "Error");
            ValidateEventRise(nameof(IsValid));
        }


        string ValidatePassworsConfirm()
        {

            var val = OnValidate2(nameof(_registrationModel.PasswordConfirm));
            if (string.IsNullOrEmpty(val))
                if (_registrationModel.PasswordConfirm != _registrationModel.Password)
                    return "Passwords are not equall.";


            return val;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual string OnValidate2(string propertyName)
        {
            var context = new ValidationContext(_registrationModel);
            string resultReturn = "";
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(_registrationModel, context, results, true);

            if (!isValid)
            {
                ValidationResult result = results.Where(p => p.MemberNames.First()
                                                == propertyName).FirstOrDefault();

                //ValidationResult result = results.SingleOrDefault(p =>
                //                                                p.MemberNames.Any(memberName =>
                //                                                                  memberName == propertyName));
                resultReturn = result == null ? "" : result.ErrorMessage;
            }

            return resultReturn;
        }




        public bool IsValid
        {
            get
            {
                // ValidateEventRise(nameof(IsValid));
                var a = string.IsNullOrEmpty(OnValidate2(nameof(_registrationModel.UserName))) &&
                         string.IsNullOrEmpty(OnValidate2(nameof(_registrationModel.Password))) &&
                         string.IsNullOrEmpty(ValidatePassworsConfirm());
                return a;
            }
        }

        protected virtual void ValidateEventRise(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsValid)));
        }

        string _userNameError = "";
        public string UserNameError =>  _userNameError;

        string _passwordError = "0";
        public string PasswordError =>  _passwordError;

        string _passwordConfirmError = "";
        public string PasswordConfirmError =>  _passwordConfirmError;


    }
}
