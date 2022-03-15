#Region "Microsoft.VisualBasic::8674823e7679f30decce87752547d6d0, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Bootstrapping_CLI\Testing.vb"

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

    '   Total Lines: 69
    '    Code Lines: 49
    ' Comment Lines: 8
    '   Blank Lines: 12
    '     File Size: 2.25 KB


    ' Module Testing
    ' 
    '     Sub: Run
    '     Class TestModel
    ' 
    '         Function: eigenvector, params, yinit
    ' 
    '         Sub: func
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.Darwinism
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper.ListenerHelper
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Serialization.JSON

Module Testing

    Public Sub Run()

        'Call mutate(2)
        'Call mutate(20)
        'Call mutate(200)
        'Call mutate(200000)
        'Call mutate(2 * 10 ^ -30)
        'Call mutate(2 * 10 ^ -8)
        'Call mutate(2 * 10 ^ 99)
        'Call mutate(2 * 10 ^ 10)

        Dim model As Model = New TestModel
        Dim outPrint As List(Of outPrint) = Nothing
        Dim result = GAF.Protocol.Fitting(model, 2000, 0, 100, popSize:=1000, evolIterations:=150000, outPrint:=outPrint)

        Call outPrint.SaveTo("x:\test_debug.csv")
        Call result.GetJson.__DEBUG_ECHO

        Pause()
    End Sub

    Public Class TestModel : Inherits Model

        Dim y As var
        Dim y2 As var
        Dim a As Double

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            dy(y) = a * Math.Sin(dx + a)
            dy(y2) = 2 * a + dx
        End Sub

        Public Overrides Function eigenvector() As Dictionary(Of String, Eigenvector)
            Throw New NotImplementedException()
        End Function

        Public Overrides Function params() As ValueRange()
            Return {
                New ValueRange(-10000, 20000) With {
                    .Name = NameOf(a)
                }
            }
        End Function

        Public Overrides Function yinit() As ValueRange()
            Return {
                New ValueRange(-10000, 3300) With {
                    .Name = NameOf(y)
                },
                New ValueRange(-10000, 3300) With {
                    .Name = NameOf(y2)
                }
            }
        End Function
    End Class
End Module
