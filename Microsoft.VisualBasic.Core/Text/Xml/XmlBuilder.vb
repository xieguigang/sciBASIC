#Region "Microsoft.VisualBasic::fd2ff3f1a3c166dddb91f6a45c9e7cd4, Microsoft.VisualBasic.Core\Text\Xml\XmlBuilder.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class XmlBuilder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: AddComment, Unescape
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder

Namespace Text.Xml.Models

    ''' <summary>
    ''' Builder for XML and html
    ''' </summary>
    Public Class XmlBuilder : Inherits ScriptBuilder

        ''' <summary>
        ''' 创建一个新的Xml文档构建工具，可以通过这个工具来构建出``Xml/Html``文档
        ''' </summary>
        Sub New()
            Call MyBase.New(1024)
        End Sub

        ''' <summary>
        ''' 从当前文本位置处添加一行``Xml/Html``注释文本
        ''' </summary>
        ''' <param name="comment"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AddComment(comment As String) As XmlBuilder
            Call AppendLine($"<!-- {comment} -->")
            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Unescape() As XmlBuilder
            Call Replace("&lt;", "<").Replace("&gt;", ">")
            Return Me
        End Function

        Public Overloads Shared Operator +(xb As XmlBuilder, node As XElement) As XmlBuilder
            Call xb.script.AppendLine(node.ToString)
            Return xb
        End Operator
    End Class
End Namespace
