#Region "Microsoft.VisualBasic::ee0cfefb25723f10902ee6cee6b060a5, mime\application%pdf\PdfReader\Document\PdfContents.vb"

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

    '   Total Lines: 49
    '    Code Lines: 40
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.65 KB


    '     Class PdfContents
    ' 
    '         Properties: Streams
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CreateParser
    ' 
    '         Sub: ResolveToStreams, Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace PdfReader

    Public Class PdfContents
        Inherits PdfObject

        Private _Streams As List(Of PdfStream)

        Public Sub New(parent As PdfObject, obj As PdfObject)
            MyBase.New(parent)
            Streams = New List(Of PdfStream)()
            ResolveToStreams(obj)
        End Sub

        Public Property Streams As List(Of PdfStream)
            Get
                Return _Streams
            End Get
            Private Set(value As List(Of PdfStream))
                _Streams = value
            End Set
        End Property

        Public Overrides Sub Visit(visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public Function CreateParser() As PdfContentsParser
            Return New PdfContentsParser(Me)
        End Function

        Private Sub ResolveToStreams(obj As PdfObject)
            Dim stream As New Value(Of PdfStream)
            Dim reference As New Value(Of PdfObjectReference)
            Dim array As New Value(Of PdfArray)

            If (stream = TryCast(obj, PdfStream)) IsNot Nothing Then
                Streams.Add(stream)
            ElseIf (reference = TryCast(obj, PdfObjectReference)) IsNot Nothing Then
                ResolveToStreams(Document.ResolveReference(reference))
            ElseIf (array = TryCast(obj, PdfArray)) IsNot Nothing Then
                For Each entry As PdfObject In CType(array, PdfArray).Objects
                    ResolveToStreams(entry)
                Next
            End If
        End Sub
    End Class
End Namespace
