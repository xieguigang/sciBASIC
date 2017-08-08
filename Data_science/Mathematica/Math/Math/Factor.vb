Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Class Factor(Of T As IComparable(Of T)) : Inherits int
    Implements Value(Of T).IValueOf

    Public Property FactorValue As T Implements Value(Of T).IValueOf.value

    Public Overrides Function ToString() As String
        Return FactorValue.ToString
    End Function
End Class

Public Class Factors(Of T As IComparable(Of T)) : Inherits Index(Of T)

    Sub New(ParamArray list As T())
        Call MyBase.New(list)
    End Sub

    Public Iterator Function GetFactors() As IEnumerable(Of Factor(Of T))
        For Each i In MyBase.Map
            Yield New Factor(Of T) With {
                .FactorValue = i.Key,
                .Value = i.Value
            }
        Next
    End Function
End Class

Public Module FactorExtensions

    ''' <summary>
    ''' 这个函数和<see cref="SeqIterator"/>类似，但是这个函数之中添加了去重和排序
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    <Extension>
    Public Function factors(Of T As IComparable(Of T))(source As IEnumerable(Of T)) As Factor(Of T)()
        Dim array = source.ToArray
        Dim unique As Index(Of T) = array _
            .Distinct _
            .OrderBy(Function(x) x) _
            .Indexing
        Dim out = array _
            .Select(Function(x)
                        Return New Factor(Of T) With {
                            .value = unique.IndexOf(x),
                            .FactorValue = x
                        }
                    End Function) _
            .ToArray
        Return out
    End Function
End Module