#Region "Microsoft.VisualBasic::3a91ac14412d89010bc4ce5abdfc7eb4, Data_science\MachineLearning\MachineLearning\QLearning\Maze-Q-Learning\QTable.vb"

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

    '   Total Lines: 38
    '    Code Lines: 28 (73.68%)
    ' Comment Lines: 5 (13.16%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (13.16%)
    '     File Size: 1.48 KB


    ' Class QTable
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: __getMapString
    ' 
    '     Sub: PrintQTable
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.QLearning

Public Class QTable : Inherits QTable(Of Char())

    Sub New(ar As Integer)
        Call MyBase.New(ar)
    End Sub

    ''' <summary>
    ''' printQtable is included for debugging purposes and uses the
    ''' action labels used in the maze class (even though the Qtable
    ''' is written so that it can more generic).
    ''' </summary>
    Public Sub PrintQTable()
        Dim [iterator] As IEnumerator = Table.Keys.GetEnumerator()
        Do While [iterator].MoveNext
            Dim key() As Char = CType([iterator].Current, Char())
            Dim qvalues() As Single = GetValues(key)

            Console.Write(AscW(key(0)) & "" & AscW(key(1)) & "" & AscW(key(2)))
            Console.WriteLine("  UP   RIGHT  DOWN  LEFT")
            Console.Write(AscW(key(3)) & "" & AscW(key(4)) & "" & AscW(key(5)))
            Console.WriteLine(": " & qvalues(0) & "   " & qvalues(1) & "   " & qvalues(2) & "   " & qvalues(3))
            Console.WriteLine(AscW(key(6)) & "" & AscW(key(7)) & "" & AscW(key(8)))
        Loop
    End Sub

    Protected Overrides Function __getMapString(map() As Char) As String
        Dim result As String = ""
        For x As Integer = 0 To map.Length - 1
            result &= "" & AscW(map(x))
            If x > 0 AndAlso x Mod 3 = 0 Then
                result += vbLf
            End If
        Next x
        Return result
    End Function
End Class
