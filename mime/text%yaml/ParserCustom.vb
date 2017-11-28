#Region "Microsoft.VisualBasic::9e0d5f641791421b8bdefbd1ec97ff64, ..\sciBASIC#\mime\text%yaml\yaml\ParserCustom.vb"

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

Imports Microsoft.VisualBasic.MIME.text.yaml.Syntax

Namespace Grammar

    Partial Public Class YamlParser

        ''' <summary>
        ''' 从源文本或者文本文件中解析出YAML数据
        ''' </summary>
        ''' <param name="content">Text content data or source link.</param>
        ''' <returns></returns>
        Public Shared Function Load(content As String) As YamlStream
            Dim text As String = content.SolveStream
            Dim input As New TextInput(text)
            Dim parser As New YamlParser()
            Dim success As Boolean
            Dim stream As YamlStream = parser.ParseYamlStream(input, success)

            If success Then
                Return stream
            Else
                Dim message$ = parser.GetEorrorMessages()
                Throw New Exception(message)
            End If
        End Function

        Private currentDocument As YamlDocument

        Private Sub SetDataItemProperty(dataItem As DataItem, [property] As NodeProperty)
            If [property].Anchor IsNot Nothing Then
                currentDocument.AnchoredItems([property].Anchor) = dataItem
            End If
            dataItem.[Property] = [property]
        End Sub

        Private Function GetAnchoredDataItem(name As String) As DataItem
            If currentDocument.AnchoredItems.ContainsKey(name) Then
                Return currentDocument.AnchoredItems(name)
            Else
                [Error](name & " is not anchored.")
                Return Nothing
            End If
        End Function

        Private currentIndent As Integer = -1
        Private detectIndent As Boolean = False

        Private Indents As New Stack(Of Integer)()

        Private Function ParseIndent() As Boolean
            Dim success As Boolean
            For i As Integer = 0 To currentIndent - 1
                MatchTerminal(" "c, success)
                If Not success Then
                    Position -= i
                    Return False
                End If
            Next
            If detectIndent Then
                Dim additionalIndent As Integer = 0
                While True
                    MatchTerminal(" "c, success)
                    If success Then
                        additionalIndent += 1
                    Else
                        Exit While
                    End If
                End While
                currentIndent += additionalIndent
                detectIndent = False
            End If
            Return True
        End Function

        ''' <summary>
        ''' Mandatory Indentation for "non-indented" Scalar
        ''' </summary>
        Private Sub IncreaseIndentIfZero()
            Indents.Push(currentIndent)
            If currentIndent = 0 Then
                currentIndent += 1
            End If
            detectIndent = True
        End Sub

        ''' <summary>
        ''' Increase Indent for Nested Block Collection
        ''' </summary>
        Private Sub IncreaseIndent()
            Indents.Push(currentIndent)
            currentIndent += 1
            detectIndent = True
        End Sub

        ''' <summary>
        ''' Decrease Indent for Nested Block Collection
        ''' </summary>
        Private Sub DecreaseIndent()
            currentIndent = Indents.Pop()
        End Sub

        Private Sub RememberIndent()
            Indents.Push(currentIndent)
        End Sub

        Private Sub RestoreIndent()
            currentIndent = Indents.Pop()
        End Sub

        Private CurrentChompingMethod As ChompingMethod

        Private Sub AddIndent(modifier As BlockScalarModifier, success As Boolean)
            If success Then
                Indents.Push(currentIndent)
                currentIndent += modifier.GetIndent()
                detectIndent = True
            Else
                IncreaseIndentIfZero()
            End If

            CurrentChompingMethod = modifier.GetChompingMethod()
        End Sub

        Private Function Chomp(linebreaks As String) As String
            Select Case CurrentChompingMethod
                Case ChompingMethod.Strip
                    Return String.Empty
                Case ChompingMethod.Keep
                    Return linebreaks
                Case Else
                    If linebreaks.StartsWith(vbCr & vbLf) Then
                        Return vbCr & vbLf
                    ElseIf linebreaks.Length = 0 Then
                        Return Environment.NewLine
                    Else
                        Return linebreaks.Substring(0, 1)
                    End If
            End Select
        End Function
    End Class
End Namespace
