Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Collection

    Public Module FuzzyGroup

        <Extension>
        Public Function FuzzyGroups(Of T As sIdEnumerable)(source As IEnumerable(Of T), Optional cut As Double = 0.6) As GroupResult(Of T, String)()
            Return source.FuzzyGroups(Function(x) x.Identifier, cut).ToArray
        End Function

        <Extension>
        Public Iterator Function FuzzyGroups(Of T)(
                                 source As IEnumerable(Of T),
                                 getKey As Func(Of T, String),
                        Optional cut As Double = 0.6) As IEnumerable(Of GroupResult(Of T, String))

            Dim gs = From x As T
                     In source
                     Select obj = New __groupHelper(Of T) With {
                         .x = x,
                         .key = getKey(x),
                         .cut = cut
                     }
                     Group obj By obj Into Group
            Dim out As GroupResult(Of T, String)

            For Each g In gs
                out = New GroupResult(Of T, String) With {
                    .Group = g.Group.ToArray(Function(x) x.x),
                    .Tag = g.obj.key
                }
                Yield out
            Next
        End Function

        Private Structure __groupHelper(Of T)

            Public key As String
            Public x As T
            Public cut As Double

            Public Overrides Function ToString() As String
                Return Me.GetJson
            End Function

            Public Overrides Function Equals(obj As Object) As Boolean
                If obj Is Nothing Then
                    Return False
                Else
                    Dim tag As String = DirectCast(obj, __groupHelper(Of T)).key
                    Dim edits As DistResult = ComputeDistance(key, tag)
                    If edits Is Nothing Then
                        Return False
                    Else
                        Return edits.MatchSimilarity >= cut
                    End If
                End If
            End Function
        End Structure
    End Module
End Namespace