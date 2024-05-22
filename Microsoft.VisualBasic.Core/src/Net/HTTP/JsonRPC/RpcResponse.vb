#Region "Microsoft.VisualBasic::9c1fe978aae9d8bbb515c69dc726e3f0, Microsoft.VisualBasic.Core\src\Net\HTTP\JsonRPC\RpcResponse.vb"

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

    '   Total Lines: 40
    '    Code Lines: 20 (50.00%)
    ' Comment Lines: 13 (32.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (17.50%)
    '     File Size: 1.10 KB


    '     Class RpcResponse
    ' 
    '         Properties: [error], id, jsonrpc, result
    ' 
    '     Enum ErrorCode
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class RpcError
    ' 
    '         Properties: code, message
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Net.Http.JsonRPC

    Public Class RpcResponse

        Public Property jsonrpc As String
        Public Property result As Object
        Public Property [error] As RpcError
        Public Property id As Integer

    End Class

    Public Enum ErrorCode As Integer
        Success = 0
        ''' <summary>
        ''' Invalid JSON was received by the server.
        ''' An Error occurred On the server While parsing the JSON text.
        ''' </summary>
        ParserError = -32700
        ''' <summary>
        ''' The JSON sent is not a valid Request object.
        ''' </summary>
        InvalidRequest = -32600
        ''' <summary>
        ''' The method does not exist / is not available.
        ''' </summary>
        MethodNotFound = -32601
        ''' <summary>
        ''' Invalid method parameter(s).
        ''' </summary>
        InvalidParams = -32602
        InternalError = -32603
    End Enum

    Public Class RpcError

        Public Property code As ErrorCode
        Public Property message As String

    End Class
End Namespace
