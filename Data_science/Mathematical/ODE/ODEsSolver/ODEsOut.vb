#Region "Microsoft.VisualBasic::8e1772cf541d1296fba919ef782abe74, ..\sciBASIC#\Data_science\Mathematical\ODE\ODEsOut.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' ODEs output, this object can populates the <see cref="ODEsOut.y"/> 
''' variables values through its enumerator interface.
''' </summary>
Public Class ODEsOut
    Implements IEnumerable(Of NamedValue(Of Double()))

    Public Property x As Double()
    Public Property y As Dictionary(Of NamedValue(Of Double()))
    Public Property y0 As Dictionary(Of String, Double)
    Public Property params As Dictionary(Of String, Double)

    Public ReadOnly Property dx As Double
        Get
            Return x(2) - x(1)
        End Get
    End Property

    ''' <summary>
    ''' Using the first value of <see cref="y"/> as ``y0``
    ''' </summary>
    ''' <returns></returns>
    Public Function GetY0() As Dictionary(Of String, Double)
        Return y.ToDictionary(Function(x) x.Key,
                              Function(x) x.Value.Value.First)
    End Function

    ''' <summary>
    ''' Is there NAN value in the function value <see cref="y"/> ???
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property HaveNaN As Boolean
        Get
            For Each val As NamedValue(Of Double()) In y.Values
                For Each x As Double In val.Value
                    If Double.IsNaN(x) OrElse
                        Double.IsInfinity(x) OrElse
                        Double.IsNegativeInfinity(x) OrElse
                        Double.IsPositiveInfinity(x) Then

                        Return True
                    End If
                Next
            Next

            Return False
        End Get
    End Property

    ''' <summary>
    ''' Merge <see cref="y0"/> into <see cref="params"/>
    ''' </summary>
    Public Sub Join()
        For Each v In y0
            Call params.Add(v.Key, v.Value)
        Next
    End Sub

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    ''' <summary>
    ''' Generates datafram and then can makes the result save data into a csv file.
    ''' </summary>
    ''' <param name="xDisp"></param>
    ''' <param name="fix%">Formats output by using <see cref="Math.Round"/></param>
    ''' <returns></returns>
    Public Function DataFrame(Optional xDisp As String = "X", Optional fix% = -1) As DocumentStream.File
        Dim ly = y.Values.ToArray
        Dim file As New DocumentStream.File
        Dim head As New RowObject(xDisp + ly.ToList(Function(s) s.Name))
        Dim round As Func(Of Double, String)

        If fix <= 0 Then
            round = Function(n) CStr(n)
        Else
            round = Function(n) CStr(Math.Round(n, fix))
        End If

        file += head

        For Each x As SeqValue(Of Double) In Me.x.SeqIterator
            file += (round(x.value) + ly.ToList(Function(n) round(n.Value(x.i))))
        Next

        Dim skips As Integer = ly.Length + 2

        For Each v In y0.SafeQuery.JoinIterates(params).SeqIterator
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
    Public Shared Function LoadFromDataFrame(csv$, Optional noVars As Boolean = False) As ODEsOut
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
        Dim yData As NamedValue(Of Double())() =
            LinqAPI.Exec(Of NamedValue(Of Double())) <= From s As String()
                                                        In y
                                                        Let name As String = s(Scan0)
                                                        Let values As Double() = s.Skip(1).ToArray(AddressOf Val)
                                                        Select New NamedValue(Of Double()) With {
                                                            .Name = name,
                                                            .Value = values
                                                        }
        Return New ODEsOut With {
            .params = args,
            .x = X.Skip(1).ToArray(AddressOf Val),
            .y = yData.ToDictionary
        }
    End Function

    Private Shared Function __getArgs(names As String(), values As String()) As Dictionary(Of String, Double)
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

    Public Iterator Function GetEnumerator() As IEnumerator(Of NamedValue(Of Double())) Implements IEnumerable(Of NamedValue(Of Double())).GetEnumerator
        For Each var As NamedValue(Of Double()) In y.Values
            Yield var
        Next
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class
