#Region "Microsoft.VisualBasic::b83e60d8a7e42bca10863507c36273ff, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Tools\Network\Protocol\Reflection\ProtocolInvoker.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

        Public Function InvokeProtocol0(CA As Long, request As RequestStream, remoteDevice As System.Net.IPEndPoint) As RequestStream
            Dim value = Method.Invoke(obj, Nothing)
            Dim data = DirectCast(value, RequestStream)
            Return data
        End Function

        Public Function InvokeProtocol1(CA As Long, request As RequestStream, remoteDevice As System.Net.IPEndPoint) As RequestStream
            Dim value = Method.Invoke(obj, {CA})
            Dim data = DirectCast(value, RequestStream)
            Return data
        End Function

        Public Function InvokeProtocol2(CA As Long, request As RequestStream, remoteDevice As System.Net.IPEndPoint) As RequestStream
            Dim value = Method.Invoke(obj, {CA, request})
            Dim data = DirectCast(value, RequestStream)
            Return data
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="CA"><see cref="SSL.Certificate.uid"/></param>
        ''' <param name="request"></param>
        ''' <param name="remoteDevice"></param>
        ''' <returns></returns>
        Public Function InvokeProtocol3(CA As Long, request As RequestStream, remoteDevice As System.Net.IPEndPoint) As RequestStream
            Try
                Dim value = Method.Invoke(obj, {CA, request, remoteDevice})
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
