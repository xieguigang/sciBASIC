# HTTP_RFC
_namespace: [Microsoft.VisualBasic.Net.Http](./index.md)_

The following is a list of Hypertext Transfer Protocol (HTTP) response status codes. This includes codes from IETF internet standards 
 as well as other IETF RFCs, other specifications and some additional commonly used codes. The first digit of the status code specifies 
 one of five classes of response; the bare minimum for an HTTP client is that it recognises these five classes. The phrases used are the 
 standard examples, but any human-readable alternative can be provided. Unless otherwise stated, the status code is part of the HTTP/1.1 
 standard (RFC 7231).

 The Internet Assigned Numbers Authority (IANA) maintains the official registry Of HTTP status codes.

 Microsoft IIS sometimes uses additional Decimal Sub-codes To provide more specific information, but these are Not listed here.




### Properties

#### RFC_ACCEPTED
202 Accepted |
 The request has been accepted For processing, but the processing has Not been completed. The request might Or might Not eventually be acted upon, 
 As it might be disallowed When processing actually takes place.
#### RFC_ALREADY_REPORTED
208 Already Reported (WebDAV; RFC 5842) |
 The members Of a DAV binding have already been enumerated In a previous reply To this request, And are Not being included again.
#### RFC_AUTH_TIMEOUT
419 Authentication Timeout (Not in RFC 2616) |
 Not a part of the HTTP standard, 419 Authentication Timeout denotes that previously valid authentication has expired. 
 It Is used as an alternative to 401 Unauthorized in order to differentiate from otherwise authenticated clients being denied access to specific server resources.[citation needed]
#### RFC_BAD_GATEWAY
502 Bad Gateway |
 The server was acting As a gateway Or proxy And received an invalid response from the upstream server.
#### RFC_BAD_REQUEST
400 Bad Request |
 The server cannot Or will Not process the request due To something that Is perceived To be a client Error (e.g., malformed request syntax, 
 invalid request message framing, Or deceptive request routing).[15]
#### RFC_BANDWIDTH_LIMITED_EXCEEDED
509 Bandwidth Limit Exceeded (Apache bw/limited extension)[31] |
 This status code Is Not specified In any RFCs. Its use Is unknown.
#### RFC_BLOCKED
450 Blocked by Windows Parental Controls (Microsoft) |
 A Microsoft extension. This Error Is given When Windows Parental Controls are turned On And are blocking access To the given webpage.[22]
#### RFC_CERT_ERROR
495 Cert Error (Nginx) |
 Nginx internal code used When SSL client certificate Error occurred To distinguish it from 4XX In a log And an Error page redirection.
 (在SSL层解密的时候错误，则为证书错误)
#### RFC_CLOSED_REQUEST
499 Client Closed Request (Nginx) |
 Used in Nginx logs to indicate when the connection has been closed by client while the server Is still processing its request, making server unable to send a status code back.[29]
#### RFC_CONFLICT
409 Conflict |
 Indicates that the request could Not be processed because Of conflict In the request, such As an edit conflict In the Case Of multiple updates.
#### RFC_CONNECT_TIMEOUT_ERROR
599 Network connect timeout error (Unknown) |
 This status code Is Not specified In any RFCs, but Is used by Microsoft HTTP proxies To signal a network connect timeout behind the proxy To a client In front Of the proxy.[citation needed]
#### RFC_CONNECTION_TIMEOUT
522 Origin Connection Time-out |
 This status code Is Not specified In any RFCs, but Is used by CloudFlare's reverse proxies to signal that a server connection timed out.
#### RFC_CONTINUTE
100 Continue |
 This means that the server has received the request headers, And that the client should proceed To send the request body 
 (In the Case Of a request For which a body needs To be sent; For example, a POST request). If the request body Is large, 
 sending it To a server When a request has already been rejected based upon inappropriate headers Is inefficient. 
 To have a server check If the request could be accepted based On the request's headers alone, a client must send Expect: 
 100-continue as a header in its initial request and check if a 100 Continue status code is received in response before 
 continuing (or receive 417 Expectation Failed and not continue).
