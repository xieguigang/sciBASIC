#Region "Microsoft.VisualBasic::be1eb4e343c88f87b2fc506ac64b84e6, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Repository\StringTemplate.vb"

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

    '   Total Lines: 120
    '    Code Lines: 69 (57.50%)
    ' Comment Lines: 25 (20.83%)
    '    - Xml Docs: 92.00%
    ' 
    '   Blank Lines: 26 (21.67%)
    '     File Size: 4.16 KB


    '     Interface IKeyDataReader
    ' 
    '         Function: GetData
    ' 
    '     Class StringTemplate
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CreateDefaultString, CreateString, GetLastMissingKeys, ParseKeys, ToString
    ' 
    '         Sub: SetDefaultKey
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text

Namespace ComponentModel.DataSourceModel.Repository

    Public Interface IKeyDataReader

        Function GetData(key As String) As String

    End Interface

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' template syntax example:
    ''' 
    ''' &lt;locus_tag>|taxid|&lt;product>
    ''' </remarks>
    Public Class StringTemplate

        ReadOnly template As String
        ReadOnly defaults As Dictionary(Of String, String)
        ''' <summary>
        ''' a collection mapping of [key -> &lt;key>] for get value and make string template interpolation
        ''' </summary>
        ReadOnly keys As NamedValue(Of String)()
        ReadOnly missing As New List(Of String)

        Sub New(template As String, Optional defaults As Dictionary(Of String, String) = Nothing)
            Me.template = template
            Me.defaults = If(defaults, New Dictionary(Of String, String))
            Me.keys = ParseKeys(template).ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Iterator Function ParseKeys(template As String) As IEnumerable(Of NamedValue(Of String))
            For Each placeholder As String In template.Matches("[<].*?[>]", RegexICSng)
                Yield New NamedValue(Of String)(placeholder.GetStackValue("<", ">"), placeholder)
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetDefaultKey(key As String, data As String)
            defaults(key) = data
        End Sub

        Public Function GetLastMissingKeys() As IEnumerable(Of String)
            Return missing
        End Function

        ''' <summary>
        ''' Create a string based on the given template with default key value
        ''' </summary>
        ''' <returns></returns>
        Public Function CreateDefaultString() As String
            Dim str As New StringBuilder(template)
            Dim value As String

            Call missing.Clear()
            Const unknown As String = "unknown"

            For Each placeholder As NamedValue(Of String) In keys
                value = defaults.TryGetValue(placeholder.Name, default:=unknown)

                If value = unknown Then
                    Call missing.Add(placeholder.Name)
                End If

                Call str.Replace(placeholder.Value, value)
            Next

            Return str.ToString
        End Function

        ''' <summary>
        ''' Create a string based on the given template
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Public Function CreateString(Of T As IKeyDataReader)(i As T) As String
            Dim str As New StringBuilder(template)
            Dim value As String

            Call missing.Clear()
            Const unknown As String = "unknown"

            For Each placeholder As NamedValue(Of String) In keys
                value = i.GetData(placeholder.Name)

                If value.StringEmpty Then
                    value = defaults.TryGetValue(placeholder.Name, default:=unknown)

                    If value = unknown Then
                        Call missing.Add(placeholder.Name)
                    End If
                End If

                Call str.Replace(placeholder.Value, value)
            Next

            Return str.ToString
        End Function

        ''' <summary>
        ''' Create a string based on the given template with default key value
        ''' </summary>
        ''' <returns><see cref="CreateDefaultString"/></returns>
        Public Overrides Function ToString() As String
            Return CreateDefaultString()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(template As String) As StringTemplate
            Return New StringTemplate(template)
        End Operator

    End Class
End Namespace
