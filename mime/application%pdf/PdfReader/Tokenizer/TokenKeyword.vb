#Region "Microsoft.VisualBasic::8611412301a9433a9fc30ad1e86addf4, mime\application%pdf\PdfReader\Tokenizer\TokenKeyword.vb"

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

    '   Total Lines: 43
    '    Code Lines: 36 (83.72%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (16.28%)
    '     File Size: 1.54 KB


    '     Class TokenKeyword
    ' 
    '         Properties: Value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetToken
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.ComponentModel

Namespace PdfReader
    Public Class TokenKeyword
        Inherits TokenObject

        Private _Value As PdfReader.ParseKeyword
        Private Shared _lookup As Dictionary(Of String, TokenKeyword)

        Shared Sub New()
            _lookup = New Dictionary(Of String, TokenKeyword)()

            For Each val As Object In [Enum].GetValues(GetType(ParseKeyword))
                Dim name = [Enum].GetName(GetType(ParseKeyword), val)
                Dim keyword = name
                Dim attrs = GetType(ParseKeyword).GetMember(name)(0).GetCustomAttributes(GetType(DescriptionAttribute), False)
                If attrs IsNot Nothing AndAlso attrs.Length > 0 Then keyword = CType(attrs(0), DescriptionAttribute).Description
                Call _lookup.Add(keyword, New TokenKeyword(val))
            Next
        End Sub

        Public Sub New(keyword As ParseKeyword)
            Value = keyword
        End Sub

        Public Property Value As ParseKeyword
            Get
                Return _Value
            End Get
            Private Set(value As ParseKeyword)
                _Value = value
            End Set
        End Property

        Public Shared Function GetToken(keyword As String) As TokenKeyword
            Dim tokenKeyword As TokenKeyword = Nothing
            _lookup.TryGetValue(keyword, tokenKeyword)
            Return tokenKeyword
        End Function
    End Class
End Namespace
