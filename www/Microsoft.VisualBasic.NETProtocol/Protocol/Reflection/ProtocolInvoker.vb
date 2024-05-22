#Region "Microsoft.VisualBasic::9a45c5f5331d177332207ca0660da19f, www\Microsoft.VisualBasic.NETProtocol\Protocol\Reflection\ProtocolInvoker.vb"

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

    '   Total Lines: 53
    '    Code Lines: 42 (79.25%)
    ' Comment Lines: 3 (5.66%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (15.09%)
    '     File Size: 2.01 KB


    '     Class ProtocolInvoker
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: InvokeProtocol0, InvokeProtocol1, InvokeProtocol2, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Parallel

Namespace Protocols.Reflection

    ''' <summary>
    ''' Working in server side
    ''' </summary>
    Friend Class ProtocolInvoker

        ReadOnly obj As Object
        ReadOnly method As MethodInfo
        ReadOnly debug As Boolean

        Sub New(obj As Object, method As MethodInfo, Optional debug As Boolean = False)
            Me.obj = obj
            Me.method = method
            Me.debug = debug
        End Sub

        Public Function InvokeProtocol0(request As RequestStream, remoteDevice As System.Net.IPEndPoint) As BufferPipe
            Try
                Return method.Invoke(obj, Nothing)
            Catch ex As Exception
                Call App.LogException(New Exception(method.FullName, ex))
                Return New DataPipe(NetResponse.RFC_UNKNOWN_ERROR(ex.ToString))
            End Try
        End Function

        Public Function InvokeProtocol1(request As RequestStream, remoteDevice As System.Net.IPEndPoint) As BufferPipe
            Try
                Return method.Invoke(obj, {request})
            Catch ex As Exception
                Call App.LogException(New Exception(method.FullName, ex))
                Return New DataPipe(NetResponse.RFC_UNKNOWN_ERROR(ex.ToString))
            End Try
        End Function

        Public Function InvokeProtocol2(request As RequestStream, remoteDevice As System.Net.IPEndPoint) As BufferPipe
            Try
                Return method.Invoke(obj, {request, remoteDevice})
            Catch ex As Exception
                Call App.LogException(New Exception(method.FullName, ex))
                Return New DataPipe(NetResponse.RFC_UNKNOWN_ERROR(ex.ToString))
            End Try
        End Function

        Public Overrides Function ToString() As String
            Return $"{obj.ToString} -> {method.Name}"
        End Function
    End Class
End Namespace
