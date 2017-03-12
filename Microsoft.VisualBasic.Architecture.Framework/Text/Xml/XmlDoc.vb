#Region "Microsoft.VisualBasic::6698ff08ee09329415c23159faffce62, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Text\Xml\XmlDoc.vb"

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

Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization

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
            xml = Regex.Match(xml, XmlDeclares & ".+?<.+?>", RegexICSng).Value
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
        Public Function SaveTo(Optional Path As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
            Return Me.ToString.SaveTo(Path, encoding)
        End Function

        Public Function Save(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return SaveTo(Path, encoding.CodePage)
        End Function
    End Class
End Namespace
