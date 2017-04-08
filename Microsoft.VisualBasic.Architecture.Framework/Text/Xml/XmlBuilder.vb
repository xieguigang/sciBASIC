
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder

Namespace Text.Xml.Models

    ''' <summary>
    ''' Builder for XML and html
    ''' </summary>
    Public Class XmlBuilder : Inherits ScriptBuilder

        Sub New()
            Call MyBase.New(1024)
        End Sub

        Public Overloads Shared Operator +(xb As XmlBuilder, node As XElement) As XmlBuilder
            Call xb.Script.AppendLine(node.ToString)
            Return xb
        End Operator
    End Class
End Namespace