#Region "Microsoft.VisualBasic::4f2fcc2de0828c1688275a428f71a035, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\UnixMan\ManualPages.vb"

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

    '   Total Lines: 106
    '    Code Lines: 65
    ' Comment Lines: 22
    '   Blank Lines: 19
    '     File Size: 4.33 KB


    '     Class ManualPages
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: FromFile, ToString
    ' 
    '         Sub: (+2 Overloads) Dispose, ShowManual
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Terminal.Utility

    Public Class ManualPages : Implements IDisposable

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
End Namespace
