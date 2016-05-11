Imports Microsoft.VisualBasic.Text

Namespace DocumentStream.NetStream

    Module StreamHelper

        Public Function GetBytes(x As RowObject) As Byte()
            Return x.Serialize
        End Function

        Public Function LoadHelper(encoding As Encodings) As Func(Of Byte(), RowObject)
            Dim helper As New EncodingHelper(encoding)
            Return AddressOf New __load(helper).Load
        End Function

        Private Class __load

            ReadOnly __encoding As EncodingHelper

            Sub New(encoding As EncodingHelper)
                __encoding = encoding
            End Sub

            Public Function Load(byts As Byte()) As RowObject
                Return New RowObject(byts, __encoding)
            End Function
        End Class
    End Module
End Namespace