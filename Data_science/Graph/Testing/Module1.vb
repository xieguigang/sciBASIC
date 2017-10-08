#Region "Microsoft.VisualBasic::9b06c4a6eaeff7cb31813c0ae2d0924d, ..\sciBASIC#\Data_science\Graph\Testing\Module1.vb"

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

Imports Graph

Module Module1

    Sub Main()
        Dim tree As BinaryTree(Of String) = BinaryTree(Of String).ROOT
        Dim rand As New Random

        For i As Integer = 10 To 100
            tree.Insert(i, rand.Next(10, 10000000))
        Next

        Dim g = tree.CreateGraph

        Pause()
    End Sub
End Module
