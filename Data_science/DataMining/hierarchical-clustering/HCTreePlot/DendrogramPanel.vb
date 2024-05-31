#Region "Microsoft.VisualBasic::a7bc8d9eacc3c5b06a533371be45bc5b, Data_science\DataMining\hierarchical-clustering\HCTreePlot\DendrogramPanel.vb"

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

    '   Total Lines: 57
    '    Code Lines: 45 (78.95%)
    ' Comment Lines: 3 (5.26%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (15.79%)
    '     File Size: 2.18 KB


    ' Class DendrogramPanel
    ' 
    '     Properties: classinfo
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetColor
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Chart = Microsoft.VisualBasic.Data.ChartPlots.Graphic.Plot

Public MustInherit Class DendrogramPanel : Inherits Chart

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

    Protected labelFont As Font
    Protected ReadOnly linkColor As Stroke
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
        Me.linkColor = Stroke.TryParse(theme.gridStrokeX)
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
