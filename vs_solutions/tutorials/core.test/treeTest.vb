#Region "Microsoft.VisualBasic::64a39e66adbca392459cee041c7dd66d, vs_solutions\tutorials\core.test\treeTest.vb"

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

    ' Module treeTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

#Region "Microsoft.VisualBasic::9756da122d5c75de635b0950ca67a476, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module treeTest
    ' 
    '     Sub: Main
    ' 
    ' 

#End Region

#Region "Microsoft.VisualBasic::80a3b9fac91571dcd682961d3f81301c, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module treeTest
    ' 
    '     Sub: Main
    ' 
    ' 
    ' 

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree

Module treeTest
    Sub Main()
        Dim tree As New BinaryTree(Of String)

        For i As Integer = 0 To 100
            Dim s = RandomASCIIString(10).MD5.Substring(0, 6)

            Thread.Sleep(10)

            Call tree.insert(s, s)
        Next


        Pause()
    End Sub
End Module
