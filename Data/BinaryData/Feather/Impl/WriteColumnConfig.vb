Imports System

Namespace Impl
    Friend Class WriteColumnConfig
        Public Property Name As String
        Public Property DotNetType As Type
        Public Property OnDiskType As ColumnType
        Public Property Length As Long
        Public Property Data As IEnumerable
        Public Property NullCount As Long

        Public Sub New(name As String, dotNetType As Type, onDiskType As ColumnType, length As Long, data As IEnumerable, nullCount As Long)
            Me.Name = name
            Me.DotNetType = dotNetType
            Me.OnDiskType = onDiskType
            Me.Length = length
            Me.Data = data
            Me.NullCount = nullCount
        End Sub
    End Class
End Namespace
