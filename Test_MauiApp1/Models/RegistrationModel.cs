using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Test_MauiApp1.Models;
public class RegistrationModel : INotifyPropertyChanged

{
    public event PropertyChangedEventHandler PropertyChanged;

    private List<string> _usedProperty = new();

    protected void SetPropertyAsUsed([CallerMemberName] string propertyName = null)
    {
        if (!_usedProperty.Contains(propertyName))
        {
            _usedProperty.Add(propertyName);
        }
    }

    protected bool IfProperyWasUsed(string propertyName)
    {
        return _usedProperty.Contains(propertyName);
    }

    public string UserNameError
    {
        get
        {
            if (!IfProperyWasUsed(nameof(UserName))) return string.Empty;
            return OnValidate2(nameof(UserName));


        }
    }
    public string PasswordError
    {
        get
        {
            if (!IfProperyWasUsed(nameof(Password))) return string.Empty;
            return OnValidate2(nameof(Password));
        }
    }
    public string PasswordConfirmError { 
        get
        {

            if (!IfProperyWasUsed(nameof(PasswordConfirm))) return string.Empty;
            return OnValidate2(nameof(PasswordConfirm));

        }
    }
    public bool IsValid
    {
        get
        {
            // ValidateEventRise(nameof(IsValid));
            var a = string.IsNullOrEmpty(OnValidate2(nameof(UserName))) &&
                     string.IsNullOrEmpty(OnValidate2(nameof(Password))) &&
                     string.IsNullOrEmpty(OnValidate2(nameof(PasswordConfirm)));
            return a;
        }
    }

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
                SetPropertyAsUsed();
                OnPropertyChanged();
            }
        }
    }
    string _userName = string.Empty;


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
                SetPropertyAsUsed();
                OnPropertyChanged();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PasswordConfirmError)));
            }
        }
    }
    string _password=string.Empty;


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
                SetPropertyAsUsed();
                OnPropertyChanged();
            }

        }
    }
    string _passwordConfirm = string.Empty;

    protected virtual string OnValidate2(string propertyName)
    {
        var context = new ValidationContext(this);
        string resultReturn = "";
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(this, context, results, true);

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

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler != null)
        {
            handler(this, new PropertyChangedEventArgs(propertyName));
            handler(this, new PropertyChangedEventArgs(propertyName+"Error"));
            handler(this, new PropertyChangedEventArgs(nameof(IsValid)));
        }
    }

}