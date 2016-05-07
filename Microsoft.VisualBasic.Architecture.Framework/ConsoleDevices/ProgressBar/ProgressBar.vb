Namespace ConsoleDevice

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' http://www.cnblogs.com/masonlu/p/4668232.html
    ''' </remarks>
    Public Class ProgressBar : Inherits AbstractBar

        Dim colorBack As ConsoleColor = Console.BackgroundColor
        Dim colorFore As ConsoleColor = Console.ForegroundColor

        Dim current As Integer

        Sub New(title As String)
            Call Console.WriteLine(title)

            Console.BackgroundColor = ConsoleColor.DarkCyan
            For i = 0 To Console.WindowWidth - 3
                '(0,1) 第二行
                Console.Write(" ")
            Next
            '(0,1) 第二行
            Console.WriteLine(" ")
            Console.BackgroundColor = colorBack
        End Sub

        Public Overrides Sub [Step]()
            current += 1
            Call SetProgress(current)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="p">Percentage</param>
        Public Sub SetProgress(p As Integer)
            Console.BackgroundColor = ConsoleColor.Yellow
            '/返回完整的商，包括余数，SetCursorPosition会自动四舍五入
            Console.SetCursorPosition(p * (Console.WindowWidth - 2) / 100, 1)
            'MsgBox(i * (Console.WindowWidth - 2) / 100)
            'MsgBox(Console.CursorLeft)
            'MsgBox(Console.CursorSize)
            Console.Write(" ")
            Console.BackgroundColor = colorBack
            Console.ForegroundColor = ConsoleColor.Green
            Console.SetCursorPosition(0, 2)
            Console.Write("{0}%", p)
            Console.ForegroundColor = colorFore
        End Sub
    End Class
End Namespace