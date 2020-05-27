Namespace ApplicationServices.Terminal.Utility

    ''' <summary>
    ''' 有显示标题的
    ''' </summary>
    Public Class IndexedManual : Inherits ManualPages

        Dim Title As String

        ''' <summary>
        ''' 与<see cref="ManualPages"></see>所不同的是，本对象之中的这个字符串数组表示的是一页帮助，而不是一行帮助信息
        ''' </summary>
        ''' <param name="Pages"></param>
        ''' <remarks></remarks>
        Sub New(Pages As IEnumerable(Of String), title$)
            Call MyBase.New(Pages)
            Me.Title = title & vbCrLf & vbCrLf & Navigations
        End Sub

        Const Navigations As String = "Press [HOME] goto first page and [END] goto last page. [PageUp] for previous page."

        ''' <summary>
        ''' 使用[Enter][Down_arrow][pagedown]翻下一页[Up_arrow][Pageup]翻上一页，[q]或者[esc]结束，[home]第一页[end]最后一页
        ''' </summary>
        ''' <param name="initLines">无用的参数</param>
        ''' <param name="printLines">无用的参数</param>
        ''' <remarks></remarks>
        Public Overrides Sub ShowManual(Optional initLines As Integer = 50, Optional printLines As Integer = 10)
            Dim currentPage As String = _manualData(p)
            Call PrintPrompted(p, initLines, printLines)

            Do While p < Me._manualData.Count - 1

                Dim c As ConsoleKeyInfo = Console.ReadKey

                If c.Key = ConsoleKey.Enter OrElse
                    c.Key = ConsoleKey.DownArrow OrElse
                    c.Key = ConsoleKey.PageDown Then

                    p += 1
                    Call PrintPrompted(p, initLines, printLines)

                ElseIf c.Key = ConsoleKey.Escape OrElse
                    c.Key = ConsoleKey.Q Then
                    Call Console.WriteLine()
                    Return

                ElseIf c.Key = ConsoleKey.UpArrow OrElse
                    c.Key = ConsoleKey.PageUp Then
                    If p > 0 Then p -= 1
                    Call PrintPrompted(p, initLines, printLines)

                ElseIf c.Key = ConsoleKey.Home Then
                    p = 0
                    Call PrintPrompted(p, initLines, printLines)

                ElseIf c.Key = ConsoleKey.End Then
                    p = _manualData.Count - 1
                    Call PrintPrompted(p, initLines, printLines)
                End If
            Loop
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="p">Current page index</param>
        Private Sub PrintPrompted(p As Integer, initLines As Integer, printLines As Integer)
            Dim currentPage As String = _manualData(p)

            Call Console.Clear()
            Call Console.WriteLine(Title)
            Call Console.WriteLine()
            Call Console.WriteLine(__sp(p, _manualData.Count))
            Call Console.WriteLine(vbCrLf)

            Dim buf As String() = Strings.Split(currentPage, vbCrLf)
            If buf.Length >= 20 Then
                Using Man As ManualPages = New ManualPages(buf)
                    Call Man.ShowManual(initLines, printLines)
                End Using
            Else
                Call Console.WriteLine(currentPage)
            End If

            If p < _manualData.Count - 1 Then
                Call Console.WriteLine(vbCrLf & vbCrLf & MANUAL_PAGE_PROMPTED)
            End If
        End Sub

        Private Shared Function __sp(p As Integer, n As Integer) As String
            Return $"---------------------------------------------------------------------------------------------------{p + 1}/{n}--"
        End Function
    End Class
End Namespace