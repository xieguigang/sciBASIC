
Namespace Xdr
    Public Delegate Function ReadOneDelegate(Of T)(reader As Reader) As T
    Public Delegate Function ReadManyDelegate(Of T)(reader As Reader, len As UInteger) As T
    Public Delegate Sub WriteOneDelegate(Of T)(writer As Writer, item As T)
    Public Delegate Sub WriteManyDelegate(Of T)(writer As Writer, len As UInteger, item As T)
End Namespace
