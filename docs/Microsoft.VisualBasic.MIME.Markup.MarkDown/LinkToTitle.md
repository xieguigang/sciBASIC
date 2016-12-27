# LinkToTitle
_namespace: [Microsoft.VisualBasic.MIME.Markup.MarkDown](./index.md)_

Add title to youtube link



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.LinkToTitle.#ctor(System.String,System.Int32)
```
FiXME: max ids?

|Parameter Name|Remarks|
|--------------|-------|
|apiKey|-|
|maxLinks|-|


#### GetExtension
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.LinkToTitle.GetExtension(System.String,System.Int32)
```
FiXME: max ids?

|Parameter Name|Remarks|
|--------------|-------|
|apiKey|-|
|maxLinks|-|


#### ParseApiResponse
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.LinkToTitle.ParseApiResponse(System.String)
```
Parse API JSON response

|Parameter Name|Remarks|
|--------------|-------|
|res|-|


#### RequestToGoogleApi
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.LinkToTitle.RequestToGoogleApi(System.String)
```
Get videos titles from youtube API
 More info: https://developers.google.com/youtube/v3/

|Parameter Name|Remarks|
|--------------|-------|
|ids|-|


_returns: Return null string if request failed_


### Properties

#### _apiKey
Google api key
#### _links
Array of links: videoID/title
