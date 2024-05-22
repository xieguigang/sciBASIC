#Region "Microsoft.VisualBasic::6b1aa8b0f8662239a8cbb4123c7ad2f7, Data_science\Mathematica\Math\ODESolver.Extensions\Extensions.vb"

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

    '   Total Lines: 163
    '    Code Lines: 114 (69.94%)
    ' Comment Lines: 25 (15.34%)
    '    - Xml Docs: 84.00%
    ' 
    '   Blank Lines: 24 (14.72%)
    '     File Size: 6.19 KB


    ' Module Extensions
    ' 
    '     Function: __getArgs, correlationImpl, (+2 Overloads) DataFrame, LoadFromDataFrame, Pcc
    '               SPcc
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File
Imports std = System.Math

<HideModuleName> Public Module Extensions

    <Extension>
    Public Function DataFrame(out As ODEOutput) As csv
        Dim csv As New csv

        csv += {"X", "Y"}
        csv += out.X _
            .ToArray _
            .Select(Function(x, i)
                        Return New RowObject(New String() {x, out.Y(i)})
                    End Function)

        Return csv
    End Function

    ''' <summary>
    ''' Generates datafram and then can makes the result save data into a csv file.
    ''' </summary>
    ''' <param name="xDisp"></param>
    ''' <param name="fix%">Formats output by using <see cref="std.Round"/></param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function DataFrame(df As ODEsOut, Optional xDisp As String = "X", Optional fix% = -1) As IO.File
        Dim ly = df.y.Values.ToArray
        Dim file As New IO.File
        Dim head As New RowObject(xDisp + ly.ToList(Function(s) s.name))
        Dim round As Func(Of Double, String)

        If fix <= 0 Then
            round = Function(n) CStr(n)
        Else
            round = Function(n) CStr(std.Round(n, fix))
        End If

        file += head

#If DEBUG Then
        Call $"len(x) = {df.x.Length}".__DEBUG_ECHO
        Call Console.WriteLine()

        For Each y As NamedCollection(Of Double) In df.y.Values
            Call $"len(y={y.Name}) = {y.Value.Length}".__DEBUG_ECHO
        Next
#End If

        For Each x As SeqValue(Of Double) In df.x.SeqIterator
            file += (round(x.value) + ly.ToList(Function(n) round(n.value(x.i))))
        Next

        Dim skips As Integer = ly.Length + 2

        For Each v In df.y0.SafeQuery.JoinIterates(df.params).SeqIterator
            Dim row As RowObject = file(v.i)
            Dim var As KeyValuePair(Of String, Double) = v.value

            row(skips) = var.Key
            row(skips + 1) = var.Value
        Next

        Return file
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="csv$"></param>
    ''' <param name="noVars">ODEs Parameter value is not exists in the data file?</param>
    ''' <returns></returns>
    Public Function LoadFromDataFrame(csv$, Optional noVars As Boolean = False) As ODEsOut
        Dim df As File = File.Load(csv)
        Dim cols = df.Columns().ToArray
        Dim X = cols(Scan0)
        Dim y$()() = If(noVars,
            cols.Skip(1).ToArray,
            cols.Skip(1).Take(cols.Length - 1 - 3).ToArray)
        Dim params$()() = cols.Skip(cols.Length - 2).ToArray
        Dim args As Dictionary(Of String, Double) = If(
            noVars,
            New Dictionary(Of String, Double),
            __getArgs(params(Scan0), params(1))) ' 由于没有信息可以了解哪些变量是y0初始值，所以在这里都把这些数据放在变量参数列表里面
        Dim yData = LinqAPI.Exec(Of NamedCollection(Of Double)) <= From s As String()
                                                                   In y
                                                                   Let name As String = s(Scan0)
                                                                   Let values As Double() = s.Skip(1).Select(AddressOf Val).ToArray
                                                                   Select New NamedCollection(Of Double) With {
                                                                       .name = name,
                                                                       .value = values
                                                                   }
        Return New ODEsOut With {
            .params = args,
            .x = X.Skip(1).Select(AddressOf Val).ToArray,
            .y = yData.ToDictionary
        }
    End Function

    Private Function __getArgs(names As String(), values As String()) As Dictionary(Of String, Double)
        Dim params As New Dictionary(Of String, Double)

        For Each var As SeqValue(Of String) In names.SeqIterator
            If String.IsNullOrEmpty(+var) Then
                Exit For
            Else
                params(+var) = Val(values(var.i))
            End If
        Next

        Return params
    End Function

    ''' <summary>
    ''' 使用PCC来了解各个变量之间的相关度
    ''' </summary>
    ''' <param name="df"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Pcc(df As ODEsOut) As IEnumerable(Of DataSet)
        Return df.correlationImpl(AddressOf Correlations.GetPearson)
    End Function

    <Extension>
    Private Iterator Function correlationImpl(df As ODEsOut, correlation As Correlations.ICorrelation) As IEnumerable(Of DataSet)
        Dim vars$() = df.y.Keys.ToArray

        For Each var As NamedCollection(Of Double) In df
            Dim x As New DataSet With {
                .ID = var.name,
                .Properties = New Dictionary(Of String, Double)
            }

            For Each name$ In vars
                x.Properties(name$) = correlation(var.value, df.y(name).value)
            Next

            Yield x
        Next
    End Function

    ''' <summary>
    ''' 使用sPCC来了解各个变量之间的相关度
    ''' </summary>
    ''' <param name="df"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function SPcc(df As ODEsOut) As IEnumerable(Of DataSet)
        Return df.correlationImpl(AddressOf Correlations.Spearman)
    End Function
End Module
