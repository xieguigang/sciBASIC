Imports Microsoft.VisualBasic.Net.Protocols.Streams.Array
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Linq

Namespace DocumentStream.NetStream

    Public Class File : Inherits VarArray(Of RowObject)

        Public ReadOnly Property Encoding As Encodings

        Sub New(encoding As Encodings)
            Call MyBase.New(
                AddressOf StreamHelper.GetBytes, StreamHelper.LoadHelper(encoding))
            Me.Encoding = encoding
        End Sub

        Sub New(raw As Byte(), encoding As Encodings)
            Call MyBase.New(raw,
                            AddressOf StreamHelper.GetBytes, StreamHelper.LoadHelper(encoding))
            Me.Encoding = encoding
        End Sub

        Sub New(source As IEnumerable(Of DocumentStream.RowObject), encoding As Encodings)
            Call Me.New(encoding)

            Dim helper As New EncodingHelper(encoding)
            Me.Values = source.ToArray(
                Function(x) New RowObject(x, AddressOf helper.GetBytes,
                                          AddressOf helper.ToString))
            Me.Encoding = encoding
        End Sub

        Public Function CreateObject() As DocumentStream.File
            Return New DocumentStream.File(Values.ToArray(Function(x) New DocumentStream.RowObject(x.Values)))
        End Function
    End Class
End Namespace