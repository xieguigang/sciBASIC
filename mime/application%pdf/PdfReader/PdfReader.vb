#Region "Microsoft.VisualBasic::297ed86dd88fcb6109f58560ead6a875, mime\application%pdf\PdfReader\PdfReader.vb"

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

    '   Total Lines: 449
    '    Code Lines: 361 (80.40%)
    ' Comment Lines: 39 (8.69%)
    '    - Xml Docs: 5.13%
    ' 
    '   Blank Lines: 49 (10.91%)
    '     File Size: 18.00 KB


    ' Class PdfReader
    ' 
    '     Properties: DataSize, ObjectCount, Trailer
    ' 
    '     Constructor: (+3 Overloads) Sub New
    ' 
    '     Function: ApplyFilter, DecodeAsciiHex, DecodeStream, FindLastOccurrence, GetPages
    '               ReadW, Resolve
    ' 
    '     Sub: Dispose, Initialize, ParseAllObjectStreams, ParseIndirectObjectAt, ParseObjectStream
    '          ParseXRefAndTrailer, ParseXRefStream, ParseXRefTable, TraversePageTree
    ' 
    ' Class XRefEntry
    ' 
    ' 
    '     Enum EntryType
    ' 
    ' 
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
'  PdfReader.vb  -  PDF 主读取器
'  ----------------------------------------------------------------------------
'  职责：
'    1. 读取整个 PDF 文件到内存
'    2. 定位 startxref，解析交叉引用表（xref table）或交叉引用流（xref stream）
'    3. 解析所有间接对象（包括对象流 ObjStm 中的压缩对象）
'    4. 解析 trailer，定位 /Root（Catalog）
'    5. 遍历页面树，返回所有页面对象
'    6. 提供 Resolve() 方法解析引用
'  支持的 PDF 特性：
'    - 传统 xref 表（PDF 1.4 及以前）
'    - xref 流（PDF 1.5+，常见于现代生成器）
'    - 对象流 ObjStm（压缩对象存储）
'    - 增量更新（仅最后一次 xref）
' ============================================================================

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.FileIO

