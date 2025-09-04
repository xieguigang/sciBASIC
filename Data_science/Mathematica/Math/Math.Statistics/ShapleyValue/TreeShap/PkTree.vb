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
