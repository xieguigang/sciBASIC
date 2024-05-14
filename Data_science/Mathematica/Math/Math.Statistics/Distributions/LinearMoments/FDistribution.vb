#Region "Microsoft.VisualBasic::3408ec15315e758abcdec009775a7bd3, Data_science\Mathematica\Math\Math.Statistics\Distributions\LinearMoments\FDistribution.vb"

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
    '    Code Lines: 22
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 985 B


    '     Class FDistribution
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetCDF, GetInvCDF, GetPDF, Validate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Distributions.LinearMoments

    Public Class FDistribution : Inherits Distributions.ContinuousDistribution

        ReadOnly freedom1 As Integer
        ReadOnly freedom2 As Integer

        Sub New(freedom1 As Integer, freedom2 As Integer)
            Me.freedom1 = freedom1
            Me.freedom2 = freedom2
        End Sub

        Public Overrides Function GetInvCDF(probability As Double) As Double
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetCDF(value As Double) As Double
            Return Distribution.FDistribution(value, freedom1, freedom2)
        End Function

        Public Overrides Function GetPDF(value As Double) As Double
            Throw New NotImplementedException()
        End Function

        Public Overrides Function Validate() As IEnumerable(Of Exception)
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
