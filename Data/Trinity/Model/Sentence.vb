Namespace Model

    Public Class Sentence

        Public Property segments As Segment()

        Public Overrides Function ToString() As String
            Return segments.JoinBy("; ")
        End Function

        Friend Shared Function Parse(line As String) As Sentence
            Return New Sentence With {
               .segments = line _
                   .Split(","c, ";"c) _
                   .Select(Function(str)
                               Return New Segment With {
                                   .tokens = str.Trim.StringSplit("\s+")
                               }
                           End Function) _
                   .ToArray
           }
        End Function

    End Class
End Namespace