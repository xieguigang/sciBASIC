Namespace ConsoleDevice

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' http://www.cnblogs.com/masonlu/p/4668232.html
    ''' </remarks>
    Public Class ProgressBar : Inherits AbstractBar



        Sub New()

        End Sub

        Public Overrides Sub [Step]()

        End Sub

        Public Sub SetProgress(p As Double)

        End Sub

        Private Sub ConsoleProcessBar()
            Dim isBreak As Boolean = False
            Dim colorBack As ConsoleColor = Console.BackgroundColor
            Dim colorFore As ConsoleColor = Console.ForegroundColor
            '(0,0)(Left,Top) 第一行
            Console.WriteLine("***********TE Mason*************")
            Console.BackgroundColor = ConsoleColor.DarkCyan
            For i = 0 To Console.WindowWidth - 3
                '(0,1) 第二行
                Console.Write(" ")
            Next
            '(0,1) 第二行
            Console.WriteLine(" ")
            Console.BackgroundColor = colorBack
            '(0,2) 第三行
            Console.WriteLine("0%")
            '(0,3) 第四行
            Console.WriteLine("<Press Enter To Break>")

            For i = 0 To 100
                If Console.KeyAvailable AndAlso Console.ReadKey(True).Key = ConsoleKey.Enter Then
                    isBreak = True
                    Exit For
                End If
                Console.BackgroundColor = ConsoleColor.Yellow
                '/返回完整的商，包括余数，SetCursorPosition会自动四舍五入
                Console.SetCursorPosition(i * (Console.WindowWidth - 2) / 100, 1)
                'MsgBox(i * (Console.WindowWidth - 2) / 100)
                'MsgBox(Console.CursorLeft)
                'MsgBox(Console.CursorSize)
                Console.Write(" ")
                Console.BackgroundColor = colorBack
                Console.ForegroundColor = ConsoleColor.Green
                Console.SetCursorPosition(0, 2)
                Console.Write("{0}%", i)
                Console.ForegroundColor = colorFore
                Threading.Thread.Sleep(1000)
            Next

            Console.SetCursorPosition(0, 3)
            Console.Write(IIf(isBreak, "Break!!!", "Finish"))
            Console.WriteLine("                           ")
            Console.ReadKey()
            Console.ReadKey(True)
        End Sub
    End Class
End Namespace