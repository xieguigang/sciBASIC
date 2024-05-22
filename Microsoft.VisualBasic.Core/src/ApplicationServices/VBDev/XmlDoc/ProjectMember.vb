#Region "Microsoft.VisualBasic::e55f0a93ccd1e1e219b1a7064bca961f, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\XmlDoc\ProjectMember.vb"

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

    '   Total Lines: 131
    '    Code Lines: 87 (66.41%)
    ' Comment Lines: 17 (12.98%)
    '    - Xml Docs: 88.24%
    ' 
    '   Blank Lines: 27 (20.61%)
    '     File Size: 4.53 KB


    '     Class ProjectMember
    ' 
    '         Properties: [Declare], author, example, Params, Returns
    '                     Type, version
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetParameterDocument, ToString
    ' 
    '         Sub: LoadFromNode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 

Imports System.Runtime.CompilerServices
Imports System.Xml
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices.Development.XmlDoc.Assembly

    ''' <summary>
    ''' Base class for a method or property.
    ''' </summary>
    Public Class ProjectMember : Inherits XmlDocs

        Dim projectType As ProjectType

        Public Property Returns() As String

        ''' <summary>
        ''' the xml document text for the function/property parameters
        ''' </summary>
        ''' <returns></returns>
        Public Property Params As param()

        ''' <summary>
        ''' 申明的原型
        ''' </summary>
        ''' <returns></returns>
        Public Property [Declare] As String

        ''' <summary>
        ''' example code for use this method
        ''' </summary>
        ''' <returns></returns>
        Public Property example As String
        Public Property author As String
        Public Property version As String

        Public ReadOnly Property Type() As ProjectType
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.projectType
            End Get
        End Property

        Public Sub New(projectType As ProjectType)
            Me.projectType = projectType
        End Sub

        Sub New()
        End Sub

        Public Function GetParameterDocument(name As String) As String
            Dim docs As param = Params.SafeQuery _
                .Where(Function(pi) pi.name.TextEquals(name)) _
                .FirstOrDefault

            If Not docs Is Nothing Then
                Return Strings.Trim(docs.text).Trim(" "c, ASCII.TAB, ASCII.CR, ASCII.LF)
            Else
                Return ""
            End If
        End Function

        Public Overrides Function ToString() As String
            Return [Declare]
        End Function

        Friend Sub LoadFromNode(xn As XmlNode)
            Dim summaryNode As XmlNode = xn.SelectSingleNode("summary")
            Dim [declare] As NamedValue(Of String) = xn _
                .Attributes _
                .GetNamedItem("name") _
                .InnerText _
                .Trim(ASCII.CR, ASCII.LF, " ") _
                .GetTagValue(":", trim:=True)

            Me.Declare = [declare].Value

            If summaryNode IsNot Nothing Then
                Me.Summary = summaryNode.InnerText.Trim(ASCII.CR, ASCII.LF, " ")
            End If

            Dim returnsNode As XmlNode = xn.SelectSingleNode("returns")

            If returnsNode IsNot Nothing Then
                Me.Returns = returnsNode.InnerText.Trim(ASCII.CR, ASCII.LF, " ")
            End If

            Dim remarksNode As XmlNode = xn.SelectSingleNode("remarks")

            If remarksNode IsNot Nothing Then
                Me.Remarks = remarksNode.InnerText.Trim(ASCII.CR, ASCII.LF, " ")
            End If

            Dim exampleNode As XmlNode = xn.SelectSingleNode("example")

            If Not exampleNode Is Nothing Then
                Me.example = exampleNode.InnerText.Trim(ASCII.CR, ASCII.LF, " ")
            End If

            Dim ns = xn.SelectNodes("param")

            If Not ns Is Nothing Then
                Dim args As New List(Of param)
                Dim text$
                Dim name$

                For Each x As XmlNode In ns
                    text = x.InnerText Or "-".AsDefault(Function()
                                                            Return Strings.Trim(x.InnerText).StringEmpty
                                                        End Function)
                    name = x.Attributes _
                        .GetNamedItem("name") _
                        .InnerText _
                        .Trim(ASCII.CR, ASCII.LF, " ")
                    args += New param With {
                        .name = name,
                        .text = (text)
                    }
                Next

                Me.Params = args
            End If
        End Sub
    End Class
End Namespace
