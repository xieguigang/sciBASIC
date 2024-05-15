#Region "Microsoft.VisualBasic::1c1ce0782d6636cf8cc303431b788867, Data_science\Mathematica\Math\Math.Statistics\RANSAC\SampleOutput.vb"

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

    '   Total Lines: 33
    '    Code Lines: 28
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 1.29 KB


    '     Class SampleOutput
    ' 
    '         Properties: Accuracy, bestPlane, bestStd, bestSupport, inliersPercentage
    '                     lost_points, N
    ' 
    '         Sub: Print
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RANSAC

    Public Class SampleOutput

        Public Property bestPlane As Double()
        Public Property bestSupport As Double
        Public Property bestStd As Double
        Public Property inliersPercentage As Double
        Public Property N As Integer

        Public ReadOnly Property lost_points As Double
            Get
                Return N * inliersPercentage - bestSupport
            End Get
        End Property

        Public ReadOnly Property Accuracy As Double
            Get
                Return bestSupport / (N * inliersPercentage)
            End Get
        End Property

        Public Shared Sub Print(x As SampleOutput)
            Console.WriteLine("###OUTPUT###")
            Console.ForegroundColor = ConsoleColor.Green
            Console.WriteLine("Best plane: {0:F6} {1:F6} {2:F6} {3:F6}", x.bestPlane(0), x.bestPlane(1), x.bestPlane(2), x.bestPlane(3))
            Console.ResetColor()
            Console.WriteLine("Best support (i.e. matched points): {0}", x.bestSupport)
            Console.WriteLine("Best standard deviation: {0}" & vbLf, x.bestStd)
            Console.WriteLine("Lost points: {0}" & vbLf & "Accuracy: {1:F6}" & vbLf, x.lost_points, x.Accuracy)
        End Sub
    End Class
End Namespace