Public Class PdfReader : Implements IDisposable

    Dim _data As Byte()
    Private ReadOnly _objects As New Dictionary(Of Integer, PdfIndirectObject)()
    Private _trailer As PdfDictionary
    Private _rootRef As PdfReference
    Private _lexer As PdfLexer
    Private ReadOnly _xrefEntries As New Dictionary(Of Integer, XRefEntry)()
    Private _objectStreamsParsed As New HashSet(Of Integer)()

    ' ---------------- 属性 ----------------

    Public ReadOnly Property Trailer As PdfDictionary
        Get
            Return _trailer
        End Get
    End Property

    Public ReadOnly Property DataSize As Integer
        Get
            Return _data.Length
        End Get
    End Property

    Public ReadOnly Property ObjectCount As Integer
        Get
            Return _objects.Count
        End Get
    End Property

    Public Sub New(filePath As String)
        Call Me.New(File.ReadAllBytes(filePath))
    End Sub

    Sub New(file As Stream)
        Call Me.New(file.Bytes)
    End Sub

    Public Sub New(data As Byte())
        _data = data
        Initialize()
    End Sub

    Private Sub Initialize()
        _lexer = New PdfLexer(_data)
        ParseXRefAndTrailer()
        ' 解析所有直接偏移对象（type 1）
        For Each kvp In _xrefEntries
            If kvp.Value.Type = XRefEntry.EntryType.InUse AndAlso kvp.Value.Offset > 0 Then
                ParseIndirectObjectAt(kvp.Key, kvp.Value.Offset)
            End If
        Next
        ' 解析对象流以获取压缩对象（type 2）
        ParseAllObjectStreams()
    End Sub

    ' ---------------- 交叉引用表/流 ----------------

    Private Sub ParseXRefAndTrailer()
        Dim startxrefPos = FindLastOccurrence(Encoding.ASCII.GetBytes("startxref"))
        If startxrefPos < 0 Then Throw New Exception("未找到 startxref 标记，文件可能不是有效 PDF")

        _lexer.Position = startxrefPos
        Dim tok = _lexer.NextToken() ' startxref 关键字
        tok = _lexer.NextToken()     ' xref 偏移量
        If tok.Type <> PdfTokenType.Number Then Throw New Exception("startxref 后缺少偏移量")
        Dim xrefOffset = CLng(tok.NumberValue)

        _lexer.Position = CInt(xrefOffset)
        Dim peekPos = _lexer.Position
        tok = _lexer.NextToken()

        If tok.Type = PdfTokenType.XRef Then
            ' 传统 xref 表（回退到 xref 关键字位置，由 ParseXRefTable 统一消费）
            _lexer.Position = peekPos
            ParseXRefTable()
            ' ParseXRefTable 已消费 'trailer' 关键字，此处直接解析字典
            Dim parser As New PdfObjectParser(_lexer)
            _trailer = DirectCast(parser.ParseObject(), PdfDictionary)
        Else
            ' xref 流（PDF 1.5+）
            _lexer.Position = peekPos
            ParseXRefStream()
        End If

        ' 获取 /Root
        If _trailer IsNot Nothing Then
            Dim rootObj = _trailer.Get("Root")
            If TypeOf rootObj Is PdfReference Then
                _rootRef = DirectCast(rootObj, PdfReference)
            End If
        End If
    End Sub

    Private Function FindLastOccurrence(pattern As Byte()) As Integer
        For i = _data.Length - pattern.Length To 0 Step -1
            Dim match = True
            For j = 0 To pattern.Length - 1
                If _data(i + j) <> pattern(j) Then
                    match = False
                    Exit For
                End If
            Next
            If match Then Return i
        Next
        Return -1
    End Function

    Private Sub ParseXRefTable()
        Dim tok = _lexer.NextToken()
        If tok.Type <> PdfTokenType.XRef Then Throw New Exception("期望 xref 关键字")

        Do
            tok = _lexer.NextToken()
            If tok.Type = PdfTokenType.Trailer Then Exit Do
            If tok.Type = PdfTokenType.EOF Then Exit Do

            ' 子段头：firstObj count
            If tok.Type <> PdfTokenType.Number Then Exit Do
            Dim firstObj = CInt(tok.NumberValue)
            tok = _lexer.NextToken()
            If tok.Type <> PdfTokenType.Number Then Exit Do
            Dim count = CInt(tok.NumberValue)

            For i = 0 To count - 1
                ' 每条：offset gen n/f
                Dim offTok = _lexer.NextToken()
                Dim genTok = _lexer.NextToken()
                Dim flagTok = _lexer.NextToken()
                Dim offset As Long = 0
                If offTok.Type = PdfTokenType.Number Then offset = CLng(offTok.NumberValue)
                Dim inUse = (flagTok.Type = PdfTokenType.Keyword AndAlso flagTok.TextValue = "n")
                Dim entry As New XRefEntry()
                entry.Type = If(inUse, XRefEntry.EntryType.InUse, XRefEntry.EntryType.Free)
                entry.Offset = offset
                _xrefEntries(firstObj + i) = entry
            Next
        Loop
    End Sub

    Private Sub ParseXRefStream()
        ' 解析为间接对象
        Dim tok = _lexer.NextToken() ' 对象号
        Dim objNum = CInt(tok.NumberValue)
        tok = _lexer.NextToken() ' 代号
        tok = _lexer.NextToken() ' 'obj'
        Dim parser As New PdfObjectParser(_lexer)
        Dim streamObj = parser.ParseObject()
        Dim xrefStream = TryCast(streamObj, PdfStream)
        If xrefStream Is Nothing Then Return
        _trailer = xrefStream.Dictionary

        Dim data = DecodeStream(xrefStream)

        ' W 数组：[w1 w2 w3] 各字段字节数
        Dim w = TryCast(xrefStream.Dictionary.Get("W"), PdfArray)
        If w Is Nothing OrElse w.Count < 3 Then Return
        Dim w1 = CInt(DirectCast(w(0), PdfNumber).Value)
        Dim w2 = CInt(DirectCast(w(1), PdfNumber).Value)
        Dim w3 = CInt(DirectCast(w(2), PdfNumber).Value)

        ' Index 数组：[first count first count ...]，默认 [0 Size]
        Dim indexArr = TryCast(xrefStream.Dictionary.Get("Index"), PdfArray)
        Dim sizeObj = xrefStream.Dictionary.Get("Size")
        Dim size = If(TypeOf sizeObj Is PdfNumber, CInt(DirectCast(sizeObj, PdfNumber).Value), 0)

        Dim sections As New List(Of Tuple(Of Integer, Integer))()
        If indexArr IsNot Nothing Then
            Dim i = 0
            While i + 1 < indexArr.Count
                Dim first = CInt(DirectCast(indexArr(i), PdfNumber).Value)
                Dim count = CInt(DirectCast(indexArr(i + 1), PdfNumber).Value)
                sections.Add(Tuple.Create(first, count))
                i += 2
            End While
        ElseIf size > 0 Then
            sections.Add(Tuple.Create(0, size))
        End If

        Dim entrySize = w1 + w2 + w3
        If entrySize = 0 Then Return
        Dim pos = 0
        For Each sec In sections
            For i = 0 To sec.Item2 - 1
                If pos + entrySize > data.Length Then Exit For
                Dim typeField = ReadW(data, pos, w1)
                Dim field2 = ReadW(data, pos + w1, w2)
                Dim field3 = ReadW(data, pos + w1 + w2, w3)
                pos += entrySize
                Dim entry As New XRefEntry()
                Select Case typeField
                    Case 0
                        entry.Type = XRefEntry.EntryType.Free
                    Case 1
                        entry.Type = XRefEntry.EntryType.InUse
                        entry.Offset = field2
                        entry.Generation = CInt(field3)
                    Case 2
                        entry.Type = XRefEntry.EntryType.Compressed
                        entry.ObjectStreamNum = CInt(field2)
                        entry.IndexInStream = CInt(field3)
                End Select
                _xrefEntries(sec.Item1 + i) = entry
            Next
        Next
    End Sub

    Private Function ReadW(data As Byte(), offset As Integer, width As Integer) As Long
        If width = 0 Then Return 0
        Dim val As Long = 0
        For i = 0 To width - 1
            val = (val << 8) Or CLng(data(offset + i))
        Next
        Return val
    End Function

    ' ---------------- 间接对象解析 ----------------

    Private Sub ParseIndirectObjectAt(objNum As Integer, offset As Long)
        If _objects.ContainsKey(objNum) Then Return
        _lexer.Position = CInt(offset)
        Dim parser As New PdfObjectParser(_lexer)
        Dim tok = _lexer.NextToken() ' 对象号
        If tok.Type <> PdfTokenType.Number Then Return
        Dim actualNum = CInt(tok.NumberValue)
        tok = _lexer.NextToken() ' 代号
        Dim gen = If(tok.Type = PdfTokenType.Number, CInt(tok.NumberValue), 0)
        tok = _lexer.NextToken() ' 'obj'
        If tok.Type <> PdfTokenType.Obj Then Return
        Dim content = parser.ParseObject()
        _objects(actualNum) = New PdfIndirectObject(actualNum, gen, content)
    End Sub

    ' ---------------- 对象流（ObjStm）解析 ----------------

    Private Sub ParseAllObjectStreams()
        Dim objStmNums As New List(Of Integer)()
        For Each kvp In _objects
            Dim stream = TryCast(kvp.Value.Content, PdfStream)
            If stream IsNot Nothing Then
                Dim t = TryCast(stream.Dictionary.Get("Type"), PdfName)
                If t IsNot Nothing AndAlso t.Value = "ObjStm" Then
                    objStmNums.Add(kvp.Key)
                End If
            End If
        Next
        For Each n In objStmNums
            ParseObjectStream(n)
        Next
    End Sub

    Private Sub ParseObjectStream(objStmNum As Integer)
        If _objectStreamsParsed.Contains(objStmNum) Then Return
        _objectStreamsParsed.Add(objStmNum)
        If Not _objects.ContainsKey(objStmNum) Then Return
        Dim stream = TryCast(_objects(objStmNum).Content, PdfStream)
        If stream Is Nothing Then Return
        Dim data = DecodeStream(stream)
        Dim nObj = stream.Dictionary.Get("N")
        Dim firstObj = stream.Dictionary.Get("First")
        If TypeOf nObj IsNot PdfNumber OrElse TypeOf firstObj IsNot PdfNumber Then Return
        Dim n = CInt(DirectCast(nObj, PdfNumber).Value)
        Dim first = CInt(DirectCast(firstObj, PdfNumber).Value)

        Dim lexer As New PdfLexer(data)
        Dim offsets As New Dictionary(Of Integer, Integer)()
        For i = 0 To n - 1
            Dim t1 = lexer.NextToken()
            If t1.Type <> PdfTokenType.Number Then Exit For
            Dim objNum = CInt(t1.NumberValue)
            Dim t2 = lexer.NextToken()
            If t2.Type <> PdfTokenType.Number Then Exit For
            Dim off = CInt(t2.NumberValue)
            offsets(objNum) = first + off
        Next

        Dim parser As New PdfObjectParser(lexer)
        For Each kvp In offsets
            lexer.Position = kvp.Value
            Dim obj = parser.ParseObject()
            _objects(kvp.Key) = New PdfIndirectObject(kvp.Key, 0, obj)
        Next
    End Sub

    ' ---------------- 引用解析 ----------------

    Public Function Resolve(ref As PdfReference) As PdfObject
        If ref Is Nothing Then Return PdfNull.Instance
        Dim obj As PdfIndirectObject = Nothing
        If _objects.TryGetValue(ref.ObjectNumber, obj) Then
            Return obj.Content
        End If
        ' 可能是压缩对象，按需解析其所在对象流
        Dim entry As XRefEntry = Nothing
        If _xrefEntries.TryGetValue(ref.ObjectNumber, entry) AndAlso
           entry.Type = XRefEntry.EntryType.Compressed Then
            ParseObjectStream(entry.ObjectStreamNum)
            If _objects.TryGetValue(ref.ObjectNumber, obj) Then
                Return obj.Content
            End If
        End If
        Return PdfNull.Instance
    End Function

    ' ---------------- 页面树遍历 ----------------

    Public Function GetPages() As List(Of PdfDictionary)
        Dim pages As New List(Of PdfDictionary)()
        If _rootRef Is Nothing Then Return pages
        Dim catalog = TryCast(Resolve(_rootRef), PdfDictionary)
        If catalog Is Nothing Then Return pages
        Dim pagesRef = TryCast(catalog.Get("Pages"), PdfReference)
        If pagesRef Is Nothing Then Return pages
        Dim pagesObj = TryCast(Resolve(pagesRef), PdfDictionary)
        If pagesObj Is Nothing Then Return pages
        TraversePageTree(pagesObj, pages)
        Return pages
    End Function

    Private Sub TraversePageTree(node As PdfDictionary, pages As List(Of PdfDictionary))
        If node Is Nothing Then Return
        Dim typeObj = TryCast(node.Get("Type"), PdfName)
        If typeObj IsNot Nothing AndAlso typeObj.Value = "Page" Then
            pages.Add(node)
            Return
        End If
        Dim kids = TryCast(node.Get("Kids"), PdfArray)
        If kids Is Nothing Then Return
        For Each kid In kids.Items
            Dim kidRef = TryCast(kid, PdfReference)
            If kidRef IsNot Nothing Then
                Dim kidDict = TryCast(Resolve(kidRef), PdfDictionary)
                TraversePageTree(kidDict, pages)
            End If
        Next
    End Sub

    ' ---------------- 流解码（共享逻辑） ----------------

    Friend Function DecodeStream(stream As PdfStream) As Byte()
        Dim data = stream.Data
        Dim filter = stream.Dictionary.Get("Filter")
        Dim parms = TryCast(stream.Dictionary.Get("DecodeParms"), PdfDictionary)
        If TypeOf filter Is PdfName Then
            data = ApplyFilter(data, DirectCast(filter, PdfName).Value, parms)
        ElseIf TypeOf filter Is PdfArray Then
            For Each f In DirectCast(filter, PdfArray).Items
                Dim fname = TryCast(f, PdfName)
                If fname IsNot Nothing Then
                    data = ApplyFilter(data, fname.Value, parms)
                End If
            Next
        End If
        Return data
    End Function

    Private Function ApplyFilter(data As Byte(), filterName As String, parms As PdfDictionary) As Byte()
        Select Case filterName
            Case "FlateDecode", "Fl"
                data = FlateDecode.Decode(data)
                If parms IsNot Nothing Then
                    Dim predictor = TryCast(parms.Get("Predictor"), PdfNumber)
                    Dim columns = TryCast(parms.Get("Columns"), PdfNumber)
                    Dim bpc = TryCast(parms.Get("BitsPerComponent"), PdfNumber)
                    Dim colors = TryCast(parms.Get("Colors"), PdfNumber)
                    If predictor IsNot Nothing AndAlso predictor.IntegerValue >= 10 AndAlso columns IsNot Nothing Then
                        Dim bpcVal = If(bpc IsNot Nothing, bpc.IntegerValue, 8)
                        Dim colorsVal = If(colors IsNot Nothing, colors.IntegerValue, 1)
                        data = FlateDecode.ApplyPredictor(data, columns.IntegerValue, bpcVal, colorsVal)
                    End If
                End If
            Case "ASCII85Decode", "A85"
                data = FlateDecode.DecodeAscii85(data)
            Case "ASCIIHexDecode", "AHx"
                data = DecodeAsciiHex(data)
            Case "LZWDecode", "LZW"
                ' LZW 解压暂不支持，返回原始数据
            Case Else
                ' 其他滤镜暂不支持
        End Select
        Return data
    End Function

    ''' <summary>解码 ASCIIHex 数据（&lt;hex&gt; 格式）。</summary>
    Private Shared Function DecodeAsciiHex(data As Byte()) As Byte()
        If data Is Nothing Then Return New Byte(-1) {}
        Dim result As New List(Of Byte)()
        Dim hi As Integer? = Nothing
        For Each b In data
            Dim c = ChrW(b)
            If c = ">"c Then Exit For
            If c = " "c OrElse c = ControlChars.Tab OrElse c = ControlChars.Cr OrElse c = ControlChars.Lf Then Continue For
            Dim val As Integer
            If Integer.TryParse(c.ToString(), Globalization.NumberStyles.HexNumber, Nothing, val) Then
                If hi Is Nothing Then
                    hi = val
                Else
                    result.Add(CByte((hi.Value << 4) Or val))
                    hi = Nothing
                End If
            End If
        Next
        If hi IsNot Nothing Then
            result.Add(CByte(hi.Value << 4))
        End If
        Return result.ToArray()
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        Erase _data
    End Sub

End Class

''' <summary>交叉引用条目。</summary>
Friend Class XRefEntry
    Public Enum EntryType As Integer
        Free = 0
        InUse = 1
        Compressed = 2
    End Enum
    Public Type As EntryType
    Public Offset As Long
    Public Generation As Integer
    Public ObjectStreamNum As Integer
    Public IndexInStream As Integer
End Class


