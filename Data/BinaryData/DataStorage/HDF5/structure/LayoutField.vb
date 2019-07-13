#Region "Microsoft.VisualBasic::781db7d6380295f0213f7597d4d2ff43, Data\BinaryData\DataStorage\HDF5\structure\LayoutField.vb"

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

    '     Class LayoutField
    ' 
    '         Properties: byteLength, dataType, name, nDims, offset
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports Microsoft.VisualBasic.Data.IO.HDF5.type

Namespace HDF5.struct

    Public Class LayoutField

        Public ReadOnly Property name As String
        Public ReadOnly Property offset As Integer
        Public ReadOnly Property nDims As Integer
        Public ReadOnly Property dataType As DataTypes
        Public ReadOnly Property byteLength As Integer

        Public Sub New(name As String, offset As Integer, ndims As Integer, dataType As Integer, byteLength As Integer)
            Me.name = name
            Me.offset = offset
            Me.nDims = ndims
            Me.dataType = CType(dataType, DataTypes)
            Me.byteLength = byteLength
        End Sub

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {dataType.ToString} = [&{offset}, {byteLength}]"
        End Function
    End Class

End Namespace
