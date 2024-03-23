using System;
using Test_MauiApp1.ViewModels;
using Test_MauiApp1.Models;

namespace Test_MauiApp1.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public Item Item { get; set; }
        public ItemDetailViewModel(Item item = null)
        {
            Title = item?.Text;
            Item = item;
        }
    }
}
