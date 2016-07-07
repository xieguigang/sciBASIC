#Region "Microsoft.VisualBasic::4dac9581c018fad2ae0661350e25f426, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.NETProtocol\User\UserProtocols.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Protocols
Imports System.Runtime.CompilerServices

Module UserProtocols

    Public Enum Protocols As Long
        PushInit
    End Enum

    Public ReadOnly Property ProtocolEntry As Long =
        New Protocol(GetType(Protocols)).EntryPoint

    Public Function NullMsg() As RequestStream
        Return New RequestStream(HTTP_RFC.RFC_NO_CONTENT, HTTP_RFC.RFC_OK, "")
    End Function

    <Extension> Public Function IsNull(msg As RequestStream) As Boolean
        Return msg.ProtocolCategory = HTTP_RFC.RFC_NO_CONTENT
    End Function

End Module

