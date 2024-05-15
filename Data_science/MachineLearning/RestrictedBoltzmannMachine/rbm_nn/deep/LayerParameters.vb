#Region "Microsoft.VisualBasic::cc25d606b956695e1c890dac5f50c456, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\deep\LayerParameters.vb"

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

    '   Total Lines: 47
    '    Code Lines: 34
    ' Comment Lines: 3
    '   Blank Lines: 10
    '     File Size: 1.38 KB


    '     Class LayerParameters
    ' 
    '         Properties: HiddenUnitsPerRBM, NumRBMS, VisibleUnitsPerRBM
    ' 
    '         Function: setHiddenUnitsPerRBM, setNumRBMS, setVisibleUnitsPerRBM
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace nn.rbm.deep
    ''' <summary>
    ''' Created by kenny on 5/16/14.
    ''' </summary>
    Public Class LayerParameters

        Private numRBMSField As Integer = 1

        Private visibleUnitsPerRBMField As Integer = 1

        Private hiddenUnitsPerRBMField As Integer = 1

        Public ReadOnly Property NumRBMS As Integer
            Get
                Return numRBMSField
            End Get
        End Property

        Public Function setNumRBMS(numRBMS As Integer) As LayerParameters
            numRBMSField = numRBMS
            Return Me
        End Function

        Public ReadOnly Property VisibleUnitsPerRBM As Integer
            Get
                Return visibleUnitsPerRBMField
            End Get
        End Property

        Public Function setVisibleUnitsPerRBM(visibleUnitsPerRBM As Integer) As LayerParameters
            visibleUnitsPerRBMField = visibleUnitsPerRBM
            Return Me
        End Function

        Public ReadOnly Property HiddenUnitsPerRBM As Integer
            Get
                Return hiddenUnitsPerRBMField
            End Get
        End Property

        Public Function setHiddenUnitsPerRBM(hiddenUnitsPerRBM As Integer) As LayerParameters
            hiddenUnitsPerRBMField = hiddenUnitsPerRBM
            Return Me
        End Function
    End Class

End Namespace
