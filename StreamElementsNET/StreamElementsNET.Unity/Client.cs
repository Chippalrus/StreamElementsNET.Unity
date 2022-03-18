using Newtonsoft.Json.Linq;
using System;
using System.Timers;
using NativeWebSocket;

namespace StreamElementsNET.Unity
{
    public class Client
    {
        private readonly string StreamElementsUrl = "wss://realtime.streamelements.com/socket.io/?cluster=main&EIO=3&transport=websocket";

        private WebSocket client;
        private string jwt;
        private Timer pingTimer;

        public event EventHandler< string > OnConnected;
        public event EventHandler< WebSocketCloseCode > OnDisconnected;
        public event EventHandler< string > OnError;
        public event EventHandler<string> OnSent;
        public event EventHandler<string> OnReceivedRawMessage;

        // authentication
        public event EventHandler<StreamElementsNET.Models.Internal.Authenticated> OnAuthenticated;
        public event EventHandler OnAuthenticationFailure;

        // follower
        public event EventHandler<StreamElementsNET.Models.Follower.Follower> OnFollower;
        public event EventHandler<string> OnFollowerLatest;
        public event EventHandler<int> OnFollowerGoal;
        public event EventHandler<int> OnFollowerMonth;
        public event EventHandler<int> OnFollowerWeek;
        public event EventHandler<int> OnFollowerTotal;
        public event EventHandler<int> OnFollowerSession;

        // cheer
        public event EventHandler<StreamElementsNET.Models.Cheer.Cheer> OnCheer;
        public event EventHandler<StreamElementsNET.Models.Cheer.CheerLatest> OnCheerLatest;
        public event EventHandler<int> OnCheerGoal;
        public event EventHandler<int> OnCheerCount;
        public event EventHandler<int> OnCheerTotal;
        public event EventHandler<int> OnCheerSession;
        public event EventHandler<StreamElementsNET.Models.Cheer.CheerSessionTopDonator> OnCheerSessionTopDonator;
        public event EventHandler<StreamElementsNET.Models.Cheer.CheerSessionTopDonation> OnCheerSessionTopDonation;
        public event EventHandler<int> OnCheerMonth;
        public event EventHandler<int> OnCheerWeek;

        // Raid
        public event EventHandler<Models.Raid.Raid> OnRaid;
        public event EventHandler<Models.Raid.RaidLatest> OnRaidLatest;

        // host
        public event EventHandler<StreamElementsNET.Models.Host.Host> OnHost;
        public event EventHandler<StreamElementsNET.Models.Host.HostLatest> OnHostLatest;

        //Redeems
        public event EventHandler<Models.Redemption.RedemptionLatest> OnRedemptionLatest;

        // tip
        public event EventHandler<StreamElementsNET.Models.Tip.Tip> OnTip;
        public event EventHandler<int> OnTipCount;
        public event EventHandler<StreamElementsNET.Models.Tip.TipLatest> OnTipLatest;
        public event EventHandler<double> OnTipSession;
        public event EventHandler<double> OnTipGoal;
        public event EventHandler<double> OnTipWeek;
        public event EventHandler<double> OnTipTotal;
        public event EventHandler<double> OnTipMonth;
        public event EventHandler<StreamElementsNET.Models.Tip.TipSessionTopDonator> OnTipSessionTopDonator;
        public event EventHandler<StreamElementsNET.Models.Tip.TipSessionTopDonation> OnTipSessionTopDonation;

