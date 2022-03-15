#Region "Microsoft.VisualBasic::ce7a4e01177a0c434afa2afda90f09c1, sciBASIC#\Data\SearchEngine\test\Module1.vb"

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

    '   Total Lines: 23
    '    Code Lines: 17
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 648.00 B


    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Data.IO.SearchEngine.Index

Module Module1

    Sub Main()
        Dim tree As New AVLTree(Of String, Double)(Function(a, b) a.CompareTo(b))
        Dim rnd As New Random

        For i As Integer = 0 To 20
            Call tree.Add(RandomASCIIString(10, skipSymbols:=True), rnd.NextDouble)
            Call Thread.Sleep(100)
        Next

        Dim repo = tree.root.ToIndex
        Dim tree2 = repo.BinaryTree

        Call repo.GetXml.SaveTo("./test_index.xml")

        Pause()
    End Sub
End Module
