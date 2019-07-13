#Region "Microsoft.VisualBasic::cd45e24c17e22eba9d35207c33c86a65, www\Microsoft.VisualBasic.NETProtocol\NETProtocol\User\UserProtocols.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace NETProtocol

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
End Namespace
