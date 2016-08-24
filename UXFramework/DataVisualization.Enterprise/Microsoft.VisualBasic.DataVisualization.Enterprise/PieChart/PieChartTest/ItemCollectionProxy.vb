#Region "Microsoft.VisualBasic::52782dd921180049be55a4693a31a158, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\PieChart\PieChartTest\ItemCollectionProxy.vb"

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
Imports PieChartTest.Nexus.Reflection
Imports System.Windows.Forms.Nexus
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

Public Class ItemCollectionProxy
	Inherits ProxyInterface
	Public Sub New(proxy As PieChart)
		AddProxyObject(proxy)
	End Sub

	<ProxyTypeBinding(GetType(PieChart), "Items")> _
	<Browsable(True)> _
	<Description("The collection of pie items.")> _
	Public Property Items() As PieChart.ItemCollection
		Get
			Return ProxyGet(Of PieChart.ItemCollection)()
		End Get
		Set
			ProxySet(value)
		End Set
	End Property

	<ProxyTypeBinding(GetType(PieChart), "Font")> _
	<Browsable(True)> _
	<Description("The font used with the control.")> _
	Public Property Font() As Font
		Get
			Return ProxyGet(Of Font)()
		End Get
		Set
			ProxySet(value)
		End Set
	End Property

	<ProxyTypeBinding(GetType(PieChart), "Padding")> _
	<Browsable(True)> _
	<Description("The padding around the pie")> _
	Public Property Padding() As Padding
		Get
			Return ProxyGet(Of Padding)()
		End Get
		Set
			ProxySet(value)
		End Set
	End Property
End Class
