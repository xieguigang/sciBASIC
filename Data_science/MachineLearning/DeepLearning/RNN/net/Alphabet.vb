#Region "Microsoft.VisualBasic::3750329b1404e1098a207e8d39fa46aa, Data_science\MachineLearning\DeepLearning\RNN\net\Alphabet.vb"

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

'   Total Lines: 81
'    Code Lines: 53 (65.43%)
' Comment Lines: 12 (14.81%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 16 (19.75%)
'     File Size: 2.32 KB


' 	Class Alphabet
' 
' 	    Constructor: (+1 Overloads) Sub New
' 	    Function: charsToIndices, charToIndex, fromString, indexToChar, indicesToChars
'                size
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace RNN

	''' <summary>
	''' Immutable set of symbols mapped indices.
	''' </summary>
	Public Class Alphabet

		Private m_indexToChar As Char()
		Private m_charToIndex As Dictionary(Of Char, Integer)

		''' <summary>
		''' Constructs an alphabet containing symbols extracted from the string.
		''' Treats null as an empty string.
		''' </summary>
		''' <param name="data"></param>
		Private Sub New(data As String)
			If data Is Nothing Then
				m_indexToChar = New Char(-1) {}
				m_charToIndex = New Dictionary(Of Char, Integer)()
				Return
			End If

			' find the alphabet
			Dim chars As SortedSet(Of Char) = New SortedSet(Of Char)()
			m_charToIndex = New Dictionary(Of Char, Integer)()
			Dim i = 0

			For i = 0 To data.Length - 1
				chars.Add(data(i))
			Next

			m_indexToChar = New Char(chars.Count - 1) {}

			i = 0
			For Each c As Char In chars
				m_indexToChar(i) = c
				m_charToIndex(c) = System.Math.Min(Threading.Interlocked.Increment(i), i - 1)
			Next
		End Sub

		''' <summary>
		''' Returns alphabet containing symbols extracted from the string.
		''' Treats null as an empty string.
		''' </summary>
		''' <param name="data"></param>
		''' <returns></returns>
		''' 
		<MethodImpl(MethodImplOptions.AggressiveInlining)>
		Public Shared Function fromString(data As String) As Alphabet
			Return New Alphabet(data)
		End Function

		''' <summary>
		''' Returns the alphabet size.
		''' </summary>
		''' <returns></returns>
		''' 
		<MethodImpl(MethodImplOptions.AggressiveInlining)>
		Public Overridable Function size() As Integer
			Return m_indexToChar.Length
		End Function

		''' <summary>
		''' Converts a character to the corresponding index.
		''' </summary>
		''' <param name="c"></param>
		''' <returns></returns>
		Public Overridable Function charToIndex(c As Char) As Integer
			Dim index As Integer = m_charToIndex(c)
			Return index
		End Function

		''' <summary>
		''' Converts an index to the corresponding character.
		''' Index must be an index returned by charToIndex.
		''' </summary>
		''' <param name="index"></param>
		''' <returns></returns>
		''' 
		<MethodImpl(MethodImplOptions.AggressiveInlining)>
		Public Overridable Function indexToChar(index As Integer) As Char
			Return m_indexToChar(index)
		End Function

		''' <summary>
		''' Converts all indices to chars using indexToChar.
		''' </summary>
		''' <param name="indices"></param>
		''' <returns></returns>
		Public Overridable Function indicesToChars(indices As Integer()) As Char()
			Dim out = New Char(indices.Length - 1) {}

			For i = 0 To indices.Length - 1
				out(i) = indexToChar(indices(i))
			Next

			Return out
		End Function

		''' <summary>
		''' Converts the string to indices using charToIndex.
		''' </summary>
		''' <param name="chars"></param>
		''' <returns></returns>
		Public Overridable Function charsToIndices(chars As String) As Integer()
			Dim out = New Integer(chars.Length - 1) {}

			For i = 0 To chars.Length - 1
				out(i) = charToIndex(chars(i))
			Next

			Return out
		End Function
	End Class
End Namespace
