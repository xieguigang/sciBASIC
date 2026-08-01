#Region "Microsoft.VisualBasic::4d269aac013502f8dd76dd5465580275, Data_science\NLP\LDA\Vocabulary.vb"

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

    '   Total Lines: 88
    '    Code Lines: 62 (70.45%)
    ' Comment Lines: 5 (5.68%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 21 (23.86%)
    '     File Size: 2.52 KB


    '     Class Vocabulary
    ' 
    '         Properties: size
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) getId, getWord, ToString
    ' 
    '         Sub: loseWeight, resize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text

Namespace LDA

    ''' <summary>
    ''' a word set mapping between the word and its index id value
    ''' 
    ''' @author hankcs
    ''' </summary>
    Public Class Vocabulary

        Dim word2idMap As IDictionary(Of String, Integer?)
        Dim id2wordMap As String()

        Public ReadOnly Property size() As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return word2idMap.Count
            End Get
        End Property

        Public Sub New()
            word2idMap = New SortedDictionary(Of String, Integer?)()
            id2wordMap = New String(1023) {}
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function getId(word As String) As Integer?
            Return getId(word, False)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function getWord(id As Integer) As String
            Return id2wordMap(id)
        End Function

        Public Overridable Function getId(word As String, create As Boolean) As Integer?
            Dim id = word2idMap.GetValueOrNull(word)

            If Not create Then
                Return id
            End If

            If id Is Nothing Then
                id = word2idMap.Count
            End If

            word2idMap(word) = id

            If id2wordMap.Length - 1 < id Then
                Call resize(word2idMap.Count * 2)
            End If

            id2wordMap(id) = word

            Return id
        End Function

        Private Sub resize(n As Integer)
            Dim nArray = New String(n - 1) {}
            Array.Copy(id2wordMap, 0, nArray, 0, id2wordMap.Length)
            id2wordMap = nArray
        End Sub

        Public Sub loseWeight()
            If size() = id2wordMap.Length Then
                Return
            End If

            Call resize(word2idMap.Count)
        End Sub

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder()

            For i As Integer = 0 To id2wordMap.Length - 1
                If id2wordMap(i) Is Nothing Then
                    Exit For
                End If

                sb.Append(i).Append("=").Append(id2wordMap(i)).Append(vbLf)
            Next

            Return sb.ToString()
        End Function
    End Class
End Namespace
