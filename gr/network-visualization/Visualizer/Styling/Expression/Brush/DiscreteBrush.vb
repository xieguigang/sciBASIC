Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Styling.FillBrushes

    Public Interface IGetBrush

        Function GetBrush(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush))

    End Interface

    Public Class DiscreteSequenceBrush : Implements IGetBrush

        ReadOnly colors As LoopArray(Of Brush)

        Sub New(map As MapExpression)

        End Sub

        Public Function GetBrush(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush)) Implements IGetBrush.GetBrush
            Throw New NotImplementedException()
        End Function
    End Class

    Public Class DiscreteBrush : Implements IGetBrush

        ReadOnly brushList As New Dictionary(Of String, Brush)
        ReadOnly selector As Func(Of Node, Object)

        Sub New(map As MapExpression)
            Dim brush As Brush

            selector = map.propertyName.SelectNodeValue

            For Each p As NamedValue(Of String) In map _
                .values _
                .Select(Function(s)
                            Return s.GetTagValue("=", trim:=True)
                        End Function)

                If p.Value.IsColorExpression Then
                    brush = New SolidBrush(p.Value.TranslateColor)
                Else
                    brush = New TextureBrush(UrlEvaluator.EvaluateAsImage(p.Value))
                End If

                Call brushList.Add(p.Name, brush)
            Next
        End Sub

        Public Iterator Function GetBrush(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush)) Implements IGetBrush.GetBrush
            Dim key As String
            Dim brush As Brush

            For Each node As Node In nodes
                key = CStrSafe(selector(node))
                brush = brushList.TryGetValue(key, default:=Brushes.Black)

                Yield New Map(Of Node, Brush) With {
                    .Key = node,
                    .Maps = brush
                }
            Next
        End Function
    End Class
End Namespace