#Region "Microsoft.VisualBasic::23d1bdb2dbdeb520c5df853d573c2b72, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Text\StringSimilarity\Abstract.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace Text.Similarity

    ''' <summary>
    ''' Summary description for StringMatcher.
    ''' </summary>
    ''' 
    Public Delegate Function Similarity(s1 As String, s2 As String) As Single

    ''' <summary>
    ''' Summary description for IEditDistance.
    ''' </summary>
    Interface ISimilarity
        Function GetSimilarity(string1 As String, string2 As String) As Single
    End Interface
End Namespace
