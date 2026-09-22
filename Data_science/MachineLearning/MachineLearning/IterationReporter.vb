#Region "Microsoft.VisualBasic::4890a8491d3712900aa0481e45b23f74, Data_science\MachineLearning\MachineLearning\IterationReporter.vb"

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
    '    Code Lines: 12 (36.36%)
    ' Comment Lines: 14 (42.42%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (21.21%)
    '     File Size: 987 B


    ' Class IterationReporter
    ' 
    ' 
    '     Delegate Sub
    ' 
    '         Function: AttachReporter
    ' 
    ' Class Model
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' 用于报告基于迭代的机器学习算法的状态进度之类的信息的框架
''' </summary>
''' <remarks>
''' 这个对象模块应该是应用于训练部分的模块
''' </remarks>
Public MustInherit Class IterationReporter(Of T As Model)

    Protected reporter As DoReport

    ''' <summary>
    ''' The delegate callback for reports the training progress and status 
    ''' of a iteration based machine learning algorithm.
    ''' </summary>
    ''' <param name="iteration">The current iteration number.</param>
    ''' <param name="error">
    ''' The training error (or the loss value) which is evaluated at the 
    ''' current <paramref name="iteration"/>.
    ''' </param>
    ''' <param name="model">
    ''' The model object instance which is currently under training.
    ''' </param>
    Public Delegate Sub DoReport(iteration%, error#, model As T)

    ''' <summary>
    ''' Attach the delegate function <see cref="DoReport"/> for reports the 
    ''' training progress of the current model.
    ''' </summary>
    ''' <param name="reporter">
    ''' A <see cref="DoReport"/> delegate callback, ``Nothing`` means no 
    ''' progress report is required.
    ''' </param>
    ''' <returns>
    ''' Returns the model itself, so that this function can be used in a 
    ''' method chaining expression.
    ''' </returns>
    <DebuggerStepThrough>
    Public Function AttachReporter(reporter As DoReport) As IterationReporter(Of T)
        Me.reporter = reporter
        Return Me
    End Function

    ''' <summary>
    ''' Run the model training task.
    ''' </summary>
    ''' <param name="parallel">
    ''' Whether the training procedure should be run in parallel mode? 
    ''' The default value of this parameter is ``False``.
    ''' </param>
    Public MustOverride Sub Train(Optional parallel As Boolean = False)

End Class

''' <summary>
''' the base type of the machine learning model
''' </summary>
Public MustInherit Class Model

End Class
