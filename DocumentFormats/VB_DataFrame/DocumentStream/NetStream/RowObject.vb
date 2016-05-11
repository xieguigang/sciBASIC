Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Streams.Array
Imports Microsoft.VisualBasic.Text

Namespace DocumentStream.NetStream

    Public Class RowObject : Inherits VarArray(Of String)

        Sub New(encoding As EncodingHelper)
            Call MyBase.New(
                AddressOf encoding.GetBytes,
                AddressOf encoding.ToString)
        End Sub

        Sub New(source As IEnumerable(Of String), encoding As Encodings)
            Call Me.New(New EncodingHelper(encoding))
            MyBase.Values = source.ToArray
        End Sub

        Sub New(source As IEnumerable(Of String), getbyts As Func(Of String, Byte()), toString As Func(Of Byte(), String))
            Call MyBase.New(getbyts, toString)
            MyBase.Values = source.ToArray
        End Sub

        Sub New(raw As Byte(), encoding As EncodingHelper)
            Call MyBase.New(raw,
                            AddressOf encoding.GetBytes,
                            AddressOf encoding.ToString)
        End Sub

        Public Shared Function CreateObject(raw As Byte(), encoding As Encodings) As RowObject
            Dim helper As New EncodingHelper(encoding)
            Return New RowObject(raw, helper)
        End Function

        Public Shared Function Load(raw As Byte(), encoding As Encodings) As DocumentStream.RowObject
            Dim source = CreateObject(raw, encoding)
            Return New DocumentStream.RowObject(source.Values)
        End Function
    End Class
End Namespace