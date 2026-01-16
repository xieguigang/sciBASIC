#Region "Microsoft.VisualBasic::4ef819db819e4dced64a93bc199b629a, Microsoft.VisualBasic.Core\src\Language\StringHelpers.vb"

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

    '   Total Lines: 149
    '    Code Lines: 66 (44.30%)
    ' Comment Lines: 67 (44.97%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 16 (10.74%)
    '     File Size: 6.21 KB


    '     Module FormatHelpers
    ' 
    '         Function: (+2 Overloads) Split, StartsWith, StringFormat, Trim, xFormat
    ' 
    '     Structure FormatHelper
    ' 
    '         Function: ToString
    '         Operators: (+2 Overloads) <=, (+2 Overloads) >=
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Language.[Default]
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Language

    ''' <summary>
    ''' ``<see cref="sprintf"/>`` syntax helpers
    ''' </summary>
    Public Module FormatHelpers

        <Extension>
        Public Function LoadJSON(Of T)(json As Value(Of String)) As T
            If json Is Nothing OrElse json.Value.StringEmpty(, True) Then
                Return Nothing
            Else
                Return CStr(json).LoadJSON(Of T)(throwEx:=False)
            End If
        End Function

        ''' <summary>
        ''' Determines whether the beginning of this string instance matches the specified
        ''' string.
        ''' </summary>
        ''' <param name="str"></param>
        ''' <param name="c">The string to compare.</param>
        ''' <returns>true if value matches the beginning of this string; otherwise, false.</returns>
        <Extension>
        Public Function StartsWith(str As Value(Of String), c As Char) As Boolean
            If str Is Nothing OrElse str.Value Is Nothing Then
                Return False
            Else
                Return str.Value.StartsWith(c)
            End If
        End Function

        ''' <summary>
        ''' Removes all leading and trailing occurrences of a set of characters specified
        ''' in an array from the current string.
        ''' </summary>
        ''' <param name="str"></param>
        ''' <param name="c">An array of Unicode characters to remove, or null.</param>
        ''' <returns>
        ''' The string that remains after all occurrences of the characters in the trimChars
        ''' parameter are removed from the start and end of the current string. If trimChars
        ''' is null or an empty array, white-space characters are removed instead. If no
        ''' characters can be trimmed from the current instance, the method returns the current
        ''' instance unchanged.
        ''' </returns>
        <Extension>
        Public Function Trim(str As Value(Of String), ParamArray c As Char()) As String
            If str Is Nothing OrElse str.Value Is Nothing Then
                Return ""
            Else
                Return str.Value.Trim(c)
            End If
        End Function

        ''' <summary>
        ''' Returns a zero-based, one-dimensional array containing a specified number of
        ''' substrings.
        ''' </summary>
        ''' <param name="str">Required. String expression containing substrings And delimiters.</param>
        ''' <param name="deli">
        ''' Optional. Any single character used to identify substring limits. If Delimiter
        ''' Is omitted, the space character (" ") Is assumed to be the delimiter.
        ''' </param>
        ''' <param name="ignoreCase"></param>
        ''' <param name="regexp"></param>
        ''' <returns>
        ''' String array. If Expression Is a zero-length string (""), 
        ''' Split returns a single-element array containing a zero-length 
        ''' string. If Delimiter Is a zero-length string, Or if it does 
        ''' Not appear anywhere in Expression, Split returns a single-element
        ''' array containing the entire Expression string.
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Split(str As Value(Of String),
                              Optional deli$ = " ",
                              Optional ignoreCase As Boolean = False,
                              Optional regexp As Boolean = False) As String()

            Return New DefaultString(str.Value).Split(deli, ignoreCase, regexp)
        End Function

        ''' <summary>
        ''' Splits a string into substrings based on a specified delimiting character and,
        ''' optionally, options.
        ''' </summary>
        ''' <param name="str"></param>
        ''' <param name="deli">A character that delimits the substrings in this string.</param>
        ''' <param name="options">
        ''' A bitwise combination of the enumeration values that specifies whether to trim
        ''' substrings and include empty substrings.
        ''' </param>
        ''' <returns>An array whose elements contain the substrings from this instance that are delimited
        ''' by separator.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Split(str As Value(Of String), deli As Char, Optional options As StringSplitOptions = StringSplitOptions.None) As String()
            If str.Value Is Nothing Then
                Return {}
            Else
                Return str.Value.Split(deli)
            End If
        End Function

        ''' <summary>
        ''' ``<see cref="sprintf"/>`` extensions
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        <Extension>
        Public Function xFormat(s As String) As FormatHelper
            Return New FormatHelper With {.source = s}
        End Function

        ''' <summary>
        ''' Synax like ``"formats" &lt;= {args}.xFormat`` 
        ''' Format by <see cref="sprintf"/>
        ''' </summary>
        ''' <param name="args"></param>
        ''' <returns></returns>
        <Extension>
        Public Function StringFormat(args As String()) As FormatHelper
            Return New FormatHelper With {.args = args}
        End Function
    End Module

    ''' <summary>
    ''' ``<see cref="sprintf"/>`` reference
    ''' </summary>
    Public Structure FormatHelper

        Dim source$, args$()

        Public Overrides Function ToString() As String
            Return source
        End Function

        Public Shared Operator <=(pattern As String, format As FormatHelper) As String
            Return sprintf(pattern, format.args)
        End Operator

        Public Shared Operator >=(pattern As String, format As FormatHelper) As String
            Throw New NotSupportedException
        End Operator

        Public Shared Operator <=(format As FormatHelper, args As String()) As String
            Return sprintf(format.source, args)
        End Operator

        Public Shared Operator >=(format As FormatHelper, args As String()) As String
            Throw New NotSupportedException
        End Operator
    End Structure
End Namespace
