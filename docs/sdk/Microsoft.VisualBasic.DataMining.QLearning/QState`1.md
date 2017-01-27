# QState`1
_namespace: [Microsoft.VisualBasic.DataMining.QLearning](./index.md)_





### Methods

#### GetNextState
```csharp
Microsoft.VisualBasic.DataMining.QLearning.QState`1.GetNextState(System.Int32)
```
Gets the @``P:Microsoft.VisualBasic.DataMining.QLearning.QState`1.Current`` states.
 Returns the map state which results from an initial map state after an
 action is applied. In case the action is invalid, the returned map is the
 same as the initial one (no move).

|Parameter Name|Remarks|
|--------------|-------|
|action| taken by the avatar ('@') |


_returns:  resulting map after the action is taken _


### Properties

#### Current
map before the action is taken, clone object: @``M:System.ICloneable.Clone``
#### State
假若操作不会涉及到数据修改，请使用这个属性来减少性能的损失，@``P:Microsoft.VisualBasic.DataMining.QLearning.QState`1.Current``属性返回的值和本属性是一样的，
 只不过@``P:Microsoft.VisualBasic.DataMining.QLearning.QState`1.Current``属性是从@``M:System.ICloneable.Clone``方法得到的数据，所以性能方面会有损失
