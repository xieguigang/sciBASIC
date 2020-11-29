Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public MustInherit Class DendrogramPanel : Inherits Plot

    Protected Friend ReadOnly hist As Cluster
    Protected Friend ReadOnly classIndex As Dictionary(Of String, ColorClass)

    ''' <summary>
    ''' leaf id map to <see cref="ColorClass.name"/>
    ''' </summary>
    Public ReadOnly Property classinfo As Dictionary(Of String, String)

    Protected Friend ReadOnly showAllLabels As Boolean
    Protected Friend ReadOnly showAllNodes As Boolean
    Protected Friend ReadOnly showLeafLabels As Boolean
    Protected Friend ReadOnly showRuler As Boolean

    Protected ReadOnly labelFont As Font
    Protected ReadOnly linkColor As Pen
    Protected ReadOnly pointColor As SolidBrush

    Protected Sub New(hist As Cluster, theme As Theme,
                      classes As ColorClass(),
                      classinfo As Dictionary(Of String, String),
                      showAllLabels As Boolean,
                      showAllNodes As Boolean,
                      pointColor$,
                      showLeafLabels As Boolean,
                      showRuler As Boolean)

        MyBase.New(theme)

        Me.hist = hist
        Me.classIndex = classes.SafeQuery.ToDictionary(Function(a) a.name)
        Me.classinfo = classinfo
        Me.showAllLabels = showAllLabels
        Me.labelFont = CSSFont.TryParse(theme.tagCSS)
        Me.linkColor = Stroke.TryParse(theme.gridStroke).GDIObject
        Me.showAllNodes = showAllNodes
        Me.pointColor = pointColor.GetBrush
        Me.showLeafLabels = showLeafLabels
        Me.showRuler = showRuler
    End Sub

    Protected Function GetColor(id As String) As Color
        If classinfo Is Nothing OrElse Not classinfo.ContainsKey(id) Then
            Return Nothing
        Else
            Return classIndex(classinfo(id)).color.TranslateColor
        End If
    End Function
End Class
