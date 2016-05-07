Imports System.Drawing

Namespace Terminal

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' http://www.cnblogs.com/masonlu/p/4668232.html
    ''' </remarks>
    Public Class ProgressBar : Inherits AbstractBar
        Implements IDisposable

        Dim colorBack As ConsoleColor = Console.BackgroundColor
        Dim colorFore As ConsoleColor = Console.ForegroundColor

        Dim current As Integer
        Dim y As Integer

        Sub New(title As String, Optional Y As Integer = 1)
            Call Console.WriteLine(title)

            Me.y = Y
            AddHandler TerminalEvents.Resize, AddressOf __resize

            Call __resize(Nothing, Nothing)
        End Sub

        Private Sub __resize(size As Size, old As Size)
            Console.ResetColor()
            Console.SetCursorPosition(0, y)
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
            Call SetProgress(current)
            current += 1
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="p">Percentage, 假设是从p到current</param>
        Public Sub SetProgress(p As Integer, Optional detail As String = "")
            Console.BackgroundColor = ConsoleColor.Yellow
            ' /运算返回完整的商，包括余数，SetCursorPosition会自动四舍五入
            Dim cx As Integer = p * (Console.WindowWidth - 2) / 100

            Console.SetCursorPosition(0, y)

            For i As Integer = 0 To cx
                Console.Write(" ")
            Next

            Console.BackgroundColor = colorBack
            Console.ForegroundColor = ConsoleColor.Green
            Console.SetCursorPosition(0, y + 1)
            Console.Write("{0}%", p)
            Console.ForegroundColor = colorFore

            If Not String.IsNullOrEmpty(detail) Then
                Console.WriteLine("  " & detail)
            End If
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    RemoveHandler TerminalEvents.Resize, AddressOf __resize
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace