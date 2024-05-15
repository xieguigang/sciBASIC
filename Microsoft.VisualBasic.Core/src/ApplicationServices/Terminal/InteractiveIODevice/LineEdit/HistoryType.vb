#Region "Microsoft.VisualBasic::f57909467f6d859630797db62835e259, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\InteractiveIODevice\LineEdit\HistoryType.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 150
    '    Code Lines: 101
    ' Comment Lines: 21
    '   Blank Lines: 28
    '     File Size: 4.95 KB


    '     Class HistoryType
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [Next], NextAvailable, Previous, PreviousAvailable, SearchBackward
    ' 
    '         Sub: Accept, Append, Close, CursorToEnd, Dump
    '              RemoveLast, Update
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Terminal.LineEdit

    ''' <summary>
    ''' Emulates the bash-like behavior, where edits done to the
    ''' history are recorded
    ''' </summary>
    Friend Class HistoryType
        Private history As String()
        Private head, tail As Integer
        Private cursor, count As Integer
        Private histfile As String

        Public Sub New(app As String, size As Integer)
            If size < 1 Then Throw New ArgumentException("size")

            If Not app Is Nothing Then
                Dim dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                histfile = $"{dir}/{app}-history.sh"
            End If

            history = New String(size - 1) {}
            cursor = 0
            tail = 0
            head = 0

            For Each line As String In histfile.IterateAllLines(verbose:=False, unsafe:=False)
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