        // subscriber
        public event EventHandler<StreamElementsNET.Models.Subscriber.Subscriber> OnSubscriber;
        public event EventHandler<StreamElementsNET.Models.Subscriber.SubscriberLatest> OnSubscriberLatest;
        public event EventHandler<int> OnSubscriberSession;
        public event EventHandler<int> OnSubscriberGoal;
        public event EventHandler<int> OnSubscriberMonth;
        public event EventHandler<int> OnSubscriberWeek;
        public event EventHandler<int> OnSubscriberTotal;
        public event EventHandler<int> OnSubscriberPoints;
        public event EventHandler<int> OnSubscriberResubSession;
        public event EventHandler<StreamElementsNET.Models.Subscriber.SubscriberResubLatest> OnSubscriberResubLatest;
        public event EventHandler<int> OnSubscriberNewSession;
        public event EventHandler<int> OnSubscriberGiftedSession;
        public event EventHandler<StreamElementsNET.Models.Subscriber.SubscriberNewLatest> OnSubscriberNewLatest;
        public event EventHandler<StreamElementsNET.Models.Subscriber.SubscriberAlltimeGifter> OnSubscriberAlltimeGifter;
        public event EventHandler<StreamElementsNET.Models.Subscriber.SubscriberGiftedLatest> OnSubscriberGiftedLatest;

        // unknowns
        public event EventHandler<string> OnUnknownComplexObject;
        public event EventHandler<string> OnUnknownSimpleUpdate;

        public Client(string jwtToken)
        {
            jwt = jwtToken;
            client = new WebSocket(StreamElementsUrl);

            client.OnOpen += Client_Opened;
            client.OnError += Client_Error;
            client.OnClose += Client_Closed;
            client.OnMessage += Client_MessageReceived;
        }

        public async void Connect()
        {
            await client.Connect();
            send( "2" );
        }

        public async void Disconnect()
        {
            if( client.State == WebSocketState.Open )
            {
                await client.Close();
            }
        }

        private async void send(string msg)
        {
            if( client.State == WebSocketState.Open )
            {
                await client.SendText(msg);
                OnSent?.Invoke(client, msg);
            }
        }

        private void handleAuthentication()
        {
            send($"42[\"authenticate\",{{\"method\":\"jwt\",\"token\":\"{jwt}\"}}]");
        }

        private void handlePingInitialization(StreamElementsNET.Models.Internal.SessionMetadata md)
        {
            // start with a ping
            send("2");
            // start ping timer
            pingTimer = new Timer(md.PingInterval);
            pingTimer.Elapsed += PingTimer_Elapsed;
            pingTimer.Start();
        }

        private void Client_MessageReceived( byte[] e )
        {
            string sMsg = System.Text.Encoding.UTF8.GetString( e );
            OnReceivedRawMessage?.Invoke(client, sMsg);
            // there's a number at the start of every message, figure out what it is, and remove it
            var raw = sMsg;
            if (raw.Contains("\""))
            {
                var number = sMsg.Split('"')[0].Substring(0, sMsg.Split('"')[0].Length - 1);
                raw = sMsg.Substring(number.Length);
            }
            if (sMsg.StartsWith("40"))
            {
                handleAuthentication();
                return;
            }
            if (sMsg.StartsWith("0{\"sid\""))
            {
                handlePingInitialization(StreamElementsNET.Parsing.Internal.handleSessionMetadata(JObject.Parse(raw)));
            }
            if (sMsg.StartsWith("42[\"authenticated\""))
            {
                OnAuthenticated?.Invoke(client, StreamElementsNET.Parsing.Internal.handleAuthenticated(JArray.Parse(raw)));
                return;
            }
            if (sMsg.StartsWith("42[\"unauthorized\""))
            {
                OnAuthenticationFailure?.Invoke(client, null);
            }
            if (sMsg.StartsWith("42[\"event\",{\"_id\""))
            {
                handleComplexObject(JArray.Parse(raw));
                return;
            }
            if (sMsg.StartsWith("42[\"event:update\",{\"name\""))
            {
                handleSimpleUpdate(JArray.Parse(raw));
                return;
            }
            if (sMsg.StartsWith("42[\"event:test\""))
            {
                HandleTest( JArray.Parse( raw ) );
                return;
            }
        }

