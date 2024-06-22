#Region "Microsoft.VisualBasic::e429487764fc08e60f156b3bb194a438, Data_science\MachineLearning\t-SNE\RandomHelper.vb"

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

    '   Total Lines: 108
    '    Code Lines: 57 (52.78%)
    ' Comment Lines: 26 (24.07%)
    '    - Xml Docs: 96.15%
    ' 
    '   Blank Lines: 25 (23.15%)
    '     File Size: 2.86 KB


    ' Class RandomHelper
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GaussRandom, randn, (+2 Overloads) randn2d
    ' 
    ' /********************************************************************************/

#End Region

Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Friend Class RandomHelper

    ReadOnly tSNE As tSNE

    Sub New(tSNE As tSNE)
        Me.tSNE = tSNE
    End Sub

    ''' <summary>
    ''' return 0 mean unit standard deviation random number
    ''' </summary>
    ''' <returns></returns>
    Private Function GaussRandom() As Double
        If tSNE.mRet Then
            tSNE.mRet = False
            Return tSNE.mVal
        End If

        Dim u As Double = randf.NextDouble() - 1
        Dim v As Double = randf.NextDouble() - 1
        Dim r As Double = u * u + v * v

        If r = 0 OrElse r > 1 Then
            Return GaussRandom()
        End If

        Dim c = std.Sqrt(-2 * std.Log(r) / r)

        ' cache this for next function call for efficiency
        tSNE.mVal = v * c
        tSNE.mRet = True

        Return u * c
    End Function

    ''' <summary>
    ''' return random normal number
    ''' </summary>
    ''' <param name="mu"></param>
    ''' <param name="std"></param>
    ''' <returns></returns>
    Private Function randn(mu As Double, std As Double) As Double
        Return mu + GaussRandom() * std
    End Function

    ''' <summary>
    ''' utility that returns 2d array filled with random numbers
    ''' or with value s, if provided
    ''' </summary>
    ''' <param name="n"></param>
    ''' <param name="d"></param>
    ''' <returns></returns>
    Friend Overloads Function randn2d(n As Integer, d As Integer) As Double()()
        Dim x As New List(Of Double())()

        For i As Integer = 0 To n - 1
            Dim xhere As New List(Of Double)()

            For j As Integer = 0 To d - 1
                xhere.Add(randn(0.0, 0.0001))
            Next

            x.Add(xhere.ToArray())
        Next

        Dim ret = New Double(x.Count - 1)() {}

        For i = 0 To x.Count - 1
            ret(i) = x(i)
        Next

        Return ret
    End Function

    ''' <summary>
    ''' utility that returns 2d array filled with random numbers
    ''' or with value s, if provided
    ''' </summary>
    ''' <param name="n"></param>
    ''' <param name="d"></param>
    ''' <param name="s"></param>
    ''' <returns></returns>
    Friend Overloads Shared Function randn2d(n As Integer, d As Integer, s As Double) As Double()()
        Dim x As New List(Of Double())()

        For i As Integer = 0 To n - 1
            Dim xhere As New List(Of Double)()

            For j As Integer = 0 To d - 1
                xhere.Add(s)
            Next

            x.Add(xhere.ToArray())
        Next

        Dim ret = New Double(x.Count - 1)() {}

        For i = 0 To x.Count - 1
            ret(i) = x(i)
        Next

        Return ret
    End Function

End Class
