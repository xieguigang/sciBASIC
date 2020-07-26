Imports System.Text

Namespace Serialization.Bencoding

    ''' <summary>
    ''' An interface for bencoded elements.
    ''' </summary>
    Public Interface BElement

        ''' <summary>
        ''' Generates the bencoded equivalent of the element.
        ''' </summary>
        ''' <returns>The bencoded equivalent of the element.</returns>
        Function ToBencodedString() As String

        ''' <summary>
        ''' Generates the bencoded equivalent of the element.
        ''' </summary>
        ''' <param name="u">The StringBuilder to append to.</param>
        ''' <returns>The bencoded equivalent of the element.</returns>
        Function ToBencodedString(ByVal u As StringBuilder) As StringBuilder
    End Interface
End Namespace