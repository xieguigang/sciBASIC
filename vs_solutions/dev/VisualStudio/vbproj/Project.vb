#Region "Microsoft.VisualBasic::6d42c46fea649bf22eff2916722e15ff, vs_solutions\dev\VisualStudio\vbproj\Project.vb"

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

    ' Class Project
    ' 
    '     Properties: [Imports], DefaultTargets, ItemGroups, PropertyGroups, Targets
    '                 ToolsVersion
    ' 
    '     Function: GetProfile, (+2 Overloads) Save, ToString
    ' 
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

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

''' <summary>
''' Visual Studio project XML file
''' </summary>
<XmlRoot("Project", [Namespace]:=Project.xmlns)>
Public Class Project : Implements ISaveHandle

    Public Const xmlns$ = "http://schemas.microsoft.com/developer/msbuild/2003"

    <XmlAttribute> Public Property ToolsVersion As String
    <XmlAttribute> Public Property DefaultTargets As String

    <XmlElement("Import")>
    Public Property [Imports] As Import()

    <XmlElement("PropertyGroup")>
    Public Property PropertyGroups As PropertyGroup()
    <XmlElement("ItemGroup")>
    Public Property ItemGroups As ItemGroup()
    <XmlElement("Target")>
    Public Property Targets As Target()

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetProfile(condition As String) As PropertyGroup
        Return LinqAPI.DefaultFirst(Of PropertyGroup) _
 _
            () <= From x As PropertyGroup
                  In PropertyGroups
                  Where Not x.Condition.StringEmpty AndAlso
                      condition.TextEquals(x.Condition)
                  Select x

    End Function

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Save(Optional path As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
        Return Me.GetXml.SaveTo(path, encoding)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Save(Optional path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
        Return Save(path, encoding.CodePage)
    End Function
End Class

Public Class Target
    <XmlAttribute> Public Property Name As String
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
