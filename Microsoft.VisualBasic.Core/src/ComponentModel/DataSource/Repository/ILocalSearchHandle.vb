#Region "Microsoft.VisualBasic::4aacb526634afe2328c66289f8b181f5, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Repository\ILocalSearchHandle.vb"

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

    '   Total Lines: 98
    '    Code Lines: 61 (62.24%)
    ' Comment Lines: 23 (23.47%)
    '    - Xml Docs: 91.30%
    ' 
    '   Blank Lines: 14 (14.29%)
    '     File Size: 3.98 KB


    '     Interface ILocalSearchHandle
    ' 
    '         Function: Match, Matches
    ' 
    '     Module SearchFramework
    ' 
    '         Function: MultipleQuery, Query, UniqueNames
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace ComponentModel.DataSourceModel.Repository

    Public Interface ILocalSearchHandle

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Keyword"></param>
        ''' <param name="CaseSensitive">是否大小写敏感，默认不敏感</param>
        ''' <returns></returns>
        Function Matches(Keyword As String, Optional CaseSensitive As CompareMethod = CompareMethod.Text) As ILocalSearchHandle()
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Keyword"></param>
        ''' <param name="CaseSensitive">是否大小写敏感，默认不敏感</param>
        ''' <returns></returns>
        Function Match(Keyword As String, Optional CaseSensitive As CompareMethod = CompareMethod.Text) As Boolean
    End Interface

    ''' <summary>
    ''' Extensions for data query
    ''' </summary>
    Public Module SearchFramework

        <Extension>
        Public Iterator Function MultipleQuery(Of T, Term)(source As IEnumerable(Of T),
                                                           query As IEnumerable(Of NamedValue(Of Term())),
                                                           assert As Func(Of T, Term, Boolean)) As IEnumerable(Of NamedValue(Of Map(Of Term, T)))

            Dim terms As NamedValue(Of Term())() = query.ToArray

            For Each entity As T In source
                For Each block In query
                    For Each match As Term In block.Value
                        If assert(entity, match) Then
                            Yield New NamedValue(Of Map(Of Term, T)) With {
                                .Name = block.Name,
                                .Value = New Map(Of Term, T) With {
                                    .Key = match,
                                    .Maps = entity
                                }
                            }
                        End If
                    Next
                Next
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Query(Of T, Term)(source As IEnumerable(Of T), queries As IEnumerable(Of Term), assert As Func(Of T, Term, Boolean)) As IEnumerable(Of Map(Of Term, T))
            Return source.MultipleQuery({New NamedValue(Of Term())("null", queries.ToArray)}, assert).Values
        End Function

        ''' <summary>
        ''' makes the name string unique by adding an additional numeric suffix
        ''' </summary>
        ''' <param name="names"></param>
        ''' <returns>
        ''' this function is a safe function, it will returns an empty string collection 
        ''' if the given <paramref name="names"/> is nothing.
        ''' </returns>
        <Extension>
        Public Function UniqueNames(names As IEnumerable(Of String), <Out> Optional ByRef duplicated As String() = Nothing) As String()
            Dim nameUniques As New Dictionary(Of String, Counter)
            Dim duplicates As New List(Of String)

            If names Is Nothing Then
                Return New String() {}
            End If

            For Each name As String In names
RE0:
                If nameUniques.ContainsKey(name) Then
                    nameUniques(name).Hit()
                    duplicates.Add(name)
                    name = name & "_" & nameUniques(name).Value
                    GoTo RE0
                Else
                    nameUniques.Add(name, Scan0)
                End If
            Next

            Erase duplicated

            If duplicates.Any Then
                duplicated = duplicates.ToArray
            End If

            Return nameUniques.Keys.ToArray
        End Function
    End Module
End Namespace
