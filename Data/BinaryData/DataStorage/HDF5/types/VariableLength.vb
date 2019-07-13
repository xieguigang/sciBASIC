#Region "Microsoft.VisualBasic::297f07398aed7beecc02ecb496b999b0, Data\BinaryData\DataStorage\HDF5\types\VariableLength.vb"

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

    '     Class VariableLength
    ' 
    '         Properties: encoding, paddingType, type, TypeInfo
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace HDF5.type

    Public Class VariableLength : Inherits DataType

        Public Property type As Integer
        Public Property paddingType As Integer
        Public Property encoding As Encoding

        Public Overrides ReadOnly Property TypeInfo As System.Type
            Get
                Return GetType(String)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"({encoding.ToString} {Me.GetType.Name}) {TypeInfo.FullName}"
        End Function
    End Class
End Namespace
