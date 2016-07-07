#Region "Microsoft.VisualBasic::24629e01f5e77baa221a9e26f2b8c8fe, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\Extensions\Math\NumberGroups.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Parallel

''' <summary>
''' Simple number vector grouping
''' </summary>
Public Module NumberGroups

    <Extension>
    Public Function Groups(source As IEnumerable(Of Integer), offset As Integer) As List(Of Integer())
        Dim list As New List(Of Integer())
        Dim orders As Integer() = (From n As Integer
                                   In source
                                   Select n
                                   Order By n Ascending).ToArray
        Dim tag As Integer = orders(Scan0)
        Dim tmp As New List(Of Integer) From {tag}

        For Each x As Integer In orders.Skip(1)
            If x - tag <= offset Then  ' 因为已经是经过排序了的，所以后面总是大于前面的
                tmp += x
            Else
                tag = x
                list += tmp.ToArray
                tmp = New List(Of Integer) From {x}
            End If
        Next

        If tmp.Count > 0 Then
            list += tmp.ToArray
        End If

        Return list
    End Function

    <Extension>
    Public Function Groups(Of TagObject As INumberTag)(source As IEnumerable(Of TagObject), offset As Integer) As GroupResult(Of TagObject, Integer)()
        Dim list As New List(Of GroupResult(Of TagObject, Integer))
        Dim orders As TagObject() = (From x As TagObject
                                     In source
                                     Select x
                                     Order By x.Tag Ascending).ToArray
        Dim tag As TagObject = orders(Scan0)
        Dim tmp As New List(Of TagObject) From {tag}

        For Each x As TagObject In orders.Skip(1)
            If x.Tag - tag.Tag <= offset Then  ' 因为已经是经过排序了的，所以后面总是大于前面的
                tmp += x
            Else
                tag = x
                list += New GroupResult(Of TagObject, Integer)(tag.Tag, tmp)
                tmp = New List(Of TagObject) From {x}
            End If
        Next

        If tmp.Count > 0 Then
            list += New GroupResult(Of TagObject, Integer)(tag.Tag, tmp)
        End If

        Return list
    End Function
End Module

Public Interface INumberTag

    ReadOnly Property Tag As Integer
End Interface
