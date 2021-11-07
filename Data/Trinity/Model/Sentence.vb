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

        ''' <summary>
        ''' exactly token matched
        ''' </summary>
        ''' <param name="token"></param>
        ''' <returns></returns>
        Public Function matchIndex(token As String) As Integer
            Dim index As Integer = -1

            For i As Integer = 0 To segments.Length - 1
                index = segments(i).matchIndex(token)

                If index > -1 Then
                    Return i * 1000 + index
                End If
            Next

            Return -1
        End Function

        ''' <summary>
        ''' search for starts with [prefix]
        ''' </summary>
        ''' <param name="token"></param>
        ''' <returns></returns>
        Public Function searchIndex(token As String) As Integer
            Dim index As Integer = -1

            For i As Integer = 0 To segments.Length - 1
                index = segments(i).searchIndex(token)

                If index > -1 Then
                    Return i * 1000 + index
                End If
            Next

            Return -1
        End Function

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