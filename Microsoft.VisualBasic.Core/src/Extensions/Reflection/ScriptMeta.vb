#Region "Microsoft.VisualBasic::10cd986be3fd926160839206cb975118, Microsoft.VisualBasic.Core\src\Extensions\Reflection\ScriptMeta.vb"

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

    ' Module ScriptMeta
    ' 
    '     Function: API, Category, Description, ExampleInfo, NamespaceEntry
    '               Usage
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection

Public Module ScriptMeta

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="type"></param>
    ''' <param name="[nameOf]"></param>
    ''' <returns></returns>
    <Extension>
    Public Function API(type As Type, [nameOf] As String, Optional strict As Boolean = False) As String
#If NET_40 = 0 Then
        Dim methods = type.GetMethods(BindingFlags.Public Or BindingFlags.Static)
        Dim mBase As MethodInfo = (From m As MethodInfo In methods
                                   Where String.Equals([nameOf], m.Name)
                                   Select m).FirstOrDefault
        Dim APIExport As ExportAPIAttribute

        If mBase Is Nothing Then
NULL:       If Not strict Then
                Return [nameOf]
            Else
                Return ""
            End If
        Else
            APIExport = mBase.GetCustomAttribute(Of ExportAPIAttribute)

            If APIExport Is Nothing Then
                GoTo NULL
            Else
                Return APIExport.Name
            End If
        End If
#Else
        Throw New NotSupportedException
#End If
    End Function

    ''' <summary>
    ''' 获取得到定义该类型成员之上的<see cref="DescriptionAttribute"/>值或者默认定义
    ''' </summary>
    ''' <param name="m"></param>
    ''' <param name="default$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Description(m As MemberInfo, Optional default$ = Nothing) As String
        Dim customAttrs() = m.GetCustomAttributes(GetType(DescriptionAttribute), inherit:=False)

        If Not customAttrs.IsNullOrEmpty Then
            Return DirectCast(customAttrs(Scan0), DescriptionAttribute).Description
        Else
            Return [default]
        End If
    End Function

    <Extension>
    Public Function Category(m As MemberInfo, Optional default$ = Nothing) As String
        Dim customAttrs() = m.GetCustomAttributes(GetType(CategoryAttribute), inherit:=False)

        If Not customAttrs.IsNullOrEmpty Then
            Return DirectCast(customAttrs(Scan0), CategoryAttribute).Category
        Else
            Return [default]
        End If
    End Function

    ''' <summary>
    ''' Get object usage information
    ''' </summary>
    ''' <param name="m"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Usage(m As MemberInfo) As String
        Dim attr As UsageAttribute = m.GetCustomAttribute(Of UsageAttribute)

        If attr Is Nothing Then
            Return Nothing
        Else
            Return attr.UsageInfo
        End If
    End Function

    ''' <summary>
    ''' Get example code of the <see cref="Usage"/>
    ''' </summary>
    ''' <param name="m"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ExampleInfo(m As MemberInfo) As String
        Dim attr As ExampleAttribute = m.GetCustomAttribute(Of ExampleAttribute)

        If attr Is Nothing Then
            Return Nothing
        Else
            Return attr.ExampleInfo
        End If
    End Function

    ''' <summary>
    ''' Get the scripting namespace value from <see cref="[Namespace]"/>
    ''' </summary>
    ''' <param name="app"></param>
    ''' <returns></returns>
    '''
    <ExportAPI("Get.API.Namespace")>
    <Extension>
    Public Function NamespaceEntry(app As Type, Optional nullWrapper As Boolean = False) As [Namespace]
        Dim attr As Object() = Nothing

        Try
            attr = app.GetCustomAttributes(GetType([Namespace]), True)
        Catch ex As Exception
            Call LogException(New Exception(app.FullName, ex))
        End Try

        If attr.IsNullOrEmpty Then
            Dim descr$ = app.FullName
            If nullWrapper Then
                descr = $"< {descr} >"
            End If
            Return New [Namespace](app.Name, descr, True)
        Else
            Return DirectCast(attr(Scan0), [Namespace])
        End If
    End Function
End Module
