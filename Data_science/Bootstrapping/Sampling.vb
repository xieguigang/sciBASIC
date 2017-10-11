Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Linq

Public Module Sampling

    <Extension>
    Public Iterator Function SamplingByPoints(Of T)(data As IEnumerable(Of NamedValue(Of DoubleTagged(Of T)())),
                                                    points As IEnumerable(Of Double),
                                                    Optional err# = 0.00001) As IEnumerable(Of NamedValue(Of DoubleTagged(Of T)()))

    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function SamplingBySplitNParts(Of T)(data As IEnumerable(Of NamedValue(Of DoubleTagged(Of T)())),
                                                n%,
                                                Optional err# = 0.00001) As IEnumerable(Of NamedValue(Of DoubleTagged(Of T)()))
        With data.ToArray
            Return .SamplingByPoints(
                points:= .Select(Function(s) s.Value.Select(Function(p) p.Tag)).IteratesALL.Range.Enumerate(n),
                err:=err)
        End With
    End Function
End Module
