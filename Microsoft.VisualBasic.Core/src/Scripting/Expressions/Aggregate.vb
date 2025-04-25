#Region "Microsoft.VisualBasic::d5505ba27b80af1091978a0c60c5f90f, Microsoft.VisualBasic.Core\src\Scripting\Expressions\Aggregate.vb"

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

    '   Total Lines: 78
    '    Code Lines: 46 (58.97%)
    ' Comment Lines: 21 (26.92%)
    '    - Xml Docs: 95.24%
    ' 
    '   Blank Lines: 11 (14.10%)
    '     File Size: 2.71 KB


    '     Enum Aggregates
    ' 
    '         Invalid, Max, Mean, Median, Min
    '         Sum
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module Aggregate
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) GetAggregateFunction, ParseFlag
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Statistics.Linq
Imports std = System.Math

Namespace Scripting.Expressions

    Public Enum Aggregates
        Invalid

        Min
        Max
        ''' <summary>
        ''' the average value
        ''' </summary>
        Mean
        Median
        Sum
    End Enum

    ''' <summary>
    ''' Helper module for get lambda function by scripting text
    ''' </summary>
    Public Module Aggregate

        ReadOnly aggregateFlags As Dictionary(Of String, Aggregates)

        Sub New()
            aggregateFlags = Enums(Of Aggregates).ToDictionary(Function(e) e.Description.ToLower)
            aggregateFlags("average") = Aggregates.Mean
            aggregateFlags("avg") = Aggregates.Mean
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ParseFlag(name As String) As Aggregates
            Return aggregateFlags.TryGetValue(LCase(name), [default]:=Aggregates.Invalid)
        End Function

        ''' <summary>
        ''' Get ``Aggregate`` function by term.
        ''' </summary>
        ''' <param name="name">
        ''' + max
        ''' + min
        ''' + average
        ''' </param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GetAggregateFunction(name As String) As Func(Of IEnumerable(Of Double), Double)
            Return ParseFlag(name).GetAggregateFunction
        End Function

        ''' <summary>
        ''' Get ``Aggregate`` function by term.
        ''' </summary>
        ''' <param name="aggregate"></param>
        ''' <returns>
        ''' A lambda function for aggregate a numeric vector
        ''' </returns>
        ''' 
        <Extension>
        Public Function GetAggregateFunction(aggregate As Aggregates) As Func(Of IEnumerable(Of Double), Double)
            Select Case aggregate
                Case Aggregates.Max : Return Function(x) x.Max
                Case Aggregates.Min : Return Function(x) x.Min
                Case Aggregates.Mean : Return Function(x) x.Average
                Case Aggregates.Median : Return Function(x) x.Median
                Case Aggregates.Sum : Return Function(x) x.Sum

                Case Else
                    Return CommonThrowInvalid(aggregate)
            End Select
        End Function

        Private Function CommonThrowInvalid(aggregate As Aggregates) As Object
            If aggregate = Aggregates.Invalid Then
                Throw New InvalidProgramException("You should choose a aggregate method!")
            End If

            Throw New NotImplementedException(aggregate)
        End Function

        ''' <summary>
        ''' Helper function for get lambda function by scripting text for two number parameter
        ''' </summary>
        ''' <param name="aggregate">
        ''' + max
        ''' + min
        ''' + average
        ''' + sum
        ''' + median
        ''' </param>
        ''' <returns>
        ''' a lambda function implements of interface y = f(a,b)
        ''' </returns>
        <Extension>
        Public Function GetAggregateFunction2(aggregate As Aggregates) As Func(Of Double, Double, Double)
            Select Case aggregate
                Case Aggregates.Max : Return AddressOf std.Max
                Case Aggregates.Mean : Return Function(a, b) (a + b) / 2
                Case Aggregates.Median : Return Function(a, b) (a + b) / 2
                Case Aggregates.Min : Return AddressOf std.Min
                Case Aggregates.Sum : Return Function(a, b) a + b

                Case Else
                    Return CommonThrowInvalid(aggregate)
            End Select
        End Function
    End Module
End Namespace
