#Region "Microsoft.VisualBasic::efa3380e71345e890f3ab365280cc513, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Text\StringSimilarity\Levenshtein\LevenshteinString.vb"

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

Namespace Text.Levenshtein

    ''' <summary>
    ''' The string like helper
    ''' </summary>
    Public Structure LevenshteinString

        Private _string$
        Private _chars%()

        Sub New(s$)
            _string = s
            _chars = s _
                .Select(AddressOf AscW) _
                .ToArray
        End Sub

        Public Overrides Function ToString() As String
            Return _string
        End Function

        Public Shared Operator Like(s$, subject As LevenshteinString) As DistResult
            Return Levenshtein.ComputeDistance(
                s.CharCodes,
                subject._chars,
                Function(a, b) a = b,
                AddressOf ChrW)
        End Operator

        Public Shared Operator Like(query As LevenshteinString, s$) As DistResult
            Return Levenshtein.ComputeDistance(
                query._chars,
                s.CharCodes,
                Function(a, b) a = b,
                AddressOf ChrW)
        End Operator

        Public Shared Operator Like(query As LevenshteinString, subject As LevenshteinString) As DistResult
            Return Levenshtein.ComputeDistance(
                query._chars,
                subject._chars,
                Function(a, b) a = b,
                AddressOf ChrW)
        End Operator

        Public Shared Narrowing Operator CType(s As LevenshteinString) As String
            Return s._string
        End Operator

        Public Shared Widening Operator CType(s$) As LevenshteinString
            Return New LevenshteinString With {
                ._string = s,
                ._chars = s.Select(AddressOf AscW).ToArray
            }
        End Operator
    End Structure
End Namespace
