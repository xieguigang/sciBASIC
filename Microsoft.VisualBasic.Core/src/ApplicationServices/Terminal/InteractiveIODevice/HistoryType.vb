﻿Namespace ApplicationServices.Terminal

    '
    ' Emulates the bash-like behavior, where edits done to the
    ' history are recorded
    '
    Friend Class HistoryType
        Private history As String()
        Private head, tail As Integer
        Private cursor, count As Integer
        Private histfile As String

        Public Sub New(app As String, size As Integer)
            If size < 1 Then Throw New ArgumentException("size")

            If Not app Is Nothing Then
                Dim dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                histfile = $"{dir}/{app}.history"
            End If

            history = New String(size - 1) {}
            cursor = 0
            tail = 0
            head = 0

            For Each line As String In histfile.IterateAllLines
                If Not line.StringEmpty Then
                    Call Append(line)
                End If
            Next
        End Sub

        Public Sub Close()
            If histfile Is Nothing Then Return

            Try
                Using sw = histfile.OpenWriter
                    Dim start = If(count = history.Length, head, tail)
                    For i = start To start + count - 1
                        Dim p = i Mod history.Length
                        sw.WriteLine(history(p))
                    Next
                End Using

            Catch
                ' ignore
            End Try
        End Sub

        '
        ' Appends a value to the history
        '
        Public Sub Append(s As String)
            'Console.WriteLine ("APPENDING {0} head={1} tail={2}", s, head, tail);
            history(head) = s
            head = (head + 1) Mod history.Length
            If head = tail Then tail = tail + 1 Mod history.Length
            If count <> history.Length Then count += 1
            'Console.WriteLine ("DONE: head={1} tail={2}", s, head, tail);
        End Sub

        '
        ' Updates the current cursor location with the string,
        ' to support editing of history items.   For the current
        ' line to participate, an Append must be done before.
        '
        Public Sub Update(s As String)
            history(cursor) = s
        End Sub

        Public Sub RemoveLast()
            head = head - 1
            If head < 0 Then head = history.Length - 1
        End Sub

        Public Sub Accept(s As String)
            Dim t = head - 1
            If t < 0 Then t = history.Length - 1

            history(t) = s
        End Sub

        Public Function PreviousAvailable() As Boolean
            'Console.WriteLine ("h={0} t={1} cursor={2}", head, tail, cursor);
            If count = 0 Then Return False
            Dim [next] = cursor - 1
            If [next] < 0 Then [next] = count - 1

            If [next] = head Then Return False

            Return True
        End Function

        Public Function NextAvailable() As Boolean
            If count = 0 Then Return False
            Dim [next] = (cursor + 1) Mod history.Length
            If [next] = head Then Return False
            Return True
        End Function


        '
        ' Returns: a string with the previous line contents, or
        ' nul if there is no data in the history to move to.
        '
        Public Function Previous() As String
            If Not PreviousAvailable() Then Return Nothing

            cursor -= 1
            If cursor < 0 Then cursor = history.Length - 1

            Return history(cursor)
        End Function

        Public Function [Next]() As String
            If Not NextAvailable() Then Return Nothing

            cursor = (cursor + 1) Mod history.Length
            Return history(cursor)
        End Function

        Public Sub CursorToEnd()
            If head = tail Then Return

            cursor = head
        End Sub

        Public Sub Dump()
            Console.WriteLine("Head={0} Tail={1} Cursor={2} count={3}", head, tail, cursor, count)
            For i = 0 To history.Length - 1
                Console.WriteLine(" {0} {1}: {2}", If(i = cursor, "==>", "   "), i, history(i))
            Next
            'log.Flush ();
        End Sub

        Public Function SearchBackward(term As String) As String
            For i = 0 To count - 1
                Dim slot = cursor - i - 1
                If slot < 0 Then slot = history.Length + slot
                If slot >= history.Length Then slot = 0
                If Not Equals(history(slot), Nothing) AndAlso history(slot).IndexOf(term) <> -1 Then
                    cursor = slot
                    Return history(slot)
                End If
            Next

            Return Nothing
        End Function
    End Class
End Namespace