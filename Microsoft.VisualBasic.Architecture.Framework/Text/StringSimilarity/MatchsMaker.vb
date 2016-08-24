#Region "Microsoft.VisualBasic::9243e3cfb21a30a38d5ec2f30d68f057, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Text\StringSimilarity\MatchsMaker.vb"

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

'
'Matching two strings
'Author: Thanh Ngoc Dao - Thanh.dao@gmx.net
'Copyright (c) 2005 by Thanh Ngoc Dao, All rights reserved.
'---------
'Thanks to Malcolm Crowe,Troy Simpson, Jeff Martin for the .NET WordNet port
'Thanks to Carl Mercier for his french characters conversion function.
'
'Please test carefully before using.
'

Imports System.Collections
Imports System.Text.RegularExpressions
Imports System.Diagnostics

Namespace Text.Similarity

    Public Class StringSimilarityMatchs
        Private _lString As String, _rString As String
        Private _leftTokens As String(), _rightTokens As String()
        Private leftLen As Integer, rightLen As Integer
        Private cost As Single(,)
        Private getSimilarity As Similarity

        Private _accentInsensitive As Boolean

        Public Sub New(left As String, right As String)
            Me.New(left, right, False)
        End Sub
        Public Sub New(left As String, right As String, accentInsensitive As Boolean)
            _accentInsensitive = accentInsensitive

            _lString = left
            _rString = right

            If _accentInsensitive Then
                _lString = StripAccents(_lString)
                _rString = StripAccents(_rString)
            End If

            MyInit()
        End Sub

        Const beforeConversion As String = "嗬饴淠崃樯枞晔胨焯钗锵蛞粼鲋缜採"
        Const afterConversion As String = "aAaAaAaAeEeEeEeEiIiIiIoOoOoOuUuUuUcC'n"

        Private Function StripAccents(input As String) As String
            Dim sb As New System.Text.StringBuilder(input)

            For i As Integer = 0 To beforeConversion.Length - 1
                Dim beforeChar As Char = beforeConversion(i)
                Dim afterChar As Char = afterConversion(i)

                sb.Replace(beforeChar, afterChar)
            Next

            sb.Replace("?", "oe")
            sb.Replace("?", "ae")

            Return sb.ToString()
        End Function

        Private Sub MyInit()
            Dim editdistance As ISimilarity = New Leven()
            getSimilarity = New Similarity(AddressOf editdistance.GetSimilarity)

            'ISimilarity lexical=new LexicalSimilarity() ;
            'getSimilarity=new Similarity(lexical.GetSimilarity) ;

            _leftTokens = Tokeniser.Partition(_lString)
            _rightTokens = Tokeniser.Partition(_rString)
            If _leftTokens.Length > _rightTokens.Length Then
                Dim tmp As String() = _leftTokens
                _leftTokens = _rightTokens
                _rightTokens = tmp
                Dim s As String = _lString
                _lString = _rString
                _rString = s
            End If

            leftLen = _leftTokens.Length - 1
            rightLen = _rightTokens.Length - 1
            Initialize()

        End Sub

        Private Sub Initialize()
            cost = New Single(leftLen, rightLen) {}
            For i As Integer = 0 To leftLen
                For j As Integer = 0 To rightLen
                    cost(i, j) = getSimilarity(_leftTokens(i), _rightTokens(j))
                Next
            Next
        End Sub


        Public Function GetScore() As Single
            Dim match As New BipartiteMatcher(_leftTokens, _rightTokens, cost)
            Return match.Score
        End Function


        Public ReadOnly Property Score() As Single
            Get
                Return GetScore()
            End Get
        End Property

    End Class
End Namespace