        private void handleComplexObject(JArray decoded)
        {
            // only handle "event" types
            if (decoded[0].ToString() != "event")
                return;
            // only handle follows from twitch
            if (decoded[1]["provider"].ToString() != "twitch")
                return;
            switch (decoded[1]["type"].ToString())
            {
                case "follow":
                    OnFollower?.Invoke(client, StreamElementsNET.Parsing.Follower.handleFollower(decoded[1]["data"]));
                    return;
                case "cheer":
                    OnCheer?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheer(decoded[1]["data"]));
                    return;
                case "host":
                    OnHost?.Invoke(client, StreamElementsNET.Parsing.Host.handleHost(decoded[1]["data"]));
                    return;
                case "tip":
                    OnTip?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTip(decoded[1]["data"]));
                    return;
                case "subscriber":
                    OnSubscriber?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriber(decoded[1]["data"]));
                    return;
                case "raid":
                    OnRaid?.Invoke( client, Parsing.Raid.handleRaid( decoded[ 1 ][ "data" ] ) ) ;
                    return;
                default:
                    OnUnknownComplexObject?.Invoke(client, decoded[1]["type"].ToString());
                    return;
            }
        }

        private void handleSimpleUpdate(JArray decoded)
        {
            // only handle "event:update" types
            if (decoded[0].ToString() != "event:update")
                return;
            var data = decoded[1]["data"];
            switch (decoded[1]["name"].ToString())
            {
                case "follower-latest":
                    OnFollowerLatest?.Invoke(client, StreamElementsNET.Parsing.Follower.handleFollowerLatest(data));
                    return;
                case "follower-goal":
                    OnFollowerGoal?.Invoke(client, StreamElementsNET.Parsing.Follower.handleFollowerGoal(data));
                    return;
                case "follower-month":
                    OnFollowerMonth?.Invoke(client, StreamElementsNET.Parsing.Follower.handleFollowerMonth(data));
                    return;
                case "follower-week":
                    OnFollowerWeek?.Invoke(client, StreamElementsNET.Parsing.Follower.handleFollowerWeek(data));
                    return;
                case "follower-total":
                    OnFollowerTotal?.Invoke(client, StreamElementsNET.Parsing.Follower.handleFollowerTotal(data));
                    return;
                case "follower-session":
                    OnFollowerSession?.Invoke(client, StreamElementsNET.Parsing.Follower.handleFollowerSession(data));
                    return;
                case "cheer-latest":
                    OnCheerLatest?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerLatest(data));
                    return;
                case "cheer-goal":
                    OnCheerGoal?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerGoal(data));
                    return;
                case "cheer-count":
                    OnCheerCount?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerCount(data));
                    return;
                case "cheer-total":
                    OnCheerTotal?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerTotal(data));
                    return;
                case "cheer-session":
                    OnCheerSession?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerSession(data));
                    return;
                case "cheer-session-top-donator":
                    OnCheerSessionTopDonator?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerSessionTopDonator(data));
                    return;
                case "cheer-session-top-donation":
                    OnCheerSessionTopDonation?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerSessionTopDonation(data));
                    return;
                case "cheer-month":
                    OnCheerMonth?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerMonth(data));
                    return;
                case "cheer-week":
                    OnCheerWeek?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerWeek(data));
                    return;
                case "host-latest":
                    OnHostLatest?.Invoke(client, StreamElementsNET.Parsing.Host.handleHostLatest(data));
                    return;
                case "tip-count":
                    OnTipCount?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipCount(data));
                    return;
                case "tip-latest":
                    OnTipLatest?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipLatest(data));
                    return;
                case "tip-session":
                    OnTipSession?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipSession(data));
                    return;
                case "tip-goal":
                    OnTipGoal?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipGoal(data));
                    return;
                case "tip-week":
                    OnTipWeek?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipWeek(data));
                    return;
                case "tip-total":
                    OnTipTotal?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipTotal(data));
                    return;
                case "tip-month":
                    OnTipMonth?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipMonth(data));
                    return;
                case "tip-session-top-donator":
                    OnTipSessionTopDonator?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipSessionTopDonator(data));
                    return;
                case "tip-session-top-donation":
                    OnTipSessionTopDonation?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipSessionTopDonation(data));
                    return;
                case "subscriber-latest":
                    OnSubscriberLatest?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberLatest(data));
                    return;
                case "subscriber-session":
                    OnSubscriberSession?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberSession(data));
                    return;
                case "subscriber-goal":
                    OnSubscriberGoal?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberGoal(data));
                    return;
                case "subscriber-month":
                    OnSubscriberMonth?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberMonth(data));
                    return;
                case "subscriber-week":
                    OnSubscriberWeek?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberWeek(data));
                    return;
                case "subscriber-total":
                    OnSubscriberTotal?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberTotal(data));
                    return;
                case "subscriber-points":
                    OnSubscriberPoints?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberPoints(data));
                    return;
                case "subscriber-resub-session":
                    OnSubscriberResubSession?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberResubSession(data));
                    return;
                case "subscriber-resub-latest":
                    OnSubscriberResubLatest?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberResubLatest(data));
                    return;
                case "subscriber-new-session":
                    OnSubscriberNewSession?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberNewSession(data));
                    return;
                case "subscriber-gifted-session":
                    OnSubscriberGiftedSession?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberGiftedSession(data));
                    return;
                case "subscriber-new-latest":
                    OnSubscriberNewLatest?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberNewLatest(data));
                    return;
                case "subscriber-alltime-gifter":
                    OnSubscriberAlltimeGifter?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberAlltimeGifter(data));
                    return;
                case "subscriber-gifted-latest":
                    OnSubscriberGiftedLatest?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberGiftedLatest(data));
                    return;
                case "raid-latest":
                    OnRaidLatest?.Invoke( client, Parsing.Raid.handleRaidLatest( data ) );
                    return;
                case "redemption-latest":
                    OnRedemptionLatest?.Invoke( client, Parsing.Redemption.handleRedemptionLatest( data ) );
                    return;
                default:
                    OnUnknownSimpleUpdate?.Invoke(client, decoded[1]["name"].ToString());
                    return;
            }
        }
        private void HandleTest( JArray decoded )
        {
            if( decoded[ 0 ].ToString() == "event:test" )
            {
                var data = decoded[ 1 ][ "event" ];
                var eventName = decoded[ 1 ][ "listener" ].ToString();

                if( data is JArray )
                {
                    foreach ( var Event in data )
                    {
                        HandleTestUpdate( eventName, Event );
                    }
                }
                else
                {
                    HandleTestUpdate( eventName, data );
                }
            }
        }
        private void HandleTestUpdate(string eventName, JToken data)
        {
            switch(eventName)
            {
                case "follower-latest":
                    OnFollowerLatest?.Invoke(client, StreamElementsNET.Parsing.Follower.handleFollowerLatest(data));
                    return;
                case "follower-goal":
                    OnFollowerGoal?.Invoke(client, StreamElementsNET.Parsing.Follower.handleFollowerGoal(data));
                    return;
                case "follower-month":
                    OnFollowerMonth?.Invoke(client, StreamElementsNET.Parsing.Follower.handleFollowerMonth(data));
                    return;
                case "follower-week":
                    OnFollowerWeek?.Invoke(client, StreamElementsNET.Parsing.Follower.handleFollowerWeek(data));
                    return;
                case "follower-total":
                    OnFollowerTotal?.Invoke(client, StreamElementsNET.Parsing.Follower.handleFollowerTotal(data));
                    return;
                case "follower-session":
                    OnFollowerSession?.Invoke(client, StreamElementsNET.Parsing.Follower.handleFollowerSession(data));
                    return;
                case "cheer-latest":
                    OnCheerLatest?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerLatest(data));
                    return;
                case "cheer-goal":
                    OnCheerGoal?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerGoal(data));
                    return;
                case "cheer-count":
                    OnCheerCount?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerCount(data));
                    return;
                case "cheer-total":
                    OnCheerTotal?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerTotal(data));
                    return;
                case "cheer-session":
                    OnCheerSession?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerSession(data));
                    return;
                case "cheer-session-top-donator":
                    OnCheerSessionTopDonator?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerSessionTopDonator(data));
                    return;
                case "cheer-session-top-donation":
                    OnCheerSessionTopDonation?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerSessionTopDonation(data));
                    return;
                case "cheer-month":
                    OnCheerMonth?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerMonth(data));
                    return;
                case "cheer-week":
                    OnCheerWeek?.Invoke(client, StreamElementsNET.Parsing.Cheer.handleCheerWeek(data));
                    return;
                case "host-latest":
                    OnHostLatest?.Invoke(client, StreamElementsNET.Parsing.Host.handleHostLatest(data));
                    return;
                case "tip-count":
                    OnTipCount?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipCount(data));
                    return;
                case "tip-latest":
                    OnTipLatest?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipLatest(data));
                    return;
                case "tip-session":
                    OnTipSession?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipSession(data));
                    return;
                case "tip-goal":
                    OnTipGoal?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipGoal(data));
                    return;
                case "tip-week":
                    OnTipWeek?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipWeek(data));
                    return;
                case "tip-total":
                    OnTipTotal?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipTotal(data));
                    return;
                case "tip-month":
                    OnTipMonth?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipMonth(data));
                    return;
                case "tip-session-top-donator":
                    OnTipSessionTopDonator?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipSessionTopDonator(data));
                    return;
                case "tip-session-top-donation":
                    OnTipSessionTopDonation?.Invoke(client, StreamElementsNET.Parsing.Tip.handleTipSessionTopDonation(data));
                    return;
                case "subscriber-latest":
                    OnSubscriberLatest?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberLatest(data));
                    return;
                case "subscriber-session":
                    OnSubscriberSession?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberSession(data));
                    return;
                case "subscriber-goal":
                    OnSubscriberGoal?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberGoal(data));
                    return;
                case "subscriber-month":
                    OnSubscriberMonth?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberMonth(data));
                    return;
                case "subscriber-week":
                    OnSubscriberWeek?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberWeek(data));
                    return;
                case "subscriber-total":
                    OnSubscriberTotal?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberTotal(data));
                    return;
                case "subscriber-points":
                    OnSubscriberPoints?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberPoints(data));
                    return;
                case "subscriber-resub-session":
                    OnSubscriberResubSession?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberResubSession(data));
                    return;
                case "subscriber-resub-latest":
                    OnSubscriberResubLatest?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberResubLatest(data));
                    return;
                case "subscriber-new-session":
                    OnSubscriberNewSession?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberNewSession(data));
                    return;
                case "subscriber-gifted-session":
                    OnSubscriberGiftedSession?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberGiftedSession(data));
                    return;
                case "subscriber-new-latest":
                    OnSubscriberNewLatest?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberNewLatest(data));
                    return;
                case "subscriber-alltime-gifter":
                    OnSubscriberAlltimeGifter?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberAlltimeGifter(data));
                    return;
                case "subscriber-gifted-latest":
                    OnSubscriberGiftedLatest?.Invoke(client, StreamElementsNET.Parsing.Subscriber.handleSubscriberGiftedLatest(data));
                    return;
                case "raid-latest":
                    OnRaidLatest?.Invoke(client, Parsing.Raid.handleRaidLatest(data));
                    return;
                case "redemption-latest":
                    OnRedemptionLatest?.Invoke(client, Parsing.Redemption.handleRedemptionLatest(data));
                    return;
                default:
                    OnUnknownSimpleUpdate?.Invoke(client, eventName);
                    return;
            }
        }

        private void Client_Closed( WebSocketCloseCode closeCode )
        {
            pingTimer.Stop();
            OnDisconnected?.Invoke( client, closeCode );
        }

        private void Client_Error( string errorMsg )
        {
            OnError?.Invoke( client, errorMsg );
        }

        private void Client_Opened()
        {
            OnConnected?.Invoke( client, "Connected" );
        }

        private void PingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // to remain connected, we need to send a "2" every 25 seconds
            send("2");
        }
        public  void    DispatchMessageQueue()
        {
            if( client.State == WebSocketState.Open )
            {
                client.DispatchMessageQueue();
            }
        }
    }
}
