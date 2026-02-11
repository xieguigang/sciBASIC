#Region "Microsoft.VisualBasic::01b85dab9045ffe8d1eeec9acee56bb5, gr\network-visualization\Datavisualization.Network\Graph\Model\data\NodeData.vb"

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

    '   Total Lines: 175
    '    Code Lines: 126 (72.00%)
    ' Comment Lines: 28 (16.00%)
    '    - Xml Docs: 92.86%
    ' 
    '   Blank Lines: 21 (12.00%)
    '     File Size: 5.96 KB


    '     Class NodeData
    ' 
    '         Properties: betweennessCentrality, color, force, initialPostion, mass
    '                     neighborhoods, neighbours, origID, size, weights
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: CheckInside, Clone, SafeGetRadius, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Linq
Imports System.Runtime.Serialization
Imports System.Xml.Serialization

#If NET48 Then
Imports Microsoft.VisualBasic.Serialization.JSON
#Else
Imports System.Text.Json.Serialization
#End If

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports LineCap = System.Drawing.Drawing2D.LineCap
Imports TextureBrush = System.Drawing.TextureBrush
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports LineCap = Microsoft.VisualBasic.Imaging.LineCap
Imports TextureBrush = Microsoft.VisualBasic.Imaging.TextureBrush
#End If

Namespace Graph

    Public Class NodeData : Inherits GraphData

        ''' <summary>
        ''' Get length of the <see cref="neighbours"/> index array
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property neighborhoods As Integer
            Get
                If neighbours Is Nothing Then
                    Return 0
                Else
                    Return neighbours.Length
                End If
            End Get
        End Property

        ''' <summary>
        ''' 这个主要是为了兼容圆形或者矩形之类的大小信息
        ''' </summary>
        ''' <returns></returns>
        Public Property size As Double()

        ''' <summary>
        ''' Mass weight
        ''' </summary>
        ''' <returns></returns>
        Public Property mass As Double

        ''' <summary>
        ''' For 2d layout <see cref="FDGVector2"/> / 3d layout <see cref="FDGVector3"/>
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <IgnoreDataMember>
        <DataIgnored>
        <JsonIgnore>
        <SoapIgnore>
        <XmlIgnore>
        Public Property initialPostion As AbstractVector
        Public Property origID As String

        <IgnoreDataMember>
        <DataIgnored>
        <JsonIgnore>
        <SoapIgnore>
        <XmlIgnore>
        Public Property force As Point

        ''' <summary>
        ''' 颜色<see cref="SolidBrush"/>或者绘图<see cref="TextureBrush"/>
        ''' </summary>
        ''' <returns></returns>
        <IgnoreDataMember>
        <DataIgnored>
        <JsonIgnore>
        <SoapIgnore>
        <XmlIgnore>
        <ScriptIgnore>
        Public Property color As Brush

        Public Property weights As Double()

        ''' <summary>
        ''' 与本节点相连接的其他节点的<see cref="Node.Label">编号</see>
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property neighbours As Integer()

        Public Property betweennessCentrality As Double

        Public Sub New()
            MyBase.New()

            mass = 1.0F
            initialPostion = Nothing
            ' for merging the graph
            origID = ""
        End Sub

        Sub New(copy As NodeData)
            Me.color = copy.color
            Me.force = copy.force
            Me.initialPostion = copy.initialPostion
            Me.label = copy.label
            Me.mass = copy.mass
            Me.neighbours = copy.neighbours.SafeQuery.ToArray
            Me.origID = copy.origID
            Me.Properties = New Dictionary(Of String, String)(copy.Properties)
            Me.size = If(copy.size Is Nothing, {}, copy.size.ToArray)
            Me.weights = copy.weights.SafeQuery.ToArray
        End Sub

        Public Overridable Function Clone() As NodeData
            Return New NodeData With {
                .label = label,
                .betweennessCentrality = betweennessCentrality,
                .color = color,
                .force = force,
                .initialPostion = New FDGVector3(initialPostion),
                .mass = mass,
                .neighbours = neighbours.SafeQuery.ToArray,
                .origID = origID,
                .Properties = New Dictionary(Of String, String)(Properties),
                .size = size.SafeQuery.ToArray,
                .weights = weights.SafeQuery.ToArray
            }
        End Function

        Public Function SafeGetRadius() As Single
            If size.IsNullOrEmpty Then
                Return 0
            Else
                Return size(0)
            End If
        End Function

        Public Function CheckInside(rect As Rectangle) As Boolean
            If initialPostion Is Nothing Then
                Return rect.X = 0 AndAlso rect.Y = 0
            End If

            Return rect.X < initialPostion.x AndAlso rect.Right > initialPostion.x AndAlso
                rect.Top < initialPostion.y AndAlso rect.Bottom > initialPostion.y
        End Function

        Public Overrides Function ToString() As String
            Return initialPostion.ToString
        End Function
    End Class
End Namespace
