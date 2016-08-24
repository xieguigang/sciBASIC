#Region "Microsoft.VisualBasic::6174d816ae0747830cffae615f8648a9, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\PieChart\PieChartControls\PieChartControl.ItemCollectionEditor.vb"

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
Imports System.ComponentModel
Imports System.Drawing.Design
Imports System.ComponentModel.Design

Namespace Windows.Forms.Nexus

    Partial Public Class PieChart
        Inherits Control
        ''' <summary>
        ''' Class used to edit an ItemCollection in design time.
        ''' </summary>
        ''' <remarks>
        ''' The designer uses the default CollectionEditor implementation, and overrides
        ''' the display text for items in the item list.
        ''' </remarks>
        Friend Class ItemCollectionEditor
            Inherits CollectionEditor
#Region "Constructor"
            Public Sub New(type As Type)
                MyBase.New(type)
            End Sub
#End Region

#Region "Overrides"
            ''' <summary>
            ''' Gets the display text for a PieChartItem in the collection list.
            ''' </summary>
            ''' <param name="value">The PieChartItem to get the display text for.</param>
            ''' <returns>The string that will be displayed for the PieChartItem.</returns>
            Protected Overrides Function GetDisplayText(value As Object) As String
                If TypeOf value Is PieChartItem Then
                    Dim item As PieChartItem = DirectCast(value, PieChartItem)
                    If Not String.IsNullOrEmpty(item.Text) Then
                        Return String.Format("{0} [weight {1:f3}]", item.Text, item.Weight)
                    Else
                        Return String.Format("[weight {0:f3}]", item.Weight)
                    End If
                End If
                Return value.[GetType]().Name
            End Function
#End Region
        End Class
    End Class
End Namespace
