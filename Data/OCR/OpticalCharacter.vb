#Region "Microsoft.VisualBasic::518e99c49cc149033d90d0cddccafbb7, sciBASIC#\Data\OCR\OpticalCharacter.vb"

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

    '   Total Lines: 59
    '    Code Lines: 40
    ' Comment Lines: 9
    '   Blank Lines: 10
    '     File Size: 1.67 KB


    ' Class OpticalCharacter
    ' 
    '     Properties: PixelLines
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: Compare, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class OpticalCharacter

    ''' <summary>
    ''' 使用-1分割，按照行进行排列
    ''' </summary>
    Public PixelsVector As Vector
    Public [char] As Char

    Public ReadOnly Property PixelLines As Vector()
        Get
            Return PixelsVector.Split(Function(x) x = -1.0R) _
                               .Select(Function(p) p.AsVector) _
                               .ToArray
        End Get
    End Property

    Sub New()
    End Sub

    Sub New(optical As Image, Optional [char] As Char = Nothing)
        Me.char = [char]
        Me.PixelsVector = optical.ToVector(fillDeli:=True).First
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="test">模板样本数据</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Compare(test As OpticalCharacter) As Double
        Return SSM(PixelsVector, test.PixelsVector)
    End Function

    Public Overrides Function ToString() As String
        Dim sb As New StringBuilder

        For Each line As Double() In PixelsVector.Split(Function(x) x = -1.0R)
            For Each c As Double In line
                If c = 0R Then
                    Call sb.Append(" ")
                Else
                    Call sb.Append("$")
                End If
            Next

            Call sb.AppendLine()
        Next

        Return sb.ToString
    End Function
End Class
