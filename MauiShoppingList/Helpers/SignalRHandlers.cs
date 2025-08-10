using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Test_MauiApp1.Models;
using Test_MauiApp1.Services;
using Test_MauiApp1.ViewModels;
using Test_MauiApp1.ViewModels.Messages;

namespace Test_MauiApp1.Helpers
{
    public class SignalRHandlers
    {

        public static Task SignalRGetUserDataTreeAsync(IMessenger messenger)
        {
            messenger.Send(new RequestForNewDataMessage());
            return Task.CompletedTask;
        }



        public static async Task SignalRListItemAreChangedAsync(SignaREnvelope envelope, StateService stateService, IMessenger messenger
            , ListItemService listItemService)
        {

            var eventName = envelope.SiglREventName;
            var signaREventSerialized = envelope.SerializedEvent;

            ListItemSignalREvent signaREvent = GetDeserializedSinglaREvent(signaREventSerialized);


            var listAggregationId = signaREvent.ListAggregationId;
            var listItemId = signaREvent.ListItemId;

            switch (eventName)
            {
                case SiganalREventName.ListItemEdited:
                    {
                        var item = await listItemService.GetItem<ListItem>(listItemId, listAggregationId);

                        var lists = stateService.StateInfo.User.ListAggregators.Where(a => a.ListAggregatorId == listAggregationId).FirstOrDefault();

                        ListItem foundListItem = null;
                        foreach (var listItem in lists.Lists)
                        {
                            foundListItem = listItem.ListItems.FirstOrDefault(a => a.Id == listItemId);
                            if (foundListItem != null) break;
                        }
                        foundListItem.ListItemName = item.ListItemName;
                        foundListItem.State = item.State;

                        break;
                    }
                case SiganalREventName.ListItemAdded:
                    {
                        var addSignalREvent = signaREvent as ListItemAddedSignalREvent;
                        var item = await listItemService.GetItem<ListItem>(listItemId, listAggregationId);


                        var tempList = stateService.StateInfo.User.ListAggregators.Where(a => a.ListAggregatorId == listAggregationId).FirstOrDefault().
                                 Lists.Where(a => a.ListId == addSignalREvent.ListId).FirstOrDefault().ListItems;

                        if (!tempList.Where(a => a.Id == item.Id).Any())
                        {
                            tempList.Insert(0, item);
                        }

                        messenger.Send(new NewDataMessage { User = stateService.StateInfo.User });
                        break;
                    }
                case SiganalREventName.ListItemDeleted:
                    {

                        var lists = stateService.StateInfo.User.ListAggregators.Where(a => a.ListAggregatorId == listAggregationId).FirstOrDefault();

                        ListItem foundListItem = null;
                        List founfList = null;

                        foreach (var listItem in lists.Lists)
                        {
                            founfList = listItem;
                            foundListItem = listItem.ListItems.FirstOrDefault(a => a.Id == listItemId);
                            if (foundListItem != null) break;
                        }

                        founfList.ListItems.Remove(foundListItem);

                        messenger.Send(new NewDataMessage { User = stateService.StateInfo.User });

                        break;
                    }
                default:
                    break;
            }



            ListItemSignalREvent GetDeserializedSinglaREvent(string signaREventSerialized)
            {

                return eventName switch
                {
                    SiganalREventName.ListItemAdded => JsonSerializer.Deserialize<ListItemAddedSignalREvent>(signaREventSerialized),
                    SiganalREventName.ListItemEdited => JsonSerializer.Deserialize<ListItemEditedSignalREvent>(signaREventSerialized),
                    SiganalREventName.ListItemDeleted => JsonSerializer.Deserialize<ListItemDeletedSignalREvent>(signaREventSerialized),
                    _ => throw new ArgumentException("Unknown signaREvent")
                };
            }

        }

        public static async Task SignalRInvitationInitAsync(Func<Task> SetInvitaionNewIndicator)
        {
            await SetInvitaionNewIndicator();
        }




    }


}

