#Region "Microsoft.VisualBasic::d53aa8baf5ca3530c0bc518abe825b29, gr\Microsoft.VisualBasic.Imaging\SVG\XML\SvgPath\Interpreter.vb"

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

    '   Total Lines: 135
    '    Code Lines: 116
    ' Comment Lines: 3
    '   Blank Lines: 16
    '     File Size: 5.99 KB


    '     Class Interpreter
    ' 
    '         Properties: Commands
    ' 
    '         Sub: ArcTo, Close, CommandsToData, CurveTo, DataToCommands
    '              Format, LineTo, MoveTo, Translate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace SVG.PathHelper

    ''' <summary>
    ''' the svg path <see cref="Command"/> interpreter
    ''' </summary>
    Public Class Interpreter

        Public Property Commands As List(Of Command)

        Dim d As New List(Of String)

        Public Sub ArcTo(rx As Double, ry As Double, xRot As Double, large As Boolean, sweep As Boolean, x As Double, y As Double, Optional isRelative As Boolean = False)
            d += $"{If(isRelative, "a"c, "A"c)}{rx} {ry},{xRot} {Convert.ToInt32(large)},{Convert.ToInt32(sweep)} {x},{y}"
        End Sub

        Public Sub CurveTo(x1 As Double, y1 As Double, x2 As Double, y2 As Double, x As Double, y As Double, Optional isRelative As Boolean = False)
            d += $"{If(isRelative, "c"c, "C"c)}{x1} {y1},{x2} {y2},{x} {y}"
        End Sub

        Public Sub Close()
            d += " Z"
        End Sub

        Public Sub LineTo(x As Double, y As Double)
            d += $" L{x} {y}"
        End Sub

        Public Sub MoveTo(x As Double, y As Double)
            d += $"{If(d.IsNullOrEmpty, String.Empty, " ")}M{x} {y}"
        End Sub

        Public Sub Translate(dx As Double, dy As Double)
            If Commands Is Nothing Then Return

            For Each command As Command In Commands
                Call command.Translate(dx, dy)
            Next
        End Sub

        Public Sub Format()
            Call DataToCommands()
            Call CommandsToData()
        End Sub

        Public Sub CommandsToData()
            If Commands Is Nothing Then
                Return
            Else
                d.Clear()
            End If

            For Each command In Commands
                d += command.ToString()
            Next
        End Sub

        Public Sub DataToCommands()
            If Me.d.IsNullOrEmpty Then Throw New ArgumentException("The path string cannot be empty.")
            Dim d As String = Me.d.JoinBy("").Trim()
            If Char.ToUpper(d(0)) <> "M"c Then Throw New ArgumentException("The path string must start with a Move command.")
            Dim idxStart = 1
            Commands = New List(Of Command)()
            Dim commandLetters = New Char() {"M"c, "L"c, "H"c, "V"c, "C"c, "S"c, "Q"c, "T"c, "A"c, "Z"c}
            For i = 1 To d.Length - 1
                Dim dU = Char.ToUpper(d(i))
                If commandLetters.Contains(dU) OrElse i = d.Length - 1 Then
                    Dim c = d(idxStart - 1)
                    Dim idxEnd = If(i = d.Length - 1, If(dU = "Z"c, i, d.Length), i)
                    Dim text = d.Substring(idxStart, idxEnd - idxStart)
                    Dim tokens = Command.Parse(text)
                    Dim cU = Char.ToUpper(c)
                    Dim isRelative = Char.IsLower(c)
                    If tokens.Count > 0 Then
                        If cU = "M"c Then
                            If tokens.Count Mod 2 = 0 Then
                                For j = 0 To tokens.Count - 1 Step 2
                                    Commands.Add(New M(tokens.GetRange(j, 2), isRelative))
                                Next
                            End If
                        ElseIf cU = "L"c Then
                            If tokens.Count Mod 2 = 0 Then
                                For j = 0 To tokens.Count - 1 Step 2
                                    Commands.Add(New L(tokens.GetRange(j, 2), isRelative))
                                Next
                            End If
                        ElseIf cU = "H"c Then
                            For j = 0 To tokens.Count - 1
                                Commands.Add(New H(tokens(j), isRelative))
                            Next
                        ElseIf cU = "V"c Then
                            For j = 0 To tokens.Count - 1
                                Commands.Add(New V(tokens(j), isRelative))
                            Next
                        ElseIf cU = "A"c Then
                            If tokens.Count Mod 7 = 0 Then
                                For j = 0 To tokens.Count - 1 Step 7
                                    Commands.Add(New A(tokens.GetRange(j, 7), isRelative))
                                Next
                            End If
                        ElseIf cU = "C"c Then
                            If tokens.Count Mod 6 = 0 Then
                                For j = 0 To tokens.Count - 1 Step 6
                                    Commands.Add(New C(tokens.GetRange(j, 6), isRelative))
                                Next
                            End If
                        ElseIf cU = "S"c Then
                            If tokens.Count Mod 4 = 0 Then
                                For j = 0 To tokens.Count - 1 Step 4
                                    Commands.Add(New C(tokens.GetRange(j, 4), isRelative))
                                Next
                            End If
                        ElseIf cU = "Q"c Then
                            If tokens.Count Mod 4 = 0 Then
                                For j = 0 To tokens.Count - 1 Step 4
                                    Commands.Add(New Q(tokens.GetRange(j, 4), isRelative))
                                Next
                            End If
                        ElseIf cU = "T"c Then
                            If tokens.Count Mod 2 = 0 Then
                                For j = 0 To tokens.Count - 1 Step 2
                                    Commands.Add(New T(tokens.GetRange(j, 2), isRelative))
                                Next
                            End If
                        End If
                    End If
                    If dU = "Z"c Then Commands.Add(New Z(Char.IsLower(d(i))))
                    idxStart = i + 1
                End If
            Next
        End Sub

    End Class
End Namespace
