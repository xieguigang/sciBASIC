#Region "Microsoft.VisualBasic::61425b732450b4a9e6446347bd269581, Microsoft.VisualBasic.Core\Extensions\WebServices\HTTP_RFC.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Module HTTP_RFC
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Net.Http

    ''' <summary>
    ''' The following is a list of Hypertext Transfer Protocol (HTTP) response status codes. This includes codes from IETF internet standards 
    ''' as well as other IETF RFCs, other specifications and some additional commonly used codes. The first digit of the status code specifies 
    ''' one of five classes of response; the bare minimum for an HTTP client is that it recognises these five classes. The phrases used are the 
    ''' standard examples, but any human-readable alternative can be provided. Unless otherwise stated, the status code is part of the HTTP/1.1 
    ''' standard (RFC 7231).
    '''
    ''' The Internet Assigned Numbers Authority (IANA) maintains the official registry Of HTTP status codes.
    '''
    ''' Microsoft IIS sometimes uses additional Decimal Sub-codes To provide more specific information, but these are Not listed here.
    ''' </summary>
    Public Module HTTP_RFC

#Region "1xx Informational"

        ''' <summary>
        ''' 100 Continue |
        ''' This means that the server has received the request headers, And that the client should proceed To send the request body 
        ''' (In the Case Of a request For which a body needs To be sent; For example, a POST request). If the request body Is large, 
        ''' sending it To a server When a request has already been rejected based upon inappropriate headers Is inefficient. 
        ''' To have a server check If the request could be accepted based On the request's headers alone, a client must send Expect: 
        ''' 100-continue as a header in its initial request and check if a 100 Continue status code is received in response before 
        ''' continuing (or receive 417 Expectation Failed and not continue).
        ''' </summary>

        Public Const RFC_CONTINUTE As Long = 100
        ''' <summary>
        ''' 101 Switching Protocols |
        ''' This means the requester has asked the server To switch protocols And the server Is acknowledging that it will Do so.
        ''' </summary>

        Public Const RFC_SWITCHING_PROTOCOLS As Long = 101
        ''' <summary>
        ''' 102 Processing (WebDAV; RFC 2518) |
        ''' As a WebDAV request may contain many sub-requests involving file operations, it may take a long time to complete the request. 
        ''' This code indicates that the server has received And Is processing the request, but no response Is available yet.[3] 
        ''' This prevents the client from timing out And assuming the request was lost.
        ''' </summary>

        Public Const RFC_PROCESSING As Long = 102
#End Region

