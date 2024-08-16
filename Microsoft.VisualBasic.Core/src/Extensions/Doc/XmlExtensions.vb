#Region "Microsoft.VisualBasic::c6c3a7eda95f07cba33856ee73bfd34e, Microsoft.VisualBasic.Core\src\Extensions\Doc\XmlExtensions.vb"

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

    '   Total Lines: 446
    '    Code Lines: 275 (61.66%)
    ' Comment Lines: 117 (26.23%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 54 (12.11%)
    '     File Size: 17.68 KB


    ' Module XmlExtensions
    ' 
    '     Properties: XmlParser
    ' 
    '     Function: CreateObjectFromXml, CreateObjectFromXmlFragment, (+2 Overloads) GetXml, (+2 Overloads) LoadFromXml, LoadFromXmlStream
    '               (+2 Overloads) LoadXml, SafeLoadXml, SaveAsXml
    ' 
    '     Sub: WriteXml, WriteXML
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml
Imports Microsoft.VisualBasic.Text.Xml.Linq

<HideModuleName>
Public Module XmlExtensions

    Public ReadOnly Property XmlParser As New [Default](Of IObjectBuilder)(AddressOf LoadFromXml)

    ''' <summary>
    ''' 这个函数主要是用作于Linq里面的Select语句拓展的，这个函数永远也不会报错，只会返回空值
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    Public Function SafeLoadXml(Of T)(xml$,
                                      Optional encoding As Encodings = Encodings.Default,
                                      Optional preProcess As Func(Of String, String) = Nothing) As T
        Return xml.LoadXml(Of T)(encoding.CodePage, False, preProcess)
    End Function

    ''' <summary>
    ''' Load class object from the exists Xml document.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="xmlFile">The path of the xml document.(XML文件的文件路径)</param>
    ''' <param name="throwEx">
    ''' If the deserialization operation have throw a exception, then this function should process this error automatically or just throw it?
    ''' (当反序列化出错的时候是否抛出错误？假若不抛出错误，则会返回空值)
    ''' </param>
    ''' <param name="preprocess">
    ''' The preprocessing on the xml document text, you can doing the text replacement or some trim operation from here.(Xml文件的预处理操作)
    ''' </param>
    ''' <param name="encoding">Default is <see cref="UTF8"/> text encoding.</param>
    ''' <returns></returns>
    ''' <remarks>(从文件之中加载XML之中的数据至一个对象类型之中)</remarks>
    <Extension>
    Public Function LoadXml(Of T)(xmlFile$,
                                  Optional encoding As Encoding = Nothing,
                                  Optional throwEx As Boolean = True,
                                  Optional preprocess As Func(Of String, String) = Nothing,
                                  Optional stripInvalidsCharacter As Boolean = False) As T

        Dim type As Type = GetType(T)
        Dim obj As Object = xmlFile.LoadXml(
            type, encoding, throwEx,
            preprocess,
            stripInvalidsCharacter:=stripInvalidsCharacter
        )

        If obj Is Nothing Then
            ' 由于在底层函数之中已经将错误给处理掉了，所以这里直接返回
            Return Nothing
        Else
            If type.ImplementInterface(GetType(IFileReference)) Then
                DirectCast(obj, IFileReference).FilePath = xmlFile
            End If

            Return DirectCast(obj, T)
        End If
    End Function


    ''' <summary>
    ''' 从文件之中加载XML之中的数据至一个对象类型之中
    ''' </summary>
    ''' <param name="xmlFile">XML文件的文件路径</param>
    ''' <param name="ThrowEx">当反序列化出错的时候是否抛出错误？假若不抛出错误，则会返回空值</param>
    ''' <param name="preprocess">Xml文件的预处理操作</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' <param name="encoding">Default is <see cref="UTF8"/> text encoding.</param>
    <ExportAPI("LoadXml")>
    <Extension>
    Public Function LoadXml(xmlFile$, type As Type,
                            Optional encoding As Encoding = Nothing,
                            Optional ThrowEx As Boolean = True,
                            Optional preprocess As Func(Of String, String) = Nothing,
                            Optional stripInvalidsCharacter As Boolean = False) As Object

        If Not xmlFile.FileExists(ZERO_Nonexists:=True) Then
            Dim exMsg$ = $"{xmlFile.ToFileURL} is not exists on your file system or it is ZERO length content!"

            With New Exception(exMsg)
                Call App.LogException(.ByRef)

                If ThrowEx Then
                    Throw .ByRef
                Else
                    Return Nothing
                End If
            End With
        End If

        Dim xmlDoc$ = File.ReadAllText(xmlFile, encoding Or UTF8)

        If Not preprocess Is Nothing Then
            xmlDoc = preprocess(xmlDoc)
        End If
        If stripInvalidsCharacter Then
            xmlDoc = xmlDoc.StripInvalidUTF8Code
        End If

        Using stream As New StringReader(s:=xmlDoc)
            Try
                ' 20231214
                ' xml comment data may be missing in the xml document file
                ' try to ignores it!
                Dim args As New XmlAttributeOverrides
                Dim ignores As New XmlAttributes()

                ignores.XmlIgnore = True
                args.Add(GetType(XmlComment), "TypeComment", ignores)

                Dim handler As New XmlSerializer(type, [overrides]:=args)
                Dim obj = handler.Deserialize(stream)
                Return obj
            Catch ex As Exception
                ex = New Exception(type.FullName, ex)
                ex = New Exception(xmlFile.ToFileURL, ex)

                Call App.LogException(ex, MethodBase.GetCurrentMethod.GetFullName)