#### RFC_CREATED
201 Created |
 The request has been fulfilled And resulted In a New resource being created.
#### RFC_ENHANCE_YOUR_CALM
420 Enhance Your Calm (Twitter) |
 Not part of the HTTP standard, but returned by version 1 of the Twitter Search And Trends API when the client Is being rate limited.[17] 
 Other services may wish to implement the 429 Too Many Requests response code instead.
#### RFC_EXPECTATION_FAILED
417 Expectation Failed |
 The server cannot meet the requirements Of the Expect request-header field.
#### RFC_FAILED_DEPENDENCY
424 Failed Dependency (WebDAV; RFC 4918) |
 The request failed due To failure Of a previous request (e.g., a PROPPATCH).[4]
#### RFC_FORBIDDEN
403 Forbidden |
 The request was a valid request, but the server Is refusing To respond To it. Unlike a 401 Unauthorized response, authenticating will make no difference.
 (被封号了)
#### RFC_FOUND
302 Found |
 This Is an example of industry practice contradicting the standard. The HTTP/1.0 specification (RFC 1945) required the client 
 to perform a temporary redirect (the original describing phrase was "Moved Temporarily"),[6] but popular browsers implemented 
 302 with the functionality of a 303 See Other. Therefore, HTTP/1.1 added status codes 303 And 307 to distinguish between the 
 two behaviours.[7] However, some Web applications And frameworks use the 302 status code as if it were the 303.[8]
#### RFC_GATEWAY_TIMEOUT
504 Gateway Timeout |
 The server was acting As a gateway Or proxy And did Not receive a timely response from the upstream server.
#### RFC_GONE
410 Gone |
 Indicates that the resource requested Is no longer available And will Not be available again. 
 This should be used When a resource has been intentionally removed And the resource should be purged. 
 Upon receiving a 410 status code, the client should Not request the resource again In the future. 
 Clients such As search engines should remove the resource from their indices.[16] Most use cases 
 Do Not require clients And search engines To purge the resource, And a "404 Not Found" may be used instead.
#### RFC_HTTP_TO_HTTPS
497 HTTP to HTTPS (Nginx) |
 Nginx internal code used For the plain HTTP requests that are sent To HTTPS port To distinguish it from 4XX In a log And an Error page redirection.
#### RFC_IM_TEAPOT
418 I'm a teapot (RFC 2324) |
 This code was defined In 1998 As one Of the traditional IETF April Fools' jokes, in RFC 2324, Hyper Text Coffee Pot Control Protocol, 
 and is not expected to be implemented by actual HTTP servers. The RFC specifies this code should be returned by tea pots requested to brew coffee.
#### RFC_IM_USED
226 IM Used (RFC 3229) |
 The server has fulfilled a request For the resource, And the response Is a representation Of the result Of one Or more instance-manipulations applied To the current instance.[5]
#### RFC_INSUFFICIENT_STORAGE
507 Insufficient Storage (WebDAV; RFC 4918) |
 The server Is unable To store the representation needed To complete the request.[4]
#### RFC_INTERNAL_SERVER_ERROR
500 Internal Server Error |
 A generic Error message, given When an unexpected condition was encountered And no more specific message Is suitable.
#### RFC_LEGAL_UNAVAILABLE
451 Unavailable For Legal Reasons (Internet draft) |
 Defined in the internet draft "A New HTTP Status Code for Legally-restricted Resources".[23] 
 Intended to be used when resource access Is denied for legal reasons, e.g. censorship Or government-mandated blocked access. 
 A reference to the 1953 dystopian novel Fahrenheit 451, where books are outlawed.[24]
#### RFC_LENGTH_REQUIRED
411 Length Required |
 The request did Not specify the length Of its content, which Is required by the requested resource.
#### RFC_LOCKED
423 Locked (WebDAV; RFC 4918) |
 The resource that Is being accessed Is locked.[4]
#### RFC_LOGIN_TIMEOUT
440 Login Timeout (Microsoft) |
 A Microsoft extension. Indicates that your session has expired.[20]