#Region "2xx Success"

        ''' <summary>
        ''' 200 OK |
        ''' Standard response For successful HTTP requests. The actual response will depend On the request method used. In a Get request, 
        ''' the response will contain an entity corresponding To the requested resource. In a POST request, the response will contain an 
        ''' entity describing Or containing the result Of the action.
        ''' (由于可能会修改附带一些其他的元素据信息，所以只读属性不会使用简写的形式的，而是需要重新生成新的对象实例以防止数据污染)
        ''' </summary>

        Public Const RFC_OK As Long = 200

        ''' <summary>
        ''' 201 Created |
        ''' The request has been fulfilled And resulted In a New resource being created.
        ''' </summary>

        Public Const RFC_CREATED As Long = 201

        ''' <summary>
        ''' 202 Accepted |
        ''' The request has been accepted For processing, but the processing has Not been completed. The request might Or might Not eventually be acted upon, 
        ''' As it might be disallowed When processing actually takes place.
        ''' </summary>

        Public Const RFC_ACCEPTED As Long = 202

        ''' <summary>
        ''' 203 Non-Authoritative Information (since HTTP/1.1) |
        ''' The server successfully processed the request, but Is returning information that may be from another source.
        ''' </summary>

        Public Const RFC_NON_AUTH_INFO As Long = 203

        ''' <summary>
        ''' 204 No Content |
        ''' The server successfully processed the request, but Is Not returning any content.
        ''' </summary>

        Public Const RFC_NO_CONTENT As Long = 204

        ''' <summary>
        ''' 205 Reset Content |
        ''' The server successfully processed the request, but Is Not returning any content. Unlike a 204 response, this response requires that the requester reset the document view.
        ''' </summary>

        Public Const RFC_RESET_CONTENT As Long = 205

        ''' <summary>
        ''' 206 Partial Content (RFC 7233) |
        ''' The server Is delivering only part Of the resource (Byte serving) due To a range header sent by the client. 
        ''' The range header Is used by HTTP clients To enable resuming Of interrupted downloads, Or split a download into multiple simultaneous streams.
        ''' </summary>

        Public Const RFC_PARTIAL_CONTENT As Long = 206

        ''' <summary>
        ''' 207 Multi-Status (WebDAV; RFC 4918) |
        ''' The message body that follows Is an XML message And can contain a number Of separate response codes, depending On how many Sub-requests were made.[4]
        ''' </summary>

        Public Const RFC_MULTI_STATUS As Long = 207
        ''' <summary>
        ''' 208 Already Reported (WebDAV; RFC 5842) |
        ''' The members Of a DAV binding have already been enumerated In a previous reply To this request, And are Not being included again.
        ''' </summary>

        Public Const RFC_ALREADY_REPORTED As Long = 208
        ''' <summary>
        ''' 226 IM Used (RFC 3229) |
        ''' The server has fulfilled a request For the resource, And the response Is a representation Of the result Of one Or more instance-manipulations applied To the current instance.[5]
        ''' </summary>

        Public Const RFC_IM_USED As Long = 226
#End Region

#Region "3xx Redirection"

        ''' <summary>
        ''' 300 Multiple Choices |
        ''' Indicates multiple options For the resource that the client may follow. It, For instance, could be used To present different 
        ''' format options For video, list files With different extensions, Or word sense disambiguation.
        ''' </summary>

        Public Const RFC_MULTI_CHOICES As Long = 300
        ''' <summary>
        ''' 301 Moved Permanently |
        ''' This And all future requests should be directed to the given URI.
        ''' </summary>

        Public Const RFC_MOVED_PERMANENTLY As Long = 301
        ''' <summary>
        ''' 302 Found |
        ''' This Is an example of industry practice contradicting the standard. The HTTP/1.0 specification (RFC 1945) required the client 
        ''' to perform a temporary redirect (the original describing phrase was "Moved Temporarily"),[6] but popular browsers implemented 
        ''' 302 with the functionality of a 303 See Other. Therefore, HTTP/1.1 added status codes 303 And 307 to distinguish between the 
        ''' two behaviours.[7] However, some Web applications And frameworks use the 302 status code as if it were the 303.[8]
        ''' </summary>

        Public Const RFC_FOUND As Long = 302
        ''' <summary>
        ''' 303 See Other (since HTTP/1.1) |
        ''' The response To the request can be found under another URI Using a Get method. When received In response To a POST (Or PUT/DELETE), 
        ''' it should be assumed that the server has received the data And the redirect should be issued With a separate Get message.
        ''' </summary>

        Public Const RFC_SEE_OTHER As Long = 303
        ''' <summary>
        ''' 304 Not Modified (RFC 7232) |
        ''' Indicates that the resource has Not been modified since the version specified by the request headers If-Modified-Since Or If-None-Match. 
        ''' This means that there Is no need To retransmit the resource, since the client still has a previously-downloaded copy.
        ''' </summary>

        Public Const RFC_NOT_MODIFIED As Long = 304
        ''' <summary>
        ''' 305 Use Proxy (since HTTP/1.1) |
        ''' The requested resource Is only available through a proxy, whose address Is provided In the response. Many HTTP clients 
        ''' (such As Mozilla[9] And Internet Explorer) Do Not correctly handle responses With this status code, primarily For security reasons.[10]
        ''' </summary>

        Public Const RFC_USE_PROXY As Long = 305
        ''' <summary>
        ''' 306 Switch Proxy |
        ''' No longer used. Originally meant "Subsequent requests should use the specified proxy."[11]
        ''' </summary>

        Public Const RFC_SWITCH_PROXY As Long = 306
        ''' <summary>
        ''' 307 Temporary Redirect (since HTTP/1.1) |
        ''' In this case, the request should be repeated with another URI; however, future requests should still use the original URI. 
        ''' In contrast to how 302 was historically implemented, the request method Is Not allowed to be changed when reissuing the original request. 
        ''' For instance, a POST request should be repeated using another POST request.[12]
        ''' </summary>

        Public Const RFC_TEMP_REDIRECT As Long = 307
        ''' <summary>
        ''' 308 Permanent Redirect (RFC 7538) |
        ''' The request, and all future requests should be repeated Using another URI. 307 And 308 (As proposed) parallel the behaviours 
        ''' Of 302 And 301, but Do Not allow the HTTP method To change. So, For example, submitting a form To a permanently redirected resource may Continue smoothly.[13]
        ''' </summary>

        Public Const RFC_PERMANENT_REDIRECT As Long = 308
        ''' <summary>
        ''' 308 Resume Incomplete (Google) |
        ''' This code Is used In the Resumable HTTP Requests Proposal To Resume aborted PUT Or POST requests.[14]
        ''' </summary>

        Public Const RFC_RESUME_INCOMPLETE As Long = 308
#End Region

#Region "4xx Client Error"

        ''' <summary>
        ''' 400 Bad Request |
        ''' The server cannot Or will Not process the request due To something that Is perceived To be a client Error (e.g., malformed request syntax, 
        ''' invalid request message framing, Or deceptive request routing).[15]
        ''' </summary>

        Public Const RFC_BAD_REQUEST As Long = 400

        ''' <summary>
        ''' 401 Unauthorized (RFC 7235) |
        ''' Similar to 403 Forbidden, but specifically for use when authentication Is required And has failed Or has Not yet been provided. 
        ''' The response must include a WWW-Authenticate header field containing a challenge applicable to the requested resource. 
        ''' See Basic access authentication And Digest access authentication.(证书未认证)
        ''' </summary>

        Public Const RFC_UNAUTHORIZED As Long = 401

        ''' <summary>
        ''' 402 Payment Required |
        ''' Reserved for future use. The original intention was that this code might be used as part of some form of digital cash Or micropayment scheme, 
        ''' but that has Not happened, And this code Is Not usually used. YouTube uses this status if a particular IP address has made excessive requests, 
        ''' And requires the person to enter a CAPTCHA.[citation needed]
        ''' </summary>

        Public Const RFC_PAYMENT_REQUIRED As Long = 402
        ''' <summary>
        ''' 403 Forbidden |
        ''' The request was a valid request, but the server Is refusing To respond To it. Unlike a 401 Unauthorized response, authenticating will make no difference.
        ''' (被封号了)
        ''' </summary>

        Public Const RFC_FORBIDDEN As Long = 403

        ''' <summary>
        ''' 404 Not Found |
        ''' The requested resource could Not be found but may be available again In the future. Subsequent requests by the client are permissible.
        ''' </summary>

        Public Const RFC_NOT_FOUND As Long = 404

        ''' <summary>
        ''' 405 Method Not Allowed |
        ''' A request was made Of a resource Using a request method Not supported by that resource; For example, 
        ''' Using Get On a form which requires data To be presented via POST, Or Using PUT On a read-only resource.
        ''' </summary>

        Public Const RFC_METHOD_NOT_ALLOWED As Long = 405
        ''' <summary>
        ''' 406 Not Acceptable |
        ''' The requested resource Is only capable Of generating content Not acceptable according To the Accept headers sent In the request.
        ''' </summary>

        Public Const RFC_NOT_ACCEPTABLE As Long = 406
        ''' <summary>
        ''' 407 Proxy Authentication Required (RFC 7235) |
        ''' The client must first authenticate itself With the proxy.
        ''' </summary>

        Public Const RFC_PROXY_AUTH_REQUIRED As Long = 407
        ''' <summary>
        ''' 408 Request Timeout |
        ''' The server timed out waiting For the request. According To HTTP specifications: 
        ''' "The client did not produce a request within the time that the server was prepared to wait. The client MAY repeat the request without modifications at any later time."
        ''' (请求超时)
        ''' </summary>

        Public Const RFC_REQUEST_TIMEOUT As Long = 408

        ''' <summary>
        ''' 409 Conflict |
        ''' Indicates that the request could Not be processed because Of conflict In the request, such As an edit conflict In the Case Of multiple updates.
        ''' </summary>

        Public Const RFC_CONFLICT As Long = 409
        ''' <summary>
        ''' 410 Gone |
        ''' Indicates that the resource requested Is no longer available And will Not be available again. 
        ''' This should be used When a resource has been intentionally removed And the resource should be purged. 
        ''' Upon receiving a 410 status code, the client should Not request the resource again In the future. 
        ''' Clients such As search engines should remove the resource from their indices.[16] Most use cases 
        ''' Do Not require clients And search engines To purge the resource, And a "404 Not Found" may be used instead.
        ''' </summary>

        Public Const RFC_GONE As Long = 410

        ''' <summary>
        ''' 411 Length Required |
        ''' The request did Not specify the length Of its content, which Is required by the requested resource.
        ''' </summary>

        Public Const RFC_LENGTH_REQUIRED As Long = 411
        ''' <summary>
        ''' 412 Precondition Failed (RFC 7232) |
        ''' The server does Not meet one Of the preconditions that the requester put On the request.
        ''' </summary>

        Public Const RFC_PRECONDITION_FAILED As Long = 412

        ''' <summary>
        ''' 413 Payload Too Large (RFC 7231) |
        ''' The request Is larger than the server Is willing Or able To process. Called "Request Entity Too Large " previously.
        ''' </summary>

        Public Const RFC_PAYLOAD_TOO_LARGE As Long = 413
        ''' <summary>
        ''' 414 Request-URI Too Long |
        ''' The URI provided was too Long For the server To process. Often the result Of too much data being encoded As a query-String Of a Get request, In which Case it should be converted To a POST request.
        ''' </summary>

        Public Const RFC_URI_TOO_LONG As Long = 414
        ''' <summary>
        ''' 415 Unsupported Media Type |
        ''' The request entity has a media type which the server Or resource does Not support. For example, the client uploads an image As image/svg+xml, but the server requires that images use a different format.
        ''' </summary>

        Public Const RFC_UNSUPPORTED_MEDIA_TYPE As Long = 415
        ''' <summary>
        ''' 416 Requested Range Not Satisfiable (RFC 7233) |
        ''' The client has asked For a portion Of the file (Byte serving), but the server cannot supply that portion. For example, If the client asked For a part Of the file that lies beyond the End Of the file.
        ''' </summary>

        Public Const RFC_RANGE_NOT_SATISFIABLE As Long = 416
        ''' <summary>
        ''' 417 Expectation Failed |
        ''' The server cannot meet the requirements Of the Expect request-header field.
        ''' </summary>

        Public Const RFC_EXPECTATION_FAILED As Long = 417

        ''' <summary>
        ''' 418 I'm a teapot (RFC 2324) |
        ''' This code was defined In 1998 As one Of the traditional IETF April Fools' jokes, in RFC 2324, Hyper Text Coffee Pot Control Protocol, 
        ''' and is not expected to be implemented by actual HTTP servers. The RFC specifies this code should be returned by tea pots requested to brew coffee.
        ''' </summary>

        Public Const RFC_IM_TEAPOT As Long = 418
        ''' <summary>
        ''' 419 Authentication Timeout (Not in RFC 2616) |
        ''' Not a part of the HTTP standard, 419 Authentication Timeout denotes that previously valid authentication has expired. 
        ''' It Is used as an alternative to 401 Unauthorized in order to differentiate from otherwise authenticated clients being denied access to specific server resources.[citation needed]
        ''' </summary>

        Public Const RFC_AUTH_TIMEOUT As Long = 419
        ''' <summary>
        ''' 420 Method Failure (Spring Framework) |
        ''' Not part of the HTTP standard, but defined by Spring in the HttpStatus class to be used when a method failed. This status code Is deprecated by Spring.
        ''' </summary>

        Public Const RFC_METHOD_FAILURE As Long = 420
        ''' <summary>
        ''' 420 Enhance Your Calm (Twitter) |
        ''' Not part of the HTTP standard, but returned by version 1 of the Twitter Search And Trends API when the client Is being rate limited.[17] 
        ''' Other services may wish to implement the 429 Too Many Requests response code instead.
        ''' </summary>

        Public Const RFC_ENHANCE_YOUR_CALM As Long = 420
        ''' <summary>
        ''' 421 Misdirected Request (HTTP/2) |
        ''' The request was directed at a server that Is Not able To produce a response (For example because a connection reuse).[18]
        ''' </summary>

        Public Const RFC_MISDIRECTED_REQUEST As Long = 421
        ''' <summary>
        ''' 422 Unprocessable Entity (WebDAV; RFC 4918) |
        ''' The request was well-formed but was unable To be followed due To semantic errors.[4]
        ''' </summary>

        Public Const RFC_UNPROCESSABLE_ENTITY As Long = 422
        ''' <summary>
        ''' 423 Locked (WebDAV; RFC 4918) |
        ''' The resource that Is being accessed Is locked.[4]
        ''' </summary>

        Public Const RFC_LOCKED As Long = 423
        ''' <summary>
        ''' 424 Failed Dependency (WebDAV; RFC 4918) |
        ''' The request failed due To failure Of a previous request (e.g., a PROPPATCH).[4]
        ''' </summary>

        Public Const RFC_FAILED_DEPENDENCY As Long = 424
        ''' <summary>
        ''' 426 Upgrade Required |
        ''' The client should switch To a different protocol such As TLS/1.0, given In the Upgrade header field.
        ''' </summary>

        Public Const RFC_UPGRADE_REQUIRED As Long = 426
        ''' <summary>
        ''' 428 Precondition Required (RFC 6585) |
        ''' The origin server requires the request To be conditional. Intended To prevent "the 'lost update' problem, 
        ''' where a client GETs a resource's state, modifies it, and PUTs it back to the server, when meanwhile a third party 
        ''' has modified the state on the server, leading to a conflict."[19]
        ''' </summary>

        Public Const RFC_PRECONDITION_REQUIRED As Long = 428
        ''' <summary>
        ''' 429 Too Many Requests (RFC 6585) |
        ''' The user has sent too many requests In a given amount Of time. Intended For use With rate limiting schemes.[19]
        ''' </summary>

        Public Const RFC_TOO_MANY_REQUEST As Long = 429
        ''' <summary>
        ''' 431 Request Header Fields Too Large (RFC 6585) |
        ''' The server Is unwilling To process the request because either an individual header field, Or all the header fields collectively, are too large.[19]
        ''' </summary>

        Public Const RFC_REQUEST_HEADER_FIELDS_TOO_LARGE As Long = 431
        ''' <summary>
        ''' 440 Login Timeout (Microsoft) |
        ''' A Microsoft extension. Indicates that your session has expired.[20]
        ''' </summary>

        Public Const RFC_LOGIN_TIMEOUT As Long = 440
        ''' <summary>
        ''' 444 No Response (Nginx) |
        ''' Used in Nginx logs to indicate that the server has returned no information to the client And closed the connection (useful as a deterrent for malware).
        ''' </summary>

        Public Const RFC_NO_RESPONSE As Long = 444
        ''' <summary>
        ''' 449 Retry With (Microsoft) |
        ''' A Microsoft extension. The request should be retried after performing the appropriate action.[21]
        ''' </summary>

        Public Const RFC_RETRY_WITH As Long = 449
        ''' <summary>
        ''' 450 Blocked by Windows Parental Controls (Microsoft) |
        ''' A Microsoft extension. This Error Is given When Windows Parental Controls are turned On And are blocking access To the given webpage.[22]
        ''' </summary>

        Public Const RFC_BLOCKED As Long = 450
        ''' <summary>
        ''' 451 Unavailable For Legal Reasons (Internet draft) |
        ''' Defined in the internet draft "A New HTTP Status Code for Legally-restricted Resources".[23] 
        ''' Intended to be used when resource access Is denied for legal reasons, e.g. censorship Or government-mandated blocked access. 
        ''' A reference to the 1953 dystopian novel Fahrenheit 451, where books are outlawed.[24]
        ''' </summary>

        Public Const RFC_LEGAL_UNAVAILABLE As Long = 451
        ''' <summary>
        ''' 451 Redirect (Microsoft) |
        ''' Used in Exchange ActiveSync if there either Is a more efficient server to use Or the server cannot access the users' mailbox.[25]
        ''' The client Is supposed To re-run the HTTP Autodiscovery protocol To find a better suited server.[26]
        ''' </summary>

        Public Const RFC_REDIRECT As Long = 451
        ''' <summary>
        ''' 494 Request Header Too Large (Nginx) |
        ''' Nginx internal code similar To 431 but it was introduced earlier In version 0.9.4 (On January 21, 2011).[27][original research?]
        ''' </summary>

        Public Const RFC_REQUEST_HEADER_TOO_LARGE As Long = 494
        ''' <summary>
        ''' 495 Cert Error (Nginx) |
        ''' Nginx internal code used When SSL client certificate Error occurred To distinguish it from 4XX In a log And an Error page redirection.
        ''' (在SSL层解密的时候错误，则为证书错误)
        ''' </summary>

        Public Const RFC_CERT_ERROR As Long = 495

        ''' <summary>
        ''' 496 No Cert (Nginx) |
        ''' Nginx internal code used When client didn't provide certificate to distinguish it from 4XX in a log and an error page redirection.
        ''' </summary>

        Public Const RFC_NO_CERT As Long = 496
        ''' <summary>
        ''' 497 HTTP to HTTPS (Nginx) |
        ''' Nginx internal code used For the plain HTTP requests that are sent To HTTPS port To distinguish it from 4XX In a log And an Error page redirection.
        ''' </summary>

        Public Const RFC_HTTP_TO_HTTPS As Long = 497
        ''' <summary>
        ''' 498 Token expired/invalid (Esri) |
        ''' Returned by ArcGIS For Server. A code Of 498 indicates an expired Or otherwise invalid token.[28]
        ''' (错误的参数信息)
        ''' </summary>

        Public Const RFC_TOKEN_INVALID As Long = 498

        ''' <summary>
        ''' 499 Client Closed Request (Nginx) |
        ''' Used in Nginx logs to indicate when the connection has been closed by client while the server Is still processing its request, making server unable to send a status code back.[29]
        ''' </summary>

        Public Const RFC_CLOSED_REQUEST As Long = 499
        ''' <summary>
        ''' 499 Token required (Esri) |
        ''' Returned by ArcGIS For Server. A code Of 499 indicates that a token Is required (If no token was submitted).[28]
        ''' </summary>

        Public Const RFC_TOKEN_REQUIRED As Long = 499

#End Region

#Region "5xx Server Error"

        ''' <summary>
        ''' 500 Internal Server Error |
        ''' A generic Error message, given When an unexpected condition was encountered And no more specific message Is suitable.
        ''' </summary>

        Public Const RFC_INTERNAL_SERVER_ERROR As Long = 500

        ''' <summary>
        ''' 501 Not Implemented |
        ''' The server either does Not recognize the request method, Or it lacks the ability To fulfill the request. Usually this implies future availability (e.g., a New feature Of a web-service API).
        ''' </summary>

        Public Const RFC_NOT_IMPLEMENTED As Long = 501

        ''' <summary>
        ''' 502 Bad Gateway |
        ''' The server was acting As a gateway Or proxy And received an invalid response from the upstream server.
        ''' </summary>

        Public Const RFC_BAD_GATEWAY As Long = 502
        ''' <summary>
        ''' 503 Service Unavailable |
        ''' The server Is currently unavailable (because it Is overloaded Or down For maintenance). Generally, this Is a temporary state.
        ''' </summary>

        Public Const RFC_SERVICE_UNAVAILABLE As Long = 503

        ''' <summary>
        ''' 504 Gateway Timeout |
        ''' The server was acting As a gateway Or proxy And did Not receive a timely response from the upstream server.
        ''' </summary>

        Public Const RFC_GATEWAY_TIMEOUT As Long = 504
        ''' <summary>
        ''' 505 HTTP Version Not Supported |
        ''' The server does Not support the HTTP protocol version used In the request.
        ''' </summary>

        Public Const RFC_VERSION_NOT_SUPPORTED As Long = 505
        ''' <summary>
        ''' 506 Variant Also Negotiates (RFC 2295) |
        ''' Transparent content negotiation For the request results In a circular reference.[30]
        ''' </summary>

        Public Const RFC_NEGOTIATES As Long = 506
        ''' <summary>
        ''' 507 Insufficient Storage (WebDAV; RFC 4918) |
        ''' The server Is unable To store the representation needed To complete the request.[4]
        ''' </summary>

        Public Const RFC_INSUFFICIENT_STORAGE As Long = 507
        ''' <summary>
        ''' 508 Loop Detected (WebDAV; RFC 5842) |
        ''' The server detected an infinite Loop While processing the request (sent In lieu Of 208 Already Reported).
        ''' </summary>

        Public Const RFC_LOOP_DETECTED As Long = 508
        ''' <summary>
        ''' 509 Bandwidth Limit Exceeded (Apache bw/limited extension)[31] |
        ''' This status code Is Not specified In any RFCs. Its use Is unknown.
        ''' </summary>

        Public Const RFC_BANDWIDTH_LIMITED_EXCEEDED As Long = 509
        ''' <summary>
        ''' 510 Not Extended (RFC 2774) |
        ''' Further extensions To the request are required For the server To fulfil it.[32]
        ''' </summary>

        Public Const RFC_NOT_EXTENDED As Long = 510
        ''' <summary>
        ''' 511 Network Authentication Required (RFC 6585) |
        ''' The client needs To authenticate To gain network access. Intended For use by intercepting proxies used To control access To the network 
        ''' (e.g., "captive portals" used To require agreement To Terms Of Service before granting full Internet access via a Wi-Fi hotspot).[19]
        ''' </summary>

        Public Const RFC_NETWORK_AUTH_REQUIRED As Long = 511
        ''' <summary>
        ''' 520 Unknown Error |
        ''' This status code Is Not specified In any RFC And Is returned by certain services, For instance Microsoft Azure And CloudFlare servers: 
        ''' "The 520 error is essentially a “catch-all” response for when the origin server returns something unexpected or something that is not 
        ''' tolerated/interpreted (protocol violation or empty response)."[33]
        ''' </summary>

        Public Const RFC_UNKNOWN_ERROR As Long = 520

        ''' <summary>
        ''' 522 Origin Connection Time-out |
        ''' This status code Is Not specified In any RFCs, but Is used by CloudFlare's reverse proxies to signal that a server connection timed out.
        ''' </summary>

        Public Const RFC_CONNECTION_TIMEOUT As Long = 522
        ''' <summary>
        ''' 598 Network read timeout error (Unknown) |
        ''' This status code Is Not specified In any RFCs, but Is used by Microsoft HTTP proxies To signal a network read timeout behind the proxy To a client In front Of the proxy.[citation needed]
        ''' </summary>

        Public Const RFC_READ_TIMEOUT_ERROR As Long = 598
        ''' <summary>
        ''' 599 Network connect timeout error (Unknown) |
        ''' This status code Is Not specified In any RFCs, but Is used by Microsoft HTTP proxies To signal a network connect timeout behind the proxy To a client In front Of the proxy.[citation needed]
        ''' </summary>

        Public Const RFC_CONNECT_TIMEOUT_ERROR As Long = 599
#End Region
    End Module
End Namespace
