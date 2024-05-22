#Region "Microsoft.VisualBasic::c1729fa951c74553a42dd0d12919398d, Data\Trinity\POSTagger\PartOfSpeech.vb"

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

    '   Total Lines: 21
    '    Code Lines: 17 (80.95%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (19.05%)
    '     File Size: 866 B


    '     Class PartOfSpeech
    ' 
    '         Properties: Tag, Word
    ' 
    '         Function: (+2 Overloads) Equals, GetHashCode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace POSTagger
    Public Class PartOfSpeech
        Public Property Word As String
        Public Property Tag As String

        Private Overloads Function Equals(other As PartOfSpeech) As Boolean
            Return Equals(Word, other.Word) AndAlso Equals(Tag, other.Tag)
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If ReferenceEquals(Nothing, obj) Then Return False
            If ReferenceEquals(Me, obj) Then Return True

            Return obj.GetType() Is [GetType]() AndAlso Equals(CType(obj, PartOfSpeech))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return ((If(Not Equals(Word, Nothing), Word.GetHashCode(), 0)) * 397) Xor (If(Not Equals(Tag, Nothing), Tag.GetHashCode(), 0))
        End Function
    End Class
End Namespace
