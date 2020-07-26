Imports System.Text

Namespace Serialization.Bencoding

    ''' <summary>
    ''' A bencode integer.
    ''' </summary>
    Public Class BInteger
        Implements BElement, IComparable(Of BInteger)

        ''' <summary>
        ''' The value of the bencoded integer.
        ''' </summary>
        Public Property Value As Long

        ''' <summary>
        ''' The main constructor.
        ''' </summary>
        ''' <param name="value">The value of the bencoded integer.</param>
        Public Sub New(ByVal value As Long)
            Me.Value = value
        End Sub

        ''' <summary>
        ''' Generates the bencoded equivalent of the integer.
        ''' </summary>
        ''' <returns>The bencoded equivalent of the integer.</returns>
        Public Function ToBencodedString() As String Implements BElement.ToBencodedString
            Return ToBencodedString(New StringBuilder()).ToString()
        End Function

        ''' <summary>
        ''' Generates the bencoded equivalent of the integer.
        ''' </summary>
        ''' <returns>The bencoded equivalent of the integer.</returns>
        Public Function ToBencodedString(ByVal u As StringBuilder) As StringBuilder Implements BElement.ToBencodedString
            If u Is Nothing Then
                u = New StringBuilder("i"c)
            Else
                u.Append("i"c)
            End If

            Return u.Append(Value.ToString()).Append("e"c)
        End Function

        ''' <see cref="Object.GetHashCode()"/>
        Public Overrides Function GetHashCode() As Integer
            Return Value.GetHashCode()
        End Function

        ''' <summary>
        ''' Int32.Equals(object)
        ''' </summary>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Try
                Return Value.Equals(CType(obj, BInteger).Value)
            Catch
                Return False
            End Try
        End Function

        ''' <see cref="Object.ToString()"/>
        Public Overrides Function ToString() As String
            Return Value.ToString()
        End Function

        ''' <see cref="IComparable.CompareTo(Object)"/>
        Public Function CompareTo(ByVal other As BInteger) As Integer Implements IComparable(Of BInteger).CompareTo
            Return Value.CompareTo(other.Value)
        End Function

        ''' <summary>
        ''' Allows you to set an integer to a BInteger.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Shared Widening Operator CType(n As Integer) As BInteger
            Return New BInteger(n)
        End Operator
    End Class
End Namespace