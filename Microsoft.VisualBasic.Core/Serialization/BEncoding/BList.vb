Imports System.Text

Namespace Serialization.Bencoding

    ''' <summary>
    ''' A bencode list.
    ''' </summary>
    Public Class BList : Inherits List(Of BElement)
        Implements BElement

        ''' <summary>
        ''' Generates the bencoded equivalent of the list.
        ''' </summary>
        ''' <returns>The bencoded equivalent of the list.</returns>
        Public Function ToBencodedString() As String Implements BElement.ToBencodedString
            Return ToBencodedString(New StringBuilder()).ToString()
        End Function

        ''' <summary>
        ''' Generates the bencoded equivalent of the list.
        ''' </summary>
        ''' <param name="u">The StringBuilder to append to.</param>
        ''' <returns>The bencoded equivalent of the list.</returns>
        Public Function ToBencodedString(ByVal u As StringBuilder) As StringBuilder Implements BElement.ToBencodedString
            If u Is Nothing Then
                u = New StringBuilder("l"c)
            Else
                u.Append("l"c)
            End If

            For Each element In ToArray()
                element.ToBencodedString(u)
            Next

            Return u.Append("e"c)
        End Function

        ''' <summary>
        ''' Adds the specified value to the list.
        ''' </summary>
        ''' <param name="value">The specified value.</param>
        Public Overloads Sub Add(ByVal value As String)
            MyBase.Add(New BString(value))
        End Sub

        ''' <summary>
        ''' Adds the specified value to the list.
        ''' </summary>
        ''' <param name="value">The specified value.</param>
        Public Overloads Sub Add(ByVal value As Integer)
            MyBase.Add(New BInteger(value))
        End Sub
    End Class

End Namespace