#If DEBUG Then
                Call ex.PrintException
#End If
                If ThrowEx Then
                    Throw ex
                Else
                    Return Nothing
                End If
            End Try
        End Using
    End Function



    ''' <summary>
    ''' Serialization the target object type into a XML document.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The type of the target object data should be a class object.(目标对象类型必须为一个Class)
    ''' </typeparam>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' <remarks>(将一个类对象序列化为XML文档)</remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function GetXml(Of T)(
                                 obj As T,
                     Optional ThrowEx As Boolean = True,
                     Optional xmlEncoding As XmlEncodings = XmlEncodings.UTF8) As String

        Return GetXml(obj, GetType(T), ThrowEx, xmlEncoding)
    End Function

    Public Function GetXml(
                        obj As Object,
                       type As Type,
           Optional throwEx As Boolean = True,
           Optional xmlEncoding As XmlEncodings = XmlEncodings.UTF8) As String

        Try

            If xmlEncoding = XmlEncodings.UTF8 Then
                ' create a MemoryStream here, we are just working
                ' exclusively in memory
                Dim stream As New MemoryStream()

                Call WriteXML(obj, type, stream, xmlEncoding)

                ' read back the contents of the stream And supply the encoding
                Dim result As String = Encoding.UTF8.GetString(stream.ToArray())
                Return result
            Else
                Dim sBuilder As New StringBuilder(1024)
                Using StreamWriter As New StringWriter(sb:=sBuilder)
                    Call (New XmlSerializer(type)).Serialize(StreamWriter, obj)
                    Return sBuilder.ToString
                End Using
            End If

        Catch ex As Exception
            ex = New Exception(type.ToString, ex)
            Call App.LogException(ex)

#If DEBUG Then
            Call ex.PrintException
#End If

            If throwEx Then
                Throw ex
            Else
                Return Nothing
            End If
        End Try
    End Function

    ''' <summary>
    ''' 写入的文本文件的编码格式和XML的编码格式应该是一致的
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <param name="type"></param>
    ''' <param name="out"></param>
    ''' <param name="encoding"></param>
    Public Sub WriteXML(obj As Object, type As Type, ByRef out As Stream, encoding As XmlEncodings)
        Dim serializer As New XmlSerializer(type)
        ' The XmlTextWriter takes a stream And encoding
        ' as one of its constructors
        Dim xtWriter As New XmlTextWriter(out, encoding.CodePage) With {
            .Indentation = 3,
            .Formatting = Formatting.Indented
        }

        Call serializer.Serialize(xtWriter, obj)
        Call xtWriter.Flush()
    End Sub

    ''' <summary>
    ''' Save the object as the XML document.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <param name="saveXml"></param>
    ''' <param name="throwEx"></param>
    ''' <param name="encoding">VB.NET的XML文件的默认编码格式为``utf-16``</param>
    ''' <returns></returns>
    <Extension> Public Function SaveAsXml(Of T As Class)(
                                    obj As T,
                                saveXml As String,
                       Optional throwEx As Boolean = True,
                       Optional encoding As Encodings = Encodings.UTF8,
    <CallerMemberName> Optional caller As String = "") As Boolean
        Try
            Return obj _
                .GetXml(ThrowEx:=throwEx, xmlEncoding:=encoding) _
                .SaveTo(saveXml, encoding.CodePage, throwEx:=throwEx)
        Catch ex As Exception
            ex = New Exception(caller, ex)

            If throwEx Then
                Throw ex
            Else
                Call App.LogException(ex)
                Call ex.PrintException
                Return False
            End If
        End Try
    End Function

    ''' <summary>
    ''' Generate a specific type object from a xml document stream.(使用一个XML文本内容创建一个XML映射对象)
    ''' </summary>
    ''' <param name="Xml">This parameter value is the document text of the xml file, not the file path of the xml file.(是Xml文件的文件内容而非文件路径)</param>
    ''' <param name="throwEx">Should this program throw the exception when the xml deserialization error happens?
    ''' if False then this function will returns a null value instead of throw exception.
    ''' (在进行Xml反序列化的时候是否抛出错误，默认抛出错误，否则返回一个空对象)</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' this function returns nothing if the error happended and 
    ''' also set <paramref name="throwEx"/> to false.
    ''' </remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <DebuggerStepThrough>
    <Extension>
    Public Function LoadFromXml(Of T)(xml$,
                                      Optional throwEx As Boolean = True,
                                      Optional doNamespaceIgnorant As Boolean = False,
                                      Optional variants As Type() = Nothing) As T

        Return LoadFromXml(xml, GetType(T),
                           throwEx:=throwEx,
                           doNamespaceIgnorant:=doNamespaceIgnorant,
                           variants:=variants)
    End Function

    Public Function LoadFromXmlStream(s As Stream, model As Type, Optional throwEx As Boolean = True) As Object
        If s Is Nothing OrElse s.Length = 0 Then
            If throwEx Then
                Throw New Exception("the given xml document stream has no data!")
            Else
                Return Nothing
            End If
        End If

        Try
            Using stream As New StreamReader(s)
                Return New XmlSerializer(model).Deserialize(stream)
            End Using
        Catch ex As Exception
            Call App.LogException(ex)

            If throwEx Then
                Throw
            Else
                Return Nothing
            End If
        End Try
    End Function

    ''' <summary>
    ''' Generate a specific type object from a xml document stream.
    ''' </summary>
    ''' <param name="Xml">
    ''' This parameter value is the document text of the xml file, 
    ''' not the file path of the xml file.
    ''' (是Xml文件的文件内容而非文件路径)</param>
    ''' <param name="throwEx">Should this program throw the exception 
    ''' when the xml deserialization error happens?
    ''' if False then this function will returns a null value instead 
    ''' of throw exception.
    ''' (在进行Xml反序列化的时候是否抛出错误，默认抛出错误，否则返回一个空对象)
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' (使用一个XML文本内容创建一个XML映射对象)
    ''' </remarks>
    ''' 
    <Extension>
    Public Function LoadFromXml(xml$, schema As Type,
                                Optional throwEx As Boolean = True,
                                Optional doNamespaceIgnorant As Boolean = False,
                                Optional variants As Type() = Nothing) As Object

        If xml.StringEmpty Then
            If throwEx Then
                Throw New XmlException("Empty xml content!")
            Else
                Return Nothing
            End If
        Else
            xml = xml.StripInvalidUTF8Code
        End If

        Try
            If doNamespaceIgnorant Then
                Using xmlDoc As New StringReader(xml), reader As New NamespaceIgnorantXmlTextReader(xmlDoc)
                    If variants.IsNullOrEmpty Then
                        Return New XmlSerializer(schema).Deserialize(reader)
                    Else
                        Return New XmlSerializer(schema, extraTypes:=variants).Deserialize(reader)
                    End If
                End Using
            Else
                Using stream As New StringReader(s:=xml)
                    If variants.IsNullOrEmpty Then
                        Return New XmlSerializer(schema).Deserialize(stream)
                    Else
                        Return New XmlSerializer(schema, extraTypes:=variants).Deserialize(stream)
                    End If
                End Using
            End If
        Catch ex As Exception
            Dim curMethod As String = MethodBase.GetCurrentMethod.GetFullName
            Dim max_debug_len As String = 4096

            If Len(xml) > max_debug_len Then
                xml = Mid(xml, 1, max_debug_len) & "..."
            End If

            ex = New Exception($"class_name: {schema.Name}, and the xml fragment: {xml}", ex)

            App.LogException(ex, curMethod)

            If throwEx Then
                Throw ex
            Else
                Return Nothing
            End If
        End Try
    End Function

    <Extension>
    Public Function CreateObjectFromXml(Xml As StringBuilder, typeInfo As Type) As Object
        Dim doc As String = Xml.ToString

        Using Stream As New StringReader(doc)
            Try
                Dim obj As Object = New XmlSerializer(typeInfo).Deserialize(Stream)
                Return obj
            Catch ex As Exception
                ex = New Exception(doc, ex)
                ex = New Exception(typeInfo.FullName, ex)

                Call App.LogException(ex)

                Throw ex
            End Try
        End Using
    End Function

    ''' <summary>
    ''' 使用一个XML文本内容的一个片段创建一个XML映射对象
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="xml">是Xml文件的文件内容而非文件路径</param>
    ''' <param name="ignoreXmlNs">
    ''' ignores of the xml namespace delcaration attribute value by default,
    ''' due to the reason of the input <paramref name="xml"/> text just a
    ''' xml document fragment text.
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' the input <paramref name="xml"/> probably has no namespace attribute data
    ''' inside the xml root tag, so the <see cref="NamespaceIgnorantXmlTextReader"/>
    ''' will be used for parse the xml in this helper function?
    ''' </remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function CreateObjectFromXmlFragment(Of T)(xml$,
                                                      Optional preprocess As Func(Of String, String) = Nothing,
                                                      Optional ignoreXmlNs As Boolean = True) As T
        Dim xmlDoc$ =
            "<?xml version=""1.0"" encoding=""UTF-8""?>" &
            ASCII.LF &
            xml

        If Not preprocess Is Nothing Then
            xmlDoc = preprocess(xmlDoc)
        End If

        Return xmlDoc.LoadFromXml(Of T)(doNamespaceIgnorant:=ignoreXmlNs)
    End Function

    ''' <summary>
    ''' Write a xml content as text into the target file data
    ''' </summary>
    ''' <param name="file">a text file writer</param>
    ''' <param name="xml">xml literal</param>
    <Extension>
    Public Sub WriteXml(file As StreamWriter, xml As XElement)
        Call file.WriteLine(xml.ToString)
    End Sub
End Module
