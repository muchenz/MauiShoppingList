﻿using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Test_MauiApp1.Helpers;
using Test_MauiApp1.Models;
using Test_MauiApp1.Services;
using Test_MauiApp1.ViewModels;
using Test_MauiApp1.ViewModels.Messages;
using Unity;
using Unity.Resolution;

namespace Test_MauiApp1.ViewModels
{
    public class ListItemViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly ListItemService _listItemService;
        private readonly ListAggregator _listAggregator;
        private readonly List _list;
        private readonly StateService _stateService;
        private readonly IMessenger _messenger;
        string _userName;

        ListItem _addListItemModel = new ListItem();
        public ListItem AddListItemModel { get { return _addListItemModel; } set { SetProperty(ref _addListItemModel, value); } }

        ListItem _selectedItem;
        public ListItem SelectedItem { get { return _selectedItem; } set { SetProperty(ref _selectedItem, value); } }
        public ListItemViewModel(UserService userService, ListItemService listItemService,  StateService stateService,IMessenger messenger,
            ListAggregator listAggregator, List list)
        {
            _stateService = stateService;
            _messenger = messenger;
            _userName = stateService.StateInfo.UserName;
            _userService = userService;
            _listItemService = listItemService;
            _listAggregator = listAggregator;
            _list = list;
            GetNewDataFromUser(_stateService.StateInfo.User);
          
            base.InitAsyncCommand.Execute(null);

        }
        public ICommand AddListItemCommand
        {
            get
            {
                return new Command(async () => {

                    if (isEdit)
                    {
                        string tempName = editListItem.ListItemName;
                        var tempeditListItem = editListItem;

                        editListItem.ListItemName = AddListItemModel.ListItemName;

                        try
                        {
                            await _listItemService.EditItem(editListItem, _listAggregator.ListAggregatorId);
                        }
                        catch (WebPermissionException ex)
                        {

                            _messenger.DontHavePermissionMessage();
                            tempeditListItem.Name = tempName;

                        }
                        catch
                        {
                            tempeditListItem.Name = tempName;
                        }

                    }
                    else
                    {
                        ListItem list = null;
                        try
                        {
                            AddListItemModel.Order = _list.ListItems.Any()? _list.ListItems.Max(a => a.Order) + 1:1;
                         
                            list = await _listItemService.AddItem(_list.ListId, AddListItemModel, _listAggregator.ListAggregatorId);
                        }
                        catch (WebPermissionException ex)
                        {
                            _messenger.DontHavePermissionMessage();

                        }
                        catch (Exception ex)
                        {

                        }

                        if (list != null)
                        {
                            //_listItemsTemp.Reverse();
                            //_listItemsTemp.Add(list);
                            //_listItemsTemp.Reverse();
                            // ListItems.Clear();
                            //  ListItems = new ObservableCollection<ListItem>(_listItemsTemp.ToList());
                            //  ListItems.CollectionChanged += ListItems_CollectionChanged;
                            ListItems.Insert(0,list);
                          //  SetAndSend(_listItemsTemp);
                           // OnPropertyChanged(nameof(ListItems));
                            //MessagingCenter.Send(this, "Save And Refresh New Order");

                        }
                    }

                    IsVisibleAddItem = false;
                    isEdit = false;
                    AddListItemModel = new ListItem();
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
                    {
                        IsVisibleAddItem = true;
                        AddListItemModel = new ListItem();
                    }


                });

            }
        }


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
                                await _listItemService.Delete<ListItem>(iDItemToDelete, _listAggregator.ListAggregatorId);

                                ListItems.Remove(ListItems.Single(a => a.ListItemId == iDItemToDelete));
                               // ListItems.Remove(ListItems.Single(a => a.ListItemId == iDItemToDelete));
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

