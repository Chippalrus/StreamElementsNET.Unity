# StreamElementsNET.Unity
 StreamElementsNET for Unity and uses NativeWebSocket instead of WebSocket4Net
 
#### Requirements
 - Uses NativeWebSocket for Unity ( https://github.com/endel/NativeWebSocket )
 - Uses StreamElementsNET Models ( https://github.com/swiftyspiffy/StreamElementsNET )
 
### Libraries
- Newtonsoft.Json - JSON parsing
 
### Usage for Unity
```
public class Test : MonoBehaviour
{
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
...
```

# StreamElementsNET

## Overview
C# library for reading event data from StreamElements. Events include tips, subscriptions, hosts, followers, and cheers. All that is required is the JWT that is obtained when logged into StreamElements. **DESIGNED FOR TWITCH INTEGRATIONS**

### Supported events
##### Tips
- `OnTip`
- `OnTipCount`
- `OnTipLatest`
- `OnTipSession`
- `OnTipGoal`
- `OnTipWeek`
- `OnTipTotal`
- `OnTipMonth`
- `OnTipSessionTopDonator`
- `OnTipSessionTopDonation`

##### Subscriptions
- `OnSubscriber`
- `OnSubscriberLatest`
- `OnSubscriberSession`
- `OnSubscriberGoal`
- `OnSubscriberMonth`
- `OnSubscriberWWeek`
- `OnSubscriberTotal`
- `OnSubscriberPoints`
- `OnSubscriberResubSession`
- `OnSubscriberResubLatest`
- `OnSubscriberNewSession`
- `OnSubscriberGiftedSession`
- `OnSubscriberNewLatest`
- `OnSubscriberAlltimeGifter`
- `OnSubscriberGiftedLatest`

##### Hosts
- `OnHost`
- `OnHostLatest`

##### Followers 
- `OnFollower`
- `OnFollowerLatest`
- `OnFollowerGoal`
- `OnFollowerMonth`
- `OnFollowerWeek`
- `OnFollowerTotal`
- `OnFollowerSession`

##### Cheers
- `OnCheer`
- `OnCheerLatest`
- `OnCheerGoal`
- `OnCheerCount`
- `OnCheerTotal`
- `OnCheerSession`
- `OnCheerSessionTopDonator`
- `OnCheerSessionTopDonation`
- `OnCheerMonth`
- `OnCheerWeek`

### Testing
A tests project has been included in this repository to demonstrate basic usage of the library.

### NuGet
Available via NuGet: Install-Package [StreamElementNET](https://www.nuget.org/packages/StreamElementsNET/)

## Libraries
- Newtonsoft.Json - JSON parsing
- Websocket4net - Websocket client

## Contributors
 * Cole ([@swiftyspiffy](http://twitter.com/swiftyspiffy))
 
## License
MIT License. &copy; 2021 Cole