#### RFC_LOOP_DETECTED
508 Loop Detected (WebDAV; RFC 5842) |
 The server detected an infinite Loop While processing the request (sent In lieu Of 208 Already Reported).
#### RFC_METHOD_FAILURE
420 Method Failure (Spring Framework) |
 Not part of the HTTP standard, but defined by Spring in the HttpStatus class to be used when a method failed. This status code Is deprecated by Spring.
#### RFC_METHOD_NOT_ALLOWED
405 Method Not Allowed |
 A request was made Of a resource Using a request method Not supported by that resource; For example, 
 Using Get On a form which requires data To be presented via POST, Or Using PUT On a read-only resource.
#### RFC_MISDIRECTED_REQUEST
421 Misdirected Request (HTTP/2) |
 The request was directed at a server that Is Not able To produce a response (For example because a connection reuse).[18]
#### RFC_MOVED_PERMANENTLY
301 Moved Permanently |
 This And all future requests should be directed to the given URI.
#### RFC_MULTI_CHOICES
300 Multiple Choices |
 Indicates multiple options For the resource that the client may follow. It, For instance, could be used To present different 
 format options For video, list files With different extensions, Or word sense disambiguation.
#### RFC_MULTI_STATUS
207 Multi-Status (WebDAV; RFC 4918) |
 The message body that follows Is an XML message And can contain a number Of separate response codes, depending On how many Sub-requests were made.[4]
#### RFC_NEGOTIATES
506 Variant Also Negotiates (RFC 2295) |
 Transparent content negotiation For the request results In a circular reference.[30]
#### RFC_NETWORK_AUTH_REQUIRED
511 Network Authentication Required (RFC 6585) |
 The client needs To authenticate To gain network access. Intended For use by intercepting proxies used To control access To the network 
 (e.g., "captive portals" used To require agreement To Terms Of Service before granting full Internet access via a Wi-Fi hotspot).[19]
#### RFC_NO_CERT
496 No Cert (Nginx) |
 Nginx internal code used When client didn't provide certificate to distinguish it from 4XX in a log and an error page redirection.
#### RFC_NO_CONTENT
204 No Content |
 The server successfully processed the request, but Is Not returning any content.
#### RFC_NO_RESPONSE
444 No Response (Nginx) |
 Used in Nginx logs to indicate that the server has returned no information to the client And closed the connection (useful as a deterrent for malware).
#### RFC_NON_AUTH_INFO
203 Non-Authoritative Information (since HTTP/1.1) |
 The server successfully processed the request, but Is returning information that may be from another source.
#### RFC_NOT_ACCEPTABLE
406 Not Acceptable |
 The requested resource Is only capable Of generating content Not acceptable according To the Accept headers sent In the request.
#### RFC_NOT_EXTENDED
510 Not Extended (RFC 2774) |
 Further extensions To the request are required For the server To fulfil it.[32]
#### RFC_NOT_FOUND
404 Not Found |
 The requested resource could Not be found but may be available again In the future. Subsequent requests by the client are permissible.
#### RFC_NOT_IMPLEMENTED
501 Not Implemented |
 The server either does Not recognize the request method, Or it lacks the ability To fulfill the request. Usually this implies future availability (e.g., a New feature Of a web-service API).
#### RFC_NOT_MODIFIED
304 Not Modified (RFC 7232) |
 Indicates that the resource has Not been modified since the version specified by the request headers If-Modified-Since Or If-None-Match. 
 This means that there Is no need To retransmit the resource, since the client still has a previously-downloaded copy.
#### RFC_OK
200 OK |
 Standard response For successful HTTP requests. The actual response will depend On the request method used. In a Get request, 
 the response will contain an entity corresponding To the requested resource. In a POST request, the response will contain an 
 entity describing Or containing the result Of the action.
 (由于可能会修改附带一些其他的元素据信息，所以只读属性不会使用简写的形式的，而是需要重新生成新的对象实例以防止数据污染)
