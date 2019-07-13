#Region "Microsoft.VisualBasic::12c058dae3a969a2d6830118e52ff61a, Data\BinaryData\BinaryData\SQLite3\Objects\ColumnDataMeta.vb"

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

    '     Class ColumnDataMeta
    ' 
    '         Properties: name, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Enums

Namespace ManagedSqlite.Core.Objects

    Public Class ColumnDataMeta

        Public ReadOnly Property type As SqliteDataType
        ''' <summary>
        ''' Field name of current column meta data
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As String

        ''' <summary>
        ''' 对于文本或者blob类型,长度是可变的
        ''' </summary>
        Public length As UShort

        Sub New(name$, type As SqliteDataType)
            Me.type = type
            Me.name = name
        End Sub

        Public Overrides Function ToString() As String
            Return $"{type.Description} ~ {length}"
        End Function
    End Class
End Namespace
