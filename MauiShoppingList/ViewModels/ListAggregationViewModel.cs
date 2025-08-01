﻿using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Test_MauiApp1.Helpers;
using Test_MauiApp1.Models;
using Test_MauiApp1.Services;
using Test_MauiApp1.ViewModels;
using Test_MauiApp1.ViewModels.Messages;
using Test_MauiApp1.Views;
using Unity;
using Unity.Injection;
using Unity.Resolution;


namespace Test_MauiApp1.ViewModels
{
    public class ListAggregationViewModel : BaseViewModel, IDisposable
    {
        HubConnection _hubConnection;

        private readonly UserService _userService;
        private readonly ListItemService _listItemService;
        private readonly IConfiguration _configuration;
        private readonly StateService _stateService;
        private readonly IMessenger _messenger;
        private readonly SignalRService _signalRService;
        private readonly LoginService _loginService;
        string _userName;
        ListAggregator _selectedItem;
        public ListAggregator SelectedItem { get { return _selectedItem; } set { SetProperty(ref _selectedItem, value); } }

        string _invitationsString = "";
        public string InvitationsString
        {
            get { return _invitationsString == "" ? "Manage" : $"Manage\n({_invitationsString})"; }
            set { SetProperty(ref _invitationsString, value); }
        }

        public int WidthRequest { get; set; }
        public string AddEdit => "Add|Edit";

        ListAggregator _addListAggregatorModel = new ListAggregator();
        public ListAggregator AddListAggregatorModel { get { return _addListAggregatorModel; } set { SetProperty(ref _addListAggregatorModel, value); } }

        public ListAggregationViewModel(UserService userService, ListItemService listItemService, IConfiguration configuration,
            StateService stateService, IMessenger messenger, SignalRService signalRService, LoginService loginService)
        {
            _userName = stateService.StateInfo.UserName;
            _userService = userService;
            _listItemService = listItemService;
            _configuration = configuration;
            _stateService = stateService;
            _messenger = messenger;
            _signalRService = signalRService;
            _loginService = loginService;
            _messenger.Register<RequestForNewDataMessage>(this, async  (r, m) =>
            {

                var data = await RequestForNewData();

                _messenger.Send(new NewDataMessage() { User = data });

            });

            _messenger.Register<SaveAndRefreshNewOrderMessage>(this,  (r, m) =>
            {
                LoadSaveOrderDataHelper.SaveAllOrder(_stateService.StateInfo.User.ListAggregators);
                LoadSaveOrderDataHelper.LoadListAggregatorsOrder(_stateService);
            });

            base.InitAsyncCommand.Execute(null);
            
        }

        int iDItemToDelete;
        string nameItemToDelete;

        public ICommand DeleteCommand
        {
            get
            {
                return new Command(() =>
                {
                    var message = new DisplayAlert();


                    message.Title = "Warning";
                    message.Message = $"You deleting '{nameItemToDelete}'.";
                    message.Accept = "I'm sure!";
                    message.Cancel = "Cancel";
                    message.OnCompletedAsync += async (accept) =>
                    {
                        if (accept)
                        {
                            try
                            {
                                await _listItemService.Delete<ListAggregator>(iDItemToDelete, iDItemToDelete);

                                ListAggr.Remove(ListAggr.Single(a => a.ListAggregatorId == iDItemToDelete));
                            }
                            catch (WebPermissionException ex)
                            {
                                _messenger.DontHavePermissionMessage();

                            }
                            catch
                            {

                            }
                        }
                    };
                    _messenger.Send(new DisplayAlertMessage(message));

                });

            }
        }
        bool _isVisibleDeleteLabel;
        public bool IsVisibleDeleteLabel { get { return _isVisibleDeleteLabel; } set { SetProperty(ref _isVisibleDeleteLabel, value); } }

