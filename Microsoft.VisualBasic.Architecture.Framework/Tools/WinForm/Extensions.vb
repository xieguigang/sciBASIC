Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Windows.Forms

    Public Module Extensions

        <Extension>
        Public Function AddFilesHistory(ByRef menu As ToolStripMenuItem, files As IEnumerable(Of String), invoke As Action(Of String), Optional formats As Func(Of String, String) = Nothing) As Boolean
            If formats Is Nothing Then
                formats = Function(path$) path$.FileName & $" ({Mid(path, 1, 30)}...)"
            End If

            For Each path$ In files.SafeQuery
                Dim file As New ToolStripMenuItem With {
                    .Text = formats(path),
                    .AutoToolTip = True,
                    .ToolTipText = path
                }

                AddHandler file.Click,
                    Sub(sender, arg)
                        Call invoke(path)
                    End Sub

                Call menu.DropDownItems.Add(file)
            Next

            Return True
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="files"></param>
        ''' <param name="path$"></param>
        ''' <param name="latestFirst">最近使用的文件被放在列表的头部，否则直接放在列表的尾部</param>
        ''' 
        <Extension>
        Public Sub AddFileHistory(ByRef files As List(Of String), path$, Optional latestFirst As Boolean = True)
            If files Is Nothing Then
                files = New List(Of String)
            End If

            Dim n As Integer = files.IndexOf(path)

            If n <> -1 Then
                Call files.RemoveAt(n)
            End If

            If latestFirst Then
                Call files.Insert(Scan0, path)
            Else
                Call files.Add(path)
            End If
        End Sub

        <Extension>
        Public Sub WriteLine(tb As TextBox, s$)
            Call tb.AppendText(s & vbCrLf)
        End Sub
    End Module
End Namespace