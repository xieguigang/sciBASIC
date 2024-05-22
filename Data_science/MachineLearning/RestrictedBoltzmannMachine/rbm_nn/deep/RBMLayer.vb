#Region "Microsoft.VisualBasic::8425375c542f477aa19c3673f0e0942d, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\deep\RBMLayer.vb"

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

    '   Total Lines: 32
    '    Code Lines: 18 (56.25%)
    ' Comment Lines: 5 (15.62%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 9 (28.12%)
    '     File Size: 818 B


    '     Class RBMLayer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: getRBM, size, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace nn.rbm.deep

    ''' <summary>
    ''' Created by kenny on 5/16/14.
    ''' 
    ''' A layer can have multiple RBMs, this allows convolution-like networks when configuring a deep rbm
    ''' </summary>
    Public Class RBMLayer

        Public ReadOnly rbms As RBM()

        Public Sub New(rbms As RBM())
            Me.rbms = rbms
        End Sub

        Public Function getRBM(r As Integer) As RBM
            Return rbms(r)
        End Function

        Public Function size() As Integer
            Return rbms.Length
        End Function

        Public Overrides Function ToString() As String
            Return "RBMLayer{" & "rbms=" & rbms.GetJson() & "}"c.ToString()
        End Function
    End Class


End Namespace
