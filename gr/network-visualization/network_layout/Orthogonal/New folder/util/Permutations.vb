Imports System.Collections.Generic

' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 
Namespace Orthogonal.util

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class Permutations
        Public Shared Function allPermutations(Of T)(l As IList(Of T)) As List(Of List(Of T))
            Return allPermutations(l, 0)
        End Function

        Public Shared Function allPermutations(Of T)(l As IList(Of T), pos As Integer) As List(Of List(Of T))
            If pos = l.Count - 1 Then
                Dim permutation As List(Of T) = New List(Of T)()
                Dim permutations As List(Of List(Of T)) = New List(Of List(Of T))()
                permutation.Add(l(pos))
                permutations.Add(permutation)
                Return permutations
            Else
                Dim permutations As List(Of List(Of T)) = New List(Of List(Of T))()
                Dim basePermutations = allPermutations(l, pos + 1)
                For Each tmp In basePermutations
                    Dim basePermutation = tmp
                    For i = 0 To basePermutation.Count + 1 - 1
                        Dim perm As List(Of T) = New List(Of T)()
                        perm.AddRange(basePermutation)
                        perm.Insert(i, l(pos))
                        permutations.Add(perm)
                    Next
                Next
                Return permutations
            End If
        End Function
    End Class

End Namespace
