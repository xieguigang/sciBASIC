#Region "Microsoft.VisualBasic::cd4e3c3318a40f872415a6dd191feef1, gr\network-visualization\Visualizer\Styling\Expression\Brush\DiscreteBrush.vb"

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

    '   Total Lines: 96
    '    Code Lines: 76 (79.17%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 20 (20.83%)
    '     File Size: 3.46 KB


    '     Interface IGetBrush
    ' 
    '         Function: GetBrush
    ' 
    '     Class DiscreteSequenceBrush
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetBrush
    ' 
    '     Class DiscreteBrush
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetBrush
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports any = Microsoft.VisualBasic.Scripting

Namespace Styling.FillBrushes

    Public Interface IGetBrush

        Function GetBrush(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush))

    End Interface

    Public Class DiscreteSequenceBrush : Implements IGetBrush

        ReadOnly pattern As String
        ReadOnly selector As Func(Of Node, Object)

        Sub New(map As MapExpression)
            pattern = map.values(Scan0)
            selector = map.propertyName.SelectNodeValue
        End Sub

        Public Iterator Function GetBrush(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush)) Implements IGetBrush.GetBrush
            Dim allNodes As Node() = nodes.ToArray
            Dim allTypes As Index(Of String) = allNodes _
                .Select(selector) _
                .Select(AddressOf any.ToString) _
                .Distinct _
                .Indexing
            Dim allColors As Brush() = Designer.GetColors(pattern, n:=allTypes.Count) _
                .Select(Function(c)
                            Return New SolidBrush(c)
                        End Function) _
                .ToArray
            Dim i As Integer

            For Each node As Node In allNodes
                i = allTypes.IndexOf(any.ToString(selector(node)))

                Yield New Map(Of Node, Brush) With {
                    .Key = node,
                    .Maps = allColors(i)
                }
            Next
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
