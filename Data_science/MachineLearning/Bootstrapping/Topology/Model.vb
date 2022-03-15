#Region "Microsoft.VisualBasic::86315a5f5faba9d19213c245841a0fd3, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Topology\Model.vb"

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

    '   Total Lines: 75
    '    Code Lines: 47
    ' Comment Lines: 16
    '   Blank Lines: 12
    '     File Size: 2.73 KB


    '     Class Model
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: y0
    ' 
    '         Sub: func
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping.Darwinism
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Language

Namespace Topology

    ''' <summary>
    ''' 这个模型只适合于常微分线性方程
    ''' ```
    ''' dy = alpha - beta
    ''' ```
    ''' </summary>
    Public Class Model : Inherits GAF.ODEs.Model

        Dim _alpha As Dictionary(Of String, (parm As NamedValue(Of Double), v As var)())
        Dim _beta As Dictionary(Of String, (parm As NamedValue(Of Double), v As var)())

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="vars"></param>
        ''' <param name="alpha">
        ''' <see cref="MonteCarlo.SPowerAlpha(String, String)"/>
        ''' </param>
        ''' <param name="beta">
        ''' <see cref="MonteCarlo.SPowerBeta(String, String)"/>
        ''' </param>
        Sub New(vars As IEnumerable(Of NamedValue(Of Double)),
                alpha As Dictionary(Of String, Double),
                beta As Dictionary(Of String, Double))

            Call MyBase.New(
                vars _
                .SeqIterator _
                .Select(Function(v) New var With {
                    .Index = v.i,
                    .Name = v.value.Name,
                    .value = v.value.Value
            }).ToArray)

            Dim name As New Value(Of String)

            For Each v As var In MyBase.vars
                Dim valAlpha As New List(Of (parm As NamedValue(Of Double), v As var))
                Dim valBeta As New List(Of (parm As NamedValue(Of Double), v As var))

                For Each x As var In MyBase.vars
                    valAlpha += (New NamedValue(Of Double)(name = MonteCarlo.SPowerAlpha(v, x), alpha(+name)), x)
                    valBeta += (New NamedValue(Of Double)(name = MonteCarlo.SPowerBeta(v, x), alpha(+name)), x)
                Next

                Call _alpha.Add(v, valAlpha)
                Call _beta.Add(v, valBeta)
            Next
        End Sub

        Protected Overrides Function y0() As var()
            Return vars
        End Function

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            For Each v As var In vars
                Dim alpha = _alpha(v.Name).Sum(
                    Function(x) x.parm.Value * x.v)
                Dim beta# = _beta(v.Name).Sum(
                    Function(x) x.parm.Value * x.v)

                dy(index:=v) = alpha - beta
            Next
        End Sub
    End Class
End Namespace
