#Region "Microsoft.VisualBasic::bdd61d961b4b2c1aa8092fa35d2f9ab3, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\PieChart\PieChartControls\PieChartItem.ItemConverter.vb"

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

Imports System.Collections
Imports System.Collections.Generic
Imports System.Text
Imports System.Drawing
Imports System.ComponentModel
Imports System.ComponentModel.Design.Serialization

Namespace Windows.Forms.Nexus

    Partial Public Class PieChartItem
        ''' <summary>
        ''' Used to convert PieChartItems to an InstanceDescriptor for runtime design.
        ''' </summary>
        Public Class ItemConverter : Inherits TypeConverter
            ''' <summary>
            ''' Returns whether this converter can convert the object to the specified type.
            ''' </summary>
            ''' <param name="context">An ITypeDescriptorContext that provides a format context.</param>
            ''' <param name="destinationType">A Type that represents the type you want to convert to.</param>
            ''' <returns>True if this converter can perform the conversion; otherwise, false.</returns>
            Public Overrides Function CanConvertTo(context As ITypeDescriptorContext, destinationType As Type) As Boolean
                If destinationType Is GetType(InstanceDescriptor) Then
                    Return True
                End If

                Return MyBase.CanConvertTo(context, destinationType)
            End Function

            ''' <summary>
            ''' Converts the given value object to the specified type, using the specified context and culture information.
            ''' </summary>
            ''' <param name="context">An ITypeDescriptorContext that provides a format context.</param>
            ''' <param name="culture">A CultureInfo. If a null reference is passed, the current culture is assumed.</param>
            ''' <param name="value">The Object to convert.</param>
            ''' <param name="destinationType">The type to convert the value parameter to.</param>
            ''' <returns>An object that represents the converted value.</returns>
            Public Overrides Function ConvertTo(context As ITypeDescriptorContext, culture As System.Globalization.CultureInfo, value As Object, destinationType As Type) As Object
                If destinationType Is Nothing Then
                    Throw New ArgumentNullException("destinationType")
                End If

                If destinationType Is GetType(InstanceDescriptor) AndAlso (TypeOf value Is PieChartItem) Then
                    Dim item As PieChartItem = DirectCast(value, PieChartItem)
                    Dim collection As New ArrayList() From {item.Weight, item.Color, item.Text, item.ToolTipText, item.Offset}
                    'collection.Add(item.Weight)
                    'collection.Add(item.Color)
                    'collection.Add(item.Text)
                    'collection.Add(item.ToolTipText)
                    'collection.Add(item.Offset)
                    Return New InstanceDescriptor(value.[GetType]().GetConstructor(New Type() {GetType(Double), GetType(Color), GetType(String), GetType(String), GetType(Single)}), collection)
                End If

                Return MyBase.ConvertTo(context, culture, value, destinationType)
            End Function
        End Class
    End Class
End Namespace
