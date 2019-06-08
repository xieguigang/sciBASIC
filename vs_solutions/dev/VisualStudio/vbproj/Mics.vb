#Region "Microsoft.VisualBasic::35a1a8b4dbee8ef34f3e55c16a20c1b8, vs_solutions\dev\VisualStudio\vbproj\Mics.vb"

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

    ' Class Target
    ' 
    '     Properties: Name
    ' 
    ' Class Import
    ' 
    '     Properties: Condition, Label, Project
    ' 
    '     Function: ToString
    ' 
    ' Class ConditionValue
    ' 
    '     Properties: Condition, value
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace vbproj

    Public Class Target
        <XmlAttribute>
        Public Property Name As String
    End Class

    Public Class Import

        <XmlAttribute> Public Property Project As String
        <XmlAttribute> Public Property Condition As String
        <XmlAttribute> Public Property Label As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class ConditionValue

        <XmlAttribute>
        Public Property Condition As String
        <XmlText>
        Public Property value As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace