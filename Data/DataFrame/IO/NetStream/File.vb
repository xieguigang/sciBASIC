#Region "Microsoft.VisualBasic::5e4083c264b60b59395448bb44146ff4, Data\DataFrame\IO\NetStream\File.vb"

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

    '     Class File
    ' 
    '         Properties: Encoding
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: CreateObject
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Net.Protocols.Streams.Array
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Linq

Namespace IO.NetStream

    Public Class File : Inherits VarArray(Of RowObject)

        Public ReadOnly Property Encoding As Encodings

        Sub New(encoding As Encodings)
            Call MyBase.New(
                AddressOf StreamHelper.GetBytes, StreamHelper.LoadHelper(encoding))
            Me.Encoding = encoding
        End Sub

        Sub New(raw As Byte(), encoding As Encodings)
            Call MyBase.New(raw, AddressOf StreamHelper.GetBytes, StreamHelper.LoadHelper(encoding))
            Me.Encoding = encoding
        End Sub

        Sub New(source As IEnumerable(Of IO.RowObject), encoding As Encodings)
            Call Me.New(encoding)

            With New EncodingHelper(encoding)
                Dim [ctype] As Func(Of IO.RowObject, RowObject) =
                    Function(row) As RowObject
                        Return New RowObject(row, AddressOf .GetBytes, AddressOf .ToString)
                    End Function

                Me.Encoding = encoding
                Me.Values = source.Select([ctype]).ToArray
            End With
        End Sub

        Public Function CreateObject() As IO.File
            Return New IO.File(Values.Select(Function(x) New IO.RowObject(x.Values)))
        End Function
    End Class
End Namespace
