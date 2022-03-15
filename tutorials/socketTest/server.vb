#Region "Microsoft.VisualBasic::0c338dc8396b46ea1971b640d3da4518, sciBASIC#\tutorials\socketTest\server.vb"

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

    '   Total Lines: 34
    '    Code Lines: 24
    ' Comment Lines: 4
    '   Blank Lines: 6
    '     File Size: 1.26 KB


    ' Class server
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: handleA, handleB, ProtocolHandler
    ' 
    '     Sub: LogException
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Text

<Protocol(GetType(Protocols.Test))>
Public Class server : Inherits ServerModule

    Public Sub New(port As Integer)
        MyBase.New(port)
    End Sub

    Protected Overrides Sub LogException(ex As Exception)
        Call ex.PrintException
    End Sub

    ''' <summary>
    ''' protocol C will returns a not implemented error code
    ''' </summary>
    ''' <returns></returns>
    Protected Overrides Function ProtocolHandler() As ProtocolHandler
        Return New ProtocolHandler(Me)
    End Function

    <Protocol(Protocols.Test.A)>
    Private Function handleA(request As RequestStream, RemoteAddress As System.Net.IPEndPoint) As RequestStream
        Return RequestStream.CreatePackage(request.GetString(UTF8WithoutBOM) & "  Hello world")
    End Function

    <Protocol(Protocols.Test.B)>
    Private Function handleB(request As RequestStream, RemoteAddress As System.Net.IPEndPoint) As RequestStream
        Return RequestStream.SystemProtocol(RequestStream.Protocols.InvalidCertificates, "Mismatched!")
    End Function
End Class
