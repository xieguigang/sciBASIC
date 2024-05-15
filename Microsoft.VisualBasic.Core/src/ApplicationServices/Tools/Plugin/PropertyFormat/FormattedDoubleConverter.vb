#Region "Microsoft.VisualBasic::7703b97c5e5d19e5ce09ae07fb0c4a32, Microsoft.VisualBasic.Core\src\ApplicationServices\Tools\Plugin\PropertyFormat\FormattedDoubleConverter.vb"

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

    '   Total Lines: 61
    '    Code Lines: 41
    ' Comment Lines: 7
    '   Blank Lines: 13
    '     File Size: 2.33 KB


    '     Class FormattedDoubleConverter
    ' 
    '         Function: CanConvertFrom, CanConvertTo, ConvertFrom, ConvertTo
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Globalization

Namespace ApplicationServices.Plugin

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://stackoverflow.com/questions/16541264/property-grid-number-formatting
    ''' </remarks>
    Public Class FormattedDoubleConverter : Inherits TypeConverter

        Public Overrides Function CanConvertFrom(context As ITypeDescriptorContext, sourceType As Type) As Boolean
            Return sourceType Is GetType(String) OrElse sourceType Is GetType(Double)
        End Function

        Public Overrides Function CanConvertTo(context As ITypeDescriptorContext, destinationType As Type) As Boolean
            Return destinationType Is GetType(String) OrElse destinationType Is GetType(Double)
        End Function

        Public Overrides Function ConvertFrom(context As ITypeDescriptorContext, culture As CultureInfo, value As Object) As Object
            If TypeOf value Is Double Then
                Return value
            End If

            Dim str As String = CStr(value)

            If Not str Is Nothing Then
                Return Double.Parse(str)
            End If

            Return Nothing
        End Function

        Public Overrides Function ConvertTo(context As ITypeDescriptorContext, culture As CultureInfo, value As Object, destinationType As Type) As Object
            If destinationType IsNot GetType(String) Then
                Return Nothing
            End If

            If TypeOf value Is Double Then
                Dim [property] = context.PropertyDescriptor

                If [property] IsNot Nothing Then
                    ' Analyze the property for a second attribute that gives the format string
                    Dim formatStrAttr = [property].Attributes.OfType(Of FormattedDoubleFormatString)().FirstOrDefault

                    If formatStrAttr IsNot Nothing Then
                        Return CDbl(value).ToString(formatStrAttr.FormatString)
                    Else
                        Return CDbl(value).ToString
                    End If
                Else
                    Return CDbl(value).ToString
                End If
            End If

            Return Nothing
        End Function
    End Class
End Namespace
