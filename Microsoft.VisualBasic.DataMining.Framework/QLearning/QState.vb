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