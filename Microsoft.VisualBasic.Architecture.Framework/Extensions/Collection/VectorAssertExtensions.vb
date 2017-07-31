Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Linq

Public Module VectorAssertExtensions

    ''' <summary>
    ''' Returns the collection element its index where the test expression <paramref name="predicate"/> result is TRUE
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="predicate"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Which(Of T)(source As IEnumerable(Of T), predicate As Func(Of T, Boolean)) As IEnumerable(Of Integer)
        Return source _
            .SeqIterator _
            .Where(Function(i) predicate(i.value)) _
            .Select(Function(o) o.i)
    End Function

    ''' <summary>
    ''' Determine that is all of the collection <paramref name="array"/> have the same size? 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="n%">collection size Length</param>
    ''' <param name="any">Is required all of the sequence must be the length equals</param>
    ''' <param name="array"></param>
    ''' <returns></returns>
    Public Function LengthEquals(Of T)(n%, any As Boolean, ParamArray array As IEnumerable(Of T)()) As Boolean
        Dim c%() = array.Select(Function(s) s.Count).ToArray
        Dim equals = c.Where(Function(x) x = n).ToArray

        If any Then
            Return equals.Length > 0
        Else
            Return equals.Length = array.Length
        End If
    End Function

    ''' <summary>
    ''' + False: 测试失败，不会满足<see cref="MappingData(Of T)(T(), T())"/>的条件
    ''' + True: 可以使用<see cref="MappingData(Of T)(T(), T())"/>来生成Mapping匹配
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <returns></returns>
    Public Function TestPairData(Of T)(a As T(), b As T()) As Boolean
        If a.Length <> b.Length AndAlso Not LengthEquals(1, True, a, b) Then
            Return False
        Else
            Return True
        End If
    End Function

    ''' <summary>
    ''' Any of the element in source <paramref name="sites"/> is in a specific <paramref name="range"/>??
    ''' </summary>
    ''' <param name="range"></param>
    ''' <param name="sites"></param>
    ''' <returns></returns>
    <Extension>
    Public Function InsideAny(range As IntRange, sites As IEnumerable(Of Integer)) As Boolean
        For Each x% In sites
            If range.IsInside(x) Then
                Return True
            End If
        Next

        Return False
    End Function
End Module
