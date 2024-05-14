#Region "Microsoft.VisualBasic::314039fa89eb2e3b95d925e781c63881, www\Microsoft.VisualBasic.NETProtocol\TcpRequest\NetResponse.vb"

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


    ' Code Statistics:

    '   Total Lines: 685
    '    Code Lines: 170
    ' Comment Lines: 476
    '   Blank Lines: 39
    '     File Size: 37.33 KB


    '     Module NetResponse
    ' 
    '         Properties: RFC_ACCEPTED, RFC_ALREADY_REPORTED, RFC_AUTH_TIMEOUT, RFC_BAD_GATEWAY, RFC_BAD_REQUEST
    '                     RFC_BANDWIDTH_LIMITED_EXCEEDED, RFC_BLOCKED, RFC_CERT_ERROR, RFC_CLOSED_REQUEST, RFC_CONFLICT
    '                     RFC_CONNECT_TIMEOUT_ERROR, RFC_CONNECTION_TIMEOUT, RFC_CONTINUTE, RFC_CREATED, RFC_ENHANCE_YOUR_CALM
    '                     RFC_EXPECTATION_FAILED, RFC_FAILED_DEPENDENCY, RFC_FORBIDDEN, RFC_FOUND, RFC_GATEWAY_TIMEOUT
    '                     RFC_GONE, RFC_HTTP_TO_HTTPS, RFC_IM_TEAPOT, RFC_IM_USED, RFC_INSUFFICIENT_STORAGE
    '                     RFC_INTERNAL_SERVER_ERROR, RFC_LEGAL_UNAVAILABLE, RFC_LENGTH_REQUIRED, RFC_LOCKED, RFC_LOGIN_TIMEOUT
    '                     RFC_LOOP_DETECTED, RFC_METHOD_FAILURE, RFC_METHOD_NOT_ALLOWED, RFC_MISDIRECTED_REQUEST, RFC_MOVED_PERMANENTLY
    '                     RFC_MULTI_CHOICES, RFC_MULTI_STATUS, RFC_NEGOTIATES, RFC_NETWORK_AUTH_REQUIRED, RFC_NO_CERT
    '                     RFC_NO_CONTENT, RFC_NO_RESPONSE, RFC_NON_AUTH_INFO, RFC_NOT_ACCEPTABLE, RFC_NOT_EXTENDED
    '                     RFC_NOT_FOUND, RFC_NOT_IMPLEMENTED, RFC_NOT_MODIFIED, RFC_OK, RFC_PARTIAL_CONTENT
    '                     RFC_PAYLOAD_TOO_LARGE, RFC_PAYMENT_REQUIRED, RFC_PERMANENT_REDIRECT, RFC_PRECONDITION_FAILED, RFC_PRECONDITION_REQUIRED
    '                     RFC_PROCESSING, RFC_PROXY_AUTH_REQUIRED, RFC_RANGE_NOT_SATISFIABLE, RFC_READ_TIMEOUT_ERROR, RFC_REDIRECT
    '                     RFC_REQUEST_HEADER_FIELDS_TOO_LARGE, RFC_REQUEST_HEADER_TOO_LARGE, RFC_REQUEST_TIMEOUT, RFC_RESET_CONTENT, RFC_RESUME_INCOMPLETE
    '                     RFC_RETRY_WITH, RFC_SEE_OTHER, RFC_SERVICE_UNAVAILABLE, RFC_SWITCH_PROXY, RFC_SWITCHING_PROTOCOLS
    '                     RFC_TEMP_REDIRECT, RFC_TOKEN_INVALID, RFC_TOKEN_REQUIRED, RFC_TOO_MANY_REQUEST, RFC_UNAUTHORIZED
    '                     RFC_UNKNOWN_ERROR, RFC_UNPROCESSABLE_ENTITY, RFC_UNSUPPORTED_MEDIA_TYPE, RFC_UPGRADE_REQUIRED, RFC_URI_TOO_LONG
    '                     RFC_USE_PROXY, RFC_VERSION_NOT_SUPPORTED
    ' 
    '         Function: IsHTTP_RFC
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Parallel
Imports r = System.Text.RegularExpressions.Regex

