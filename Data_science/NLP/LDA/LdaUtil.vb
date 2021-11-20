Imports stdNum = System.Math

Namespace LDA
    Friend Class descOrder
        Implements IComparer(Of Double)

        Public Function Compare(ByVal x As Double, ByVal y As Double) As Integer Implements IComparer(Of Double).Compare
            Return y.CompareTo(x)
        End Function
    End Class

    ''' <summary>
    ''' @author hankcs
    ''' </summary>
    Public Class LdaUtil
        ''' <summary>
        ''' To translate a LDA matrix to readable result </summary>
        ''' <param name="phi"> the LDA model </param>
        ''' <param name="vocabulary"> </param>
        ''' <param name="limit"> limit of max words in a topic </param>
        ''' <returns> a map array </returns>
        Public Shared Function translate(ByVal phi As Double()(), ByVal vocabulary As Vocabulary, ByVal limit As Integer) As IDictionary(Of String, Double)()
            Dim result As IDictionary(Of String, Double)() = New IDictionary(Of String, Double)(phi.Length - 1) {}

            limit = stdNum.Min(limit, phi(0).Length)

            For k = 0 To phi.Length - 1
                Dim rankMap As IDictionary(Of Double, String) = New SortedDictionary(Of Double, String)(New descOrder())

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

        Public Shared Function translate(ByVal tp As Double(), ByVal phi As Double()(), ByVal vocabulary As Vocabulary, ByVal limit As Integer) As IDictionary(Of String, Double)
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
        Public Shared Sub explain(ByVal result As IDictionary(Of String, Double)())
            Dim i = 0

            For Each topicMap In result
                Console.Write("topic {0:D} :" & vbLf, stdNum.Min(Threading.Interlocked.Increment(i), i - 1))
                explain(topicMap)
                Console.WriteLine()
            Next
        End Sub

        Public Shared Sub explain(ByVal topicMap As IDictionary(Of String, Double))
            For Each entry In topicMap
                Console.WriteLine(entry)
            Next
        End Sub
    End Class
End Namespace
