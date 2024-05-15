#Region "Microsoft.VisualBasic::2847cc47d8661f7df702f6cafc16e335, Microsoft.VisualBasic.Core\src\Language\Language\Java\StringTokenizer.vb"

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

    '   Total Lines: 34
    '    Code Lines: 26
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 925 B


    '     Class StringTokenizer
    ' 
    '         Properties: countTokens
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: nextToken, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Language.Java

    Public Class StringTokenizer

        ReadOnly rawText As String
        ReadOnly sep As String
        ReadOnly tokens As String()

        Dim i As i32 = 0

        Public ReadOnly Property countTokens As Integer
            Get
                Return tokens.Length
            End Get
        End Property

        Public Sub New(text As String, sep As String)
            Me.rawText = text
            Me.sep = sep
            Me.tokens = text.StringSplit(sep)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function nextToken() As String
            Return tokens.ElementAtOrDefault(++i)
        End Function

        Public Overrides Function ToString() As String
            Return $"{countTokens} tokens from '{rawText}'"
        End Function
    End Class
End Namespace
