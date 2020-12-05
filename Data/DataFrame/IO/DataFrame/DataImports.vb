#Region "Microsoft.VisualBasic::937998757567489ef929954fdff8b737, Data\DataFrame\IO\DataFrame\DataImports.vb"

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

    '     Module DataImports
    ' 
    '         Function: (+2 Overloads) ParseVector, SampleForType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace IO

    Public Module DataImports

        Public Function SampleForType(column As String()) As Type
            Dim typeHits As New Dictionary(Of Type, Integer)

            For Each type As Type In New Type() {
                GetType(Integer),
                GetType(Double),
                GetType(Boolean),
                GetType(Date),
                GetType(String)
            }
                typeHits(type) = 0
            Next

            For Each r As String In column
                If r.IsInteger Then
                    typeHits(GetType(Integer)) += 1
                ElseIf IsBooleanFactor(r) Then
                    typeHits(GetType(Boolean)) += 1
                ElseIf r.IsNumeric(includesNaNFactor:=True) Then
                    typeHits(GetType(Double)) += 1
                ElseIf Date.TryParse(r, Nothing) Then
                    typeHits(GetType(Date)) += 1
                Else
                    typeHits(GetType(String)) += 1
                End If
            Next

            Return typeHits _
                .OrderByDescending(Function(a) a.Value) _
                .First _
                .Key
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ParseVector(column As String()) As Array
            Return ParseVector(column, SampleForType(column))
        End Function

        Public Function ParseVector(column As String(), type As Type) As Array
            Select Case type
                Case GetType(Integer)
                    Return column.Select(Function(str) str.ParseInteger).ToArray
                Case GetType(Double)
                    Return column.Select(Function(str) str.ParseDouble).ToArray
                Case GetType(Boolean)
                    Return column.Select(Function(str) str.ParseBoolean).ToArray
                Case GetType(Date)
                    Return column.Select(Function(str) str.ParseDate).ToArray
                Case Else
                    Return column
            End Select
        End Function
    End Module
End Namespace
