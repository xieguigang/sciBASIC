#Region "Microsoft.VisualBasic::b8b7a5d7cdd7444734d36cb556058c2a, Data\BinaryData\DataStorage\HDF5\types\StringData.vb"

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

    '     Class StringData
    ' 
    '         Properties: TypeInfo
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace HDF5.type

    Public Class StringData : Inherits DataType

        Public Overrides ReadOnly Property TypeInfo As System.Type
            Get
                Return GetType(String)
            End Get
        End Property
    End Class
End Namespace
