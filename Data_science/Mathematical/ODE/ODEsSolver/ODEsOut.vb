#Region "Microsoft.VisualBasic::3fc857efae4a050bc96c45ae5e32132d, ..\sciBASIC#\Data_science\Mathematical\ODE\ODEsSolver\ODEsOut.vb"

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
    ''' 
    ''' </summary>
    ''' <param name="csv$"></param>
    ''' <param name="noVars">ODEs Parameter value is not exists in the data file?</param>
    ''' <returns></returns>
    Public Shared Function LoadFromDataFrame(csv$, Optional noVars As Boolean = False) As ODEsOut
        Return StreamExtension.LoadFromDataFrame(csv, noVars)
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
