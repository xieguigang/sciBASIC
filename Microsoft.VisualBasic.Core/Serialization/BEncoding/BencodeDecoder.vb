' ***
'  Encoding usage:
'  
'  new BDictionary()
'  {
'   {"Some Key", "Some Value"},
'   {"Another Key", 42}
'  }.ToBencodedString();
'  
'  Decoding usage:
'  
'  BencodeDecoder.Decode("d8:Some Key10:Some Value13:Another Valuei42ee");
'  
'  Feel free to use it.
'  More info about Bencoding at http://wiki.theory.org/BitTorrentSpecification#bencoding
'  
'  Originally posted at http://snipplr.com/view/37790/ by SuprDewd.
'  

Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Serialization.Bencoding

    ''' <summary>
    ''' A class used for decoding Bencoding.
    ''' </summary>
    Public Module BencodeDecoder
        ''' <summary>
        ''' Decodes the string.
        ''' </summary>
        ''' <paramname="bencodedString">The bencoded string.</param>
        ''' <returns>An array of root elements.</returns>
        Public Function Decode(ByVal bencodedString As String) As BElement()
            Dim index = 0

            Try
                If Equals(bencodedString, Nothing) Then Return Nothing
                Dim rootElements As List(Of BElement) = New List(Of BElement)()

                While bencodedString.Length > index
                    rootElements.Add(ReadElement(bencodedString, index))
                End While

                Return rootElements.ToArray()
            Catch __unusedBencodingException1__ As BencodingException
                Throw
            Catch e As Exception
                Throw [Error](e)
            End Try
        End Function

        Private Function ReadElement(ByRef bencodedString As String, ByRef index As Integer) As BElement
            Select Case bencodedString(index)
                Case "0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c
                    Return ReadString(bencodedString, index)
                Case "i"c
                    Return ReadInteger(bencodedString, index)
                Case "l"c
                    Return ReadList(bencodedString, index)
                Case "d"c
                    Return ReadDictionary(bencodedString, index)
                Case Else
                    Throw [Error]()
            End Select
        End Function

        Private Function ReadDictionary(ByRef bencodedString As String, ByRef index As Integer) As BDictionary
            index += 1
            Dim dict As BDictionary = New BDictionary()

            Try

                While bencodedString(index) <> "e"c
                    Dim K = ReadString(bencodedString, index)
                    Dim V = ReadElement(bencodedString, index)
                    dict.Add(K, V)
                End While

            Catch __unusedBencodingException1__ As BencodingException
                Throw
            Catch e As Exception
                Throw [Error](e)
            End Try

            index += 1
            Return dict
        End Function

        Private Function ReadList(ByRef bencodedString As String, ByRef index As Integer) As BList
            index += 1
            Dim lst As BList = New BList()

            Try

                While bencodedString(index) <> "e"c
                    lst.Add(ReadElement(bencodedString, index))
                End While

            Catch __unusedBencodingException1__ As BencodingException
                Throw
            Catch e As Exception
                Throw [Error](e)
            End Try

            index += 1
            Return lst
        End Function

        Private Function ReadInteger(ByRef bencodedString As String, ByRef index As Integer) As BInteger
            index += 1
            Dim [end] = bencodedString.IndexOf("e"c, index)
            If [end] = -1 Then Throw [Error]()
            Dim [integer] As Long

            Try
                [integer] = Convert.ToInt64(bencodedString.Substring(index, [end] - index))
                index = [end] + 1
            Catch e As Exception
                Throw [Error](e)
            End Try

            Return New BInteger([integer])
        End Function

        Private Function ReadString(ByRef bencodedString As String, ByRef index As Integer) As BString
            Dim length, colon As Integer

            Try
                colon = bencodedString.IndexOf(":"c, index)
                If colon = -1 Then Throw [Error]()
                length = Convert.ToInt32(bencodedString.Substring(index, colon - index))
            Catch e As Exception
                Throw [Error](e)
            End Try

            index = colon + 1
            Dim tmpIndex = index
            index += length

            Try
                Return New BString(bencodedString.Substring(tmpIndex, length))
            Catch e As Exception
                Throw [Error](e)
            End Try
        End Function

        Private Function [Error](ByVal e As Exception) As Exception
            Return New BencodingException("Bencoded string invalid.", e)
        End Function

        Private Function [Error]() As Exception
            Return New BencodingException("Bencoded string invalid.")
        End Function
    End Module

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
        ''' <paramname="u">The StringBuilder to append to.</param>
        ''' <returns>The bencoded equivalent of the element.</returns>
        Function ToBencodedString(ByVal u As StringBuilder) As StringBuilder
    End Interface

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

    ''' <summary>
    ''' A bencode list.
    ''' </summary>
    Public Class BList
        Inherits List(Of BElement)
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
        ''' <paramname="u">The StringBuilder to append to.</param>
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
        ''' <paramname="value">The specified value.</param>
        Public Sub Add(ByVal value As String)
            MyBase.Add(New BString(value))
        End Sub

        ''' <summary>
        ''' Adds the specified value to the list.
        ''' </summary>
        ''' <paramname="value">The specified value.</param>
        Public Sub Add(ByVal value As Integer)
            MyBase.Add(New BInteger(value))
        End Sub
    End Class

    ''' <summary>
    ''' A bencode dictionary.
    ''' </summary>
    Public Class BDictionary
        Inherits SortedDictionary(Of BString, BElement)
        Implements BElement
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
        ''' <paramname="u">The StringBuilder to append to.</param>
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
        ''' <paramname="key">The specified key.</param>
        ''' <paramname="value">The specified value.</param>
        Public Sub Add(ByVal key As String, ByVal value As BElement)
            MyBase.Add(New BString(key), value)
        End Sub

        ''' <summary>
        ''' Adds the specified key-value pair to the dictionary.
        ''' </summary>
        ''' <paramname="key">The specified key.</param>
        ''' <paramname="value">The specified value.</param>
        Public Sub Add(ByVal key As String, ByVal value As String)
            MyBase.Add(New BString(key), New BString(value))
        End Sub

        ''' <summary>
        ''' Adds the specified key-value pair to the dictionary.
        ''' </summary>
        ''' <paramname="key">The specified key.</param>
        ''' <paramname="value">The specified value.</param>
        Public Sub Add(ByVal key As String, ByVal value As Integer)
            MyBase.Add(New BString(key), New BInteger(value))
        End Sub

        ''' <summary>
        ''' Gets or sets the value assosiated with the specified key.
        ''' </summary>
        ''' <paramname="key">The key of the value to get or set.</param>
        ''' <returns>The value assosiated with the specified key.</returns>
        Default Public Property Item(ByVal key As String) As BElement
            Get
                Return Me(New BString(key))
            End Get
            Set(ByVal value As BElement)
                Me(New BString(key)) = value
            End Set
        End Property
    End Class

    ''' <summary>
    ''' A bencoding exception.
    ''' </summary>
    Public Class BencodingException
        Inherits FormatException
        ''' <summary>
        ''' Creates a new BencodingException.
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Creates a new BencodingException.
        ''' </summary>
        ''' <paramname="message">The message.</param>
        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub

        ''' <summary>
        ''' Creates a new BencodingException.
        ''' </summary>
        ''' <paramname="message">The message.</param>
        ''' <paramname="inner">The inner exception.</param>
        Public Sub New(ByVal message As String, ByVal inner As Exception)
            MyBase.New(message, inner)
        End Sub
    End Class

    ''' <summary>
    ''' A class with extension methods for use with Bencoding.
    ''' </summary>
    Public Module BencodingExtensions
        ''' <summary>
        ''' Decode the current instance.
        ''' </summary>
        ''' <paramname="s">The current instance.</param>
        ''' <returns>The root elements of the decoded string.</returns>
        <Extension()>
        Public Function BDecode(ByVal s As String) As BElement()
            Return Decode(s)
        End Function
    End Module
End Namespace
