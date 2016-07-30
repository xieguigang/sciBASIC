Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Linq

Public Module Abundance

    <Extension>
    Public Iterator Function RelativeAbundances(Of T As ISample)(source As IEnumerable(Of T)) As IEnumerable(Of T)
        Dim array As T() = source.ToArray
        Dim allTags As String() = array _
            .Select(Function(x) x.Samples.Keys) _
            .MatrixAsIterator _
            .Distinct _
            .ToArray
        Dim max As Dictionary(Of String, Double) = (
            From tag As String
            In allTags
            Select mxVal = array _
                .Where(Function(x) x.Samples.ContainsKey(tag)) _
                .Select(Function(x) x.Samples(tag)).Max,
                tag).ToDictionary(Function(x) x.tag,
                                  Function(x) x.mxVal)
        For Each x As T In array
            x.Samples = (From tag As String
                         In allTags
                         Select tag,
                             value = x.Samples.TryGetValue(tag) / max(tag)) _
                             .ToDictionary(Function(xx) xx.tag,
                                           Function(xx) xx.value)
            Yield x
        Next
    End Function
End Module

Public Interface ISample : Inherits sIdEnumerable

    Property Samples As Dictionary(Of String, Double)
End Interface