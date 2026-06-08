#Region "Microsoft.VisualBasic::269cce6c4917d85dc33b00461b298761, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Repository\SearchFramework.vb"

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

    '   Total Lines: 181
    '    Code Lines: 110 (60.77%)
    ' Comment Lines: 42 (23.20%)
    '    - Xml Docs: 88.10%
    ' 
    '   Blank Lines: 29 (16.02%)
    '     File Size: 7.15 KB


    '     Module SearchFramework
    ' 
    '         Function: GetUniqueNames, MakeUniqueNames, MultipleQuery, Query, (+2 Overloads) UniqueNames
    ' 
    '     Class MakeUniqueName
    ' 
    '         Properties: UniqueIDs
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetUniqueID
    ' 
    '         Sub: Clear
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.DataSourceModel.Repository

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
        ''' A general method for make the tuple name unique
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="list"></param>
        ''' <param name="duplicated"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' the name is uniqued via the <see cref="UniqueNames"/> method 
        ''' via add numeric suffix to the name key string.
        ''' </remarks>
        <Extension>
        Public Function UniqueNames(Of T)(list As IEnumerable(Of NamedValue(Of T)), <Out> Optional ByRef duplicated As String() = Nothing) As NamedValue(Of T)()
            Dim alldata As NamedValue(Of T)() = list.SafeQuery.ToArray
            Dim unique As String() = alldata.GetUniqueNames(duplicated)

            If duplicated.IsNullOrEmpty Then
                Return alldata
            End If

            For i As Integer = 0 To alldata.Length - 1
                alldata(i) = New NamedValue(Of T)(unique(i), alldata(i).Value, alldata(i).Description)
            Next

            Return alldata
        End Function

        <Extension>
        Public Function GetUniqueNames(Of T As INamedValue)(list As IEnumerable(Of T), <Out> Optional ByRef duplicated As String() = Nothing) As String()
            Return list.Select(Function(a) a.Key).UniqueNames(duplicated)
        End Function

        ''' <summary>
        ''' update and set the unique names to the index collection
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="list"></param>
        ''' <param name="duplicated"></param>
        ''' <returns></returns>
        <Extension>
        Public Function MakeUniqueNames(Of T As INamedValue)(list As IEnumerable(Of T), <Out> Optional ByRef duplicated As String() = Nothing) As IEnumerable(Of T)
            Dim alldata As T() = list.SafeQuery.ToArray
            Dim allnames As String() = alldata.Select(Function(i) i.Key).UniqueNames(duplicated).ToArray

            If duplicated.IsNullOrEmpty Then
                ' no duplicated items
                ' returns the rawdata directly
                Return alldata
            Else
                ' replace the old name with new unique names
                ' value and description has no changes
                For i As Integer = 0 To alldata.Length - 1
                    alldata(i).Key = allnames(i)
                Next

                Return alldata
            End If
        End Function

        ''' <summary>
        ''' makes the name string unique by adding an additional numeric suffix
        ''' </summary>
        ''' <param name="names"></param>
        ''' <param name="duplicated">
        ''' get the duplicated names
        ''' </param>
        ''' <returns>
        ''' this function is a safe function, it will returns an empty string collection 
        ''' if the given <paramref name="names"/> is nothing. this function returns a string
        ''' array with the same size equals to the input string collection with duplicated values
        ''' renamed with numeric counter suffix
        ''' </returns>
        ''' <remarks>
        ''' this function will erase the <paramref name="duplicated"/> at first and then make value assigned.
        ''' </remarks>
        <Extension>
        Public Function UniqueNames(names As IEnumerable(Of String), <Out> Optional ByRef duplicated As String() = Nothing, Optional sep As String = "_") As String()
            Erase duplicated

            If names Is Nothing Then
                Return New String() {}
            End If

            Dim unique As New MakeUniqueName(sep)

            For Each name As String In names
                Call unique.GetUniqueID(name)
            Next

            If unique.duplicates.Any Then
                duplicated = unique.duplicates.ToArray
            End If

            Return unique.UniqueIDs.ToArray
        End Function
    End Module

    Public Class MakeUniqueName

        Dim nameUniques As New Dictionary(Of String, Counter)
        Dim sep As String

        Friend duplicates As New List(Of String)

        Public ReadOnly Property UniqueIDs As IEnumerable(Of String)
            Get
                Return nameUniques.Keys
            End Get
        End Property

        Sub New(sep As String)
            Me.sep = sep
        End Sub

        Public Sub Clear()
            Call nameUniques.Clear()
            Call duplicates.Clear()
        End Sub

        Public Function GetUniqueID(name As String) As String
            ' 20251109 null value will be safely treated as empty string at here
            name = If(name, "")
RE0:
            If nameUniques.ContainsKey(name) Then
                nameUniques(name).Hit()
                duplicates.Add(name)
                name = name & sep & CInt(nameUniques(name)).ToString
                GoTo RE0
            Else
                nameUniques.Add(name, Scan0)
            End If

            Return name
        End Function

    End Class
End Namespace
