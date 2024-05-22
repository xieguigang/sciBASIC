#Region "Microsoft.VisualBasic::441610f0aca2e5e816c9c303b18c4490, Microsoft.VisualBasic.Core\src\Language\Value\DefaultValue\DefaultExtensions.vb"

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

    '   Total Lines: 86
    '    Code Lines: 59 (68.60%)
    ' Comment Lines: 18 (20.93%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (10.47%)
    '     File Size: 3.64 KB


    '     Module DefaultExtensions
    ' 
    '         Function: BaseName, FileExists, NormalizePathString, Replace, Split
    '                   ToLower, TrimSuffix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Text

Namespace Language.Default

    Public Module DefaultExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToLower(str As [Default](Of String)) As String
            Return Strings.LCase(str.value)
        End Function

        <Extension>
        Public Function FileExists(str As DefaultString, Optional zeroLenAsNonExists As Boolean = True) As Boolean
            If Not str.DefaultValue.FileExists Then
                Return False
            ElseIf zeroLenAsNonExists AndAlso str.DefaultValue.FileLength <= 0 Then
                Return False
            Else
                Return True
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
        Public Function Split(str As DefaultString,
                              Optional deli$ = " ",
                              Optional ignoreCase As Boolean = False,
                              Optional regexp As Boolean = False) As String()
            If regexp Then
                Return str _
                    .DefaultValue _
                    .StringSplit(
                        pattern:=deli,
                        opt:=If(ignoreCase, RegexICSng, RegexOptions.Singleline)
                    )
            Else
                Return Splitter.Split(str.DefaultValue, deli, True, compare:=StringHelpers.IgnoreCase(flag:=ignoreCase))
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Replace(str As DefaultString, find$, replaceAs$) As String
            Return str.DefaultValue.Replace(find, replaceAs)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function BaseName(path As DefaultString) As String
            Return path.DefaultValue.BaseName
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function TrimSuffix(path As DefaultString) As String
            Return path.DefaultValue.TrimSuffix
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function NormalizePathString(path As DefaultString, Optional alphabetOnly As Boolean = True) As String
            Return path.DefaultValue.NormalizePathString(alphabetOnly)
        End Function
    End Module
End Namespace
