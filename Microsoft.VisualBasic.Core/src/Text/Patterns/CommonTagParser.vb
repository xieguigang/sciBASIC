Namespace Text.Patterns

    Public Class CommonTagParser

        Public ReadOnly Property nameMatrix As Char()()
        Public ReadOnly Property maxLen As Integer

        ''' <summary>
        ''' the max length of the labels common parts
        ''' </summary>
        Public ReadOnly Property MaxColumnIndex As Integer

        ''' <summary>
        ''' get the max common prefixed label value
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LabelPrefix As String
            Get
                Return nameMatrix(Scan0).Take(MaxColumnIndex).CharString
            End Get
        End Property

        Sub New(allSampleNames As IEnumerable(Of String), Optional maxDepth As Boolean = False)
            nameMatrix = allSampleNames.Select(Function(name) name.ToArray).ToArray
            maxLen% = Aggregate name As Char()
                      In nameMatrix
                      Into Max(name.Length)

            Call walkLabels(maxDepth)
        End Sub

        Private Sub walkLabels(maxDepth As Boolean)
            Dim col As Char()

            For i As Integer = 0 To maxLen - 1
                _MaxColumnIndex = i
                col = nameMatrix _
                    .Select(Function(name)
                                Return name.ElementAtOrNull(MaxColumnIndex)
                            End Function) _
                    .ToArray

                '      colIndex
                '      |
                ' iBAQ-AA-1
                ' iBAQ-BB-2
                If maxDepth Then
                    If Not nameMatrix _
                        .GroupBy(Function(cs)
                                     Return cs.Take(MaxColumnIndex + 1).CharString
                                 End Function) _
                        .All(Function(c)
                                 Return c.Count > 1
                             End Function) Then

                        _MaxColumnIndex -= 1
                        Exit For
                    End If
                Else
                    If col.Distinct.Count > 1 Then
                        ' group label at here
                        Exit For
                    End If
                End If
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return LabelPrefix
        End Function
    End Class
End Namespace