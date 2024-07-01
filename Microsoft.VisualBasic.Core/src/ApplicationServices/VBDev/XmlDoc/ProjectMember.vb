#Region "Microsoft.VisualBasic::5d13a5279c6c65600946354c3842ac87, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\XmlDoc\ProjectMember.vb"

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

    '   Total Lines: 110
    '    Code Lines: 72 (65.45%)
    ' Comment Lines: 17 (15.45%)
    '    - Xml Docs: 88.24%
    ' 
    '   Blank Lines: 21 (19.09%)
    '     File Size: 3.69 KB


    '     Class ProjectMember
    ' 
    '         Properties: [Declare], author, example, Params, Returns
    '                     Type, version
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetParameterDocument, ReadParameters, ToString
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

        Friend Overrides Sub LoadFromNode(xn As XmlNode)
            Dim [declare] As NamedValue(Of String) = xn _
                .Attributes _
                .GetNamedItem("name") _
                .InnerText _
                .Trim(ASCII.CR, ASCII.LF, " ") _
                .GetTagValue(":", trim:=True)

            Me.Declare = [declare].Value
            Me.Returns = readFieldText(xn, "returns")
            Me.example = readFieldText(xn, "example")
            Me.Params = ReadParameters(xn).ToArray

            Call MyBase.LoadFromNode(xn)
        End Sub

        Private Shared Iterator Function ReadParameters(xn As XmlNode) As IEnumerable(Of param)
            Dim name$, text$
            Dim ns = xn.SelectNodes("param")

            If ns Is Nothing Then
                Return
            End If

            For Each subNode As XmlNode In ns
                text = subNode.InnerText Or "-".AsDefault(Function() Strings.Trim(subNode.InnerText).StringEmpty)
                name = subNode.Attributes _
                    .GetNamedItem("name").InnerText _
                    .Trim(ASCII.CR, ASCII.LF, " ")

                Yield New param With {
                    .name = name,
                    .text = (text)
                }
            Next
        End Function
    End Class
End Namespace
