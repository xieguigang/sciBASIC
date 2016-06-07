Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MarkupLanguage.HTML

Namespace SVG.CSS

    ''' <summary>
    ''' Style generator for the value of <see cref="XmlMeta.CSS.style"/>
    ''' </summary>
    Public Class DirectedForceGraph
        Public Property node As CssValue
        Public Property link As CssValue

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder

            Call sb.AppendLine(".node {")
            Call sb.AppendLine(node.ToString)
            Call sb.AppendLine("}")
            Call sb.AppendLine(".link {")
            Call sb.AppendLine(link.ToString)
            Call sb.AppendLine("}")

            Return sb.ToString
        End Function
    End Class

    Public Class CssValue : Inherits ClassObject
        <DataFrameColumn> Public Property stroke As String
        <DataFrameColumn("stroke-width")> Public Property strokeWidth As String
        <DataFrameColumn("stroke-opacity")> Public Property strokeOpacity As String

        Protected Overrides Function __toString() As String
            Dim sb As New StringBuilder

            For Each prop In DataFrameColumnAttribute.LoadMapping(Of CssValue)

            Next

            Return sb.ToString
        End Function
    End Class
End Namespace