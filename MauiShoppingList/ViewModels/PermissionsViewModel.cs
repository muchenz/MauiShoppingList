﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui;
using Test_MauiApp1.ViewModels;
using Test_MauiApp1.Models;
using Test_MauiApp1.Services;
using CommunityToolkit.Mvvm.Messaging;
using Test_MauiApp1.ViewModels.Messages;
using Test_MauiApp1.Helpers;

namespace Test_MauiApp1.ViewModels
{
    public class PermissionsViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly ListItemService _listItemService;
        private readonly StateService _stateService;
        private readonly IMessenger _messenger;
        string _userName;
      

        ListAggregationWithUsersPermission _selectedItem;
        public ListAggregationWithUsersPermission SelectedItem
        {
            get { return _selectedItem; }
            set
            {              

                SetProperty(ref _selectedItem, value);
            }
        }
        public PermissionsViewModel(UserService userService, ListItemService listItemService, StateService stateService, IMessenger messenger)
        {
            _stateService = stateService;
            _messenger = messenger;
            _userName = stateService.StateInfo.UserName;
            _userService = userService;
            _listItemService = listItemService;
            base.InitAsyncCommand.Execute(null);

        }

        

        bool _isVisibleInvitateItem;
        public bool IsVisibleInviteteItem
        {
            get { return _isVisibleInvitateItem; }
            set
            {
                SetProperty(ref _isVisibleInvitateItem, value);
                IsNotVisibleInvitationFrame = !value;
            }
        }

       
        bool _isNotVisibleInvitationFrame = true;
        public bool IsNotVisibleInvitationFrame
        {
            get { return _isNotVisibleInvitationFrame; }
            set
            {
                SetProperty(ref _isNotVisibleInvitationFrame, value);

            }
        }


        UserPermissionToListAggregation _invitationNew = new UserPermissionToListAggregation { Permission=2, User = new User() };
        public UserPermissionToListAggregation InvitationNew { get { return _invitationNew; } set { SetProperty(ref _invitationNew, value); } }
        public ICommand InvitToolbarCommand
        {
            get
            {
                return new Command(() => {
                                      

                    if (IsVisibleInviteteItem)
                        IsVisibleInviteteItem = false;
                    else
                        IsVisibleInviteteItem = true;


                });

            }
        }


        public ICommand SendInvitationCommand => new Command(async () =>
        {
            if (SelectedItem == null)
            {
                _messenger.SimpleInformationMessage("Choose list");
                return;
            }
           // InvitationNew.Name = SelectedItem.ListAggregatorEntity.Name;
            int listAggrId = SelectedItem.ListAggregator.ListAggregatorId;


            MessageAndStatus messageAndStatus=null;

            try
            {
                messageAndStatus = await _userService.InviteUserPermission(InvitationNew, listAggrId);
            }
            catch { }   

            if (messageAndStatus == null || string.IsNullOrEmpty(messageAndStatus.Message)) return;

            _messenger.SimpleInformationMessage(messageAndStatus.Message);


        });

        public ICommand LoadItemsCommand => new Command(async () =>
        {
            await InitAsync();
            IsBusy = false;         
        });

        bool _isBusy2;
        public bool IsBusy2 { get { return _isBusy2; } set { SetProperty(ref _isBusy2, value); } }

        public ICommand LoadItemsCommand2 => new Command(async () =>
        {
            if (SelectedItem == null)
            {
                IsBusy2 = false;
                return;
            }
            await InitAsync();

            SelectedItem = ListAggrWithUsersPerm.Where(a => a.ListAggregator.ListAggregatorId == SelectedItem.ListAggregator.ListAggregatorId).FirstOrDefault();

            IsBusy2 = false;
        });

