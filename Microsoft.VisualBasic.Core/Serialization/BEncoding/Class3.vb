
Namespace Serialization.Bencoding

    ''' <summary>
    ''' A bencode string.
    ''' </summary>
    ''' <summary>
    ''' Allows you to set a string to a BString.
    ''' </summary>
    ''' <paramname="s">The string.</param>
    ''' <returns>The BString.</returns>
    Public Class BString
        Implements BElement, IComparable(Of BString)

        ''' Cannot convert ConversionOperatorDeclarationSyntax, CONVERSION ERROR: Conversion for ConversionOperatorDeclaration not implemented, please report this issue in 'public static implicit oper...' at character 7838
        ''' 
        ''' 
        ''' Input:
        ''' 		/// <summary>
        ''' 		/// Allows you to set a string to a BString.
        ''' 		/// </summary>
        ''' 		/// <param name="s">The string.</param>
        ''' 		/// <returns>The BString.</returns>
        ''' 		public static implicit operator Bencoding.BString(string s)
        ''' 		{
        ''' 			return new Bencoding.BString(s);
        ''' 		}
        ''' 
        ''' 

        ''' <summary>
        ''' The value of the bencoded integer.
        ''' </summary>
        Public Property Value As String

        ''' <summary>
        ''' The main constructor.
        ''' </summary>
        ''' <paramname="value"></param>
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
        ''' <paramname="u">The StringBuilder to append to.</param>
        ''' <returns>The bencoded equivalent of the string.</returns>
        Public Function ToBencodedString(ByVal u As StringBuilder) As StringBuilder Implements BElement.ToBencodedString
            If u Is Nothing Then
                u = New StringBuilder(Value.Length)
            Else
                u.Append(Value.Length)
            End If

            Return u.Append(":"c).Append(Value)
        End Function

        ''' <seecref="Object.GetHashCode()"/>
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

        ''' <seecref="Object.ToString()"/>
        Public Overrides Function ToString() As String
            Return Value.ToString()
        End Function

        ''' <seecref="IComparable.CompareTo(Object)"/>
        Public Function CompareTo(ByVal other As BString) As Integer Implements IComparable(Of BString).CompareTo
            Return Value.CompareTo(other.Value)
        End Function
    End Class
End Namespace