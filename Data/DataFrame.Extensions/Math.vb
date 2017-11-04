Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports sys = System.Math

''' <summary>
''' Vector math extensions for <see cref="DataSet"/> or its collection.
''' </summary>
Public Module DataSetMath

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Log(d As DataSet, Optional base# = sys.E) As DataSet
        Return New DataSet With {
            .ID = d.ID,
            .Properties = d.Properties _
            .ToDictionary(Function(c) c.Key,
                          Function(c) sys.Log(c.Value))
        }
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Log(data As IEnumerable(Of DataSet), Optional base# = sys.E) As IEnumerable(Of DataSet)
        Return data.Select(Function(d) d.Log(base))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Log2(data As IEnumerable(Of DataSet)) As IEnumerable(Of DataSet)
        Return data.Log(base:=2)
    End Function
End Module
