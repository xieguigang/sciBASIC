#Region "Microsoft.VisualBasic::8e609cc4628ddde42a5d217fd7e81775, Microsoft.VisualBasic.Core\src\Serialization\BEncoding\BElements\BDictionary.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 80
    '    Code Lines: 38
    ' Comment Lines: 32
    '   Blank Lines: 10
    '     File Size: 3.10 KB


    '     Class BDictionary
    ' 
    '         Function: (+2 Overloads) ToBencodedString
    ' 
    '         Sub: (+3 Overloads) Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace Serialization.Bencoding

    ''' <summary>
    ''' A bencode dictionary.
    ''' </summary>
    Public Class BDictionary : Inherits SortedDictionary(Of BString, BElement)
        Implements BElement

        ''' <summary>
        ''' Gets or sets the value assosiated with the specified key.
        ''' </summary>
        ''' <param name="key">The key of the value to get or set.</param>
        ''' <returns>The value assosiated with the specified key.</returns>
        Default Public Overloads Property Item(key As String) As BElement
            Get
                Return Me(New BString(key))
            End Get
            Set(value As BElement)
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
        Public Function ToBencodedString(u As StringBuilder) As StringBuilder Implements BElement.ToBencodedString
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
        Public Overloads Sub Add(key As String, value As BElement)
            MyBase.Add(New BString(key), value)
        End Sub

        ''' <summary>
        ''' Adds the specified key-value pair to the dictionary.
        ''' </summary>
        ''' <param name="key">The specified key.</param>
        ''' <param name="value">The specified value.</param>
        Public Overloads Sub Add(key As String, value As String)
            MyBase.Add(New BString(key), New BString(value))
        End Sub

        ''' <summary>
        ''' Adds the specified key-value pair to the dictionary.
        ''' </summary>
        ''' <param name="key">The specified key.</param>
        ''' <param name="value">The specified value.</param>
        Public Overloads Sub Add(key As String, value As Integer)
            MyBase.Add(New BString(key), New BInteger(value))
        End Sub
    End Class
End Namespace
