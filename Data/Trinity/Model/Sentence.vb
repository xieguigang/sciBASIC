Namespace Model

    Public Class Sentence

        Public Property segments As Segment()

        Public ReadOnly Property IsEmpty As Boolean
            Get
                If segments.IsNullOrEmpty Then
                    Return True
                End If

                If segments.All(Function(s) s.IsEmpty) Then
                    Return True
                End If

                Return False
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return segments.JoinBy("; ")
        End Function

        Friend Shared Function Parse(line As String) As Sentence
            Return New Sentence With {
               .segments = line _
                   .Split(","c, ";"c, """"c, "`"c, "~"c) _
                   .Select(Function(str)
                               Return New Segment With {
                                   .tokens = str.Trim.StringSplit("\s+")
                               }
                           End Function) _
                   .ToArray
           }
        End Function

        Friend Function Trim() As Sentence
            Dim list As New List(Of Segment)
            Dim data As Segment

            For Each block As Segment In segments
                data = New Segment With {
                    .tokens = block.tokens _
                        .Where(Function(str)
                                   Return Not TextRank.IsEmpty(str)
                               End Function) _
                        .ToArray
                }

                If Not data.tokens.IsNullOrEmpty Then
                    list.Add(data)
                End If
            Next

            Return New Sentence With {
               .segments = list.ToArray
            }
        End Function

    End Class
End Namespace