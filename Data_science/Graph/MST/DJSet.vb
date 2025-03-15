#Region "Microsoft.VisualBasic::1182f0740c05f067761940c6d438ae55, Data_science\Graph\MST\DJSet.vb"

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

    '   Total Lines: 43
    '    Code Lines: 34 (79.07%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (20.93%)
    '     File Size: 1.02 KB


    '     Class DJSet
    ' 
    '         Properties: Root
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: add, print
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace MinimumSpanningTree

    Friend Class DJSet

        Public Property Root As Integer

        Private [set] As HashSet(Of Integer)

        Public Sub New(root As Integer)
            [set] = New HashSet(Of Integer)()
            _Root = root
        End Sub

        Public Sub add(i As Integer)
            [set].Add(i)
        End Sub

        Public Sub print()
            Dim firstTime = True

            Console.Write("{")
            For Each i In [set]
                If firstTime Then
                    firstTime = False
                    Console.Write(i)
                Else
                    Console.Write(",{0}", i)
                End If
            Next
            Console.Write("}")
        End Sub
    End Class

End Namespace
