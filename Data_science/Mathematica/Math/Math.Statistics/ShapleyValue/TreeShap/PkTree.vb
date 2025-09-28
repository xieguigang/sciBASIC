#Region "Microsoft.VisualBasic::5a3d3b5cf20e617330f0a9c370376418, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\TreeShap\PkTree.vb"

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

    '   Total Lines: 61
    '    Code Lines: 43 (70.49%)
    ' Comment Lines: 6 (9.84%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (19.67%)
    '     File Size: 2.31 KB


    '     Class PkTree
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: printNode, printNodeConstructor, printTree, printTreeConstructor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices

Namespace ShapleyValue.TreeShape

    Public Class PkTree

        Public ReadOnly root As PkNode

        Public Sub New(root As PkNode)
            Me.root = root
        End Sub

        Private Const INDENT_STEP As String = "    "

        ''' <summary>
        ''' Prints the decision tree using java "if" tree as its notation, for convenient reading
        ''' </summary>
        Public Overridable Sub printTree(pw As TextWriter)
            printNode(pw, "", root)
            pw.Flush()
        End Sub

        Private Shared Sub printNode(pw As TextWriter, indent As String, node As PkNode)
            If node.SplitProp Then
                pw.printf("%sif (data[%d] <= %A) {  // dc = %f%n", indent, node.splitFeatureIndex, node.splitValue, node.dataCount)
                printNode(pw, indent & INDENT_STEP, node.yes)
                pw.printf("%s} else {%n", indent)
                printNode(pw, indent & INDENT_STEP, node.no)
                pw.printf("%s}%n", indent)
            Else
                pw.printf("%sreturn %A;  // dc = %f%n", indent, node.leafValue, node.dataCount)
            End If
        End Sub

        ''' <summary>
        ''' Prints the tree in form of java builder - useful for importing more test trees from other formats
        ''' </summary>
        Public Overridable Sub printTreeConstructor(pw As TextWriter)
            pw.print("new PkTree(")
            printNodeConstructor(pw, INDENT_STEP, root)
            pw.printf("%n)%n")
            pw.Flush()
        End Sub

        Private Shared Sub printNodeConstructor(pw As TextWriter, indent As String, node As PkNode)
            If node.SplitProp Then
                pw.printf("%n%sPkNode.split(/*dc=*/%A, /*col=*/%d, %A,", indent, node.dataCount, node.splitFeatureIndex, node.splitValue)
                printNodeConstructor(pw, indent & INDENT_STEP, node.yes)
                pw.print(",")
                printNodeConstructor(pw, indent & INDENT_STEP, node.no)
                pw.print(")")
            Else
                pw.printf("%n%sPkNode.leaf(/*dc=*/%A, %A)", indent, node.dataCount, node.leafValue)
            End If
        End Sub

    End Class


End Namespace