        public ICommand IsVisibleDeleteLabelCommand
        {
            get
            {
                return new Command(() =>
                {

                    if (IsVisibleAddItem) IsVisibleAddItem = false;

                    SelectedItem = null;

                    if (IsVisibleDeleteLabel)
                        IsVisibleDeleteLabel = false;
                    else
                    {
                        IsVisibleDeleteLabel = true;

                    }


                });

            }
        }
        public ICommand AddListAggregatorCommand
        {
            get
            {

                return new Command(async () =>
                {

                    if (_isEdit)
                    {
                        var tempSelectedItem = SelectedItem;
                        var tempName = SelectedItem.Name;

                        SelectedItem.Name = AddListAggregatorModel.Name;
                        AddListAggregatorModel.Name = "";
                        _isEdit = false;
                        try
                        {
                            await _listItemService.EditItem(SelectedItem, SelectedItem.ListAggregatorId);
                            // AddListAggregatorModel = new ListAggregator();
                        }
                        catch (WebPermissionException)
                        {

                            _messenger.DontHavePermissionMessage();
                            tempSelectedItem.Name = tempName;


                        }
                        catch
                        {
                            tempSelectedItem.Name = tempName;
                        }

                    }
                    else
                    {
                        ListAggregator listAggr = null;
                        try
                        {
                            AddListAggregatorModel.Order = ListAggr.Any() ? ListAggr.Max(a => a.Order) + 1 : 1;

                            listAggr = await _listItemService.AddItem(_stateService.StateInfo.User.UserId, AddListAggregatorModel, -1);
                        }
                        catch (WebPermissionException)
                        {
                            _messenger.DontHavePermissionMessage();
                        }
                        catch { }

                        if (listAggr != null)
                            ListAggr.Insert(0, listAggr);
                    }

                    SelectedItem = null;
                    IsVisibleAddItem = false;
                    _isEdit = false;
                    AddListAggregatorModel = new ListAggregator();
                });

            }
        }

        public ICommand LoadItemsCommand
        {
            get
            {

                return new Command(async () =>
                {

                    _ = await RequestForNewData();

                    IsBusy = false;

                });

            }
        }
        public ICommand Logout
        {
            get
            {

                return new Command(() =>
                {


                    var message = new DisplayAlert();


                    message.Title = "Logout";
                    message.Message = $"Are you want logout?";
                    message.Accept = "I'm sure!";
                    message.Cancel = "Cancel";
                    message.OnCompletedAsync += async (accept) =>
                    {
                        if (accept)
                        {
                            try
                            {
                                var a = DependencyService.Get<IClearCookies>();
                                a.ClearAllCookies();

                                await _loginService.LogOutAsync();

                                //await Navigation.PushAsync(App.Container.Resolve<LoginPage>());
                                App.Current.MainPage = new NavigationPage(App.Container.Resolve<LoginPage>())
                                {
                                    BarBackgroundColor = Colors.WhiteSmoke,
                                    BarTextColor = Colors.Black //color of arrow in ToolbarItem
                                };
                            }

                            catch (Exception ex)
                            {

                            }
                        }
                    };
                    _messenger.Send(new DisplayAlertMessage(message));
                    //WeakReferenceMessenger.Default.Send(new DisplayAlertMessage(message));

                    IsBusy = false;

                });

            }
        }


        //public ICommand ItemClickedCommand
        //{
        //    get
        //    {
        //        return new Command(async (listAggr) =>
        //        {


        //            await Navigation.PushAsync(App.Container.Resolve<ListPage>(
        //                new ResolverOverride[] { new ParameterOverride("listAggregator", listAggr) }));
        //        });

        //    }
        //}
        public ICommand InvitationsCommand
        {
            get
            {
                return new Command(async (listAggr) =>
                {
                    //InvitationsString = "";

                    await Navigation.PushAsync(App.Container.Resolve<InvitationsPage>(
                        new ResolverOverride[] { new ParameterOverride("listAggregator", listAggr) }));
                });

            }
        }

        bool _isVisibleAddItem;
        public bool IsVisibleAddItem { get { return _isVisibleAddItem; } set { SetProperty(ref _isVisibleAddItem, value); } }
        public ICommand AddToolbarCommand
        {
            get
            {
                return new Command(() =>
                {

                    if (IsVisibleDeleteLabel) IsVisibleDeleteLabel = false;

                    SelectedItem = null;

                    if (IsVisibleAddItem)
                        IsVisibleAddItem = false;
                    else
                        IsVisibleAddItem = true;


                });

            }
        }
        bool _isEdit;
        public ICommand SelectionChangedCommand
        {
            get
            {
                return new Command(async () =>
                {

                    if (SelectedItem == null) return;

                    if (IsVisibleDeleteLabel)
                    {
                        iDItemToDelete = SelectedItem.ListAggregatorId;
                        nameItemToDelete = SelectedItem.ListAggregatorName;
                        DeleteCommand.Execute(null);
                        return;
                    }

                    if (IsVisibleAddItem)
                    {
                        AddListAggregatorModel.Name = SelectedItem.Name;
                        _isEdit = true;
                        return;
                    }

                    var temp = SelectedItem;

                    await Navigation.PushAsync(App.Container.Resolve<ListPage>(
                        new ResolverOverride[] { new ParameterOverride("listAggregator", temp) }));

                    SelectedItem = null;
                });

            }
        }


        ObservableCollection<ListAggregator> _listAggr;

        public ObservableCollection<ListAggregator> ListAggr
        {
            get { return _listAggr; }
            set
            {
                _listAggr = value;
                OnPropertyChanged();

            }
        }

        Task reperterTask;


        async Task<User> RequestForNewData()
        {
            User data = null;

            try
            {
                data = await _userService.GetUserDataTreeAsync();

                _stateService.StateInfo.User = data;

                LoadSaveOrderDataHelper.LoadListAggregatorsOrder(_stateService);
                ListAggr = new ObservableCollection<ListAggregator>(data.ListAggregators);
                _stateService.StateInfo.User.ListAggregators = ListAggr;
            }
            catch (Exception ex) 
            { 
            
            
            }


            return data;
        }

        List<IDisposable> _listDisposable = null;
        protected override async Task InitAsync()
        {



            if (!string.IsNullOrEmpty(_userName))
            {
                _ = await RequestForNewData();

                // reperterTask = Task.Run(() => reperterTaskFunctionAsync());
            }

            try
            {
                //(_listDisposable, _hubConnection) = await HubConnectionHelper.EstablishSignalRConnectionAsync(_configuration,
                //    _listItemService, SetInvitaionNewIndicator, _stateService, _messenger);

                //_stateService.StateInfo.ClientSignalRID = _hubConnection.ConnectionId;



                await SetSignalR();

                await SetInvitaionNewIndicator();

            }

            catch (Exception ex)
            {


            }
        }
        async Task   SetSignalR()
        {
            await _signalRService.StartConnectionAsync();

            //TODO: add dispose !!

            _signalRService.RegisterDataAreChangedHandlers(() => SignalRHandlers.SignalRGetUserDataTreeAsync(_messenger));
            _signalRService.RegisterInvitationAreChanedHandlers(() => SignalRHandlers.SignalRInvitationInitAsync(SetInvitaionNewIndicator));
            _signalRService.RegisterListItemAreChangedHandlers((envelope) =>
                SignalRHandlers.SignalRListItemAreChangedAsync(envelope, _stateService, _messenger, _listItemService));
            
        }
        async Task SetInvitaionNewIndicator()
        {
            var invList = await _userService.GetInvitationsListAsync();

            if (invList.Count > 0)
                InvitationsString = "NEW";

        }


        async Task reperterTaskFunctionAsync()
        {

            while (true)
            {
                await Task.Delay(60000);

                User data;
                try
                {
                    data = await RequestForNewData();

                    _messenger.Send(new NewDataMessage { User = data });
                }
                catch { }

            }
        }


        public void Dispose()
        {

            _listDisposable?.ForEach(x => x?.Dispose());
            //Task.Run(async () =>                         //hang
            //{
            //    if (_hubConnection != null)
            //        await _hubConnection.DisposeAsync();

            //}).GetAwaiter().GetResult();


            //if (_hubConnection != null)
            //    _hubConnection.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();  //hang

            if (_hubConnection != null)
                _ = _hubConnection.DisposeAsync();// from codeoverflow

        }
    }
}
