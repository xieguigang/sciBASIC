#Region "Microsoft.VisualBasic::c5cfcb8cacf64b48c43696f527c35e71, Data_science\Mathematica\SignalProcessing\MachineVision\OCR\ConfusionChars.vb"

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

    '   Total Lines: 66
    '    Code Lines: 50 (75.76%)
    ' Comment Lines: 4 (6.06%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (18.18%)
    '     File Size: 1.79 KB


    ' Class ConfusionChars
    ' 
    '     Function: Check, CreateDefaultMatrix, GetTuple
    ' 
    '     Sub: Add, AddInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Text

Public Class ConfusionChars

    ReadOnly matrix As New Dictionary(Of Char, Dictionary(Of Char, Boolean))

    Sub Add(c1 As Char, c2 As Char)
        Call AddInternal(c1, c2)
        Call AddInternal(c2, c1)
    End Sub

    Public Function Check(c1 As Char, c2 As Char) As Boolean
        If c1 = c2 Then
            Return True
        End If

        If matrix.ContainsKey(c1) Then
            Return matrix(c1).ContainsKey(c2)
        ElseIf matrix.ContainsKey(c2) Then
            Return matrix(c2).ContainsKey(c1)
        End If

        Return False
    End Function

    Private Sub AddInternal(c1 As Char, c2 As Char)
        If Not matrix.ContainsKey(c1) Then
            Call matrix.Add(c1, New Dictionary(Of Char, Boolean))
        End If
        If Not matrix(c1).ContainsKey(c2) Then
            Call matrix(c1).Add(c2, True)
        End If
    End Sub

    Public Shared Function CreateDefaultMatrix() As ConfusionChars
        Dim chars As New ConfusionChars

        For Each c As (c1 As Char, c2 As Char) In GetTuple()
            Call chars.Add(c.c1, c.c2)
        Next

        Return chars
    End Function

    ''' <summary>
    ''' load default in-memory data
    ''' </summary>
    ''' <returns></returns>
    Private Shared Iterator Function GetTuple() As IEnumerable(Of (Char, Char))
        Yield ("2", "z")
        Yield ("0", "O")
        Yield ("i", "l")
        Yield ("1", "l")
        Yield ("7", "l")
        Yield ("s", "5")
        Yield ("q", "9")
        Yield ("6", "b")
        Yield ("-", "_")
        Yield (" ", ASCII.TAB)
        Yield ("7", "t")
        Yield ("d", "0")
        Yield ("l", "j")
        Yield ("3", "t")
    End Function

End Class
