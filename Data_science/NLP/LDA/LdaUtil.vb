Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

Namespace LDA

    ''' <summary>
    ''' @author hankcs
    ''' </summary>
    Public Class LdaUtil

        Private Class descOrder : Implements IComparer(Of Double)

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Public Function Compare(x As Double, y As Double) As Integer Implements IComparer(Of Double).Compare
                Return y.CompareTo(x)
            End Function
        End Class

        ''' <summary>
        ''' To translate a LDA matrix to readable result </summary>
        ''' <param name="phi"> the LDA model </param>
        ''' <param name="vocabulary"> </param>
        ''' <param name="limit"> limit of max words in a topic </param>
        ''' <returns> a map array </returns>
        Public Shared Function translate(phi As Double()(), vocabulary As Vocabulary, limit As Integer) As IDictionary(Of String, Double)()
            Dim result As IDictionary(Of String, Double)() = New IDictionary(Of String, Double)(phi.Length - 1) {}

            limit = stdNum.Min(limit, phi(0).Length)

            For k As Integer = 0 To phi.Length - 1
                Dim rankMap As New SortedDictionary(Of Double, String)(New descOrder())

                For ii As Integer = 0 To phi(k).Length - 1
                    rankMap(phi(k)(ii)) = vocabulary.getWord(ii)
                Next

                Dim iterator As IEnumerator(Of KeyValuePair(Of Double, String)) = rankMap.GetEnumerator()
                result(k) = New Dictionary(Of String, Double)()
                Dim i = 0

                While i < limit

                    If Not iterator.MoveNext() Then
                        Exit While
                    End If

                    Dim entry = iterator.Current
                    result(k)(entry.Value) = entry.Key
                    Threading.Interlocked.Increment(i)
                End While
            Next

            Return result
        End Function

        Public Shared Function translate(tp As Double(), phi As Double()(), vocabulary As Vocabulary, limit As Integer) As IDictionary(Of String, Double)
            Dim topicMapArray = translate(phi, vocabulary, limit)
            Dim p = -1.0
            Dim t = -1

            For k = 0 To tp.Length - 1

                If tp(k) > p Then
                    p = tp(k)
                    t = k
                End If
            Next

            Return topicMapArray(t)
        End Function

        ''' <summary>
        ''' To print the result in a well formatted form </summary>
        ''' <param name="result"> </param>
        Public Shared Sub explain(result As IDictionary(Of String, Double)())
            Dim i = 0

            For Each topicMap In result
                Console.Write("topic {0:D} :" & vbLf, stdNum.Min(Threading.Interlocked.Increment(i), i - 1))
                explain(topicMap)
                Console.WriteLine()
            Next
        End Sub

        Public Shared Sub explain(topicMap As IDictionary(Of String, Double))
            For Each entry In topicMap
                Console.WriteLine(entry)
            Next
        End Sub
    End Class
End Namespace
