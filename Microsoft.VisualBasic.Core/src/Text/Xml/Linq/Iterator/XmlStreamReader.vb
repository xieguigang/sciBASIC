Imports System.IO
Imports System.Xml
Imports System.Xml.Schema
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.Language

Namespace Text.Xml.Linq

    Public Class XmlStreamReader

        ReadOnly nodeName As String
        ReadOnly documentText As Stream
        ReadOnly selector As Func(Of XElement, Boolean)
        ReadOnly settings As New XmlReaderSettings With {
            .ValidationFlags = XmlSchemaValidationFlags.None,
            .CheckCharacters = False,
            .ConformanceLevel = ConformanceLevel.Document,
            .ValidationType = ValidationType.None,
            .DtdProcessing = DtdProcessing.Ignore
        }

        Public Property ShowProgress As Boolean = False

        Dim el As New Value(Of XElement)
        Dim reader As XmlReader

        Sub New(nodeName$, documentText As Stream, selector As Func(Of XElement, Boolean))
            Me.nodeName = nodeName
            Me.documentText = documentText
            Me.selector = selector
        End Sub

        Private Function CheckStreamSize() As Long
            Dim sizeOfBytes As Long = -1

            Try
                sizeOfBytes = documentText.Length
            Catch ex As Exception
                ' stream can not be seek
                ' probably is a network stream or other stream with length not suports
            End Try

            If sizeOfBytes > 0 AndAlso documentText.CanSeek Then
                Return sizeOfBytes
            Else
                Return -1
            End If
        End Function

        Public Iterator Function UltraLargeXmlNodesIterator() As IEnumerable(Of XElement)
            Dim sizeOfBytes As Value(Of Long) = 0

            Using reader As XmlReader = XmlReader.Create(documentText, settings)
                Me.reader = reader

                ' 20191218
                ' 
                ' System.Xml.XmlException
                '  HResult = 0x80131940
                '  Message = There Is no Unicode Byte order mark. Cannot switch To Unicode.
                '  Source  = System.Xml
                '  StackTrace:
                '   at System.Xml.XmlTextReaderImpl.Throw(Exception e)
                '   at System.Xml.XmlTextReaderImpl.CheckEncoding(String newEncodingName)
                '   at System.Xml.XmlTextReaderImpl.ParseXmlDeclaration(Boolean isTextDecl)
                '   at System.Xml.XmlTextReaderImpl.Read()
                '   at System.Xml.XmlReader.MoveToContent()
                '   at Microsoft.VisualBasic.Text.Xml.Linq.Data.VB$StateMachine_12_UltraLargeXmlNodesIterator.MoveNext() in D:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\Text\Xml\Linq\Linq.vb:line 322
                '   at Microsoft.VisualBasic.Text.Xml.Linq.Data.VB$StateMachine_11_UltraLargeXmlNodesIterator.MoveNext() in D:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\Text\Xml\Linq\Linq.vb:line 305
                '   at System.Linq.Enumerable.WhereSelectEnumerableIterator`2.MoveNext()
                '   at Microsoft.VisualBasic.Text.Xml.Linq.Data.VB$StateMachine_6_NodeInstanceBuilder`1.MoveNext() in D:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\Text\Xml\Linq\Linq.vb:line 235
                '   at System.Linq.Buffer`1..ctor(IEnumerable`1 source)
                '   at System.Linq.Enumerable.ToArray[TSource](IEnumerable`1 source)
                '   at metadbkit.builderPipeline._Closure$__._Lambda$__12-0(IEnumerable`1 ds) in D:\biodeep\biodeepdb_v3\metadb\metadbkit\builderPipeline.vb:line 173
                '   at Microsoft.VisualBasic.Linq.PipelineExtensions.DoCall[T, Tout](T input, Func`2 apply) in D:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\Extensions\Collection\Linq\Pipeline.vb:line 63
                '   at metadbkit.builderPipeline.readMetaDb(String file, Boolean linq) in D:\biodeep\biodeepdb_v3\metadb\metadbkit\builderPipeline.vb:line 170
                '
                ' Due to the reason of small xml file save in sciBASIC.NET is serialize to xml text at first
                ' save then save the generated xml text to file
                ' so the xml file encoding in its declaration and the actual text encoding of the saved text file
                ' could be different
                ' The the text encoding bugs will happened when read xml file in linq method by this function.

                Call reader.MoveToContent()

                If ShowProgress AndAlso (sizeOfBytes = CheckStreamSize()) > 0 Then
                    For Each xelement As XElement In UltraLargeXmlNodesIteratorTqdm(sizeOfBytes)
                        If xelement IsNot Nothing Then
                            Yield xelement
                        End If
                    Next
                Else
                    For Each xelement As XElement In UltraLargeXmlNodesIteratorNoTqdm()
                        If xelement IsNot Nothing Then
                            Yield xelement
                        End If
                    Next
                End If
            End Using
        End Function

        Private Function UltraLargeXmlNodesIteratorTqdm(sizeOfBytes As Long) As IEnumerable(Of XElement)
            Return Tqdm.WrapStreamReader(sizeOfBytes, AddressOf ResolveXmlElement)
        End Function

        Private Iterator Function UltraLargeXmlNodesIteratorNoTqdm() As IEnumerable(Of XElement)
            Do While reader.Read()
                Yield ResolveXmlElement()
            Loop
        End Function

        Private Function ResolveXmlElement(ByRef offset As Long, bar As Tqdm.ProgressBar) As XElement
            If reader.Read() Then
                ' do nothing
                offset = documentText.Position
            ElseIf Not bar Is Nothing Then
                bar.Finish()
            End If

            Return ResolveXmlElement()
        End Function

        ''' <summary>
        ''' Parse the file And return each of the child_node
        ''' </summary>
        ''' <returns></returns>
        Private Function ResolveXmlElement() As XElement
            ' Parse the file And return each of the child_node
            If (reader.NodeType = XmlNodeType.Element AndAlso reader.Name = nodeName) Then
                If (Not (el = DirectCast(XNode.ReadFrom(reader), XElement)) Is Nothing) Then
                    If Not selector Is Nothing Then
                        If selector(el.Value) Then
                            Return el.Value
                        End If
                    Else
                        Return el.Value
                    End If
                End If
            End If

            Return Nothing
        End Function
    End Class
End Namespace