#Region "Microsoft.VisualBasic::3b2e2b65dc5c784a45777ba9333eba88, mime\application%pdf\PdfReader\Document\PdfDocument.vb"

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

    '   Total Lines: 273
    '    Code Lines: 203 (74.36%)
    ' Comment Lines: 20 (7.33%)
    '    - Xml Docs: 25.00%
    ' 
    '   Blank Lines: 50 (18.32%)
    '     File Size: 10.92 KB


    '     Class PdfDocument
    ' 
    '         Properties: Catalog, DecryptHandler, IndirectObjects, Info, Version
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+4 Overloads) ResolveReference
    ' 
    '         Sub: BackgroundResolveReference, Close, (+2 Overloads) Load, Parser_ResolveReference, Visit
    '         Class BackgroundArgs
    ' 
    '             Properties: Count, Ids, Index, Parser
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.IO
Imports System.Linq
Imports System.Threading

Namespace PdfReader

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/ComponentFactory/PdfReader
    ''' </remarks>
    Public Class PdfDocument
        Inherits PdfObject

        Private _Version As PdfReader.PdfVersion, _IndirectObjects As PdfReader.PdfIndirectObjects, _DecryptHandler As PdfReader.PdfDecrypt

        Private Class BackgroundArgs
            Public Property Parser As Parser
            Public Property Ids As List(Of Integer)
            Public Property Index As Integer
            Public Property Count As Integer
        End Class

        Private Shared ReadOnly BACKGROUND_TRIGGER As Integer = 5000
        Private Shared ReadOnly NUM_BACKGROUND_ITEMS As Integer = 50
        Private _open As Boolean
        Private _stream As Stream
        Private _reader As StreamReader
        Private _parser As Parser
        Private _refCatalog As PdfObjectReference
        Private _refInfo As PdfObjectReference
        Private _pdfCatalog As PdfCatalog
        Private _pdfInfo As PdfInfo
        Private _backgroundCount As Integer
        Private _backgroundEvent As ManualResetEvent

        Public Sub New()
            MyBase.New(Nothing)
            Version = New PdfVersion(Me, 0, 0)
            IndirectObjects = New PdfIndirectObjects(Me)
            DecryptHandler = New PdfDecryptNone(Me)
        End Sub

        Public Overrides Sub Visit(visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public Property Version As PdfVersion
            Get
                Return _Version
            End Get
            Private Set(value As PdfVersion)
                _Version = value
            End Set
        End Property

        Public Property IndirectObjects As PdfIndirectObjects
            Get
                Return _IndirectObjects
            End Get
            Private Set(value As PdfIndirectObjects)
                _IndirectObjects = value
            End Set
        End Property

        Public Property DecryptHandler As PdfDecrypt
            Get
                Return _DecryptHandler
            End Get
            Private Set(value As PdfDecrypt)
                _DecryptHandler = value
            End Set
        End Property

        Public Sub Load(filename As String, Optional immediate As Boolean = False)
            If _open Then Throw New ApplicationException("Document already has a stream open.")

            If immediate Then
                ' Faster to read all of the file contents at once and then parse, rather than read progressively during parsing
                Dim bytes = File.ReadAllBytes(filename)
                Load(New MemoryStream(bytes), immediate, bytes)
            Else
                _reader = New StreamReader(filename)
                Load(_reader.BaseStream, immediate)
            End If
        End Sub

        Public Sub Load(stream As Stream, Optional immediate As Boolean = False, Optional bytes As Byte() = Nothing)
            If _open Then Throw New ApplicationException("Document already has a stream open.")
            _stream = stream
            _parser = New Parser(_stream)
            AddHandler _parser.ResolveReference, AddressOf Parser_ResolveReference

            ' PDF file should have a well known marker at top of file
            Dim versionMajor As Integer = Nothing, versionMinor As Integer = Nothing
            _parser.ParseHeader(versionMajor, versionMinor)
            Version = New PdfVersion(Me, versionMajor, versionMinor)

            ' Find stream position of the last cross-reference table
            Dim xRefPosition As Long = _parser.ParseXRefOffset()
            Dim lastHeader = True

            Do
                ' Get the aggregated set of entries from all the cross-reference table sections
                Dim xrefs = _parser.ParseXRef(xRefPosition)

                ' Should always be positioned at the trailer after parsing cross-table references
                Dim trailer As PdfDictionary = New PdfDictionary(Me, _parser.ParseTrailer())
                Dim size = trailer.MandatoryValue(Of PdfInteger)("Size")

                For Each xref In xrefs
                    ' Ignore unused entries and entries smaller than the defined size from the trailer dictionary
                    If xref.Used AndAlso xref.Id < size.Value Then IndirectObjects.AddXRef(xref)
                Next

                If lastHeader Then
                    ' Replace the default decryption handler with one from the document settings
                    DecryptHandler = PdfDecrypt.CreateDecrypt(Me, trailer)

                    ' We only care about the latest defined catalog and information dictionary
                    _refCatalog = trailer.MandatoryValue(Of PdfObjectReference)("Root")
                    _refInfo = trailer.OptionalValue(Of PdfObjectReference)("Info")
                End If

                ' If there is a previous cross-reference table, then we want to process that as well
                Dim prev = trailer.OptionalValue(Of PdfInteger)("Prev")

                If prev IsNot Nothing Then
                    xRefPosition = prev.Value
                Else
                    xRefPosition = 0
                End If

                lastHeader = False
            Loop While xRefPosition > 0

            _open = True

            ' Must load all objects immediately so the stream can then be closed
            If immediate Then
                ' Is there enough work to justify using multiple threads
                If bytes IsNot Nothing AndAlso IndirectObjects.Count > BACKGROUND_TRIGGER Then
                    ' Setup the synchronization event so we wait until all work is completed
                    _backgroundCount = NUM_BACKGROUND_ITEMS
                    _backgroundEvent = New ManualResetEvent(False)
                    Dim ids As List(Of Integer) = IndirectObjects.Ids.ToList()
                    Dim idCount = ids.Count
                    Dim batchSize As Integer = idCount / NUM_BACKGROUND_ITEMS
                    Dim i = 0, index = 0

                    While i < NUM_BACKGROUND_ITEMS
                        ' Create a parser per unit of work, so they can work in parallel
                        Dim memoryStream As MemoryStream = New MemoryStream(bytes)
                        Dim parser As Parser = New Parser(memoryStream)

                        ' Make sure the last batch includes all the remaining Ids
                        Call ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf BackgroundResolveReference), New BackgroundArgs() With {
                            .Parser = parser,
                            .Ids = ids,
                            .Index = index,
                            .Count = If(i = NUM_BACKGROUND_ITEMS - 1, idCount - index, batchSize)
                        })
                        i += 1
                        index += batchSize
                    End While

                    _backgroundEvent.WaitOne()
                    _backgroundEvent.Dispose()
                    _backgroundEvent = Nothing
                Else
                    IndirectObjects.ResolveAllReferences(Me)
                End If

                Close()
            End If
        End Sub

        Public Sub Close()
            If _open Then
                If _reader IsNot Nothing Then
                    _reader.Dispose()
                    _reader = Nothing
                End If

                If _stream IsNot Nothing Then
                    _stream.Dispose()
                    _stream = Nothing
                End If

                If _parser IsNot Nothing Then
                    _parser.Dispose()
                    _parser = Nothing
                End If

                _open = False
            End If
        End Sub

        Public ReadOnly Property Catalog As PdfCatalog
            Get

                If _pdfCatalog Is Nothing AndAlso _refCatalog IsNot Nothing Then
                    Dim dictionary = IndirectObjects.MandatoryValue(Of PdfDictionary)(_refCatalog)
                    _pdfCatalog = New PdfCatalog(dictionary.Parent, TryCast(dictionary.ParseObject, ParseDictionary))
                End If

                Return _pdfCatalog
            End Get
        End Property

        Public ReadOnly Property Info As PdfInfo
            Get

                If _pdfInfo Is Nothing AndAlso _refInfo IsNot Nothing Then
                    Dim dictionary = IndirectObjects.MandatoryValue(Of PdfDictionary)(_refInfo)
                    _pdfInfo = New PdfInfo(dictionary.Parent, TryCast(dictionary.ParseObject, ParseDictionary))
                End If

                Return _pdfInfo
            End Get
        End Property

        Public Function ResolveReference(reference As PdfObjectReference) As PdfObject
            Return ResolveReference(reference.Id, reference.Gen)
        End Function

        Public Function ResolveReference(id As Integer, gen As Integer) As PdfObject
            Return ResolveReference(IndirectObjects(id, gen))
        End Function

        Public Function ResolveReference(indirect As PdfIndirectObject) As PdfObject
            Return ResolveReference(_parser, indirect)
        End Function

        Public Function ResolveReference(parser As Parser, indirect As PdfIndirectObject) As PdfObject
            If indirect IsNot Nothing Then
                If indirect.Child Is Nothing Then
                    Dim parseIndirectObject = parser.ParseIndirectObject(indirect.Offset)
                    indirect.Child = indirect.WrapObject(parseIndirectObject.Object)
                End If

                Return indirect.Child
            End If

            Return Nothing
        End Function

        Private Sub Parser_ResolveReference(sender As Object, e As ParseResolveEventArgs)
            e.Object = ResolveReference(e.Id, e.Gen).ParseObject
        End Sub

        Private Sub BackgroundResolveReference(state As Object)
            Dim args = CType(state, BackgroundArgs)

            Try
                Dim i = 0, index = args.Index

                While i < args.Count
                    Dim id = args.Ids(index)
                    Dim gens = IndirectObjects(id)
                    gens.ResolveAllReferences(args.Parser, Me)
                    i += 1
                    index += 1
                End While

            Finally
                If Interlocked.Decrement(_backgroundCount) = 0 Then _backgroundEvent.Set()
            End Try
        End Sub
    End Class
End Namespace
