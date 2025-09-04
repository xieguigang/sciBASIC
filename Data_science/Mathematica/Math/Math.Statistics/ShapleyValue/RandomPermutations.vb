Imports System.Runtime.CompilerServices
Imports rand = Microsoft.VisualBasic.Math.RandomExtensions

Namespace ShapleyValue

    Public Class RandomPermutations

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function getRandom(min As Integer, max As Integer) As Integer
            Return rand.NextInteger(min, max + 1)
        End Function

        Public Shared Function getRandom(size As Long) As IList(Of Integer)

            Dim res As IList(Of Integer) = New List(Of Integer)()
            Dim temp As IList(Of Integer) = New List(Of Integer)()
            For i As Integer = 1 To size
                temp.Add(i)
            Next

            While temp.Count > 0
                Dim random = getRandom(0, temp.Count - 1)
                res.Add(temp(random))
                temp.RemoveAt(random)
            End While

            Return res
        End Function

    End Class

End Namespace
