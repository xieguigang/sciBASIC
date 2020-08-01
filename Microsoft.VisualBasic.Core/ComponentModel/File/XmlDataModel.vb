#Region "Microsoft.VisualBasic::48dfaba4c17e2492ed6fee012ec01c0d, Microsoft.VisualBasic.Core\ComponentModel\File\XmlDataModel.vb"

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

    '     Class XmlDataModel
    ' 
    '         Properties: Stylesheet, TypeComment
    ' 
    '         Function: CreateTypeReferenceComment, (+2 Overloads) GetTypeReferenceComment
    ' 
    '         Sub: SaveTypeComment
    '         Interface IXmlType
    ' 
    '             Properties: TypeComment
    ' 
    '         Class XmlStyleProcessor
    ' 
    '             Properties: alternate, href, media, title, type
    ' 
    '             Function: getAttributes, ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports System.Web.Script.Serialization
Imports System.Xml
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.SecurityString

Namespace ComponentModel

    ''' <summary>
    ''' 这个基类型对象主要是用来生成类型全称注释方便编写XML文件加载代码功能的
    ''' </summary>
    Public MustInherit Class XmlDataModel : Implements IXmlType

        ''' <summary>
        ''' 只适合最外层面的容器类型的对象来实现
        ''' </summary>
        Public Interface IXmlType
            Property TypeComment As XmlComment
        End Interface

        ''' <summary>
        ''' The external css/xsl file name for styling current xml file
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' This property is only works for the root element type
        ''' </remarks>
        <XmlIgnore>
        Public Property Stylesheet As XmlStyleProcessor

        ''' <summary>
        ''' ReadOnly, Data model type tracking use Xml Comment.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' JSON存储的时候,这个属性会被自动忽略掉
        ''' </remarks>
        <DataMember>
        <IgnoreDataMember>
        <ScriptIgnore>
        <SoapIgnore>
        <XmlAnyElement>
        Public Property TypeComment As XmlComment Implements IXmlType.TypeComment
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetTypeReferenceComment()
            End Get
            Set(value As XmlComment)
                ' Do Nothing
                ' 2018-6-5 this xml comment node cause bug 
                ' when using xml deserialization
            End Set
        End Property

        Private Function GetTypeReferenceComment() As XmlComment
            Return New XmlDocument().CreateComment(GetTypeReferenceComment(Me.GetType))
        End Function

        Public Shared Function CreateTypeReferenceComment(type As Type) As XmlComment
            Return New XmlDocument().CreateComment(GetTypeReferenceComment(type))
        End Function

        Public Shared Sub SaveTypeComment(model As IXmlType)
            model.TypeComment = New XmlDocument().CreateComment(GetTypeReferenceComment(model.GetType))
        End Sub

        Public Class XmlStyleProcessor

            Public Property href As String
            Public Property alternate As Boolean
            Public Property title As String
            Public Property media As String

            Public ReadOnly Property type As String
                Get
                    Select Case href.ExtensionSuffix.ToLower
                        Case "css" : Return "text/css"
                        Case "xsl" : Return ""
                        Case Else
                            Throw New InvalidDataException("Unknown file type!")
                    End Select
                End Get
            End Property

            Private Iterator Function getAttributes() As IEnumerable(Of NamedValue(Of String))
                If href.StringEmpty Then
                    Throw New EntryPointNotFoundException("No style file was specific!")
                Else
                    Yield New NamedValue(Of String) With {.Name = NameOf(href), .Value = href}
                End If
                If Not title.StringEmpty Then
                    Yield New NamedValue(Of String) With {.Name = NameOf(title), .Value = title}
                End If
                If Not media.StringEmpty Then
                    Yield New NamedValue(Of String) With {.Name = NameOf(media), .Value = media}
                End If

                Yield New NamedValue(Of String) With {.Name = NameOf(type), .Value = type}
                Yield New NamedValue(Of String) With {
                    .Name = NameOf(alternate),
                    .Value = "no" Or "yes".When(alternate)
                }
            End Function

            Public Overrides Function ToString() As String
                Dim attrs As NamedValue(Of String)() = getAttributes.ToArray
                Dim attrVals$() = attrs _
                    .Select(Function(a) $"{a.Name}=""{a.Value}""") _
                    .ToArray
                Dim declares$ = $"<?xml-stylesheet {attrVals.JoinBy(" ")} ?>"

                Return declares
            End Function
        End Class

        ''' <summary>
        ''' 生成的注释信息是默认空了四个空格的
        ''' </summary>
        ''' <param name="modelType"></param>
        ''' <returns></returns>
        Public Shared Function GetTypeReferenceComment(modelType As Type, Optional indent% = 4) As String
            Dim fullName$ = modelType.FullName
            Dim devtools = modelType.Assembly.FromAssembly
            Dim assembly$ = modelType.Assembly.FullName
            Dim md5$ = modelType.Assembly.Location.GetFileMd5
            Dim indentBlank As New String(" "c, indent)
            Dim traceInfo$ = vbCrLf &
                $"{indentBlank} model:     " & fullName & vbCrLf &
                $"{indentBlank} assembly:  " & assembly & vbCrLf &
                $"{indentBlank} version:   " & devtools.AssemblyVersion & vbCrLf &
                $"{indentBlank} built:     " & devtools.BuiltTime.ToString & vbCrLf &
                $"{indentBlank} md5:       " & md5 & vbCrLf &
                $"{indentBlank} timestamp: " & Now.ToString & vbCrLf &
                "  "

            Return traceInfo
        End Function
    End Class
End Namespace
