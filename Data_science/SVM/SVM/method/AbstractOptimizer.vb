#Region "Microsoft.VisualBasic::025a62c63083e9eafc00650b70ec81ab, ..\sciBASIC#\Data_science\SVM\SVM\method\AbstractOptimizer.vb"

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
    Public MustInherit Class Optimizer

        Protected Friend _line As Line
        Protected Friend _points As LabeledPoint()
        Protected Friend _iterations As Integer
        Protected Friend _cancelled As Boolean

        Public Sub New(line As Line, points As IList(Of LabeledPoint), iterations As Integer)
            _line = line.Clone()
            _points = New LabeledPoint(points.Count - 1) {}
            For i As Integer = 0 To _points.Length - 1
                _points(i) = points(i).Clone()
            Next

            _iterations = iterations
            _cancelled = False
        End Sub

        Public Function Optimize() As Line
            Dim result As Line = innerOptimize()
            Return result
        End Function

        Protected Friend MustOverride Function innerOptimize() As Line

        Public Sub Cancel()
            _cancelled = True
        End Sub
    End Class
End Namespace
