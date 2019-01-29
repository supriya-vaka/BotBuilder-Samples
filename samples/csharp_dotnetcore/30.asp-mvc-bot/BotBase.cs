﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Schema;

namespace Asp_Mvc_Bot
{
    public class BotBase : IIntegrationBot
    {
        private IAdapterIntegration _adapter;

        public BotBase(IAdapterIntegration adapter)
        {
            _adapter = adapter;
        }

        public async Task<InvokeResponse> ProcessAsync(string authHeader, Activity activity)
        {
            var invokeResponse = await _adapter.ProcessActivityAsync(
                authHeader,
                activity,
                OnTurnAsync,
                default(CancellationToken));

            return invokeResponse;
        }

        protected virtual Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                return OnMessageAsync(new DelegatingTurnContext<IMessageActivity>(turnContext), cancellationToken);
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                return OnConversationUpdateAsync(new DelegatingTurnContext<IConversationUpdateActivity>(turnContext), cancellationToken);
            }
            else
            {
                return OnUnrecognizedActivityTypeAsync(turnContext, cancellationToken);
            }
        }

        protected virtual Task OnMessageAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnConversationUpdateAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private class DelegatingTurnContext<T> : ITurnContext<T>
        {
            private ITurnContext _innerTurnContext;

            public DelegatingTurnContext(ITurnContext innerTurnContext)
            {
                _innerTurnContext = innerTurnContext;
            }

            T ITurnContext<T>.Activity => (T)(object)_innerTurnContext.Activity;

            public BotAdapter Adapter => _innerTurnContext.Adapter;

            public TurnContextStateCollection TurnState => _innerTurnContext.TurnState;

            public Activity Activity => _innerTurnContext.Activity;

            public bool Responded => _innerTurnContext.Responded;

            public Task DeleteActivityAsync(string activityId, CancellationToken cancellationToken = default(CancellationToken))
            {
                return _innerTurnContext.DeleteActivityAsync(activityId, cancellationToken);
            }

            public Task DeleteActivityAsync(ConversationReference conversationReference, CancellationToken cancellationToken = default(CancellationToken))
            {
                return _innerTurnContext.DeleteActivityAsync(conversationReference, cancellationToken);
            }

            public ITurnContext OnDeleteActivity(DeleteActivityHandler handler)
            {
                return _innerTurnContext.OnDeleteActivity(handler);
            }

            public ITurnContext OnSendActivities(SendActivitiesHandler handler)
            {
                return _innerTurnContext.OnSendActivities(handler);
            }

            public ITurnContext OnUpdateActivity(UpdateActivityHandler handler)
            {
                return _innerTurnContext.OnUpdateActivity(handler);
            }

            public Task<ResourceResponse[]> SendActivitiesAsync(IActivity[] activities, CancellationToken cancellationToken = default(CancellationToken))
            {
                return _innerTurnContext.SendActivitiesAsync(activities, cancellationToken);
            }

            public Task<ResourceResponse> SendActivityAsync(string textReplyToSend, string speak = null, string inputHint = "acceptingInput", CancellationToken cancellationToken = default(CancellationToken))
            {
                return _innerTurnContext.SendActivityAsync(textReplyToSend, speak, inputHint, cancellationToken);
            }

            public Task<ResourceResponse> SendActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
            {
                return _innerTurnContext.SendActivityAsync(activity, cancellationToken);
            }

            public Task<ResourceResponse> UpdateActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
            {
                return _innerTurnContext.UpdateActivityAsync(activity, cancellationToken);
            }
        }
    }
}