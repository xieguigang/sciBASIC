Imports System.Text

Namespace Serialization.Bencoding

    ''' <summary>
    ''' A bencode dictionary.
    ''' </summary>
    Public Class BDictionary
        Inherits SortedDictionary(Of BString, BElement)
        Implements BElement

        ''' <summary>
        ''' Gets or sets the value assosiated with the specified key.
        ''' </summary>
        ''' <param name="key">The key of the value to get or set.</param>
        ''' <returns>The value assosiated with the specified key.</returns>
        Default Public Overloads Property Item(ByVal key As String) As BElement
            Get
                Return Me(New BString(key))
            End Get
            Set(ByVal value As BElement)
                Me(New BString(key)) = value
            End Set
        End Property

        ''' <summary>
        ''' Generates the bencoded equivalent of the dictionary.
        ''' </summary>
        ''' <returns>The bencoded equivalent of the dictionary.</returns>
        Public Function ToBencodedString() As String Implements BElement.ToBencodedString
            Return ToBencodedString(New StringBuilder()).ToString()
        End Function

        ''' <summary>
        ''' Generates the bencoded equivalent of the dictionary.
        ''' </summary>
        ''' <param name="u">The StringBuilder to append to.</param>
        ''' <returns>The bencoded equivalent of the dictionary.</returns>
        Public Function ToBencodedString(ByVal u As StringBuilder) As StringBuilder Implements BElement.ToBencodedString
            If u Is Nothing Then
                u = New StringBuilder("d"c)
            Else
                u.Append("d"c)
            End If

            For i = 0 To Count - 1
                Enumerable.ElementAt(Keys, i).ToBencodedString(u)
                Enumerable.ElementAt(Values, i).ToBencodedString(u)
            Next

            Return u.Append("e"c)
        End Function

        ''' <summary>
        ''' Adds the specified key-value pair to the dictionary.
        ''' </summary>
        ''' <param name="key">The specified key.</param>
        ''' <param name="value">The specified value.</param>
        Public Overloads Sub Add(ByVal key As String, ByVal value As BElement)
            MyBase.Add(New BString(key), value)
        End Sub

        ''' <summary>
        ''' Adds the specified key-value pair to the dictionary.
        ''' </summary>
        ''' <param name="key">The specified key.</param>
        ''' <param name="value">The specified value.</param>
        Public Overloads Sub Add(ByVal key As String, ByVal value As String)
            MyBase.Add(New BString(key), New BString(value))
        End Sub

        ''' <summary>
        ''' Adds the specified key-value pair to the dictionary.
        ''' </summary>
        ''' <param name="key">The specified key.</param>
        ''' <param name="value">The specified value.</param>
        Public Overloads Sub Add(ByVal key As String, ByVal value As Integer)
            MyBase.Add(New BString(key), New BInteger(value))
        End Sub
    End Class
End Namespace