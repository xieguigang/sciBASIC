#Region "Microsoft.VisualBasic::6e10f9e85dbc1c78d4616d54ce68cfe7, ..\sciBASIC#\Data_science\SVM\SVM\method\SvmArgument.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.DataMining.SVM.Model

Namespace Method

    ''' <summary>
    ''' @author Ralf Wondratschek
    ''' </summary>
    Public Class SvmArgument : Implements ICloneable

        Public Property Offset As Double
        Public Property NormalVector As NormalVector

        Public Sub New(normalVector As NormalVector, offset As Double)
            Me.NormalVector = normalVector
            Me.Offset = offset
        End Sub

        Public Function Multipy(factor As Double) As SvmArgument
            Dim res As SvmArgument = Me.Clone()
            res.NormalVector.W = res.NormalVector.W * factor
            res.Offset = res.Offset * factor
            Return res
        End Function

        Public Function Minus(arg As SvmArgument) As SvmArgument
            Dim res As SvmArgument = Me.Clone()
            res.NormalVector.W = res.NormalVector.W - arg.NormalVector.W
            res.Offset = res.Offset - arg.Offset
            Return res
        End Function

        Public Function [Next](stepSize As Double, derivation As SvmArgument) As SvmArgument
            Return Minus(derivation.Multipy(stepSize))
        End Function

        Public Function ToLine() As Line
            Dim ___offset As Double = Offset / NormalVector.W2 * -1
            Return New Line(NormalVector.Clone(), ___offset)
        End Function

        Public Overrides Function ToString() As String
            Return ToLine.ToString
        End Function

        Public Function Norm() As Double
            Return Math.Sqrt(Math.Pow(NormalVector.W1, 2) + Math.Pow(NormalVector.W2, 2) + Math.Pow(Offset, 2))
        End Function

        Public Function Clone() As SvmArgument
            Return New SvmArgument(NormalVector.Clone(), Offset)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function
    End Class

End Namespace