Namespace HTTP

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
    Public Module NetResponse

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
        ''' <returns></returns>
        Public ReadOnly Property RFC_CONTINUTE As RequestStream
        ''' <summary>
        ''' 101 Switching Protocols |
        ''' This means the requester has asked the server To switch protocols And the server Is acknowledging that it will Do so.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_SWITCHING_PROTOCOLS As RequestStream
        ''' <summary>
        ''' 102 Processing (WebDAV; RFC 2518) |
        ''' As a WebDAV request may contain many sub-requests involving file operations, it may take a long time to complete the request. 
        ''' This code indicates that the server has received And Is processing the request, but no response Is available yet.[3] 
        ''' This prevents the client from timing out And assuming the request was lost.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_PROCESSING As RequestStream
#End Region

#Region "2xx Success"

        ''' <summary>
        ''' 200 OK |
        ''' Standard response For successful HTTP requests. The actual response will depend On the request method used. In a Get request, 
        ''' the response will contain an entity corresponding To the requested resource. In a POST request, the response will contain an 
        ''' entity describing Or containing the result Of the action.
        ''' (由于可能会修改附带一些其他的元素据信息，所以只读属性不会使用简写的形式的，而是需要重新生成新的对象实例以防止数据污染)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_OK As RequestStream
            Get
                Return New RequestStream(0, HTTP_RFC.RFC_OK, "HTTP/200")
            End Get
        End Property

        ''' <summary>
        ''' 201 Created |
        ''' The request has been fulfilled And resulted In a New resource being created.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_CREATED As RequestStream

        ''' <summary>
        ''' 202 Accepted |
        ''' The request has been accepted For processing, but the processing has Not been completed. The request might Or might Not eventually be acted upon, 
        ''' As it might be disallowed When processing actually takes place.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_ACCEPTED As RequestStream

        ''' <summary>
        ''' 203 Non-Authoritative Information (since HTTP/1.1) |
        ''' The server successfully processed the request, but Is returning information that may be from another source.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_NON_AUTH_INFO As RequestStream

        ''' <summary>
        ''' 204 No Content |
        ''' The server successfully processed the request, but Is Not returning any content.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_NO_CONTENT As RequestStream
            Get
                Return New RequestStream(0, 204, "HTTP/204")
            End Get
        End Property

        ''' <summary>
        ''' 205 Reset Content |
        ''' The server successfully processed the request, but Is Not returning any content. Unlike a 204 response, this response requires that the requester reset the document view.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_RESET_CONTENT As RequestStream

        ''' <summary>
        ''' 206 Partial Content (RFC 7233) |
        ''' The server Is delivering only part Of the resource (Byte serving) due To a range header sent by the client. 
        ''' The range header Is used by HTTP clients To enable resuming Of interrupted downloads, Or split a download into multiple simultaneous streams.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_PARTIAL_CONTENT As RequestStream

        ''' <summary>
        ''' 207 Multi-Status (WebDAV; RFC 4918) |
        ''' The message body that follows Is an XML message And can contain a number Of separate response codes, depending On how many Sub-requests were made.[4]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_MULTI_STATUS As RequestStream
        ''' <summary>
        ''' 208 Already Reported (WebDAV; RFC 5842) |
        ''' The members Of a DAV binding have already been enumerated In a previous reply To this request, And are Not being included again.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_ALREADY_REPORTED As RequestStream
        ''' <summary>
        ''' 226 IM Used (RFC 3229) |
        ''' The server has fulfilled a request For the resource, And the response Is a representation Of the result Of one Or more instance-manipulations applied To the current instance.[5]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_IM_USED As RequestStream
#End Region

