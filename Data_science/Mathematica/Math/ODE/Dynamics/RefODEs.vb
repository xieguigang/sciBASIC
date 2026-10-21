#Region "Microsoft.VisualBasic::0d1f0532dd0f53ee3c9099991f87a726, Data_science\Mathematica\Math\ODE\Dynamics\RefODEs.vb"

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
    '    Code Lines: 17 (51.52%)
    ' Comment Lines: 8 (24.24%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (24.24%)
    '     File Size: 1.05 KB


    '     Class RefODEs
    ' 
    '         Properties: RefValues
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Dynamics

    ''' <summary>
    ''' ``dy`` reference to the exists values.
    ''' </summary>
    ''' <remarks>
    ''' 参考数据的定位使用了求解器维护的 <see cref="ODEs.CurrentStep"/>，因此与积分方法的
    ''' 阶段个数(4 阶段 RK4 或者 7 阶段 RK45)完全解耦，也不会在最后一步越界。
    ''' </remarks>
    Public MustInherit Class RefODEs : Inherits ODEs

        ''' <summary>
        ''' The reference data cursor, its position is the index of the current integration step.
        ''' </summary>
        ''' <returns></returns>
        Public Property RefValues As ValueVector

        ''' <summary>
        ''' 每一次阶段求值都会调用这个函数，参考数据的游标位置由求解器给出的当前步索引决定
        ''' </summary>
        ''' <param name="dx"></param>
        ''' <param name="dy"></param>
        Protected NotOverridable Overrides Sub func(dx As Double, ByRef dy As Vector)
            Dim ref As ValueVector = RefValues

            If ref Is Nothing Then
                Throw New InvalidOperationException($"The reference data ``{NameOf(RefValues)}`` of the model '{GetType(RefODEs).FullName}' is not initialized!")
            End If

            ' 使用求解器维护的步索引来定位参考数据，不再依赖每步调用次数的隐式约定
            ref.Value = CurrentStep

            Call func(dx, dy, ref)
        End Sub

        ''' <summary>
        ''' Compute the derivatives with the reference data of the current integration step.
        ''' </summary>
        ''' <param name="dx"></param>
        ''' <param name="dy"></param>
        ''' <param name="Y"></param>
        Protected MustOverride Overloads Sub func(dx#, ByRef dy As Vector, Y As ValueVector)

    End Class
End Namespace
