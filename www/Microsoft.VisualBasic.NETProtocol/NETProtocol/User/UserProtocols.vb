#Region "Microsoft.VisualBasic::c132fad788b0d1a8fe7c8a395cc48735, www\Microsoft.VisualBasic.NETProtocol\NETProtocol\User\UserProtocols.vb"

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

    '   Total Lines: 26
    '    Code Lines: 19
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 792 B


    '     Module UserProtocols
    ' 
    ' 
    '         Enum Protocols
    ' 
    '             PushInit
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: ProtocolEntry
    ' 
    '     Function: IsNull, NullMsg
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Parallel

Namespace NETProtocol

    Module UserProtocols

        Public Enum Protocols As Long
            PushInit
        End Enum

        Public ReadOnly Property ProtocolEntry As Long =
        New ProtocolAttribute(GetType(Protocols)).EntryPoint

        Public Function NullMsg() As RequestStream
            Return New RequestStream(HTTP_RFC.RFC_NO_CONTENT, HTTP_RFC.RFC_OK, "")
        End Function

        <Extension> Public Function IsNull(msg As RequestStream) As Boolean
            Return msg.ProtocolCategory = HTTP_RFC.RFC_NO_CONTENT
        End Function

    End Module
End Namespace
