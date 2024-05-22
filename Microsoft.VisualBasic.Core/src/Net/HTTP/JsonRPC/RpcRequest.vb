﻿#Region "Microsoft.VisualBasic::e7ea1948380fd5753567f01fa14860a1, Microsoft.VisualBasic.Core\src\Net\HTTP\JsonRPC\RpcRequest.vb"

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

    '   Total Lines: 11
    '    Code Lines: 8 (72.73%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (27.27%)
    '     File Size: 280 B


    '     Class RpcRequest
    ' 
    '         Properties: id, jsonrpc, method, params
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Net.Http.JsonRPC

    Public Class RpcRequest

        Public Property jsonrpc As String
        Public Property method As String
        Public Property params As Dictionary(Of String, Object)
        Public Property id As Integer

    End Class
End Namespace
