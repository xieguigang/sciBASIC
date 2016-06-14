Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace ComponentModel.Collection

    ''' <summary>
    ''' 对数据进行分组，通过标签数据的相似度
    ''' </summary>
    Public Module FuzzyGroup

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="cut">字符串相似度的阈值</param>
        ''' <returns></returns>
        <Extension>
        Public Function FuzzyGroups(Of T As sIdEnumerable)(
                        source As IEnumerable(Of T),
               Optional cut As Double = 0.6,
               Optional parallel As Boolean = False) As GroupResult(Of T, String)()

            Return source.FuzzyGroups(Function(x) x.Identifier, cut, parallel).ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="getKey"></param>
        ''' <param name="cut">字符串相似度的阈值</param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function FuzzyGroups(Of T)(
                                 source As IEnumerable(Of T),
                                 getKey As Func(Of T, String),
                        Optional cut As Double = 0.6,
                        Optional parallel As Boolean = False) As IEnumerable(Of GroupResult(Of T, String))

            Dim tmp As New List(Of NamedValue(Of T))
            Dim key As String
            Dim list As New List(Of NamedValue(Of T))(
                source.Select(Function(x) New NamedValue(Of T)(getKey(x), x)))
            Dim out As GroupResult(Of T, String)

            Do While list.Count > 0
                key = list(Scan0).Name
                tmp.Clear()
                tmp.Add(list(Scan0))
                list.RemoveAt(Scan0)

                If parallel Then
                    tmp += From x As NamedValue(Of T)
                           In list.AsParallel
                           Where Equals(key, x.Name, cut)
                           Select x
                Else
                    For Each x As NamedValue(Of T) In list
                        If Equals(key, x.Name, cut) Then
                            Call tmp.Add(x)
                        End If
                    Next
                End If

                For Each x In tmp
                    Call list.Remove(x)
                Next

                out = New GroupResult(Of T, String) With {
                    .Group = tmp.ToArray(Function(x) x.x),
                    .Tag = key
                }
                Yield out
            Loop
        End Function

        Public Function Equals(key As String, tag As String, cut As Double) As Boolean
            Dim edits As DistResult = ComputeDistance(key, tag)
            If edits Is Nothing Then
                Return False
            Else
                Return edits.MatchSimilarity >= cut
            End If
        End Function

        '        Private Structure __groupHelper(Of T)

        '            Public key As String
        '            Public x As T
        '            Public cut As Double

        '            Public Overrides Function ToString() As String
        '                Return Me.GetJson
        '            End Function

        '            Public Overloads Function Equals(tag As String) As Boolean
        '                Dim edits As DistResult = ComputeDistance(key, tag)
        '                If edits Is Nothing Then
        '                    Return False
        '                Else
        '#If DEBUG Then
        '                    If edits.MatchSimilarity <> 1.0R Then
        '                        Call "".__DEBUG_ECHO
        '                    End If
        '#End If
        '                    Return edits.MatchSimilarity >= cut
        '                End If
        '            End Function

        '            Public Overrides Function Equals(obj As Object) As Boolean
        '                If obj Is Nothing Then
        '                    Return False
        '                Else
        '                    Dim tag As String = DirectCast(obj, __groupHelper(Of T)).key
        '                    Return Equals(tag)
        '                End If
        '            End Function
        '        End Structure
    End Module
End Namespace