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