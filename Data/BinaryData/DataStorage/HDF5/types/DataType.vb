#Region "Microsoft.VisualBasic::79e9de4aaf28d505eb76b6bc22fecb18, Data\BinaryData\DataStorage\HDF5\types\DataType.vb"

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

    '     Class DataType
    ' 
    '         Properties: [class], size, version
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.HDF5.device

Namespace HDF5.type

    Public MustInherit Class DataType

        Public Property version As Integer
        Public Property [class] As DataTypes

        ''' <summary>
        ''' 数据元素<see cref="DataTypes"/>的单位大小
        ''' </summary>
        ''' <returns></returns>
        Public Property size As Integer

        Public MustOverride ReadOnly Property TypeInfo As System.Type

    End Class
End Namespace
