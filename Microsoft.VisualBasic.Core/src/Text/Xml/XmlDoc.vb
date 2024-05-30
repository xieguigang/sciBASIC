#Region "Microsoft.VisualBasic::a4753efeac0d1feb787b4669385b714a, Microsoft.VisualBasic.Core\src\Text\Xml\XmlDoc.vb"

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

    '   Total Lines: 123
    '    Code Lines: 72 (58.54%)
    ' Comment Lines: 32 (26.02%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (15.45%)
    '     File Size: 4.67 KB


    '     Class XmlDoc
    ' 
    '         Properties: encoding, rootNode, standalone, version, xmlns
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __rootString, CreateObject, FromObject, FromXmlFile, (+2 Overloads) Save
    '                   SaveTo, (+2 Overloads) ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Text.Xml

    ''' <summary>
    ''' 请使用<see cref="XmlDoc.ToString"/>方法获取修改之后的Xml文档
    ''' </summary>
    Public Class XmlDoc : Implements ISaveHandle

        Public Const XmlDeclares As String = "<\?xml.+?>"

        ReadOnly xml As String

        Public Property version As String
        Public Property standalone As Boolean
        Public Property encoding As XmlEncodings

        Public ReadOnly Property rootNode As String
        ''' <summary>
        ''' Xml namespace definitions
        ''' </summary>
        ''' <returns></returns>
        Public Property xmlns As Xmlns

        ''' <summary>
        ''' Create a xml tools from xml document text.
        ''' </summary>
        ''' <param name="xml"></param>
        Sub New(xml As String)
            Dim [declare] As New XmlDeclaration(
                Regex.Match(xml, XmlDeclares, RegexICSng).Value)
            version = [declare].version
            standalone = [declare].standalone
            encoding = [declare].encoding

            Dim root As NamedValue(Of Xmlns) =
                Xmlns.RootParser(__rootString(xml))
            rootNode = root.Name
            xmlns = root.Value
            Me.xml = xml
        End Sub

        Protected Friend Shared Function __rootString(xml As String) As String
            xml = Regex.Match(xml, XmlDeclares & ".*?<.+?>", RegexICSng).Value
            xml = xml.Replace(Regex.Match(xml, XmlDeclares, RegexICSng).Value, "").Trim
            Return xml
        End Function

        ''' <summary>
        ''' 使用这个函数可以得到修改之后的Xml文档
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return ToString(False)
        End Function

        ''' <summary>
        ''' 使用这个函数可以得到修改之后的Xml文档
        ''' </summary>
        ''' <param name="usingDefault_xmlns"><see cref="xmlns.DefaultXmlns"/></param>
        ''' <returns></returns>
        Public Overloads Function ToString(Optional usingDefault_xmlns As Boolean = False) As String
            Dim [declare] As String = Regex.Match(xml, XmlDeclares, RegexICSng).Value
            Dim setDeclare As New XmlDeclaration With {
                .encoding = encoding,
                .standalone = standalone,
                .version = version
            }

            Dim doc As New StringBuilder(xml)
            Call doc.Replace([declare], setDeclare.ToString)
            Call xmlns.WriteNamespace(doc, usingDefault_xmlns)
            Return doc.ToString
        End Function

        ''' <summary>
        ''' 从新修改过的xml文档之中通过反序列化构建目标对象
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="usingDefault_xmlns"></param>
        ''' <returns></returns>
        Public Function CreateObject(Of T)(Optional usingDefault_xmlns As Boolean = False) As T
            Return ToString(usingDefault_xmlns).LoadFromXml(Of T)
        End Function

        Public Shared Function FromObject(Of T As Class)(x As T) As XmlDoc
            Return New XmlDoc(x.GetXml)
        End Function

        Public Shared Function FromXmlFile(path As String) As XmlDoc
            Return New XmlDoc(path.ReadAllText)
        End Function

        ''' <summary>
        ''' Me.ToString.SaveTo(Path, encoding)
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Public Function SaveTo(Path$, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Using file As Stream = Path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
                Return Save(file, encoding)
            End Using
        End Function

        Public Function Save(Path$, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return SaveTo(Path, encoding.CodePage)
        End Function

        Public Function Save(s As Stream, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Using wr As New StreamWriter(s, encoding)
                Call wr.WriteLine(Me.ToString)
                Call wr.Flush()
            End Using

            Return True
        End Function
    End Class
End Namespace