        public ICommand SelectedIndexChangedCommand
        {
            get
            {
                return new Command<UserPermissionToListAggregation>(async (item) =>
                {
                  
                    var message = new DisplayAlert();
                    MessageAndStatus messageAndStatus;
                    UserPermissionToListAggregation tempUserPermToListAggregation = item;
                    if (item.Name == _userName)
                    {
                        message.Title = "Warning";
                        message.Message = $"You changing YOURS ({item.Name}) prermission.\n If you changed you lose Administration permission.";
                        message.Accept = "I'm sure!";
                        message.Cancel = "Cancel";
                        message.OnCompletedAsync += async (accept) =>
                        {
                            if (accept)
                            {
                                try
                                {
                                    messageAndStatus = await _userService.ChangeUserPermission(item, SelectedItem.ListAggregator.ListAggregatorId);

                                    if (messageAndStatus.Status != "OK")
                                        tempUserPermToListAggregation.PermissionForPicker = tempUserPermToListAggregation.OldValueForPickerCommand;

                                    if (string.IsNullOrEmpty(messageAndStatus.Message)) return;

                                    _messenger.SimpleInformationMessage(messageAndStatus.Message);
                                }
                                catch
                                {
                                    tempUserPermToListAggregation.PermissionForPicker = tempUserPermToListAggregation.OldValueForPickerCommand;
                                }

                            }
                        };

                        _messenger.Send(new DisplayAlertMessage(message));
                                                
                    }
                    else
                    {
                        try
                        {
                            messageAndStatus = await _userService.ChangeUserPermission(item, SelectedItem.ListAggregator.ListAggregatorId);

                            if (messageAndStatus.Status != "OK")
                                tempUserPermToListAggregation.PermissionForPicker = tempUserPermToListAggregation.OldValueForPickerCommand;

                            if (string.IsNullOrEmpty(messageAndStatus.Message)) return;

                            _messenger.SimpleInformationMessage(messageAndStatus.Message);

                        }
                        catch
                        {
                            tempUserPermToListAggregation.PermissionForPicker = tempUserPermToListAggregation.OldValueForPickerCommand;
                        }
                    }

                 


                });

            }
        }
        

              public ICommand DeleteCommand
        {
            get
            {
                return new Command<UserPermissionToListAggregation>((item) =>
                {                   

                    var message = new DisplayAlert();
                    MessageAndStatus messageAndStatus;

                    if (item.Name == _userName)
                    {
                        message.Title = "Warning!!! ";
                        message.Message = $"You delete YOURS ({item.Name}) prermission.\n If you delete, you lose access to this list!";
                        message.Accept = "I'm sure!";
                        message.Cancel = "Cancel";
                        message.OnCompletedAsync += async (accept) =>
                        {
                            if (accept)
                            {
                                try
                                {
                                    messageAndStatus = await _userService.DeleteUserPermission(item, SelectedItem.ListAggregator.ListAggregatorId);
                                    if (messageAndStatus.Status == "OK")
                                        SelectedItem.UsersPermToListAggr.Remove(item);

                                    if (string.IsNullOrEmpty(messageAndStatus.Message)) return;

                                    _messenger.SimpleInformationMessage(messageAndStatus.Message);
                                } 
                                catch
                                { 
                                }
                            }
                        };

                        _messenger.Send(new DisplayAlertMessage(message));

                    }
                    else
                    {
                        message.Title = "Warning";
                        message.Message = $"You delete '{item.Name}' prermission.";
                        message.Accept = "I'm sure!";
                        message.Cancel = "Cancel";
                        
                        message.OnCompletedAsync += async (accept) =>
                        {
                            if (accept)
                            { 
                                try
                                {
                                    messageAndStatus = await _userService.DeleteUserPermission(item, SelectedItem.ListAggregator.ListAggregatorId);

                                    if (messageAndStatus.Status == "OK")
                                        SelectedItem.UsersPermToListAggr.Remove(item);

                                    if (string.IsNullOrEmpty(messageAndStatus.Message)) return;

                                    _messenger.SimpleInformationMessage(messageAndStatus.Message);
                                }
                                catch
                                {

                                }
                           }    
                        };

                        _messenger.Send(new DisplayAlertMessage(message));
                    }


                });

            }
        }
        
      ObservableCollection<ListAggregationWithUsersPermission> _listAggrWithUsersPerm { get; set; }

        public ObservableCollection<ListAggregationWithUsersPermission> ListAggrWithUsersPerm
        {
            get { return _listAggrWithUsersPerm; }
            set
            {
                _listAggrWithUsersPerm = value;
                OnPropertyChanged();

            }
        }

        protected override async Task InitAsync()
        {
            try
            {
                var tempList =  await _userService.GetListAggrWithUsersPermAsync();

                ListAggrWithUsersPerm = new ObservableCollection<ListAggregationWithUsersPermission>(tempList);               
            }
            catch
            {

            }
        }
    }
}
