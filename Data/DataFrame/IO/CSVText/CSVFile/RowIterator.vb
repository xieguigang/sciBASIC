#Region "Microsoft.VisualBasic::d26c7d0436e34b483ded495910e39b35, Data\DataFrame\IO\CSVText\CSVFile\RowIterator.vb"

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

    '   Total Lines: 90
    '    Code Lines: 62 (68.89%)
    ' Comment Lines: 12 (13.33%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 16 (17.78%)
    '     File Size: 3.27 KB


    '     Class RowIterator
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetRows, RowSolver
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace IO.CSVFile

    Public Class RowIterator : Implements IDisposable

        ReadOnly s As StreamReader

        Dim disposedValue As Boolean
        Dim parser As RowTokenizer

        Sub New(file As Stream)
            s = New StreamReader(file)
        End Sub

        Public Iterator Function GetRows() As IEnumerable(Of RowObject)
            Dim line As Value(Of String) = ""
            Dim row As New List(Of String)
            Dim lastOpen As Boolean = False

            Do While (line = s.ReadLine) IsNot Nothing
                Dim parser As New RowTokenizer(line, If(row > 0, row.LastOrDefault & vbCrLf, Nothing))

                ' continute
                With parser.GetTokens.ToArray
                    If lastOpen Then
                        ' continute
                        If row > 0 Then
                            Call row.Pop()
                        End If
                    End If

                    Call row.AddRange(.ByRef)

                    If .Length > 0 Then
                        lastOpen = parser.GetStackOpenStatus
                    End If
                End With

                If Not lastOpen Then
                    Yield New RowObject(row.PopAll)
                End If
            Loop
        End Function

        Public Shared Function RowSolver(file As Stream, simple As Boolean) As IEnumerable(Of RowObject)
            If simple Then
                Return New StreamReader(file) _
                    .IteratesStream _
                    .SeqIterator _
                    .AsParallel _
                    .Select(Function(line)
                                Return (line.i, New RowObject(New RowTokenizer(line.value).GetTokens))
                            End Function) _
                    .OrderBy(Function(r) r.i) _
                    .Select(Function(r) r.Item2)
            Else
                Return New RowIterator(file).GetRows
            End If
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Call s.Dispose()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
