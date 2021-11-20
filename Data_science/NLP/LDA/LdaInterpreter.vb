Imports stdNum = System.Math

Namespace LDA

    ''' <summary>
    ''' @author hankcs
    ''' </summary>
    Public Class LdaInterpreter

        ''' <summary>
        ''' To translate a LDA matrix to readable result </summary>
        ''' <param name="phi"> the LDA model </param>
        ''' <param name="vocabulary"> </param>
        ''' <param name="limit"> limit of max words in a topic </param>
        ''' <returns> a map array </returns>
        Public Shared Function translate(phi As Double()(), vocabulary As Vocabulary, limit As Integer) As Dictionary(Of String, Double)()
            Dim result As Dictionary(Of String, Double)() = New Dictionary(Of String, Double)(phi.Length - 1) {}

            limit = stdNum.Min(limit, phi(0).Length)

            For k As Integer = 0 To phi.Length - 1
                Dim rankMap As New Dictionary(Of Double, String)

                For ii As Integer = 0 To phi(k).Length - 1
                    rankMap(phi(k)(ii)) = vocabulary.getWord(ii)
                Next

                result(k) = rankMap _
                    .OrderByDescending(Function(d) d.Key) _
                    .Take(limit) _
                    .ToDictionary(Function(d) d.Value,
                                  Function(d)
                                      Return d.Key
                                  End Function)
            Next

            Return result
        End Function

        Public Shared Function translate(tp As Double(), phi As Double()(), vocabulary As Vocabulary, limit As Integer) As Dictionary(Of String, Double)
            Dim topicMapArray = translate(phi, vocabulary, limit)
            Dim p As Double = -1.0
            Dim t As Integer = -1

            For k As Integer = 0 To tp.Length - 1
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
