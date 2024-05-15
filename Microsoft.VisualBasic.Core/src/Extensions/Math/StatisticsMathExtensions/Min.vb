#Region "Microsoft.VisualBasic::8a23b3f3ab00bbcfc05cb0d5141312bc, Microsoft.VisualBasic.Core\src\Extensions\Math\StatisticsMathExtensions\Min.vb"

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

    '   Total Lines: 27
    '    Code Lines: 20
    ' Comment Lines: 1
    '   Blank Lines: 6
    '     File Size: 753 B


    '     Class Min
    ' 
    '         Properties: MinValue
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: getEigenvalue, ToString
    ' 
    '         Sub: addObservation
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Math.Statistics

    Public Class Min : Inherits SampleObservation

        Public ReadOnly Property MinValue As Double

        Sub New(value As Double)
            Call MyBase.New({value})
            ' intialize value is the min value
            MinValue = value
        End Sub

        Protected Overrides Sub addObservation(observation As Double)
            If observation < MinValue Then
                _MinValue = observation
            End If
        End Sub

        Protected Overrides Function getEigenvalue() As Double
            Return MinValue
        End Function

        Public Overrides Function ToString() As String
            Return MinValue
        End Function
    End Class
End Namespace
