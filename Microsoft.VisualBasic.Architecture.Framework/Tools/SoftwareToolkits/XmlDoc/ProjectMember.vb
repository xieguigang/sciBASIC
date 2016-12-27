#Region "Microsoft.VisualBasic::4a510a3041b749066e48512df1e105a2, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Tools\SoftwareToolkits\XmlDoc\ProjectMember.vb"

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

' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 

Imports System.Xml
Imports Microsoft.VisualBasic.SoftwareToolkits.XmlDoc.Serialization
Imports Microsoft.VisualBasic.Language

Namespace SoftwareToolkits.XmlDoc.Assembly

    ''' <summary>
    ''' Base class for a method or property.
    ''' </summary>
    Public Class ProjectMember

        Dim projectType As ProjectType

        Public Property Name() As String
        Public Property Summary() As String
        Public Property Returns() As String
        Public Property Remarks As String
        Public Property Params As param()

        ''' <summary>
        ''' 申明的原型
        ''' </summary>
        ''' <returns></returns>
        Public Property [Declare] As String

        Public ReadOnly Property Type() As ProjectType
            Get
                Return Me.projectType
            End Get
        End Property

        Public Sub New(projectType As ProjectType)
            Me.projectType = projectType
        End Sub

        Public Sub LoadFromNode(xn As XmlNode)
            Dim summaryNode As XmlNode = xn.SelectSingleNode("summary")

            [Declare] = xn.Attributes.GetNamedItem("name").InnerText
            [Declare] = [Declare].Split(":"c).Last

            If summaryNode IsNot Nothing Then
                Me._Summary = summaryNode.InnerText
            End If

            Dim returnsNode As XmlNode = xn.SelectSingleNode("returns")

            If returnsNode IsNot Nothing Then
                Me._Returns = returnsNode.InnerText
            End If

            Dim remarksNode As XmlNode = xn.SelectSingleNode("remarks")

            If remarksNode IsNot Nothing Then
                Me.Remarks = remarksNode.InnerText
            End If

            Dim ns = xn.SelectNodes("param")

            If Not ns Is Nothing Then
                Dim args As New List(Of param)

                For Each x As XmlNode In ns
                    args += New param With {
                        .name = x.Attributes.GetNamedItem("name").InnerText,
                        .text = If(Trim(x.InnerText).IsNullOrEmpty, "-", x.InnerText)
                    }
                Next

                Me.Params = args
            End If
        End Sub
    End Class
End Namespace
