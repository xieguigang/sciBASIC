#Region "Microsoft.VisualBasic::a684284fc174e9ff91fce335628b77c0, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\PieChart\PieChartControls\PieChartControl.Events.vb"

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
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D

Namespace Windows.Forms.Nexus

    Public Delegate Sub PieChartItemEventHandler(sender As Object, e As PieChart.PieChartItemEventArgs)
    Public Delegate Sub PieChartItemFocusEventHandler(sender As Object, e As PieChart.PieChartItemFocusEventArgs)

    Partial Public Class PieChart
        Inherits Control
#Region "Events"
        ''' <summary>
        ''' Fired when an item is clicked.
        ''' </summary>
        Public Event ItemClicked As PieChartItemEventHandler

        ''' <summary>
        ''' Fired when an item is double-clicked.
        ''' </summary>
        Public Event ItemDoubleClicked As PieChartItemEventHandler

        ''' <summary>
        ''' Fired when the focus is changing from one item to another.
        ''' </summary>
        Public Event ItemFocusChanging As PieChartItemFocusEventHandler

        ''' <summary>
        ''' Fired when the focus has changed to another item.
        ''' </summary>
        Public Event ItemFocusChanged As EventHandler

        ''' <summary>
        ''' Fired when the AutoSizePie property has changed.
        ''' </summary>
        Public Event AutoSizePieChanged As EventHandler

        ''' <summary>
        ''' Fired when the radius of the control has changed.
        ''' </summary>
        Public Event RadiusChanged As EventHandler

        ''' <summary>
        ''' Fired when the inclination of the control has changed.
        ''' </summary>
        Public Event InclinationChanged As EventHandler

        ''' <summary>
        ''' Fired when the rotation has changed.
        ''' </summary>
        Public Event RotationChanged As EventHandler

        ''' <summary>
        ''' Fired when the thickness of the control has changed.
        ''' </summary>
        Public Event ThicknessChanged As EventHandler

        ''' <summary>
        ''' Fired when the ShowEdges property has changed.
        ''' </summary>
        Public Event ShowEdgesChanged As EventHandler

        ''' <summary>
        ''' Fired when the TextDisplayMode property has changed.
        ''' </summary>
        Public Event TextDisplayModeChanged As EventHandler

        ''' <summary>
        ''' Fired when the ShowToolTips property has changed.
        ''' </summary>
        Public Event ShowToolTipsChanged As EventHandler

        ''' <summary>
        ''' Called to fire an ItemClicked event.
        ''' </summary>
        ''' <param name="item">The event arguments.</param>
        Private Sub FireItemClicked(item As PieChartItem)
            RaiseEvent ItemClicked(Me, New PieChartItemEventArgs(item))
        End Sub

        ''' <summary>
        ''' Called to fire an ItemDoubleClicked event.
        ''' </summary>
        ''' <param name="item">The event arguments.</param>
        Private Sub FireItemDoubleClicked(item As PieChartItem)
            RaiseEvent ItemDoubleClicked(Me, New PieChartItemEventArgs(item))
        End Sub

        ''' <summary>
        ''' Called to fire an ItemFocusChanging event.
        ''' </summary>
        ''' <param name="oldItem">The item that was previously focused.</param>
        ''' <param name="newItem">The item that is gaining focus.</param>
        Private Sub FireItemFocusChanging(oldItem As PieChartItem, newItem As PieChartItem)
            RaiseEvent ItemFocusChanging(Me, New PieChartItemFocusEventArgs(oldItem, newItem))
        End Sub

        ''' <summary>
        ''' Called to fire an ItemFocusChanged event.
        ''' </summary>
        Private Sub FireItemFocusChanged()
            RaiseEvent ItemFocusChanged(Me, EventArgs.Empty)
        End Sub

        ''' <summary>
        ''' Called to fire an AutoSizePieChanged event.
        ''' </summary>
        Friend Sub FireAutoSizePieChanged()
            RaiseEvent AutoSizePieChanged(Me, EventArgs.Empty)
        End Sub

        ''' <summary>
        ''' Called to fire an RadiusChanged event.
        ''' </summary>
        Friend Sub FireRadiusChanged()
            RaiseEvent RadiusChanged(Me, EventArgs.Empty)
        End Sub

        ''' <summary>
        ''' Called to fire an InclinationChanged event.
        ''' </summary>
        Friend Sub FireInclinationChanged()
            RaiseEvent InclinationChanged(Me, EventArgs.Empty)
        End Sub

        ''' <summary>
        ''' Called to fire an RotationChanged event.
        ''' </summary>
        Friend Sub FireRotationChanged()
            RaiseEvent RotationChanged(Me, EventArgs.Empty)
        End Sub

        ''' <summary>
        ''' Called to fire an ThicknessChanged event.
        ''' </summary>
        Friend Sub FireThicknessChanged()
            RaiseEvent ThicknessChanged(Me, EventArgs.Empty)
        End Sub

        ''' <summary>
        ''' Called to fire an ShowEdgesChanged event.
        ''' </summary>
        Friend Sub FireShowEdgesChanged()
            RaiseEvent ShowEdgesChanged(Me, EventArgs.Empty)
        End Sub

        ''' <summary>
        ''' Called to fire an TextDisplayModeChanged event.
        ''' </summary>
        Friend Sub FireTextDisplayModeChanged()
            RaiseEvent TextDisplayModeChanged(Me, EventArgs.Empty)
        End Sub

        ''' <summary>
        ''' Called to fire an ShowToolTipsChanged event.
        ''' </summary>
        Friend Sub FireShowToolTipsChanged()
            RaiseEvent ShowToolTipsChanged(Me, EventArgs.Empty)
        End Sub
