#Region "Microsoft.VisualBasic::50df7952ff08afe91832df90ea4fe4b5, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Terminal\ProgressBar\ProgressBar.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

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

        Sub New(title As String, Optional Y As Integer = 1, Optional cls As Boolean = False)
            If cls AndAlso App.IsConsoleApp Then
                Call Console.Clear()
            End If

            Call Console.WriteLine(title)

            Me.y = Y
            AddHandler TerminalEvents.Resize, AddressOf __resize

            Try
                Call __resize(Nothing, Nothing)
            Catch ex As Exception

            End Try
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

        Dim timer As Stopwatch = Stopwatch.StartNew

        ''' <summary>
        ''' 获取当前实例测量得出的总运行时间（以毫秒为单位）。
        ''' </summary>
        ''' <returns>
        ''' 一个只读长整型，表示当前实例测量得出的总毫秒数。
        ''' </returns>
        Public ReadOnly Property ElapsedMilliseconds As Long
            Get
                Return timer.ElapsedMilliseconds
            End Get
        End Property

        Private Sub __tick(p As Integer, details As String)
            Console.BackgroundColor = ConsoleColor.Yellow
            ' /运算返回完整的商，包括余数，SetCursorPosition会自动四舍五入
            Dim cx As Integer = p * (Console.WindowWidth - 2) / 100

            Console.SetCursorPosition(0, y)

            If p < current Then
                Call __resize(Nothing, Nothing)
            End If

            For i As Integer = 0 To cx
                Console.Write(" ")
            Next

            Console.BackgroundColor = colorBack
            Console.ForegroundColor = ConsoleColor.Green
            Console.SetCursorPosition(0, y + 1)
            Console.Write("{0}%", p)
            Console.ForegroundColor = colorFore

            If Not String.IsNullOrEmpty(details) Then
                Console.WriteLine(vbTab & details)
            End If
        End Sub

        ''' <summary>
        ''' <paramref name="p"/>是进度条的百分比
        ''' </summary>
        ''' <param name="p">Percentage, 假设是从p到current</param>
        Public Sub SetProgress(p As Integer, Optional detail As String = "")
            current = p
            Call __tick(current, detail)
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    RemoveHandler TerminalEvents.Resize, AddressOf __resize
                    Call timer.Stop()
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

    Public Class ProgressProvider

        ''' <summary>
        ''' 整个工作的总的tick数
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Target As Integer
        ''' <summary>
        ''' 当前已经完成的tick数
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Current As Integer

        ''' <summary>
        ''' 生成进度条的百分比值
        ''' </summary>
        ''' <param name="total"></param>
        Sub New(total As Integer)
            Target = total
        End Sub

        Dim previous#
        Dim previousTime&

        Public Function ETA(elapsed&, Optional avg As Boolean = True) As TimeSpan
            Dim out As TimeSpan

            If avg Then
                out = ETA(0R, Current / Target, elapsed)
            Else
                out = ETA(
                    previous,
                    Current / Target,
                    elapsed - previousTime)

                previousTime = elapsed
                previous = Current / Target
            End If

            Return out
        End Function

        ''' <summary>
        ''' 返回来的百分比小数，还需要乘以100才能得到进度
        ''' </summary>
        ''' <returns></returns>
        Public Function [Step]() As Double
            _Current += 1
            Return Current / Target
        End Function

        ''' <summary>
        ''' 百分比进度，不需要再乘以100了
        ''' </summary>
        ''' <returns></returns>
        Public Function StepProgress() As Integer
            Return CInt([Step]() * 100)
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="previous#">百分比</param>
        ''' <param name="cur#">百分比</param>
        ''' <param name="Elapsed#">当前的这个百分比差所经历过的时间</param>
        ''' <returns></returns>
        Public Shared Function ETA(previous#, cur#, Elapsed&) As TimeSpan
            Dim d# = cur - previous

            If d# = 0R Then
                d = 0.000000001
            End If

            Dim lefts = (1 - cur) / d   ' lefts = 100% - currents
            Dim time = lefts * Elapsed
            Dim estimates = TimeSpan.FromMilliseconds(time)
            Return estimates
        End Function
    End Class
End Namespace
