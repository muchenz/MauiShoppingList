﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Test_MauiApp1.ViewModels;
using Unity;
using Unity.Resolution;
using Test_MauiApp1.Helpers;
using Test_MauiApp1.Models;
using Test_MauiApp1.Services;
using Test_MauiApp1.Views;
using CommunityToolkit.Mvvm.Messaging;
using Test_MauiApp1.ViewModels.Messages;

namespace Test_MauiApp1.ViewModels
{
    public class ListViewModel:BaseViewModel
    {
        private readonly UserService _userService;
        private readonly ListItemService _listItemService;
        private readonly ListAggregator _listAggregator;
        private readonly StateService _stateService;
        private readonly IMessenger _messenger;
        string userName;
        List _addListModel = new List();
        public List AddListModel { get { return _addListModel; } set { SetProperty(ref _addListModel, value); } } 

        List _selectedItem;
        public List SelectedItem { get { return _selectedItem; } set { SetProperty(ref _selectedItem, value); } }
        public ListViewModel(UserService userService, ListItemService listItemService, ListAggregator listAggregator, StateService stateService,
            IMessenger messenger)
        {
            userName = stateService.StateInfo.UserName;
            _userService = userService;
            _listItemService = listItemService;
            _listAggregator = listAggregator;
            _stateService = stateService;
            _messenger = messenger;
            GetNewData(_stateService.StateInfo.User);

            base.InitAsyncCommand.Execute(null);

        }

        ICollection<List> _listFromAppuserUser;
              

        int iDItemToDelete;
        string nameItemToDelete;
        public ICommand DeleteCommand
        {
            get
            {
                return new Command(() => {
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
                                await _listItemService.Delete<List>(iDItemToDelete, _listAggregator.ListAggregatorId);

                                List.Remove(List.Single(a => a.ListId == iDItemToDelete));
                                
                                //_listAggregator.Lists = List;
                            
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

                    //_listAggregator.Lists = List;

                });

            }
        }
        bool _isVisibleDeleteLabel;
        public bool IsVisibleDeleteLabel { get { return _isVisibleDeleteLabel; } set { SetProperty(ref _isVisibleDeleteLabel, value); } }

        public ICommand IsVisibleDeleteLabelCommand
        {
            get
            {
                return new Command(() => {

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
        public ICommand AddListCommand
        {
            get
            {

                return new Command(async () =>
                {


                    if (isEdit)
                    {
                        string tempName = SelectedItem.Name;
                        var  temSelectedItem = SelectedItem;
                        SelectedItem.Name = AddListModel.Name;
                        try
                        {
                            await _listItemService.EditItem(SelectedItem, _listAggregator.ListAggregatorId);
                        }
                        catch (WebPermissionException ex)
                        {
                            _messenger.DontHavePermissionMessage();
                            temSelectedItem.Name = tempName;

                        }
                        catch
                        {
                            temSelectedItem.Name = tempName;
                        }


                    }
                    else
                    {
                        List list = null;
                        try
                        {
                            AddListModel.Order = List.Any() ? List.Max(a => a.Order) + 1 : 1;

                            list = await _listItemService.AddItem(_listAggregator.ListAggregatorId, AddListModel, _listAggregator.ListAggregatorId);
                        }
                        catch (WebPermissionException ex)
                        {
                            _messenger.DontHavePermissionMessage();

                        }
                        catch
                        {
                        }
                        if (list != null)
                        {
                            List.Insert(0, list);                           
                        }
                    }

                    AddListModel = new List();
                    isEdit = false;
                    SelectedItem = null;
                    IsVisibleAddItem = false;
                    //_listAggregator.Lists = List;

                });

            }
        }

        bool _isVisibleAddItem;
        public bool IsVisibleAddItem { get { return _isVisibleAddItem; } set { SetProperty(ref _isVisibleAddItem, value); } }

        public ICommand AddToolbarCommand
        {
            get
            {
                return new Command(() => {

                    if (IsVisibleDeleteLabel) IsVisibleDeleteLabel = false;

                    SelectedItem = null;

                    if (IsVisibleAddItem)
                        IsVisibleAddItem = false;
                    else
                        IsVisibleAddItem = true;


                });

            }
        }

        ListAggregator _listAggr;
        public ListAggregator ListAggr
        {
            get { return _listAggr; }
            set { SetProperty(ref _listAggr, value); }
        }

        private void GetNewData(User arg)
        {
            if (arg== null) return;
          
            try
            {
                ListAggregator temPlist = arg.ListAggregators.Where(a => a.ListAggregatorId == _listAggregator.ListAggregatorId).FirstOrDefault();
                ListAggr = temPlist;

                if (temPlist==null)
                {
                    List.Clear();
                    NavigateWhenListArgIsNull();

                }
                else
                {
                    List = new ObservableCollection<List>(temPlist.Lists);
                    temPlist.Lists = List;
                }
            }
            catch
            {
                List.Clear();
            }
        }
        public bool _isVisibleDeletedListLabel = false;
        public bool IsVisibleDeletedListLabel { get { return _isVisibleDeletedListLabel; } set { SetProperty(ref _isVisibleDeletedListLabel, value); } }
        void NavigateWhenListArgIsNull()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                IsVisibleDeletedListLabel = true;
                await Task.Delay(2000);
                await Navigation.PopAsync();
            });
        }
        protected override async  Task OnAppearingAsync()
        {

            _messenger.Register<NewDataMessage>(this, (r, m) =>
            {

                GetNewData(m.User);

                if (IsBusy) IsBusy = false;

            });
                     

            GetNewData(_stateService.StateInfo.User);

        }

        protected override async Task OnDisappearingAsync()
        {


             _messenger.Unregister<NewDataMessage>(this); // ???? maybe not necessary?
        }

        public ICommand LoadItemsCommand => new Command(()=> _messenger.Send(new RequestForNewDataMessage()));


        public ICommand ItemClickedCommand
        {
            get
            {
                return new Command(async (list) => {

                    await Navigation.PushAsync(App.Container.Resolve<ListItemPage>(
                       new ResolverOverride[] { new ParameterOverride("listAggregator", _listAggregator), 
                           new  ParameterOverride("list", list) }));
                });

            }
        }

        bool isEdit;
        public ICommand SelectionChangedCommand
        {
            get
            {
                return new Command(async () => {

                    if (SelectedItem == null) return;

                    if (IsVisibleDeleteLabel)
                    {
                        iDItemToDelete = SelectedItem.ListId;
                        nameItemToDelete = SelectedItem.ListName;
                        DeleteCommand.Execute(null);
                        return;
                    }

                    if (IsVisibleAddItem)
                    {
                        AddListModel.Name = SelectedItem.Name;
                        isEdit = true;
                        return;
                    }

                    var temp = SelectedItem;

                    await Navigation.PushAsync(App.Container.Resolve<ListItemPage>(
                       new ResolverOverride[] { new ParameterOverride("listAggregator", _listAggregator),
                           new  ParameterOverride("list", temp) }));

                    SelectedItem = null;
                });

            }
        }



        ObservableCollection<List> _list { get; set; }

        public ObservableCollection<List> List
        {
            get { return _list; }
            set
            {
                _list = value;
                OnPropertyChanged();

            }
        }

        

        protected override async Task InitAsync()
        {

        }
    }
}
