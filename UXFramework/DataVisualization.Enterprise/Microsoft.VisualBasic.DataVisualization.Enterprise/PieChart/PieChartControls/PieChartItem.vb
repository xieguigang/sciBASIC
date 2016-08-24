#Region "Microsoft.VisualBasic::4b43d000a7d75e470d029ae7c61cf4f4, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\PieChart\PieChartControls\PieChartItem.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Collections.Generic
Imports System.Text
Imports System.Drawing
Imports System.ComponentModel
Imports System.Runtime.Serialization

Namespace Windows.Forms.Nexus

    ''' <summary>
    ''' Represents a single pie slice in a pie chart.
    ''' </summary>
    <Serializable, TypeConverter(GetType(PieChartItem.ItemConverter)), DesignTimeVisible(False), DefaultProperty("Text")>
    Partial Public Class PieChartItem
        Implements ICloneable, ISerializable, IComparable(Of PieChartItem)

#Region "Constructor"
        ''' <summary>
        ''' Constructs a new instance.
        ''' </summary>
        Public Sub New()
            Me.New(10)
        End Sub

        ''' <summary>
        ''' Constructs a new instance.
        ''' </summary>
        ''' <param name="weight">The weight the item has.</param>
        Public Sub New(weight As Double)
            Me.New(weight, Color.Gray)
        End Sub

        ''' <summary>
        ''' Constructs a new instance.
        ''' </summary>
        ''' <param name="weight">The weight the item has.</param>
        ''' <param name="color">The fill color of the item.</param>
        Public Sub New(weight As Double, color As Color)
            Me.New(weight, color, "")
        End Sub

        ''' <summary>
        ''' Constructs a new instance.
        ''' </summary>
        ''' <param name="weight">The weight the item has.</param>
        ''' <param name="color">The fill color of the item.</param>
        ''' <param name="text">The textual representation of the item.</param>
        Public Sub New(weight As Double, color As Color, text As String)
            Me.New(weight, color, text, "")
        End Sub

        ''' <summary>
        ''' Constructs a new instance.
        ''' </summary>
        ''' <param name="weight">The weight the item has.</param>
        ''' <param name="color">The fill color of the item.</param>
        ''' <param name="text">The textual representation of the item.</param>
        ''' <param name="toolTipText">The tool tip text displayed when the mouse hovers over the item.</param>
        Public Sub New(weight As Double, color As Color, text As String, toolTipText As String)
            Me.New(weight, color, text, toolTipText, 0)
        End Sub

        ''' <summary>
        ''' Constructs a new instance.
        ''' </summary>
        ''' <param name="weight">The weight the item has.</param>
        ''' <param name="color">The fill color of the item.</param>
        ''' <param name="text">The textual representation of the item.</param>
        ''' <param name="toolTipText">The tool tip text displayed when the mouse hovers over the item.</param>
        ''' <param name="offset">The offset of the item from the center of the pie, in pixels.</param>
        Public Sub New(weight As Double, color As Color, text As String, toolTipText As String, offset As Single)
            Me._Weight = weight
            Me._Color = color
            Me._Text = text
            Me._ToolTipText = toolTipText
            Me._Offset = offset
        End Sub

        ''' <summary>
        ''' Constructs a new instance.
        ''' </summary>
        ''' <param name="info">The serialization information for deserialization.</param>
        ''' <param name="context">The context for deserialization.</param>
        Protected Sub New(info As SerializationInfo, context As StreamingContext)
            Deserialize(info, context)
        End Sub
#End Region

#Region "Fields"
        ''' <summary>
        ''' The textual representation of the item.
        ''' </summary>
        Private _Text As String

        ''' <summary>
        ''' The tool tip text displayed when the mouse hovers over the item.
        ''' </summary>
        Private _ToolTipText As String

        ''' <summary>
        ''' The weight the item has.
        ''' </summary>
        Private _Weight As Double

        ''' <summary>
        ''' The offset of the item from the center of the pie, in pixels.
        ''' </summary>
        Private _Offset As Single

        ''' <summary>
        ''' The fill color of the item.
        ''' </summary>
        Private _Color As Color

        ''' <summary>
        ''' A user-defined tag object
        ''' </summary>
        Private _Tag As Object

        ''' <summary>
        ''' The control which contains this item.
        ''' </summary>
        Private owner As PieChart
#End Region

