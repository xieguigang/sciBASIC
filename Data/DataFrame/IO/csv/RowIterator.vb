Imports System.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace IO

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
                Dim parser As New RowTokenizer(line)

                ' continute
                With parser.GetTokens.ToArray
                    If lastOpen Then
                        ' continute
                        If row > 0 Then
                            If .Length > 0 Then
                                line = row.Last & ASCII.LF & .First
                                row.Pop()
                                row.Add(line)
                                row.AddRange(.Skip(1))
                            Else
                                ' do nothing?
                            End If
                        Else
                            row.AddRange(.ByRef)
                        End If
                    Else
                        row.AddRange(.ByRef)
                    End If

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