#Region "3xx Redirection"

        ''' <summary>
        ''' 300 Multiple Choices |
        ''' Indicates multiple options For the resource that the client may follow. It, For instance, could be used To present different 
        ''' format options For video, list files With different extensions, Or word sense disambiguation.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_MULTI_CHOICES As RequestStream
        ''' <summary>
        ''' 301 Moved Permanently |
        ''' This And all future requests should be directed to the given URI.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_MOVED_PERMANENTLY As RequestStream
        ''' <summary>
        ''' 302 Found |
        ''' This Is an example of industry practice contradicting the standard. The HTTP/1.0 specification (RFC 1945) required the client 
        ''' to perform a temporary redirect (the original describing phrase was "Moved Temporarily"),[6] but popular browsers implemented 
        ''' 302 with the functionality of a 303 See Other. Therefore, HTTP/1.1 added status codes 303 And 307 to distinguish between the 
        ''' two behaviours.[7] However, some Web applications And frameworks use the 302 status code as if it were the 303.[8]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_FOUND As RequestStream
        ''' <summary>
        ''' 303 See Other (since HTTP/1.1) |
        ''' The response To the request can be found under another URI Using a Get method. When received In response To a POST (Or PUT/DELETE), 
        ''' it should be assumed that the server has received the data And the redirect should be issued With a separate Get message.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_SEE_OTHER As RequestStream
        ''' <summary>
        ''' 304 Not Modified (RFC 7232) |
        ''' Indicates that the resource has Not been modified since the version specified by the request headers If-Modified-Since Or If-None-Match. 
        ''' This means that there Is no need To retransmit the resource, since the client still has a previously-downloaded copy.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_NOT_MODIFIED As RequestStream
        ''' <summary>
        ''' 305 Use Proxy (since HTTP/1.1) |
        ''' The requested resource Is only available through a proxy, whose address Is provided In the response. Many HTTP clients 
        ''' (such As Mozilla[9] And Internet Explorer) Do Not correctly handle responses With this status code, primarily For security reasons.[10]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_USE_PROXY As RequestStream
        ''' <summary>
        ''' 306 Switch Proxy |
        ''' No longer used. Originally meant "Subsequent requests should use the specified proxy."[11]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_SWITCH_PROXY As RequestStream
        ''' <summary>
        ''' 307 Temporary Redirect (since HTTP/1.1) |
        ''' In this case, the request should be repeated with another URI; however, future requests should still use the original URI. 
        ''' In contrast to how 302 was historically implemented, the request method Is Not allowed to be changed when reissuing the original request. 
        ''' For instance, a POST request should be repeated using another POST request.[12]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_TEMP_REDIRECT As RequestStream
            Get
                Return New RequestStream(0, HTTP_RFC.RFC_TEMP_REDIRECT, "HTTP/307")
            End Get
        End Property

        ''' <summary>
        ''' 308 Permanent Redirect (RFC 7538) |
        ''' The request, and all future requests should be repeated Using another URI. 307 And 308 (As proposed) parallel the behaviours 
        ''' Of 302 And 301, but Do Not allow the HTTP method To change. So, For example, submitting a form To a permanently redirected resource may Continue smoothly.[13]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_PERMANENT_REDIRECT As RequestStream
        ''' <summary>
        ''' 308 Resume Incomplete (Google) |
        ''' This code Is used In the Resumable HTTP Requests Proposal To Resume aborted PUT Or POST requests.[14]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_RESUME_INCOMPLETE As RequestStream
#End Region

