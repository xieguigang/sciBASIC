#Region "Microsoft.VisualBasic::ed65746ce4718e7f8721a0a4c81ef19f, sciBASIC#\CLI_tools\FindKeyWord\FormFoundTools.vb"

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

    '   Total Lines: 210
    '    Code Lines: 152
    ' Comment Lines: 4
    '   Blank Lines: 54
    '     File Size: 7.62 KB


    ' Class FormFoundTools
    ' 
    '     Sub: CheckBox2_CheckedChanged, FormFoundTools_Load, ListView1_DoubleClick, ListView1_KeyUp, ListView1_SelectedIndexChanged
    '          OpenEditor, UpdateProcess
    ' 
    ' Class Settings
    ' 
    '     Properties: DefaultEditor, DefaultFile, Editors, History
    ' 
    '     Function: Open, Save
    ' 
    '     Sub: AddEditor, SetPath
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic

Public Class FormFoundTools

    Private Sub 打开文件夹ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 打开文件夹ToolStripMenuItem.Click

        If String.IsNullOrEmpty(tbKeyword.Text) Then
            MsgBox("请先输入需要查找的关键词")
            Return
        End If

        Using DirOpen = New FolderBrowserDialog
            If Not DirOpen.ShowDialog = DialogResult.OK Then
                Return
            End If

            Call ListView1.Items.Clear()

            ToolStripProgressBar1.Value = 0

            Dim Result = CLI.Found(Keyword:=tbKeyword.Text,
                                   DIR:=DirOpen.SelectedPath,
                                   FilteringExt:=cbFilteringExt.Checked,
                                   _extList:=tbExtList.Text,
                                   _usingRegex:=cbRegex.Checked,
                                   Process:=AddressOf Me.UpdateProcess)

            For Each item In Result

                For Each Line In item.Index
                    Dim s_data As String() = {item.File, Line.Line, Line.TextLine}
                    Call ListView1.Items.Add(New ListViewItem(s_data))
                Next
            Next

            If Result.IsNullOrEmpty Then
                Call MsgBox("搜索完毕！" & vbCrLf & vbCrLf & "没有命中纪录", MsgBoxStyle.Information)
            Else
                Call MsgBox("搜索完毕！" & vbCrLf & vbCrLf & $"命中 {(From obj In Result Select obj.Index.Count).Sum} 条记录，在 {Result.Count} 个文件之中！", MsgBoxStyle.Information)
            End If

        End Using
    End Sub

    Private Sub ListView1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListView1.SelectedIndexChanged

    End Sub

    Private Sub ListView1_KeyUp(sender As Object, e As KeyEventArgs) Handles ListView1.KeyUp

    End Sub

    Private Sub ListView1_DoubleClick(sender As Object, e As EventArgs) Handles ListView1.DoubleClick

        If ListView1.SelectedIndices.Count = 0 Then
            Return
        End If

        Dim idx = ListView1.SelectedIndices.Item(0)
        Dim item = ListView1.Items.Item(idx)
        Dim file As String = item.Text

        Call Process.Start(file)
    End Sub

    Private Sub 退出ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 退出ToolStripMenuItem.Click
        Call Settings.Save()
        Call Close()
    End Sub

    Dim Settings As Settings

    Private Sub 复制文件路径ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 复制文件路径ToolStripMenuItem.Click
        If ListView1.SelectedIndices.Count = 0 Then
            Return
        End If

        Dim idx = ListView1.SelectedIndices.Item(0)
        Dim item = ListView1.Items.Item(idx)
        Dim file As String = item.Text

        Call My.Computer.Clipboard.Clear()
        Call My.Computer.Clipboard.SetText(file)
    End Sub

    Private Sub 使用编辑器打开ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 使用编辑器打开ToolStripMenuItem.Click
        Call ListView1_DoubleClick(Nothing, Nothing)
    End Sub

    Private Sub OpenEditor(sender As Object, e As EventArgs)
        If ListView1.SelectedIndices.Count = 0 Then
            Return
        End If

        Dim idx = ListView1.SelectedIndices.Item(0)
        Dim item = ListView1.Items.Item(idx)
        Dim file As String = item.Text

        Call Settings.Open(DirectCast(sender.text, Control).Text, file)
    End Sub

    Private Sub FormFoundTools_Load(sender As Object, e As EventArgs) Handles Me.Load
        Settings = Global.FindKeyWord.Settings.DefaultFile.LoadXml(Of Settings)(ThrowEx:=False)
        If Settings Is Nothing Then
            Settings = New Settings With {
                .Editors = New List(Of Microsoft.VisualBasic.ComponentModel.TripleKeyValuesPair),
                .History = New List(Of String)
            }
            Call Settings.Save(Global.FindKeyWord.Settings.DefaultFile)
        End If

        Call Settings.SetPath(Global.FindKeyWord.Settings.DefaultFile)

        For Each editor In Settings.Editors
            Call Me.ContextMenuStrip1.Items.Add($"使用 {editor.Key} 打开", Nothing, AddressOf OpenEditor)
        Next

        ToolStripProgressBar1.Maximum = 100
        ToolStripProgressBar1.Minimum = 0
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles cbRegex.CheckedChanged

    End Sub

    Private Sub 复制关键词ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 复制关键词ToolStripMenuItem.Click
        If ListView1.SelectedIndices.Count = 0 Then
            Return
        End If

        Dim idx = ListView1.SelectedIndices.Item(0)
        Dim item = ListView1.Items.Item(idx)
        Dim Line As String = item.SubItems(0).Text

        Call My.Computer.Clipboard.Clear()
        Call My.Computer.Clipboard.SetText(Line)
    End Sub

    Private Sub UpdateProcess(process As String, percentage As Integer)
        Call Me.Invoke(Sub()
                           ToolStripStatusLabel1.Text = process
                           ToolStripProgressBar1.Value = percentage
                       End Sub)
    End Sub
