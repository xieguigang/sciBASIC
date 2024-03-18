Namespace ApplicationServices.Terminal.LineEdit

    Friend Class CompletionState

        Public Prefix As String
        Public Completions As String()
        Public Col, Row, Width, Height As Integer
        Private selected_item, top_item As Integer

        Public Sub New(col As Integer, row As Integer, width As Integer, height As Integer)
            Me.Col = col
            Me.Row = row
            Me.Width = width
            Me.Height = height

            If Me.Col < 0 Then Throw New ArgumentException("Cannot be less than zero" & Me.Col.ToString(), "Col")
            If Me.Row < 0 Then Throw New ArgumentException("Cannot be less than zero", "Row")
            If Me.Width < 1 Then Throw New ArgumentException("Cannot be less than one", "Width")
            If Me.Height < 1 Then Throw New ArgumentException("Cannot be less than one", "Height")

        End Sub

        Private Sub DrawSelection()
            For r = 0 To Height - 1
                Dim item_idx = top_item + r
                Dim selected = item_idx = selected_item

                Console.ForegroundColor = If(selected, ConsoleColor.Black, ConsoleColor.Gray)
                Console.BackgroundColor = If(selected, ConsoleColor.Cyan, ConsoleColor.Blue)

                Dim item = Prefix & Completions(item_idx)
                If item.Length > Width Then item = item.Substring(0, Width)

                Console.CursorLeft = Col
                Console.CursorTop = Row + r
                Console.Write(item)
                For space As Integer = item.Length To Width
                    Console.Write(" ")
                Next
            Next
        End Sub

        Public ReadOnly Property Current As String
            Get
                Return Completions(selected_item)
            End Get
        End Property

        Private Shared Sub SaveExcursion(code As Action)
            Dim saved_col = Console.CursorLeft
            Dim saved_row = Console.CursorTop
            Dim saved_fore = Console.ForegroundColor
            Dim saved_back = Console.BackgroundColor

            code()

            Console.CursorLeft = saved_col
            Console.CursorTop = saved_row
            If LineEditor.unix_reset_colors IsNot Nothing Then
                LineEditor.unix_raw_output.Write(LineEditor.unix_reset_colors, 0, LineEditor.unix_reset_colors.Length)
            Else
                Console.ForegroundColor = saved_fore
                Console.BackgroundColor = saved_back
            End If
        End Sub

        Public Sub Show()
            Call SaveExcursion(New Action(AddressOf DrawSelection))
        End Sub

        Public Sub SelectNext()
            If selected_item + 1 < Completions.Length Then
                selected_item += 1
                If selected_item - top_item >= Height Then top_item += 1
                Call SaveExcursion(New Action(AddressOf DrawSelection))
            End If
        End Sub

        Public Sub SelectPrevious()
            If selected_item > 0 Then
                selected_item -= 1
                If selected_item < top_item Then top_item = selected_item
                Call SaveExcursion(New Action(AddressOf DrawSelection))
            End If
        End Sub

        Private Sub Clear()
            For r = 0 To Height - 1
                Console.CursorLeft = Col
                Console.CursorTop = Row + r
                For space As Integer = 0 To Width
                    Console.Write(" ")
                Next
            Next
        End Sub

        Public Sub Remove()
            Call SaveExcursion(New Action(AddressOf Clear))
        End Sub
    End Class

End Namespace