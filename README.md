# StreamElementsNET.Unity
 StreamElementsNET for Unity that uses [NativeWebSocket](https://github.com/endel/NativeWebSocket) instead of [WebSocket4Net](https://github.com/kerryjiang/WebSocket4Net)
 
#### Required Libraries
 - NativeWebSocket for Unity ( https://github.com/endel/NativeWebSocket )
 - StreamElementsNET Models ( https://github.com/swiftyspiffy/StreamElementsNET )
 - Newtonsoft.Json - JSON parsing
 
### Usage for Unity
```
private StreamElementsNET.Unity.Client streamElements;

void Start()
{
    streamElements = new StreamElementsNET.Unity.Client( "<JWT-TOKEN>" );
    streamElements.OnConnected += StreamElements_OnConnected;
    streamElements.OnAuthenticated += StreamElements_OnAuthenticated;
    streamElements.OnFollower += StreamElements_OnFollower;
    streamElements.OnSubscriber += StreamElements_OnSubscriber;
    streamElements.OnHost += StreamElements_OnHost;
    streamElements.OnTip += StreamElements_OnTip;
    streamElements.OnCheer += StreamElements_OnCheer;
    streamElements.OnAuthenticationFailure += StreamElements_OnAuthenticationFailure;
    streamElements.OnReceivedRawMessage += StreamElements_OnReceivedRawMessage;
    streamElements.OnSent += StreamElements_OnSent;

    streamElements.Connect();
}

void Update()
{
    streamElements.DispatchMessageQueue();
}
```

# StreamElementsNET
 [StreamElementsNET](https://github.com/swiftyspiffy/StreamElementsNET) by [@swiftyspiffy](http://twitter.com/swiftyspiffy)
