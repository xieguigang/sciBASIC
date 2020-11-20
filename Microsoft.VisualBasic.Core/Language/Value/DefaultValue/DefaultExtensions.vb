#Region "Microsoft.VisualBasic::4091dd747b2b333e672287cd8d132f88, Microsoft.VisualBasic.Core\Language\Value\DefaultValue\DefaultExtensions.vb"

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

    '     Module DefaultExtensions
    ' 
    '         Function: BaseName, FileExists, NormalizePathString, Replace, Split
    '                   ToLower, TrimSuffix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Split(str As DefaultString, deli$, Optional ignoreCase As Boolean = False) As String()
            Return Splitter.Split(str.DefaultValue, deli, True, compare:=StringHelpers.IgnoreCase(flag:=ignoreCase))
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
