#Region "Microsoft.VisualBasic::f8f03288d5572db3b7ad15ef97839315, Data_science\Mathematica\Math\KaplanMeierEstimator\Models\GeneExpression.vb"

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

    '   Total Lines: 28
    '    Code Lines: 17
    ' Comment Lines: 4
    '   Blank Lines: 7
    '     File Size: 778 B


    '     Class GeneExpression
    ' 
    '         Properties: AbsoluteDifference, After, Before, GeneId, PatientId
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

Namespace Models

    ''' <summary>
    '''  Holds the gene expression before and after a procedure, 
    '''  referring to a specific patient
    ''' </summary>
    Public Class GeneExpression
        Public Property GeneId As String

        Public Property PatientId As Integer

        Public Property Before As Double = Double.NaN

        Public Property After As Double = Double.NaN

        Public ReadOnly Property AbsoluteDifference As Double
            Get
                If Double.IsNaN(Before) OrElse Double.IsNaN(After) Then
                    Return Double.NaN
                End If

                Return stdNum.Abs(Before - After)
            End Get
        End Property
    End Class
End Namespace
