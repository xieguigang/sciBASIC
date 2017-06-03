#Region "Microsoft.VisualBasic::16a05abe41f9ef114953fe7e3468f6b7, ..\sciBASIC#\Data_science\MachineLearning\Darwinism\GeneticAlgorithm\Fitness.vb"

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

' *****************************************************************************
' Copyright 2012 Yuriy Lagodiuk
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'   http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************

Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF

    Public Interface Fitness(Of C As Chromosome(Of C))

        ''' <summary>
        ''' Assume that chromosome1 is better than chromosome2 <br/>
        ''' fit1 = calculate(chromosome1) <br/>
        ''' fit2 = calculate(chromosome2) <br/>
        ''' So the following condition must be true <br/>
        ''' fit1.compareTo(fit2) &lt;= 0 <br/>
        ''' (假若是并行模式的之下，还要求这个函数是线程安全的)
        ''' </summary>
        Function Calculate(chromosome As C) As Double
    End Interface
End Namespace