        protected override async Task OnAppearingAsync()
        {


            _messenger.Register<NewDataMessage>(this, (r, m) =>
            {

                try
                {
                    GetNewDataFromUser(m.User);
                }
                catch (ArgumentNullException ex)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _ = NavigateWhenArgumentNullException(ex);
                    });
                }
                if (IsBusy) IsBusy = false;
            });
                               

            GetNewDataFromUser(_stateService.StateInfo.User);

        }

        public bool _isVisibleDeletedListLabel = false;
        public bool IsVisibleDeletedListLabel { get { return _isVisibleDeletedListLabel; } set { SetProperty(ref _isVisibleDeletedListLabel, value); } }

        async Task  NavigateWhenArgumentNullException(ArgumentNullException ex)
        {
            IsVisibleDeletedListLabel = true;
            await Task.Delay(2000);
            if (ex.ParamName == "ListAggr")
            {
                var a = Navigation.NavigationStack.Count;
                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                await Navigation.PopAsync();

            }
            else
            {
                await Navigation.PopAsync();
            }
        }
        

        ListAggregator _listAggr;
        public ListAggregator ListAggr
        {
            get { return _listAggr; }
            set { SetProperty(ref _listAggr, value); }
        }

        
        private void GetNewDataFromUser(User arg)
        {
            if (arg == null) return;

            try
            {
                ListAggr = arg.ListAggregators.Where(a => a.ListAggregatorId == _listAggregator.ListAggregatorId).FirstOrDefault();
                if (ListAggr == null) throw new ArgumentNullException("ListAggr");

                var tempList = ListAggr.Lists.Where(a => a.ListId == _list.ListId).FirstOrDefault();
                if (tempList == null) throw new ArgumentNullException("List");


                var tempListItem = tempList.ListItems.ToList();



                if (tempListItem == null)
                {

                    //ListItems = new ObservableCollection<ListItem>();

                    ListItems.Clear();
                }
                else
                {
                    //_listItemsTemp = new List<ListItem>(temLlist);

                    ListItems.CollectionChanged -= ListItems_CollectionChanged;
                    ListItems.Clear();
                    //ListItems = new ObservableCollection<ListItem>(temLlist);
                    tempListItem.ForEach(a => ListItems.Add(a));
                    ListItems.CollectionChanged += ListItems_CollectionChanged;
                }

            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch
            {
                ListItems.Clear();

                //ListItems = new ObservableCollection<ListItem>();

            }
        }

        private void ListItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            SetAndSend(ListItems);

        }


        void SetAndSend(ICollection<ListItem> collection)
        {

            var tempListItem = _stateService.StateInfo.User.ListAggregators
                .Where(a => a.ListAggregatorId == _listAggregator.ListAggregatorId).FirstOrDefault()
                .Lists.Where(a => a.ListId == _list.ListId).FirstOrDefault();

            tempListItem.ListItems = new List<ListItem>(collection);
            //tempListItem.ListItems = ListItems as ICollection<ListItem>;


            //MessagingCenter.Send(this, "Save And Refresh New Order");
            _messenger.Send(new SaveAndRefreshNewOrderMessage());
        }

        protected override async Task OnDisappearingAsync()
        {
            _messenger.Unregister<NewDataMessage>(this); // ???? maybe not necessary?
        }

        public ICommand LoadItemsCommand
        {
            get
            {
                return new Command(() =>
               {
                   _messenger.Send(new RequestForNewDataMessage());

               });

            }
        }

        ListItem oldItem;
        DateTime oldTime;
        public ICommand ItemDoubleClickedCommand
        {
            get
            {
                return new Command<ListItem>(async (item) => {
                   

                    if (IsVisibleDeleteLabel)
                    {
                        iDItemToDelete = item.ListItemId;
                        nameItemToDelete = item.ListItemName;
                        DeleteCommand.Execute(null);
                        return;
                    }


                    if (IsVisibleAddItem)
                    {
                        editListItem = ListItems.Where(a => a.ListItemId == item.ListItemId).First();

                        AddListItemModel.ListItemName = editListItem.ListItemName;
                        isEdit = true;
                    }


                    if (item == null) return;

                    var tempItem = (ListItem)item;

                    if (oldItem != tempItem || DateTime.Now.Subtract(oldTime).TotalMilliseconds>500)
                    {
                        oldItem = tempItem;
                        oldTime = DateTime.Now;
                        return;
                    }
                    


                    SelectedItem = item;

                    if (item.State == ItemState.Normal)
                        item.State = ItemState.Buyed;
                    else
                        item.State = ItemState.Normal;
                    try
                    {
                        await _listItemService.SaveItemProperty<ListItem>(item, nameof(ListItem.State), _listAggregator.ListAggregatorId);
                    }
                    catch
                    {

                    }
                });

            }
        }

        bool isEdit;
        ListItem editListItem;
        public ICommand SelectionChangedCommand
        {
            get
            {
                return new Command(async (item) => {
                   // SelectedItem = item as ListItem;

                    if (IsVisibleDeleteLabel && SelectedItem != null)
                    {
                        iDItemToDelete = SelectedItem.ListItemId;
                        nameItemToDelete = SelectedItem.ListItemName;
                        DeleteCommand.Execute(null);
                        return;
                    }


                    if (IsVisibleAddItem && SelectedItem != null)
                    {
                        editListItem = ListItems.Where(a => a.ListItemId == SelectedItem.ListItemId).First();

                        AddListItemModel.ListItemName = editListItem.ListItemName;
                        isEdit = true;
                    }

                    ListItem tempSelectedItem = SelectedItem;

                    await Task.Run(async () => {
                        if (tempSelectedItem == null) return;

                        if (!Comparer.Compare(tempSelectedItem)) return;

                        if (tempSelectedItem.State == ItemState.Normal)
                            tempSelectedItem.State = ItemState.Buyed;
                        else
                            tempSelectedItem.State = ItemState.Normal;
                        try
                        {
                            await _listItemService.SaveItemProperty<ListItem>(tempSelectedItem, nameof(ListItem.State), _listAggregator.ListAggregatorId);
                        }
                        catch
                        {
                           await  SetDeleyedSelectedItemToNull();
                        }
                    });


                    await SetDeleyedSelectedItemToNull();

                 
                });

            }
        }

        async Task SetDeleyedSelectedItemToNull()
        {
            await Task.Run(async () =>
            {
                await Task.Delay(130);
                SelectedItem = null;

            });
        }


        List<ListItem> _listItemsTemp = new List<ListItem>();

        ObservableCollection<ListItem> _listItems = new ObservableCollection<ListItem>();

        public ObservableCollection<ListItem> ListItems
        {
            get { return _listItems; }
            set
            {
                _listItems = value;
                //OnPropertyChanged(nameof(ListItems));
                OnPropertyChanged();

            }
        }


        protected override async Task InitAsync()
        {

        }

        private async Task<ObservableCollection<ListItem>> LoadListItemsAsync()
        {
          

            // Test the drag and drop lock
            // result.First().Lock();
            // result.Last().Lock();
            return await Task.FromResult(ListItems);
        }
            

        ListItem _itemBeingDragged = null;
      
        public ICommand ItemDraggedCommand => new Command<ListItem>((listItem) =>
        {
            _itemBeingDragged = listItem;
        });

        public ICommand ItemDroppedCommand => new Command<ListItem>((listItem) =>
        {
            try
            {
                var itemToMove = _itemBeingDragged;
                var itemToInsertBefore = listItem;

                if (itemToMove == null || itemToInsertBefore == null || itemToMove == itemToInsertBefore)
                    return;

                int insertAtIndex = ListItems.IndexOf(itemToInsertBefore);

                if (insertAtIndex >= 0 && insertAtIndex < ListItems.Count)
                {
                    ListItems.Remove(itemToMove);
                    ListItems.Insert(insertAtIndex, itemToMove);

                    //itemToMove.IsBeingDragged = false;
                    //itemToInsertBefore.IsBeingDraggedOver = false;
                }

                // Debug.WriteLine($"ItemDropped: [{itemToMove?.UserName}] => [{itemToInsertBefore?.UserName}], target index = [{insertAtIndex}]");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        });


    }


    public static class Comparer
    {
        static object item1=null;
        static object item2=null;

        static readonly int MaxTimeDelay = 1000;

        static DateTime  _time1;

        public static bool Compare(object item)
        {


            if ((DateTime.Now - _time1).TotalMilliseconds >= MaxTimeDelay || item1 == null)
            {
                item1 = item;
                _time1 = DateTime.Now;
                return false;

            }
            
            if (item1==item)
            {

                item1 = null;
                _time1 = DateTime.Now;
                return true;

            }

            item1 = item;
            _time1 = DateTime.Now;
            return false;

        } 


    }
}
