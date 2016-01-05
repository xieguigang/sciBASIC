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
