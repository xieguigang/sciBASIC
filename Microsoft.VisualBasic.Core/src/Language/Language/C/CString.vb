#Region "Microsoft.VisualBasic::46224d76a9fc50fe95ba7935ae3270f8, Microsoft.VisualBasic.Core\src\Language\Language\C\CString.vb"

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

    '   Total Lines: 215
    '    Code Lines: 109 (50.70%)
    ' Comment Lines: 76 (35.35%)
    '    - Xml Docs: 72.37%
    ' 
    '   Blank Lines: 30 (13.95%)
    '     File Size: 8.33 KB


    '     Module CString
    ' 
    '         Function: ChangeCharacter, Decode, IsXDigit, StrChr, StrRChr
    '                   StrStr, StrTok
    '         Structure __tokensHelper
    ' 
    '             Function: StrTok
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'----------------------------------------------------------------------------------------
'	Copyright © 2006 - 2012 Tangible Software Solutions Inc.
'	This class can be used by anyone provided that the copyright notice remains intact.
'
'	This class provides the ability to simulate various classic C string functions
'	which don't have exact equivalents in the .NET Framework.
'----------------------------------------------------------------------------------------

Imports System.Runtime.CompilerServices

Namespace Language.C

    ''' <summary>
    ''' This class provides the ability to simulate various classic C string functions
    '''	which don't have exact equivalents in the .NET Framework.
    ''' </summary>
    Public Module CString

        ''' <summary>
        ''' decode of the meta char inside the given format string
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Decode(s As String) As String
            If s.StringEmpty Then
                Return s
            Else
                s = s.Replace("\U", "\u").Replace("\A", "\a")
            End If

            Try
                ' Hex Unicode \u0000
                Do
                    Dim i = s.IndexOf("\u")

                    If i = -1 Then
                        Exit Do
                    End If

                    Dim u = s.Substring(i, 6)
                    Dim n = Convert.ToInt16(u.Replace("\u", ""), 16)

                    s = s.Replace(u, ChrW(n))
                Loop

                ' Decimal ASCII \a000
                Do
                    Dim i = s.IndexOf("\a")

                    If i = -1 Then
                        Exit Do
                    End If

                    Dim a = s.Substring(i, 5)
                    Dim n = CByte(a.Replace("\a", ""))

                    s = s.Replace(a, Strings.Chr(n))
                Loop

            Catch ex As Exception
                Throw New Exception("bad format")
            End Try

            Return s
        End Function

        ''' <summary>
        ''' This method allows replacing a single character in a string, to help convert
        ''' C++ code where a single character in a character array is replaced.
        ''' </summary>
        ''' <param name="sourcestring"></param>
        ''' <param name="charindex"></param>
        ''' <param name="changechar"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function ChangeCharacter(sourcestring As String, charindex As Integer, changechar As Char) As String
            Return (If(charindex > 0, sourcestring.Substring(0, charindex), "")) & changechar.ToString() & (If(charindex < sourcestring.Length - 1, sourcestring.Substring(charindex + 1), ""))
        End Function

        ''' <summary>
        ''' This method simulates the classic C string function 'isxdigit' (and 'iswxdigit').
        ''' </summary>
        ''' <param name="character"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function IsXDigit(character As Char) As Boolean
            If Char.IsDigit(character) Then
                Return True
            ElseIf "ABCDEFabcdef".IndexOf(character) > -1 Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' This method simulates the classic C string function 'strchr' (and 'wcschr').
        ''' </summary>
        ''' <param name="stringtosearch"></param>
        ''' <param name="chartofind"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function StrChr(stringtosearch As String, chartofind As Char) As String
            Dim index As Integer = stringtosearch.IndexOf(chartofind)
            If index > -1 Then
                Return stringtosearch.Substring(index)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' This method simulates the classic C string function 'strrchr' (and 'wcsrchr').
        ''' </summary>
        ''' <param name="stringtosearch"></param>
        ''' <param name="chartofind"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function StrRChr(stringtosearch As String, chartofind As Char) As String
            Dim index As Integer = stringtosearch.LastIndexOf(chartofind)
            If index > -1 Then
                Return stringtosearch.Substring(index)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' This method simulates the classic C string function 'strstr' (and 'wcsstr').
        ''' </summary>
        ''' <param name="stringtosearch"></param>
        ''' <param name="stringtofind"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function StrStr(stringtosearch As String, stringtofind As String) As String
            Dim index As Integer = stringtosearch.IndexOf(stringtofind)
            If index > -1 Then
                Return stringtosearch.Substring(index)
            Else
                Return Nothing
            End If
        End Function

        Private Structure __tokensHelper

            Private activestring As String
            Private activeposition As Integer

            ''' <summary>
            ''' This method simulates the classic C string function 'strtok' (and 'wcstok').
            ''' Note that the .NET string 'Split' method cannot be used to simulate 'strtok' since
            ''' it doesn't allow changing the delimiters between each token retrieval.
            ''' </summary>
            ''' <param name="stringtotokenize"></param>
            ''' <param name="delimiters"></param>
            ''' <returns></returns>
            Public Function StrTok(stringtotokenize As String, delimiters As String) As String
                If stringtotokenize IsNot Nothing Then
                    activestring = stringtotokenize
                    activeposition = -1
                End If

                'the stringtotokenize was never set:
                If activestring Is Nothing Then
                    Return Nothing
                End If

                'all tokens have already been extracted:
                If activeposition = activestring.Length Then
                    Return Nothing
                End If

                'bypass delimiters:
                activeposition += 1
                While activeposition < activestring.Length AndAlso delimiters.IndexOf(activestring(activeposition)) > -1
                    activeposition += 1
                End While

                'only delimiters were left, so return null:
                If activeposition = activestring.Length Then
                    Return Nothing
                End If

                'get starting position of string to return:
                Dim startingposition As Integer = activeposition

                'read until next delimiter:
                Do
                    activeposition += 1
                Loop While activeposition < activestring.Length AndAlso delimiters.IndexOf(activestring(activeposition)) = -1

                Return activestring.Substring(startingposition, activeposition - startingposition)
            End Function
        End Structure

        ''' <summary>
        ''' This method simulates the classic C string function 'strtok' (and 'wcstok').
        ''' Note that the .NET string 'Split' method cannot be used to simulate 'strtok' since
        ''' it doesn't allow changing the delimiters between each token retrieval.
        ''' </summary>
        ''' <param name="stringtotokenize"></param>
        ''' <param name="delimiters"></param>
        ''' <returns></returns>
        <Extension>
        Public Function StrTok(stringtotokenize As String, delimiters As String) As String
            Return New __tokensHelper().StrTok(stringtotokenize, delimiters)
        End Function
    End Module
End Namespace
