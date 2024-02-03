Imports System.Collections.Generic

Namespace SVG.XML.Enums

    ''' <summary>
    ''' a string constant value
    ''' </summary>
    Public MustInherit Class SvgEnum

        ReadOnly _value As String

        Protected Sub New(value As String)
            _value = value
        End Sub

        Public Overrides Function ToString() As String
            Return _value
        End Function

        Public Shared Widening Operator CType(value As SvgEnum) As String
            Return value.ToString()
        End Operator

        Public Overrides Function Equals(obj As Object) As Boolean
            If obj Is Nothing Then Return False
            If Me.GetType() IsNot obj.GetType() Then Return False

            Return _value = DirectCast(obj, SvgEnum)._value
        End Function

        ''' <summary>
        ''' the hashcode is related to the constant string value
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetHashCode() As Integer
            Dim hashCode = -862051595
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode(_value)
            hashCode = hashCode * -1521134295 + EqualityComparer(Of String).Default.GetHashCode([GetType]().Name)
            Return hashCode
        End Function
    End Class
End Namespace
