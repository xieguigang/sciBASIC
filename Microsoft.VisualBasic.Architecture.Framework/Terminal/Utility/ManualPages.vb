#Region "Microsoft.VisualBasic::022c48ff60aa11fbd66b2c657c58ba1c, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Terminal\Utility\ManualPages.vb"

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

Namespace Terminal.Utility

    Public Class ManualPages : Implements System.IDisposable

        Protected _manualData As List(Of String)
        Protected p As Integer

        Public Const MANUAL_PAGE_PROMPTED As String = "Press [ENTER] or [DONW_ARROW] to continute the next page, press [Q] or [ESC] to exit manual...."

        Sub New(strManual As String)
            Me._manualData = Strings.Split(strManual, vbCr).AsList
        End Sub

        Sub New(strManual As IEnumerable(Of String))
            _manualData = strManual.AsList
        End Sub

        Public Overrides Function ToString() As String
            Return _manualData.FirstOrDefault & "....."
        End Function

        ''' <summary>
        ''' 使用回车键或者箭头下显示下一行，字母q或者ESC键退出Manual
        ''' </summary>
        ''' <param name="initLines">最开始显示的行数</param>
        ''' <remarks></remarks>
        Public Overridable Sub ShowManual(Optional initLines As Integer = 50, Optional printLines As Integer = 10)
            If initLines >= _manualData.Count - 1 Then
                Call Console.WriteLine(String.Join(vbCrLf, _manualData))
                Return
            End If

            Dim s As String = String.Join(vbCrLf, _manualData.Take(initLines).ToArray)
            Dim PrintPrompted = Sub()
                                    Call Console.WriteLine(s)
                                    Call Console.WriteLine(vbCrLf & vbCrLf & MANUAL_PAGE_PROMPTED)
                                End Sub
            Call PrintPrompted()

            p = initLines
            _manualData = _manualData.Skip(initLines).AsList

            Do While _manualData.Count > 0
                Dim c As ConsoleKeyInfo = Console.ReadKey

                If c.Key = ConsoleKey.Enter OrElse c.Key = ConsoleKey.DownArrow Then
                    p += printLines
                    s = String.Join(vbCrLf, _manualData.Take(printLines).ToArray)
                    _manualData = _manualData.Skip(printLines).AsList

                    Console.CursorTop -= 1
                    Call Console.WriteLine(New String(" "c, Console.BufferWidth))
                    Console.CursorTop -= 2
                    Call PrintPrompted()
                ElseIf c.Key = ConsoleKey.Escape OrElse c.Key = ConsoleKey.Q Then
                    Call Console.WriteLine()
                    Return
                End If
            Loop

            Console.CursorTop -= 1
            Call Console.WriteLine(New String(" "c, Console.BufferWidth))
            Console.CursorTop -= 2
        End Sub

        ''' <summary>
        ''' 从文本文件之中加载Manual数据
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function FromFile(path As String) As ManualPages
            Return New ManualPages(IO.File.ReadAllLines(path))
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(      disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(      disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class

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