#### RFC_PARTIAL_CONTENT
206 Partial Content (RFC 7233) |
 The server Is delivering only part Of the resource (Byte serving) due To a range header sent by the client. 
 The range header Is used by HTTP clients To enable resuming Of interrupted downloads, Or split a download into multiple simultaneous streams.
#### RFC_PAYLOAD_TOO_LARGE
413 Payload Too Large (RFC 7231) |
 The request Is larger than the server Is willing Or able To process. Called "Request Entity Too Large " previously.
#### RFC_PAYMENT_REQUIRED
402 Payment Required |
 Reserved for future use. The original intention was that this code might be used as part of some form of digital cash Or micropayment scheme, 
 but that has Not happened, And this code Is Not usually used. YouTube uses this status if a particular IP address has made excessive requests, 
 And requires the person to enter a CAPTCHA.[citation needed]
#### RFC_PERMANENT_REDIRECT
308 Permanent Redirect (RFC 7538) |
 The request, and all future requests should be repeated Using another URI. 307 And 308 (As proposed) parallel the behaviours 
 Of 302 And 301, but Do Not allow the HTTP method To change. So, For example, submitting a form To a permanently redirected resource may Continue smoothly.[13]
#### RFC_PRECONDITION_FAILED
412 Precondition Failed (RFC 7232) |
 The server does Not meet one Of the preconditions that the requester put On the request.
#### RFC_PRECONDITION_REQUIRED
428 Precondition Required (RFC 6585) |
 The origin server requires the request To be conditional. Intended To prevent "the 'lost update' problem, 
 where a client GETs a resource's state, modifies it, and PUTs it back to the server, when meanwhile a third party 
 has modified the state on the server, leading to a conflict."[19]
#### RFC_PROCESSING
102 Processing (WebDAV; RFC 2518) |
 As a WebDAV request may contain many sub-requests involving file operations, it may take a long time to complete the request. 
 This code indicates that the server has received And Is processing the request, but no response Is available yet.[3] 
 This prevents the client from timing out And assuming the request was lost.
#### RFC_PROXY_AUTH_REQUIRED
407 Proxy Authentication Required (RFC 7235) |
 The client must first authenticate itself With the proxy.
#### RFC_RANGE_NOT_SATISFIABLE
416 Requested Range Not Satisfiable (RFC 7233) |
 The client has asked For a portion Of the file (Byte serving), but the server cannot supply that portion. For example, If the client asked For a part Of the file that lies beyond the End Of the file.
#### RFC_READ_TIMEOUT_ERROR
598 Network read timeout error (Unknown) |
 This status code Is Not specified In any RFCs, but Is used by Microsoft HTTP proxies To signal a network read timeout behind the proxy To a client In front Of the proxy.[citation needed]
#### RFC_REDIRECT
451 Redirect (Microsoft) |
 Used in Exchange ActiveSync if there either Is a more efficient server to use Or the server cannot access the users' mailbox.[25]
 The client Is supposed To re-run the HTTP Autodiscovery protocol To find a better suited server.[26]
#### RFC_REQUEST_HEADER_FIELDS_TOO_LARGE
431 Request Header Fields Too Large (RFC 6585) |
 The server Is unwilling To process the request because either an individual header field, Or all the header fields collectively, are too large.[19]
#### RFC_REQUEST_HEADER_TOO_LARGE
494 Request Header Too Large (Nginx) |
 Nginx internal code similar To 431 but it was introduced earlier In version 0.9.4 (On January 21, 2011).[27][original research?]
#### RFC_REQUEST_TIMEOUT
408 Request Timeout |
 The server timed out waiting For the request. According To HTTP specifications: 
 "The client did not produce a request within the time that the server was prepared to wait. The client MAY repeat the request without modifications at any later time."
 (请求超时)
#### RFC_RESET_CONTENT
205 Reset Content |
 The server successfully processed the request, but Is Not returning any content. Unlike a 204 response, this response requires that the requester reset the document view.
#### RFC_RESUME_INCOMPLETE
308 Resume Incomplete (Google) |
 This code Is used In the Resumable HTTP Requests Proposal To Resume aborted PUT Or POST requests.[14]
#### RFC_RETRY_WITH
449 Retry With (Microsoft) |
 A Microsoft extension. The request should be retried after performing the appropriate action.[21]
#### RFC_SEE_OTHER
303 See Other (since HTTP/1.1) |
 The response To the request can be found under another URI Using a Get method. When received In response To a POST (Or PUT/DELETE), 
 it should be assumed that the server has received the data And the redirect should be issued With a separate Get message.
#### RFC_SERVICE_UNAVAILABLE
503 Service Unavailable |
 The server Is currently unavailable (because it Is overloaded Or down For maintenance). Generally, this Is a temporary state.
#### RFC_SWITCH_PROXY
306 Switch Proxy |
 No longer used. Originally meant "Subsequent requests should use the specified proxy."[11]
#### RFC_SWITCHING_PROTOCOLS
101 Switching Protocols |
 This means the requester has asked the server To switch protocols And the server Is acknowledging that it will Do so.
#### RFC_TEMP_REDIRECT
307 Temporary Redirect (since HTTP/1.1) |
 In this case, the request should be repeated with another URI; however, future requests should still use the original URI. 
 In contrast to how 302 was historically implemented, the request method Is Not allowed to be changed when reissuing the original request. 
 For instance, a POST request should be repeated using another POST request.[12]
#### RFC_TOKEN_INVALID
498 Token expired/invalid (Esri) |
 Returned by ArcGIS For Server. A code Of 498 indicates an expired Or otherwise invalid token.[28]
 (错误的参数信息)
#### RFC_TOKEN_REQUIRED
499 Token required (Esri) |
 Returned by ArcGIS For Server. A code Of 499 indicates that a token Is required (If no token was submitted).[28]
#### RFC_TOO_MANY_REQUEST
429 Too Many Requests (RFC 6585) |
 The user has sent too many requests In a given amount Of time. Intended For use With rate limiting schemes.[19]
#### RFC_UNAUTHORIZED
401 Unauthorized (RFC 7235) |
 Similar to 403 Forbidden, but specifically for use when authentication Is required And has failed Or has Not yet been provided. 
 The response must include a WWW-Authenticate header field containing a challenge applicable to the requested resource. 
 See Basic access authentication And Digest access authentication.(证书未认证)
#### RFC_UNKNOWN_ERROR
520 Unknown Error |
 This status code Is Not specified In any RFC And Is returned by certain services, For instance Microsoft Azure And CloudFlare servers: 
 "The 520 error is essentially a “catch-all” response for when the origin server returns something unexpected or something that is not 
 tolerated/interpreted (protocol violation or empty response)."[33]
#### RFC_UNPROCESSABLE_ENTITY
422 Unprocessable Entity (WebDAV; RFC 4918) |
 The request was well-formed but was unable To be followed due To semantic errors.[4]
#### RFC_UNSUPPORTED_MEDIA_TYPE
415 Unsupported Media Type |
 The request entity has a media type which the server Or resource does Not support. For example, the client uploads an image As image/svg+xml, but the server requires that images use a different format.
#### RFC_UPGRADE_REQUIRED
426 Upgrade Required |
 The client should switch To a different protocol such As TLS/1.0, given In the Upgrade header field.
#### RFC_URI_TOO_LONG
414 Request-URI Too Long |
 The URI provided was too Long For the server To process. Often the result Of too much data being encoded As a query-String Of a Get request, In which Case it should be converted To a POST request.
#### RFC_USE_PROXY
305 Use Proxy (since HTTP/1.1) |
 The requested resource Is only available through a proxy, whose address Is provided In the response. Many HTTP clients 
 (such As Mozilla[9] And Internet Explorer) Do Not correctly handle responses With this status code, primarily For security reasons.[10]
#### RFC_VERSION_NOT_SUPPORTED
505 HTTP Version Not Supported |
 The server does Not support the HTTP protocol version used In the request.
