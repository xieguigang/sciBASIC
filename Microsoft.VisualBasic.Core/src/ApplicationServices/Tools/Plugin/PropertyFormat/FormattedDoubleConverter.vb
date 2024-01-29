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