using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Test_MauiApp1.Models
{
    public class TokenAndEmailData
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }

    public class PermissionsToListAggr
    {

        public int Id { get; set; }
        public int Permission { get; set; }

    }


    public class DisplayAlertMessageMessage : ValueChangedMessage<DisplayAlertMessage>
    {
        public DisplayAlertMessageMessage(DisplayAlertMessage message) : base(message)
        {
        }
    }

    public class DisplayAlertMessage
    {
        public DisplayAlertMessage()
        {

        }

        public string Title { get; set; }
        public string Message { get; set; }
        public string Cancel { get; set; }
        public string Accept { get; set; }

        public Action<bool> OnCompleted { get; set; }
    }

    public class MessageSatus
    {
        public const string OK = "OK";
        public const string Error = "ERROR";

    }
    public class MessageAndStatus
    {
        public bool IsError => !IsSuccess;
        public bool IsSuccess => Status == MessageSatus.OK;
        public string Status { get; set; }
        public string Message { get; set; }

        protected MessageAndStatus(string message, string status)
        {
            Status = status;
            Message = message;
        }
        public MessageAndStatus()
        {

        }
        public static MessageAndStatus Ok(string msg) => new MessageAndStatus(msg, MessageSatus.OK);
        public static MessageAndStatus Fail(string msg) => new MessageAndStatus(msg, MessageSatus.Error);
    }

    public class MessageAndStatusAndData<T> : MessageAndStatus
    {
        public MessageAndStatusAndData(T data, string msg, string status)
        {
            Data = data;
            Message = msg;
            Status = status;
        }

        public T Data { get; set; }

        public static MessageAndStatusAndData<T> Ok(T data) =>
            new MessageAndStatusAndData<T>(data, string.Empty, MessageSatus.OK);

        public static MessageAndStatusAndData<T> Fail(string msg) =>
           new MessageAndStatusAndData<T>(default, msg, MessageSatus.Error);
    }
    public static class ItemState
    {
        public static int Normal => 0;
        public static int Buyed => 1;

    }




    public class Invitation : IModelItemView
    {
        public int InvitationId { get; set; }
        public string EmailAddress { get; set; }
        public int PermissionLevel { get; set; }
        public int ListAggregatorId { get; set; }
        public string ListAggregatorName { get; set; }
        public string SenderName { get; set; }

        public int Id => InvitationId;

        public string Name
        {
            get { return EmailAddress; }
            set { EmailAddress = value; }
        }

    }
    public class ListAggregationWithUsersPermission
    {

        public ListAggregator ListAggregator { get; set; }

        public List<UserPermissionToListAggregation> UsersPermToListAggr { get; set; }
    }

    public class UserPermissionToListAggregation : IModelItemView, INotifyPropertyChanged
    {
        public User User { get; set; }
        public int Permission { get; set; }


        public int Order { get; set; }

        public int Id => User.UserId;
        public string Name
        {
            get { return User.EmailAddress; }
            set { User.EmailAddress = value; }
        }

        public int PermissionForPicker
        {
            get { return Permission - 1; }
            set
            {
                _oldValueForPicker = Permission; Permission = value + 1;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PermissionForPicker"));
            }
        }

        int _oldValueForPicker;

        public event PropertyChangedEventHandler PropertyChanged;

        public int OldValueForPickerCommand => _oldValueForPicker - 1;
    }

    public class User
    {


        public User()
        {
            ListAggregators = new List<ListAggregator>();


        }

        public int UserId { get; set; }
        public string EmailAddress { get; set; }
        // public string Password { get; set; }


        public IList<ListAggregator> ListAggregators { get; set; }


    }




    public class ListAggregator : IModelItem, INotifyPropertyChanged
    {

        public ListAggregator()
        {
            Lists = new List<List>();

        }

        public int ListAggregatorId { get; set; }
        public string ListAggregatorName { get; set; }
        public int Order { get; set; }
        public int PermissionLevel { get; set; } = 1;

        public string Name
        {
            get { return ListAggregatorName; }
            set
            {
                ListAggregatorName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListAggregatorName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));

            }
        }

        public int Id => ListAggregatorId;

        public IList<List> Lists { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }



    public class OrderListItem : IModelItemOrder
    {

        public OrderListItem()
        {
            List = new List<OrderItem>();
        }

        public List<OrderItem> List { get; set; }
        public int Id { get; set; }

        public int Order { get; set; }
    }

    public class OrderListAggrItem : IModelItemOrder
    {

        public OrderListAggrItem()
        {
            List = new List<OrderListItem>();
        }

        public List<OrderListItem> List { get; set; }
        public int Id { get; set; }

        public int Order { get; set; }
    }

    public class OrderItem : IModelItemOrder
    {
        public int Id { get; set; }

        public int Order { get; set; }

    }


    public class List : IModelItem, INotifyPropertyChanged
    {
        public List()
        {
            ListItems = new List<ListItem>();

        }
        public int ListId { get; set; }
        public string ListName { get; set; }
        public int Order { get; set; }
        public int Id => ListId;
        public string Name
        {
            get { return ListName; }
            set
            {
                ListName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));

            }
        }
        public IList<ListItem> ListItems { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }


    public interface IModelItem : IModelItemView, IModelItemOrder
    {


    }

    public interface IModelItemView : IModelItemBase
    {

        string Name { get; set; }

        //public int Id { get; set; }

    }

    public interface IModelItemOrder : IModelItemBase
    {

        int Order { get; set; }

        //public int Id { get; set; }

    }

    public interface IModelItemBase
    {

        int Id { get; }
    }


    public class ListItem : IModelItem, INotifyPropertyChanged
    {
        public int ListItemId { get; set; }

        public int Order { get; set; }


        private int _stete;

        public int State
        {
            get { return _stete; }
            set
            {
                _stete = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));

            }
        }


        [Required]
        [MinLength(2, ErrorMessage = "Minimalna długośc to 2")]
        string _listItemName;
        public string ListItemName
        {
            get { return _listItemName; }
            set
            {
                _listItemName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListItemName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameForView)));
            }
        }

        public int Id => ListItemId;
        public string Name
        {
            get { return ListItemName; }
            set
            {
                ListItemName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListItemName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameForView)));

            }
        }

        public string NameForView
        {
            get { return "  " + ListItemName + "  "; }
            set
            {
                ListItemName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameForView)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListItemName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));


            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public static class SiganalREventName
    {
        public const string ListItemEdited = nameof(ListItemEdited);
        public const string ListItemAdded = nameof(ListItemAdded);
        public const string ListItemDeleted = nameof(ListItemDeleted);
        public const string InvitationAreChanged = nameof(InvitationAreChanged);
        public const string DataAreChanged = nameof(DataAreChanged);
    }


    public record SignaREnvelope(string SignalRId, string SiglREventName, string SerializedEvent);
    public abstract record ListItemSignalREvent(int ListItemId, int ListAggregationId);
    public record ListItemAddedSignalREvent(int ListItemId, int ListAggregationId, int ListId) : ListItemSignalREvent(ListItemId, ListAggregationId);
    public record ListItemDeletedSignalREvent(int ListItemId, int ListAggregationId) : ListItemSignalREvent(ListItemId, ListAggregationId);
    public record ListItemEditedSignalREvent(int ListItemId, int ListAggregationId) : ListItemSignalREvent(ListItemId, ListAggregationId);
}
