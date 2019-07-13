#Region "Microsoft.VisualBasic::aaf71878651b3428101866e53df62e57, Data\DataFrame\IO\NetStream\RowObject.vb"

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

    '     Class RowObject
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: CreateObject, Load
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Net.Protocols.Streams.Array
Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports Microsoft.VisualBasic.Text

Namespace IO.NetStream

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

        Sub New(source As IEnumerable(Of String), getbyts As IGetBuffer(Of String), toString As IGetObject(Of String))
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

        Public Shared Function Load(raw As Byte(), encoding As Encodings) As IO.RowObject
            Dim source = CreateObject(raw, encoding)
            Return New IO.RowObject(source.Values)
        End Function
    End Class
End Namespace
