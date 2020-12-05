Imports System.Runtime.CompilerServices

<HideModuleName>
Public Module ObjectExtensions

    <Extension()>
    Public Function ToMsgPack(o As Object) As Byte()
        If o Is Nothing Then
            Throw New ArgumentException("Can't serialize null references", "o")
        Else
            Return MsgPackSerializer.SerializeObject(o)
        End If
    End Function
End Module
