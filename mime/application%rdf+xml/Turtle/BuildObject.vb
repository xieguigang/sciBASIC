#Region "Microsoft.VisualBasic::683409d253c45d404a140a7ba3eb46a3, mime\application%rdf+xml\Turtle\BuildObject.vb"

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

    '   Total Lines: 54
    '    Code Lines: 38 (70.37%)
    ' Comment Lines: 5 (9.26%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (20.37%)
    '     File Size: 1.79 KB


    '     Module BuildObject
    ' 
    '         Function: CreateObject, PopulateObjects
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Turtle

    Public Module BuildObject

        ''' <summary>
        ''' build rdf object from the ttl data
        ''' </summary>
        ''' <param name="ttl"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function PopulateObjects(ttl As IEnumerable(Of Triple)) As IEnumerable(Of RDFEntity)
            Dim objs = ttl.GroupBy(Function(t) t.subject).ToArray

            For Each group As IGrouping(Of String, Triple) In objs
                Yield group.CreateObject
            Next
        End Function

        <Extension>
        Private Function CreateObject(group As IGrouping(Of String, Triple)) As RDFEntity
            Dim objData As New Dictionary(Of String, RDFEntity)

            For Each tuple As IGrouping(Of String, Relation) In group _
                .Select(Function(t) t.relations) _
                .IteratesALL _
                .GroupBy(Function(r) r.predicate)

                Dim data As New Dictionary(Of String, RDFEntity)
                Dim i As i32 = 1

                For Each val As String In tuple _
                    .Select(Function(ti) ti.objs) _
                    .IteratesALL

                    Call data.Add(++i, New RDFEntity With {.value = {val}})
                Next

                objData(tuple.Key) = New RDFEntity With {
                    .RDFId = tuple.Key,
                    .Properties = data
                }
            Next

            Return New RDFEntity With {
                .RDFId = group.Key,
                .Properties = objData
            }
        End Function
    End Module
End Namespace
