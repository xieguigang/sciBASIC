Imports System
Imports System.Runtime.CompilerServices

Namespace scopely.msgpacksharp.Extensions
    Public Module ObjectExtensions
        <Extension()>
        Public Function ToMsgPack(ByVal o As Object) As Byte()
            If o Is Nothing Then Throw New ArgumentException("Can't serialize null references", "o")
            Return MsgPackSerializer.SerializeObject(o)
        End Function
    End Module
End Namespace
