# RSS
_namespace: [Microsoft.VisualBasic.Net.WWW.Google.News](./index.md)_

RSS (Rich Site Summary; originally RDF Site Summary; often called Really Simple Syndication) uses a family of standard web feed formats[2] to publish frequently 
 updated information: blog entries, news headlines, audio, video. An RSS document (called "feed", "web feed",[3] or "channel") includes full or summarized text, 
 and metadata, like publishing date and author's name.
 RSS feeds enable publishers To syndicate data automatically. A standard XML file format ensures compatibility With many different machines/programs. RSS feeds 
 also benefit users who want To receive timely updates from favourite websites Or To aggregate data from many sites.
 Subscribing to a website RSS removes the need for the user to manually check the website for New content. Instead, their browser constantly monitors the site And 
 informs the user of any updates. The browser can also be commanded to automatically download the New data for the user.
 Software termed "RSS reader", "aggregator", Or "feed reader", which can be web-based, desktop-based, Or mobile-device-based, presents RSS feed data To users. 
 Users subscribe To feeds either by entering a feed's URI into the reader or by clicking on the browser's feed icon. The RSS reader checks the user's feeds 
 regularly for new information and can automatically download it, if that function is enabled. The reader also provides a user interface.
 
 Google news RSS URL Example:
 
 ```
 http://news.google.com/news?pz=1&cf=all&ned=us&hl=en&as_maxm=11&q=allintitle:+zika&as_qdr=a&as_drrb=q&as_mind=26&as_minm=10&cf=all&as_maxd=25&scoring=n&output=rss
 ```



### Methods

#### Fetch
```csharp
Microsoft.VisualBasic.Net.WWW.Google.News.RSS.Fetch(System.String,System.String)
```
Download rss data from a exists url specific.

|Parameter Name|Remarks|
|--------------|-------|
|url|-|
|proxy|Some region required of proxy server for visit google.|


#### GetCurrent
```csharp
Microsoft.VisualBasic.Net.WWW.Google.News.RSS.GetCurrent(System.String,System.String)
```
Download rss info from google news by keyword query.

|Parameter Name|Remarks|
|--------------|-------|
|query|-|
|proxy|-|



