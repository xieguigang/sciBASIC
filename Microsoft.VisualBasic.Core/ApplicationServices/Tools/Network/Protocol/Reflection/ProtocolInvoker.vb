#Region "Microsoft.VisualBasic::b83e60d8a7e42bca10863507c36273ff, Microsoft.VisualBasic.Core\ApplicationServices\Tools\Network\Protocol\Reflection\ProtocolInvoker.vb"

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

    '     Class __protocolInvoker
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: InvokeProtocol0, InvokeProtocol1, InvokeProtocol2, InvokeProtocol3, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Win32

Namespace Net.Protocols.Reflection

    Friend Class __protocolInvoker

        ReadOnly obj As Object, Method As MethodInfo

        Sub New(obj As Object, Method As MethodInfo)
            Me.obj = obj
            Me.Method = Method
        End Sub

        Public Function InvokeProtocol0(request As RequestStream, remoteDevice As System.Net.IPEndPoint) As RequestStream
            Dim value = Method.Invoke(obj, Nothing)
            Dim data = DirectCast(value, RequestStream)
            Return data
        End Function

        Public Function InvokeProtocol1(request As RequestStream, remoteDevice As System.Net.IPEndPoint) As RequestStream
            Dim value = Method.Invoke(obj, {})
            Dim data = DirectCast(value, RequestStream)
            Return data
        End Function

        Public Function InvokeProtocol2(request As RequestStream, remoteDevice As System.Net.IPEndPoint) As RequestStream
            Dim value = Method.Invoke(obj, {request})
            Dim data = DirectCast(value, RequestStream)
            Return data
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="request"></param>
        ''' <param name="remoteDevice"></param>
        ''' <returns></returns>
        Public Function InvokeProtocol3(request As RequestStream, remoteDevice As System.Net.IPEndPoint) As RequestStream
            Try
                Dim value = Method.Invoke(obj, {request, remoteDevice})
                Dim data = DirectCast(value, RequestStream)
                Return data
            Catch ex As Exception
                ex = New Exception(Method.FullName, ex)

                If WindowsServices.Initialized Then
                    Call ServicesLogs.LogException(ex)
                Else
                    Call App.LogException(ex)
                End If
                Return NetResponse.RFC_UNKNOWN_ERROR
            End Try
        End Function

        Public Overrides Function ToString() As String
            Return $"{obj.ToString} -> {Method.Name}"
        End Function
    End Class
End Namespace