#Region "Properties"
        ''' <summary>
        ''' Gets or sets the textual representation of the item.
        ''' </summary>
        <Browsable(True)> <DefaultValue("")> <Description("The text that will be drawn on the control.")>
        Public Property Text() As String
            Get
                Return _Text
            End Get
            Set(value As String)
                If _Text <> value Then
                    _Text = value
                    MarkVisual()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the tool tip text displayed when the mouse hovers over the item.
        ''' </summary>
        <Browsable(True)> <DefaultValue("")> <Description("The text that will be placed in a tooltip when this slice is hovered over.")>
        Public Property ToolTipText() As String
            Get
                Return _ToolTipText
            End Get
            Set(value As String)
                If _ToolTipText <> value Then
                    _ToolTipText = value
                    MarkVisual()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the weight the item has.  This weight is taken over the total weight of all items
        ''' to determine how large of an angle this slice has.
        ''' </summary>
        <Browsable(True)> <DefaultValue(10)> <Description("The weight of this slice.")>
        Public Property Weight() As Double
            Get
                Return _Weight
            End Get
            Set(value As Double)
                If _Weight <> value Then
                    UpdateWeight(value - _Weight)
                    _Weight = value
                    MarkStructure()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets the percent of the control occupied by this item.
        ''' </summary>
        <Browsable(False)>
        Public ReadOnly Property Percent() As Double
            Get
                ' not calculatable if the item is not in a control.
                If owner Is Nothing Then
                    Throw New InvalidOperationException("The item percent cannot be calculated because the item does not belong to a pie chart.")
                End If

                If owner.Items.TotalItemWeight = 0 Then
                    Return 0
                End If

                Return Weight / owner.Items.TotalItemWeight
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the offset of the item from the center of the pie, in pixels.
        ''' </summary>
        <Browsable(True)> <DefaultValue(0)> <Description("The offset of this slice from the center of the pie, in pixels.")>
        Public Property Offset() As Single
            Get
                Return _Offset
            End Get
            Set(value As Single)
                If Me._Offset <> value Then
                    _Offset = value
                    MarkStructure()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the fill color of the item.
        ''' </summary>
        <Browsable(True)> <DefaultValue(GetType(Color), "Color.Gray")> <Description("The color of the slice.")>
        Public Property Color() As Color
            Get
                Return _Color
            End Get
            Set(value As Color)
                If _Color <> value Then
                    _Color = value
                    MarkVisual(True)
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a user-defined tag value.
        ''' </summary>
        <Browsable(False)>
        Public Property Tag() As Object
            Get
                Return _Tag
            End Get
            Set(value As Object)
                _Tag = value
            End Set
        End Property
#End Region

#Region "Methods"
        ''' <summary>
        ''' Shortcut method to inform the control that the structure has changed.
        ''' </summary>
        Private Sub MarkStructure()
            If owner IsNot Nothing Then
                owner.MarkStructuralChange()
            End If
        End Sub

        ''' <summary>
        ''' Shortcut method to inform the control that the control needs refreshing.
        ''' </summary>
        Private Sub MarkVisual()
            MarkVisual(False)
        End Sub

        ''' <summary>
        ''' Shortcut method to inform the control that the control needs refreshing.
        ''' </summary>
        ''' <param name="recreateGraphics">True if the underlying pens and brushes need to be recreated.</param>
        Private Sub MarkVisual(recreateGraphics As Boolean)
            If owner IsNot Nothing Then
                owner.MarkVisualChange(recreateGraphics)
            End If
        End Sub

        ''' <summary>
        ''' Sets the owning control.
        ''' </summary>
        ''' <param name="control">The control that contains this item.</param>
        Friend Sub SetOwner(control As PieChart)
            If owner IsNot control AndAlso owner IsNot Nothing AndAlso control IsNot Nothing Then
                Throw New InvalidOperationException("A pie chart item cannot belong to two different controls.")
            End If

            UpdateWeight(-Me.Weight)
            Me.owner = control
            UpdateWeight(Me.Weight)
        End Sub

        Private Sub UpdateWeight(difference As Double)
            If Me.owner IsNot Nothing Then
                Me.owner.Items.ChangeItemWeight(difference)
            End If
        End Sub
#End Region

#Region "Serialization"
        ''' <summary>
        ''' Populates a SerializationInfo with the data needed to serialize the target object.
        ''' </summary>
        ''' <param name="info">The SerializationInfo to populate with data.</param>
        ''' <param name="context">The destination for this serialization.</param>
        Private Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
            Serialize(info, context)
        End Sub

        ''' <summary>
        ''' Populates a SerializationInfo with the data needed to serialize the target object.
        ''' </summary>
        ''' <param name="info">The SerializationInfo to populate with data.</param>
        ''' <param name="context">The destination for this serialization.</param>
        Protected Overridable Sub Serialize(info As SerializationInfo, context As StreamingContext)
            If Not String.IsNullOrEmpty(Text) Then
                info.AddValue("Text", Text)
            End If
            If Not String.IsNullOrEmpty(ToolTipText) Then
                info.AddValue("ToolTipText", ToolTipText)
            End If

            info.AddValue("Weight", Weight)
            info.AddValue("Offset", Offset)
            info.AddValue("Color", Color)
        End Sub

        ''' <summary>
        ''' Loads the state of the item from the specified SerializationInfo. 
        ''' </summary>
        ''' <param name="info">The SerializationInfo that describes the item.</param>
        ''' <param name="context">The state of the stream during deserialization.</param>
        Protected Overridable Sub Deserialize(info As SerializationInfo, context As StreamingContext)
            For Each entry As SerializationEntry In info
                Select Case entry.Name
                    Case "Text"
                        Me.Text = info.GetString(entry.Name)
                        Exit Select
                    Case "ToolTipText"
                        Me.ToolTipText = info.GetString(entry.Name)
                        Exit Select
                    Case "Weight"
                        Me.Weight = info.GetDouble(entry.Name)
                        Exit Select
                    Case "Offset"
                        Me.Offset = info.GetSingle(entry.Name)
                        Exit Select
                    Case "Color"
                        Me.Color = CType(entry.Value, Color)
                        Exit Select
                End Select
            Next
        End Sub

        ''' <summary>
        ''' Makes an exact copy of this item.
        ''' </summary>
        ''' <returns>A new replica of this item.</returns>
        Public Overridable Function Clone() As Object Implements ICloneable.Clone
            Return New PieChartItem(Weight, Color, Text, ToolTipText, Offset)
        End Function
#End Region

#Region "IComparable Members"
        Public Function CompareTo(other As PieChartItem) As Integer Implements IComparable(Of PieChartItem).CompareTo
            Return Me.Weight.CompareTo(other.Weight)
        End Function
#End Region
    End Class
End Namespace
