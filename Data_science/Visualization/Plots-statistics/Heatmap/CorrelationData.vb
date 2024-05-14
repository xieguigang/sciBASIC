#Region "Microsoft.VisualBasic::2925e31f95f6bafbad82bbb6c889e7ad, Data_science\Visualization\Plots-statistics\Heatmap\CorrelationData.vb"

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

    '   Total Lines: 77
    '    Code Lines: 60
    ' Comment Lines: 0
    '   Blank Lines: 17
    '     File Size: 2.29 KB


    '     Class CorrelationData
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetLevel, GetMatrix, SetKeyOrders, SetLevels
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.DataFrame

Namespace Heatmap

    Public Class CorrelationData

        Friend data As DataMatrix
        Friend min#, max#
        Friend range As DoubleRange
        Friend levelRange As DoubleRange

        Dim reoderKeys As String()

        Default Public ReadOnly Property Value(i As Integer, j As Integer) As Double
            Get
                If reoderKeys Is Nothing Then
                    Return data(i, j)
                Else
                    Return data(reoderKeys(i), reoderKeys(j))
                End If
            End Get
        End Property

        Sub New(data As DataMatrix, Optional range As DoubleRange = Nothing)
            With range Or data _
                .PopulateRows _
                .IteratesALL _
                .ToArray _
                .Range _
                .AsDefault

                min = .Min
                max = .Max

                If TypeOf data Is DistanceMatrix Then
                    range = {0, .Max}
                Else
                    range = {min, max}
                End If
            End With

            Me.data = data
            Me.range = range
        End Sub

        Public Function GetLevel(i As Integer, j As Integer) As Integer
            Dim value As Double = Me(i, j)
            Dim level As Integer = CInt(range.ScaleMapping(value, levelRange))

            Return level
        End Function

        Public Function SetLevels(levels As Integer) As CorrelationData
            levelRange = New Double() {0, levels - 1}
            Return Me
        End Function

        Public Function SetKeyOrders(orders As IEnumerable(Of String)) As CorrelationData
            reoderKeys = orders.ToArray
            Return Me
        End Function

        Public Function GetMatrix() As Double()()
            Dim rows As New List(Of Double())

            For Each row As IReadOnlyCollection(Of Double) In data.PopulateRows
                rows.Add(row.ToArray)
            Next

            Return rows.ToArray
        End Function
    End Class

End Namespace