#Region "4xx Client Error"

        ''' <summary>
        ''' 400 Bad Request |
        ''' The server cannot Or will Not process the request due To something that Is perceived To be a client Error (e.g., malformed request syntax, 
        ''' invalid request message framing, Or deceptive request routing).[15]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_BAD_REQUEST As RequestStream
            Get
                Return New RequestStream(0, HTTP_RFC.RFC_BAD_REQUEST, "HTTP/400")
            End Get
        End Property

        ''' <summary>
        ''' 401 Unauthorized (RFC 7235) |
        ''' Similar to 403 Forbidden, but specifically for use when authentication Is required And has failed Or has Not yet been provided. 
        ''' The response must include a WWW-Authenticate header field containing a challenge applicable to the requested resource. 
        ''' See Basic access authentication And Digest access authentication.(证书未认证)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_UNAUTHORIZED As RequestStream
            Get
                Return New RequestStream(0, HTTP_RFC.RFC_UNAUTHORIZED, "HTTP/401")
            End Get
        End Property

        ''' <summary>
        ''' 402 Payment Required |
        ''' Reserved for future use. The original intention was that this code might be used as part of some form of digital cash Or micropayment scheme, 
        ''' but that has Not happened, And this code Is Not usually used. YouTube uses this status if a particular IP address has made excessive requests, 
        ''' And requires the person to enter a CAPTCHA.[citation needed]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_PAYMENT_REQUIRED As RequestStream
        ''' <summary>
        ''' 403 Forbidden |
        ''' The request was a valid request, but the server Is refusing To respond To it. Unlike a 401 Unauthorized response, authenticating will make no difference.
        ''' (被封号了)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_FORBIDDEN As RequestStream
            Get
                Return New RequestStream(0, HTTP_RFC.RFC_FORBIDDEN, "HTTP/403")
            End Get
        End Property

        ''' <summary>
        ''' 404 Not Found |
        ''' The requested resource could Not be found but may be available again In the future. Subsequent requests by the client are permissible.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_NOT_FOUND As RequestStream
            Get
                Return New RequestStream(0, HTTP_RFC.RFC_NOT_FOUND, "HTTP/404")
            End Get
        End Property

        ''' <summary>
        ''' 405 Method Not Allowed |
        ''' A request was made Of a resource Using a request method Not supported by that resource; For example, 
        ''' Using Get On a form which requires data To be presented via POST, Or Using PUT On a read-only resource.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_METHOD_NOT_ALLOWED As RequestStream
        ''' <summary>
        ''' 406 Not Acceptable |
        ''' The requested resource Is only capable Of generating content Not acceptable according To the Accept headers sent In the request.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_NOT_ACCEPTABLE As RequestStream
        ''' <summary>
        ''' 407 Proxy Authentication Required (RFC 7235) |
        ''' The client must first authenticate itself With the proxy.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_PROXY_AUTH_REQUIRED As RequestStream
        ''' <summary>
        ''' 408 Request Timeout |
        ''' The server timed out waiting For the request. According To HTTP specifications: 
        ''' "The client did not produce a request within the time that the server was prepared to wait. The client MAY repeat the request without modifications at any later time."
        ''' (请求超时)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_REQUEST_TIMEOUT As RequestStream = New RequestStream(0, HTTP_RFC.RFC_REQUEST_TIMEOUT, "HTTP/408")

        ''' <summary>
        ''' 409 Conflict |
        ''' Indicates that the request could Not be processed because Of conflict In the request, such As an edit conflict In the Case Of multiple updates.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_CONFLICT As RequestStream = New RequestStream(0, HTTP_RFC.RFC_CONFLICT, "HTTP/409")
        ''' <summary>
        ''' 410 Gone |
        ''' Indicates that the resource requested Is no longer available And will Not be available again. 
        ''' This should be used When a resource has been intentionally removed And the resource should be purged. 
        ''' Upon receiving a 410 status code, the client should Not request the resource again In the future. 
        ''' Clients such As search engines should remove the resource from their indices.[16] Most use cases 
        ''' Do Not require clients And search engines To purge the resource, And a "404 Not Found" may be used instead.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_GONE As RequestStream

        ''' <summary>
        ''' 411 Length Required |
        ''' The request did Not specify the length Of its content, which Is required by the requested resource.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_LENGTH_REQUIRED As RequestStream
        ''' <summary>
        ''' 412 Precondition Failed (RFC 7232) |
        ''' The server does Not meet one Of the preconditions that the requester put On the request.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_PRECONDITION_FAILED As RequestStream

        ''' <summary>
        ''' 413 Payload Too Large (RFC 7231) |
        ''' The request Is larger than the server Is willing Or able To process. Called "Request Entity Too Large " previously.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_PAYLOAD_TOO_LARGE As RequestStream
        ''' <summary>
        ''' 414 Request-URI Too Long |
        ''' The URI provided was too Long For the server To process. Often the result Of too much data being encoded As a query-String Of a Get request, In which Case it should be converted To a POST request.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_URI_TOO_LONG As RequestStream
        ''' <summary>
        ''' 415 Unsupported Media Type |
        ''' The request entity has a media type which the server Or resource does Not support. For example, the client uploads an image As image/svg+xml, but the server requires that images use a different format.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_UNSUPPORTED_MEDIA_TYPE As RequestStream
        ''' <summary>
        ''' 416 Requested Range Not Satisfiable (RFC 7233) |
        ''' The client has asked For a portion Of the file (Byte serving), but the server cannot supply that portion. For example, If the client asked For a part Of the file that lies beyond the End Of the file.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_RANGE_NOT_SATISFIABLE As RequestStream
        ''' <summary>
        ''' 417 Expectation Failed |
        ''' The server cannot meet the requirements Of the Expect request-header field.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_EXPECTATION_FAILED As RequestStream
            Get
                Return New RequestStream(0, 417, "HTTP/417")
            End Get
        End Property
        ''' <summary>
        ''' 418 I'm a teapot (RFC 2324) |
        ''' This code was defined In 1998 As one Of the traditional IETF April Fools' jokes, in RFC 2324, Hyper Text Coffee Pot Control Protocol, 
        ''' and is not expected to be implemented by actual HTTP servers. The RFC specifies this code should be returned by tea pots requested to brew coffee.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_IM_TEAPOT As RequestStream
        ''' <summary>
        ''' 419 Authentication Timeout (Not in RFC 2616) |
        ''' Not a part of the HTTP standard, 419 Authentication Timeout denotes that previously valid authentication has expired. 
        ''' It Is used as an alternative to 401 Unauthorized in order to differentiate from otherwise authenticated clients being denied access to specific server resources.[citation needed]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_AUTH_TIMEOUT As RequestStream
        ''' <summary>
        ''' 420 Method Failure (Spring Framework) |
        ''' Not part of the HTTP standard, but defined by Spring in the HttpStatus class to be used when a method failed. This status code Is deprecated by Spring.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_METHOD_FAILURE As RequestStream
        ''' <summary>
        ''' 420 Enhance Your Calm (Twitter) |
        ''' Not part of the HTTP standard, but returned by version 1 of the Twitter Search And Trends API when the client Is being rate limited.[17] 
        ''' Other services may wish to implement the 429 Too Many Requests response code instead.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_ENHANCE_YOUR_CALM As RequestStream
        ''' <summary>
        ''' 421 Misdirected Request (HTTP/2) |
        ''' The request was directed at a server that Is Not able To produce a response (For example because a connection reuse).[18]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_MISDIRECTED_REQUEST As RequestStream
        ''' <summary>
        ''' 422 Unprocessable Entity (WebDAV; RFC 4918) |
        ''' The request was well-formed but was unable To be followed due To semantic errors.[4]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_UNPROCESSABLE_ENTITY As RequestStream
        ''' <summary>
        ''' 423 Locked (WebDAV; RFC 4918) |
        ''' The resource that Is being accessed Is locked.[4]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_LOCKED As RequestStream
        ''' <summary>
        ''' 424 Failed Dependency (WebDAV; RFC 4918) |
        ''' The request failed due To failure Of a previous request (e.g., a PROPPATCH).[4]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_FAILED_DEPENDENCY As RequestStream
        ''' <summary>
        ''' 426 Upgrade Required |
        ''' The client should switch To a different protocol such As TLS/1.0, given In the Upgrade header field.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_UPGRADE_REQUIRED As RequestStream
        ''' <summary>
        ''' 428 Precondition Required (RFC 6585) |
        ''' The origin server requires the request To be conditional. Intended To prevent "the 'lost update' problem, 
        ''' where a client GETs a resource's state, modifies it, and PUTs it back to the server, when meanwhile a third party 
        ''' has modified the state on the server, leading to a conflict."[19]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_PRECONDITION_REQUIRED As RequestStream
        ''' <summary>
        ''' 429 Too Many Requests (RFC 6585) |
        ''' The user has sent too many requests In a given amount Of time. Intended For use With rate limiting schemes.[19]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_TOO_MANY_REQUEST As RequestStream
        ''' <summary>
        ''' 431 Request Header Fields Too Large (RFC 6585) |
        ''' The server Is unwilling To process the request because either an individual header field, Or all the header fields collectively, are too large.[19]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_REQUEST_HEADER_FIELDS_TOO_LARGE As RequestStream
        ''' <summary>
        ''' 440 Login Timeout (Microsoft) |
        ''' A Microsoft extension. Indicates that your session has expired.[20]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_LOGIN_TIMEOUT As RequestStream
        ''' <summary>
        ''' 444 No Response (Nginx) |
        ''' Used in Nginx logs to indicate that the server has returned no information to the client And closed the connection (useful as a deterrent for malware).
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_NO_RESPONSE As RequestStream
        ''' <summary>
        ''' 449 Retry With (Microsoft) |
        ''' A Microsoft extension. The request should be retried after performing the appropriate action.[21]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_RETRY_WITH As RequestStream
        ''' <summary>
        ''' 450 Blocked by Windows Parental Controls (Microsoft) |
        ''' A Microsoft extension. This Error Is given When Windows Parental Controls are turned On And are blocking access To the given webpage.[22]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_BLOCKED As RequestStream
        ''' <summary>
        ''' 451 Unavailable For Legal Reasons (Internet draft) |
        ''' Defined in the internet draft "A New HTTP Status Code for Legally-restricted Resources".[23] 
        ''' Intended to be used when resource access Is denied for legal reasons, e.g. censorship Or government-mandated blocked access. 
        ''' A reference to the 1953 dystopian novel Fahrenheit 451, where books are outlawed.[24]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_LEGAL_UNAVAILABLE As RequestStream
        ''' <summary>
        ''' 451 Redirect (Microsoft) |
        ''' Used in Exchange ActiveSync if there either Is a more efficient server to use Or the server cannot access the users' mailbox.[25]
        ''' The client Is supposed To re-run the HTTP Autodiscovery protocol To find a better suited server.[26]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_REDIRECT As RequestStream
        ''' <summary>
        ''' 494 Request Header Too Large (Nginx) |
        ''' Nginx internal code similar To 431 but it was introduced earlier In version 0.9.4 (On January 21, 2011).[27][original research?]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_REQUEST_HEADER_TOO_LARGE As RequestStream
        ''' <summary>
        ''' 495 Cert Error (Nginx) |
        ''' Nginx internal code used When SSL client certificate Error occurred To distinguish it from 4XX In a log And an Error page redirection.
        ''' (在SSL层解密的时候错误，则为证书错误)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_CERT_ERROR As RequestStream
            Get
                Return New RequestStream(0, 495, "HTTP/495")
            End Get
        End Property

        ''' <summary>
        ''' 496 No Cert (Nginx) |
        ''' Nginx internal code used When client didn't provide certificate to distinguish it from 4XX in a log and an error page redirection.
        ''' (客户端在向ssl服务器发送ssl请求的时候没有应用密匙加密，直接发送明文给服务器了，则服务器直接拒绝请求)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_NO_CERT As RequestStream =
        New RequestStream(0, HTTP_RFC.RFC_NO_CERT, "HTTP/496")

        ''' <summary>
        ''' 497 HTTP to HTTPS (Nginx) |
        ''' Nginx internal code used For the plain HTTP requests that are sent To HTTPS port To distinguish it from 4XX In a log And an Error page redirection.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_HTTP_TO_HTTPS As RequestStream
        ''' <summary>
        ''' 498 Token expired/invalid (Esri) |
        ''' Returned by ArcGIS For Server. A code Of 498 indicates an expired Or otherwise invalid token.[28]
        ''' (错误的参数信息)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_TOKEN_INVALID As RequestStream
            Get
                Return New RequestStream(0, 498, "HTTP/498")
            End Get
        End Property

        ''' <summary>
        ''' 499 Client Closed Request (Nginx) |
        ''' Used in Nginx logs to indicate when the connection has been closed by client while the server Is still processing its request, making server unable to send a status code back.[29]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_CLOSED_REQUEST As RequestStream
        ''' <summary>
        ''' 499 Token required (Esri) |
        ''' Returned by ArcGIS For Server. A code Of 499 indicates that a token Is required (If no token was submitted).[28]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_TOKEN_REQUIRED As RequestStream

#End Region

#Region "5xx Server Error"

        ''' <summary>
        ''' 500 Internal Server Error |
        ''' A generic Error message, given When an unexpected condition was encountered And no more specific message Is suitable.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_INTERNAL_SERVER_ERROR As RequestStream
            Get
                Return New RequestStream(0, 500, "HTTP/500")
            End Get
        End Property

        ''' <summary>
        ''' 501 Not Implemented |
        ''' The server either does Not recognize the request method, Or it lacks the ability To fulfill the request. Usually this implies future availability (e.g., a New feature Of a web-service API).
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_NOT_IMPLEMENTED As RequestStream
            Get
                Return New RequestStream(0, 501, "HTTP/501")
            End Get
        End Property

        ''' <summary>
        ''' 502 Bad Gateway |
        ''' The server was acting As a gateway Or proxy And received an invalid response from the upstream server.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_BAD_GATEWAY As RequestStream = New RequestStream(0, HTTP_RFC.RFC_BAD_GATEWAY, "HTTP/502")
        ''' <summary>
        ''' 503 Service Unavailable |
        ''' The server Is currently unavailable (because it Is overloaded Or down For maintenance). Generally, this Is a temporary state.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_SERVICE_UNAVAILABLE As RequestStream
            Get
                Return New RequestStream(0, 503, "HTTP/503")
            End Get
        End Property

        ''' <summary>
        ''' 504 Gateway Timeout |
        ''' The server was acting As a gateway Or proxy And did Not receive a timely response from the upstream server.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_GATEWAY_TIMEOUT As RequestStream
        ''' <summary>
        ''' 505 HTTP Version Not Supported |
        ''' The server does Not support the HTTP protocol version used In the request.
        ''' (服务器所不支持的协议类型)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_VERSION_NOT_SUPPORTED As RequestStream =
        New RequestStream(0, HTTP_RFC.RFC_VERSION_NOT_SUPPORTED, "HTTP/505")

        ''' <summary>
        ''' 506 Variant Also Negotiates (RFC 2295) |
        ''' Transparent content negotiation For the request results In a circular reference.[30]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_NEGOTIATES As RequestStream
        ''' <summary>
        ''' 507 Insufficient Storage (WebDAV; RFC 4918) |
        ''' The server Is unable To store the representation needed To complete the request.[4]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_INSUFFICIENT_STORAGE As RequestStream
        ''' <summary>
        ''' 508 Loop Detected (WebDAV; RFC 5842) |
        ''' The server detected an infinite Loop While processing the request (sent In lieu Of 208 Already Reported).
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_LOOP_DETECTED As RequestStream
        ''' <summary>
        ''' 509 Bandwidth Limit Exceeded (Apache bw/limited extension)[31] |
        ''' This status code Is Not specified In any RFCs. Its use Is unknown.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_BANDWIDTH_LIMITED_EXCEEDED As RequestStream
        ''' <summary>
        ''' 510 Not Extended (RFC 2774) |
        ''' Further extensions To the request are required For the server To fulfil it.[32]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_NOT_EXTENDED As RequestStream
        ''' <summary>
        ''' 511 Network Authentication Required (RFC 6585) |
        ''' The client needs To authenticate To gain network access. Intended For use by intercepting proxies used To control access To the network 
        ''' (e.g., "captive portals" used To require agreement To Terms Of Service before granting full Internet access via a Wi-Fi hotspot).[19]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_NETWORK_AUTH_REQUIRED As RequestStream
        ''' <summary>
        ''' 520 Unknown Error |
        ''' This status code Is Not specified In any RFC And Is returned by certain services, For instance Microsoft Azure And CloudFlare servers: 
        ''' "The 520 error is essentially a “catch-all” response for when the origin server returns something unexpected or something that is not 
        ''' tolerated/interpreted (protocol violation or empty response)."[33]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_UNKNOWN_ERROR(Optional msg As String = Nothing) As RequestStream
            Get
                Return New RequestStream(0, 520, If(msg.StringEmpty, "HTTP/520", $"HTTP/520: {msg}"))
            End Get
        End Property

        ''' <summary>
        ''' 522 Origin Connection Time-out |
        ''' This status code Is Not specified In any RFCs, but Is used by CloudFlare's reverse proxies to signal that a server connection timed out.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_CONNECTION_TIMEOUT As RequestStream
        ''' <summary>
        ''' 598 Network read timeout error (Unknown) |
        ''' This status code Is Not specified In any RFCs, but Is used by Microsoft HTTP proxies To signal a network read timeout behind the proxy To a client In front Of the proxy.[citation needed]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_READ_TIMEOUT_ERROR As RequestStream
        ''' <summary>
        ''' 599 Network connect timeout error (Unknown) |
        ''' This status code Is Not specified In any RFCs, but Is used by Microsoft HTTP proxies To signal a network connect timeout behind the proxy To a client In front Of the proxy.[citation needed]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RFC_CONNECT_TIMEOUT_ERROR As RequestStream
#End Region

        ''' <summary>
        ''' 服务器所返回来的数据是否为HTTP错误代码
        ''' </summary>
        ''' <param name="response"></param>
        ''' <returns></returns>
        <Extension>
        Public Function IsHTTP_RFC(response As RequestStream) As Boolean
            If response.Protocol <> HTTP_RFC.RFC_OK AndAlso response.Protocol <> 0 Then
                Return True
            Else
                Dim data As String = response.GetUTF8String
                Dim isHTTPErrText As Boolean = r.Match(data, "HTTP[/]\d{3}").Value.TextEquals(data)

                isHTTPErrText = isHTTPErrText OrElse data.StartsWith("HTTP/")

                Return isHTTPErrText AndAlso response.Protocol <> HTTP_RFC.RFC_OK
            End If
        End Function
    End Module
End Namespace
