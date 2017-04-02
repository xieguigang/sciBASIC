#Region "Microsoft.VisualBasic::c789c5458203acb65a91cdd9c5f60775, ..\sciBASIC#\Data_science\Microsoft.VisualBasic.DataMining.Framework\QLearning\QState.vb"

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

Namespace QLearning

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T">Status object</typeparam>
    Public MustInherit Class QState(Of T As ICloneable)

        Protected __state As T

        Public Sub SetState(x As T)
            __state = x
        End Sub

        ''' <summary>
        ''' 假若操作不会涉及到数据修改，请使用这个属性来减少性能的损失，<see cref="Current"/>属性返回的值和本属性是一样的，
        ''' 只不过<see cref="Current"/>属性是从<see cref="ICloneable.Clone()"/>方法得到的数据，所以性能方面会有损失
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property State As T
            Get
                Return __state
            End Get
        End Property

        ''' <summary>
        ''' map before the action is taken, clone object: <see cref="ICloneable.Clone()"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Current As T
            Get
                Return DirectCast(__state.Clone, T)
            End Get
        End Property

        ''' <summary>
        ''' Gets the <see cref="Current"/> states.
        ''' Returns the map state which results from an initial map state after an
        ''' action is applied. In case the action is invalid, the returned map is the
        ''' same as the initial one (no move). </summary>
        ''' <param name="action"> taken by the avatar ('@') </param>
        ''' <returns> resulting map after the action is taken </returns>
        Public MustOverride Function GetNextState(action As Integer) As T
    End Class
End Namespace
