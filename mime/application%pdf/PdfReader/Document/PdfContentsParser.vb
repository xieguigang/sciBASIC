#Region "Microsoft.VisualBasic::71b14357a52e636a760a8e03156f3762, mime\application%pdf\PdfReader\Document\PdfContentsParser.vb"

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

    '     Class PdfContentsParser
    ' 
    '         Properties: Contents
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetObject, Text
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.IO
Imports Microsoft.VisualBasic.Language
Imports stdNum = System.Math

Namespace PdfReader
    Public Class PdfContentsParser
        Inherits PdfObject

        Private _index As Integer = 0
        Private _parser As Parser

        Public Sub New(ByVal parent As PdfContents)
            MyBase.New(parent)
        End Sub

        Public ReadOnly Property Contents As PdfContents
            Get
                Return TypedParent(Of PdfContents)()
            End Get
        End Property

        Public Function GetObject() As PdfObject
            ' First time around we setup the parser to the first stream
            If _parser Is Nothing AndAlso _index < Contents.Streams.Count Then
                _parser = New Parser(New MemoryStream(Contents.Streams(stdNum.Min(Threading.Interlocked.Increment(_index), _index - 1)).ValueAsBytes), True)
            End If

            ' Keep trying to get a parsed object as long as there is a parser for a stream
            While _parser IsNot Nothing
                Dim obj = _parser.ParseObject(True)
                If obj IsNot Nothing Then Return WrapObject(obj)
                _parser.Dispose()
                _parser = Nothing

                ' Is there another stream we can continue parsing with
                If _index < Contents.Streams.Count Then _parser = New Parser(New MemoryStream(Contents.Streams(stdNum.Min(Threading.Interlocked.Increment(_index), _index - 1)).ValueAsBytes), True)
            End While

            Return Nothing
        End Function

        ''' <summary>
        ''' get all text contents in current PDF page
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function Text() As IEnumerable(Of String)
            ' Keep getting new content commands until no more left
            Dim obj As New Value(Of PdfObject)

            While (obj = GetObject()) IsNot Nothing
                If obj.GetUnderlyingType() Is GetType(PdfArray) Then
                    Yield CType(obj, PdfArray).GetAllTextContent
                End If
            End While
        End Function
    End Class
End Namespace
