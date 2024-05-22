#Region "Microsoft.VisualBasic::49858a28b558a9ab68c7a3553a77ee89, Data_science\MachineLearning\MachineLearning\SVM\RangeTransform\IRangeTransform.vb"

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

    '   Total Lines: 38
    '    Code Lines: 6 (15.79%)
    ' Comment Lines: 30 (78.95%)
    '    - Xml Docs: 46.67%
    ' 
    '   Blank Lines: 2 (5.26%)
    '     File Size: 1.57 KB


    '     Interface IRangeTransform
    ' 
    '         Function: (+2 Overloads) Transform
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
' * SVM.NET Library
' * Copyright (C) 2008 Matthew Johnson
' * 
' * This program is free software: you can redistribute it and/or modify
' * it under the terms of the GNU General Public License as published by
' * the Free Software Foundation, either version 3 of the License, or
' * (at your option) any later version.
' * 
' * This program is distributed in the hope that it will be useful,
' * but WITHOUT ANY WARRANTY; without even the implied warranty of
' * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' * GNU General Public License for more details.
' * 
' * You should have received a copy of the GNU General Public License
' * along with this program.  If not, see <http://www.gnu.org/licenses/>.

Namespace SVM

    ''' <summary>
    ''' Interface implemented by range transforms.
    ''' </summary>
    Public Interface IRangeTransform
        ''' <summary>
        ''' Transform the input value using the transform stored for the provided index.
        ''' </summary>
        ''' <param name="input">Input value</param>
        ''' <param name="index">Index of the transform to use</param>
        ''' <returns>The transformed value</returns>
        Function Transform(input As Double, index As Integer) As Double
        ''' <summary>
        ''' Transforms the input array.
        ''' </summary>
        ''' <param name="input">The array to transform</param>
        ''' <returns>The transformed array</returns>
        Function Transform(input As Node()) As Node()
    End Interface
End Namespace
