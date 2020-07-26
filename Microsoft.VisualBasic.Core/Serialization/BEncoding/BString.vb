
Imports System.Text

Namespace Serialization.Bencoding

    ''' <summary>
    ''' A bencode string.
    ''' </summary>
    Public Class BString
        Implements BElement, IComparable(Of BString)

        ''' <summary>
        ''' The value of the bencoded integer.
        ''' </summary>
        Public Property Value As String

        ''' <summary>
        ''' The main constructor.
        ''' </summary>
        ''' <param name="value"></param>
        Public Sub New(ByVal value As String)
            Me.Value = value
        End Sub

        ''' <summary>
        ''' Generates the bencoded equivalent of the string.
        ''' </summary>
        ''' <returns>The bencoded equivalent of the string.</returns>
        Public Function ToBencodedString() As String Implements BElement.ToBencodedString
            Return ToBencodedString(New StringBuilder()).ToString()
        End Function

        ''' <summary>
        ''' Generates the bencoded equivalent of the string.
        ''' </summary>
        ''' <param name="u">The StringBuilder to append to.</param>
        ''' <returns>The bencoded equivalent of the string.</returns>
        Public Function ToBencodedString(ByVal u As StringBuilder) As StringBuilder Implements BElement.ToBencodedString
            If u Is Nothing Then
                u = New StringBuilder(Value.Length)
            Else
                u.Append(Value.Length)
            End If

            Return u.Append(":"c).Append(Value)
        End Function

        ''' <see cref="Object.GetHashCode()"/>
        Public Overrides Function GetHashCode() As Integer
            Return Value.GetHashCode()
        End Function

        ''' <summary>
        ''' String.Equals(object)
        ''' </summary>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Try
                Return Value.Equals(CType(obj, BString).Value)
            Catch
                Return False
            End Try
        End Function

        ''' <see cref="Object.ToString()"/>
        Public Overrides Function ToString() As String
            Return Value.ToString()
        End Function

        ''' <see cref="IComparable.CompareTo(Object)"/>
        Public Function CompareTo(ByVal other As BString) As Integer Implements IComparable(Of BString).CompareTo
            Return Value.CompareTo(other.Value)
        End Function

        ''' <summary>
        ''' Allows you to set a string to a BString.
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Overloads Shared Widening Operator CType(s As String) As BString
            Return New BString(value:=s)
        End Operator
    End Class
End Namespace