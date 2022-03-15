#Region "Microsoft.VisualBasic::487251907ece296d2d6c797dc49ada2c, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Darwinism\GAF\Drivers\RawCompare.vb"

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

    '   Total Lines: 90
    '    Code Lines: 67
    ' Comment Lines: 10
    '   Blank Lines: 13
    '     File Size: 3.47 KB


    '     Class RawCompare
    ' 
    '         Properties: Cacheable
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Calculate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ODEs
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.Math.Calculus
Imports DynamicsSystem = Microsoft.VisualBasic.Math.Calculus.ODEs

Namespace Darwinism.GAF.Driver

    ''' <summary>
    ''' 不像<see cref="GAFFitness"/>接受的是经过插值处理的原始数据，
    ''' 这个``fitness``驱动程序接受的是未经过任何处理的原始数据
    ''' </summary>
    Public Class RawCompare : Implements Fitness(Of ParameterVector)

        ''' <summary>
        ''' TIME
        ''' </summary>
        Dim X#()
        Dim n%
        Dim a, b As Double
        Dim time As New Dictionary(Of NamedValue(Of Dictionary(Of Double, Integer)))
        Dim observation As NamedValue(Of TimeValue())()

        ''' <summary>
        ''' <see cref="MonteCarlo.Model"/>
        ''' </summary>
        Dim model As Type
        Dim y0 As Dictionary(Of String, Double)

        Sub New(model As Type, observation As NamedValue(Of TimeValue())(), n%, a#, b#, y0 As Dictionary(Of String, Double))
            With Me
                .n = n
                .a = a
                .b = b
                .X = DynamicsSystem.TimePopulator(n, a, b).ToArray
                .observation = observation
                .y0 = y0
                .model = model
            End With

            For Each var As NamedValue(Of TimeValue()) In observation
                time += New NamedValue(Of Dictionary(Of Double, Integer)) With {
                    .Name = var.Name,
                    .Value = TimeValue.BuildIndex(X, var.Value)
                }
            Next
        End Sub

        Public ReadOnly Property Cacheable As Boolean Implements Fitness(Of ParameterVector).Cacheable
            Get
                Return False
            End Get
        End Property

        Public Function Calculate(chromosome As ParameterVector) As Double Implements Fitness(Of ParameterVector).Calculate
            Dim result As ODEsOut = MonteCarlo.Model.RunTest(model, y0, chromosome.vars, n, a, b)
            Dim fitness As New List(Of Double)
            Dim NaN As New List(Of Integer)

            For Each var As NamedValue(Of TimeValue()) In observation
                Dim y = result.y(var.Name)
                Dim index As Dictionary(Of Double, Integer) = time(var.Name).Value
                Dim indices%() = var.Value _
                    .Select(Function(t) index(t.Time)) _
                    .ToArray
                Dim cData#() = indices _
                    .Select(Function(i) y.Value(i)) _
                    .ToArray

                NaN += cData.Where(AddressOf IsNaNImaginary).Count
                fitness += Math.Sqrt(
                    FitnessHelper.Calculate(
                    var.Value.Select(Function(t) t.Y).ToArray,
                    cData))
            Next

            Dim out# = fitness.Average

            If out.IsNaNImaginary Then
                out = Integer.MaxValue * 100.0R
                out += NaN.Max * 10
            End If

            Return out
        End Function
    End Class
End Namespace
