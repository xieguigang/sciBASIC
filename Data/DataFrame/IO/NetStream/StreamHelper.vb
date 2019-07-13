#Region "Microsoft.VisualBasic::358e468d33d7ef53c45d5e57acf2986b, Data\DataFrame\IO\NetStream\StreamHelper.vb"

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

    '     Module StreamHelper
    ' 
    '         Function: GetBytes, LoadHelper
    '         Class __load
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: Load
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports Microsoft.VisualBasic.Text

Namespace IO.NetStream

    Module StreamHelper

        Public Function GetBytes(x As RowObject) As Byte()
            Return x.Serialize
        End Function

        Public Function LoadHelper(encoding As Encodings) As IGetObject(Of RowObject)
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