#End Region

#Region "PieChartItemEventArgs"
        ''' <summary>
        ''' Stores a PieChartItem that is involved with an event.
        ''' </summary>
        Public Class PieChartItemEventArgs
            Inherits EventArgs
#Region "Constructor"
            ''' <summary>
            ''' Constructs a new instance.
            ''' </summary>
            ''' <param name="item">The item involved with an event.</param>
            Public Sub New(item As PieChartItem)
                Me.m_item = item
            End Sub
#End Region

#Region "Fields"
            ''' <summary>
            ''' The item involved with the event.
            ''' </summary>
            Private m_item As PieChartItem
#End Region

#Region "Methods"
            ''' <summary>
            ''' Gets the item involved with the event.
            ''' </summary>
            Public ReadOnly Property Item() As PieChartItem
                Get
                    Return m_item
                End Get
            End Property
#End Region
        End Class
#End Region

#Region "PieChartFocusEventArgs"
        ''' <summary>
        ''' Stores the PieChartItems that are involved with an focus changing event.
        ''' </summary>
        Public Class PieChartItemFocusEventArgs
            Inherits EventArgs
#Region "Constructor"
            ''' <summary>
            ''' Constructs a new instance.
            ''' </summary>
            ''' <param name="oldItem">The item that is losing focus.</param>
            ''' <param name="newItem">The item that is gaining focus.</param>
            Public Sub New(oldItem As PieChartItem, newItem As PieChartItem)
                Me.m_oldItem = oldItem
                Me.m_newItem = newItem
            End Sub
#End Region

#Region "Fields"
            ''' <summary>
            ''' The item that is losing focus.
            ''' </summary>
            Private m_oldItem As PieChartItem

            ''' <summary>
            ''' The item that is gaining focus.
            ''' </summary>
            Private m_newItem As PieChartItem
#End Region

#Region "Methods"
            ''' <summary>
            ''' Gets the item that is losing focus.
            ''' </summary>
            Public ReadOnly Property OldItem() As PieChartItem
                Get
                    Return m_oldItem
                End Get
            End Property

            ''' <summary>
            ''' Gets the item that is gaining focus.
            ''' </summary>
            Public ReadOnly Property NewItem() As PieChartItem
                Get
                    Return m_newItem
                End Get
            End Property
#End Region
        End Class
#End Region
    End Class
End Namespace
