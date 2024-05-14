#Region "Microsoft.VisualBasic::4e6aa29fde2386fc384bb7219ad91c34, Data_science\Mathematica\Math\Math.Statistics\MomentFunctions\BasicProductMomentsHistogram.vb"

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

    '   Total Lines: 60
    '    Code Lines: 39
    ' Comment Lines: 15
    '   Blank Lines: 6
    '     File Size: 2.62 KB


    '     Class BasicProductMomentsHistogram
    ' 
    '         Properties: Bins
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: AddObservation
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace MomentFunctions

    ''' 
    ''' <summary>
    ''' @author Will_and_Sara
    ''' </summary>
    Public Class BasicProductMomentsHistogram
        Inherits BasicProductMoments

        Public ReadOnly Property Bins As Integer()

        Private _ExpectedMin As Double
        Private _ExpectedMax As Double

        Public Sub New(NumBins As Integer, Min As Double, Max As Double)
            MyBase.New()
            _Bins = New Integer(NumBins - 1) {}
            _ExpectedMin = Min
            _ExpectedMax = Max
        End Sub
        '    public BasicProductMomentsHistogram(double binwidth){
        '        //need to change the logic to be based on binwidth....
        '        _Bins = new int[0];
        '        
        '    }
        Public Overrides Sub AddObservation(observation As Double)
            MyBase.AddObservation(observation)
            'histogram logic. Currently this is not designed to be as efficent as possible.  needs work (buffer block copy for instance)
            If observation < _ExpectedMin Then
                Dim binwidth As Double = (_ExpectedMax - _ExpectedMin) / _Bins.Length
                Dim overdist As Integer = CInt(Fix(stdNum.Ceiling(-(observation - _ExpectedMin) / binwidth)))
                _ExpectedMin = _ExpectedMin - overdist * binwidth
                Dim tmparray As Integer() = New Integer(_Bins.Length + overdist - 2) {}
                For i As Integer = overdist To _Bins.Length - 1
                    tmparray(i) = _Bins(i)
                Next i
                _Bins = tmparray
            ElseIf observation > _ExpectedMax Then
                Dim binwidth As Double = (_ExpectedMax - _ExpectedMin) / _Bins.Length
                Dim overdist As Integer = CInt(Fix(stdNum.Ceiling((observation - _ExpectedMax) / binwidth)))
                _ExpectedMax = _ExpectedMax + overdist * binwidth
                Dim tmparray As Integer() = New Integer(_Bins.Length + overdist - 2) {}
                For i As Integer = 0 To _Bins.Length - 1
                    tmparray(i) = _Bins(i)
                Next i
                _Bins = tmparray
            End If
            Dim index As Integer = CInt(Fix(stdNum.Floor(_Bins.Length * (observation - _ExpectedMin) / (_ExpectedMax - _ExpectedMin))))
            _Bins(index) += 1
        End Sub
    End Class

End Namespace