End Class

Public Class Settings : Inherits Microsoft.VisualBasic.ComponentModel.ITextFile

    Public Property DefaultEditor As String = "notepad"

    Public Shared ReadOnly Property DefaultFile As String = App.ExecutablePath.TrimFileExt & ".cfg"

    Public Property History As List(Of String)

    ''' <summary>
    ''' {EXE, argvs}
    ''' </summary>
    ''' <returns></returns>
    Public Property Editors As List(Of Microsoft.VisualBasic.ComponentModel.TripleKeyValuesPair)

    Sub SetPath(path As String)
        FilePath = path
    End Sub

    Public Function Open(editor As String, file As String) As Boolean
        Dim LQuery = (From item In Editors Where String.Equals(editor, item.Key) Select item).FirstOrDefault

        If LQuery Is Nothing Then
            Return False
        End If

        Dim process = New ProcessStartInfo(LQuery.Value1, String.Format(LQuery.Value2, file))
        Try
            Call System.Diagnostics.Process.Start(process)
        Catch ex As Exception
            Call MsgBox(ex.ToString, MsgBoxStyle.Critical)
            Return False
        End Try

        Return True
    End Function

    Public Sub AddEditor(Name As String, File As String, argvs As String)
        If String.IsNullOrEmpty(argvs) Then
            argvs = """{0}"""
        End If

        If String.IsNullOrEmpty(Name) Then
            Name = IO.Path.GetFileNameWithoutExtension(File)
        End If

        Dim LQuery = (From item In Editors Where String.Equals(Name, item.Key) Select item).FirstOrDefault

        If LQuery Is Nothing Then
            LQuery = New Microsoft.VisualBasic.ComponentModel.TripleKeyValuesPair With {.Key = Name, .Value1 = File, .Value2 = argvs}
            Call Editors.Add(LQuery)
        Else
            LQuery.Value1 = File
            LQuery.Value2 = argvs
        End If

        Call Save()
    End Sub

    Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean
        Return Me.GetXml.SaveTo(getPath(FilePath), Encoding)
    End Function
End Class
