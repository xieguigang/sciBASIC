#Region "Microsoft.VisualBasic::d098abc1b9f8528d5aaa10cf4ba44ad7, Data_science\DataMining\DataMining\Evaluation\RegressionClassify.vb"

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

    '   Total Lines: 29
    '    Code Lines: 16 (55.17%)
    ' Comment Lines: 7 (24.14%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (20.69%)
    '     File Size: 786 B


    '     Class RegressionClassify
    ' 
    '         Properties: actual, errors, predicts, sampleID
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

Namespace Evaluation

    ''' <summary>
    ''' The regression classifier result.
    ''' </summary>
    Public Class RegressionClassify

        Public Property sampleID As String
        Public Property actual As Double
        Public Property predicts As Double

        ''' <summary>
        ''' the absolute value of the error value
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property errors As Double
            Get
                Return stdNum.Abs(predicts - actual)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{sampleID}] {errors} = |{actual} - {predicts}|"
        End Function

    End Class
End Namespace
