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

        Sub New(raw As Byte(), encoding As Encodings)

        End Sub

        Public Overrides Function Serialize() As Byte()

        End Function
    End Class
End Namespace