# Search
_namespace: [Microsoft.VisualBasic.Webservices.Github.API](./index.md)_





### Methods

#### Users
```csharp
Microsoft.VisualBasic.Webservices.Github.API.Search.Users(System.Collections.Specialized.NameValueCollection,Microsoft.VisualBasic.Webservices.Github.API.Search.UserSorts,Microsoft.VisualBasic.Webservices.Github.API.Search.UserSortOrders)
```
[Search users]

 Find users via various criteria. (This method returns up To 100 results per page.)

|Parameter Name|Remarks|
|--------------|-------|
|q|The search terms.|
|sort|
 The sort field. Can be @``F:Microsoft.VisualBasic.Webservices.Github.API.Search.UserSorts.followers``, @``F:Microsoft.VisualBasic.Webservices.Github.API.Search.UserSorts.repositories``, 
 or @``F:Microsoft.VisualBasic.Webservices.Github.API.Search.UserSorts.joined``. Default: results are sorted by best match.
 |
|[order]|
 The sort order if sort parameter is provided. One of @``F:Microsoft.VisualBasic.Webservices.Github.API.Search.UserSortOrders.asc`` or 
 @``F:Microsoft.VisualBasic.Webservices.Github.API.Search.UserSortOrders.desc``. Default: @``F:Microsoft.VisualBasic.Webservices.Github.API.Search.UserSortOrders.desc``
 |



