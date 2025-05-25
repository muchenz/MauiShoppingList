using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Test_MauiApp1.Models
{
    public class RegistrationModel_V0 : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;


        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserName)));
                }
            }
        }
        string _userName;


        [MinLength(6, ErrorMessage = "Minimal lenght is 6")]
        [MaxLength(50)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));
                }
            }
        }
        string _password;


        [Compare(nameof(Password), ErrorMessage = "Passwords are not equall.")]
        [DataType(DataType.Password)]
        [Required]
        [Display(Name = "PasswordConfirm")]
        public string PasswordConfirm
        {
            get { return _passwordConfirm; }
            set
            {
                if (_passwordConfirm != value)
                {
                    _passwordConfirm = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PasswordConfirm)));
                }

            }
        }
        string _passwordConfirm;


    }


}
