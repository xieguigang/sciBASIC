#Region "Microsoft.VisualBasic::6b34b2b584e8a1743bdbb092ba006eaa, Microsoft.VisualBasic.Core\src\Data\TypeCast\DataImports.vb"

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

    '   Total Lines: 123
    '    Code Lines: 83
    ' Comment Lines: 24
    '   Blank Lines: 16
    '     File Size: 4.53 KB


    '     Module DataImports
    ' 
    '         Function: IsPossibleEnumFactor, (+2 Overloads) ParseVector, SampleForType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.DataSourceModel.TypeCast

    Public Module DataImports

        ''' <summary>
        ''' Sampling column data for test data type
        ''' </summary>
        ''' <param name="column"></param>
        ''' <returns></returns>
        Public Function SampleForType(column As IReadOnlyCollection(Of String)) As Type
            Dim integers As Integer = 0,
                doubles As Integer = 0,
                booleans As Integer = 0,
                dates As Integer = 0,
                strings As Integer = 0

            For Each r As String In column
                If String.IsNullOrEmpty(r) Then
                    Continue For
                End If

                If r.IsInteger Then
                    integers += 1

                    ' 20230613 we should disable the boolean extenede literal
                    ' at here, or some enum string factor example like yes/no/right/wrong
                    ' will be get confused with the string or enum type
                    '
                    ' just check for the true/false at here
                    '
                ElseIf IsBooleanFactor(r, extendedLiteral:=False) Then
                    booleans += 1
                ElseIf r.IsNumeric(includesNaNFactor:=True) Then
                    doubles += 1
                ElseIf Date.TryParse(r, Nothing) Then
                    dates += 1
                Else
                    strings += 1
                End If
            Next

            If strings > 0 Then
                Return GetType(String)
            End If

            If {integers, doubles, booleans, dates}.All(Function(x) x = 0) Then
                Return GetType(String)
            End If

            If {integers, doubles, booleans, dates}.All(Function(x) x > 0) Then
                Return GetType(String)
            End If

            Dim multiples As Integer = {integers, doubles, booleans, dates} _
                .Where(Function(a) a > 0) _
                .Count

            If multiples > 1 Then
                If integers > 0 AndAlso doubles > 0 Then
                    Return GetType(Double)
                Else
                    Return GetType(String)
                End If
            End If

            If doubles > 0 Then
                Return GetType(Double)
            End If
            If integers > 0 Then
                Return GetType(Integer)
            End If
            If booleans > 0 Then
                Return GetType(Boolean)
            End If
            If dates > 0 Then
                Return GetType(Date)
            End If

            Return GetType(String)
        End Function

        ''' <summary>
        ''' Check the given string collection is a possible enum factor type?
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsPossibleEnumFactor(v As IReadOnlyCollection(Of String)) As Boolean
            Return v.Distinct.Count <= CInt(v.Count * 0.9)
        End Function

        ''' <summary>
        ''' Measuring the data type automatically, and then try to parse the given
        ''' string vector as the array of result data type
        ''' </summary>
        ''' <param name="column"></param>
        ''' <returns></returns>
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
                    ' do nothing for string vector type
                    Return column
            End Select
        End Function
    End Module
End Namespace
