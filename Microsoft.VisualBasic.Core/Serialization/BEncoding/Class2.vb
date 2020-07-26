Namespace Serialization.Bencoding

    ''' <summary>
    ''' A bencode integer.
    ''' </summary>
    ''' <summary>
    ''' Allows you to set an integer to a BInteger.
    ''' </summary>
    ''' <paramname="i">The integer.</param>
    ''' <returns>The BInteger.</returns>
    Public Class BInteger
        Implements BElement, IComparable(Of BInteger)

        ''' Cannot convert ConversionOperatorDeclarationSyntax, CONVERSION ERROR: Conversion for ConversionOperatorDeclaration not implemented, please report this issue in 'public static implicit oper...' at character 5739
        ''' 
        ''' 
        ''' Input:
        ''' 		/// <summary>
        ''' 		/// Allows you to set an integer to a BInteger.
        ''' 		/// </summary>
        ''' 		/// <param name="i">The integer.</param>
        ''' 		/// <returns>The BInteger.</returns>
        ''' 		public static implicit operator Bencoding.BInteger(int i)
        ''' 		{
        ''' 			return new Bencoding.BInteger(i);
        ''' 		}
        ''' 
        ''' 

        ''' <summary>
        ''' The value of the bencoded integer.
        ''' </summary>
        Public Property Value As Long

        ''' <summary>
        ''' The main constructor.
        ''' </summary>
        ''' <paramname="value">The value of the bencoded integer.</param>
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

        ''' <seecref="Object.GetHashCode()"/>
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

        ''' <seecref="Object.ToString()"/>
        Public Overrides Function ToString() As String
            Return Value.ToString()
        End Function

        ''' <seecref="IComparable.CompareTo(Object)"/>
        Public Function CompareTo(ByVal other As BInteger) As Integer Implements IComparable(Of BInteger).CompareTo
            Return Value.CompareTo(other.Value)
        End Function
    End Class
End Namespace