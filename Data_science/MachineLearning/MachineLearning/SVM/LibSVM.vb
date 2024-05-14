#Region "Microsoft.VisualBasic::93a172c904a317663d2712285efb0fc2, Data_science\MachineLearning\MachineLearning\SVM\LibSVM.vb"

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

    '   Total Lines: 24
    '    Code Lines: 18
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 733 B


    '     Class LibSVM
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: getSvmModel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder

Namespace SVM

    Public NotInheritable Class LibSVM

        Private Sub New()
        End Sub

        Public Shared Function getSvmModel(problem As Problem, par As Parameter) As SVMModel
            Dim transform As RangeTransform = RangeTransform.Compute(problem)
            Dim scale = transform.Scale(problem)
            Dim model As SVM.Model = Training.Train(scale, par)

            Call Logging.flush()

            Return New SVMModel With {
                .transform = transform,
                .model = model,
                .factors = New ClassEncoder(problem.Y)
            }
        End Function
    End Class
End Namespace
