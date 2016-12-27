# QTable`1
_namespace: [Microsoft.VisualBasic.DataMining.QLearning](./index.md)_

The heart of the Q-learning algorithm, the QTable contains the table
 which maps states, actions and their Q values. This class has elaborate
 documentation, and should be the focus of the students' body of work
 for the purposes of this tutorial.

 @author A.Liapis (Original author), A. Hartzen (2013 modifications); xie.guigang@gcmodeller.org (2016 modifications)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.DataMining.QLearning.QTable`1.#ctor(System.Int32)
```
Q table constructor, initiates variables.

|Parameter Name|Remarks|
|--------------|-------|
|actionRange| number of actions available at any map state |


#### __explore
```csharp
Microsoft.VisualBasic.DataMining.QLearning.QTable`1.__explore
```
The explore function is called for e-greedy algorithms.
 It can choose an action at random from all available,
 or can put more weight towards actions that have not been taken
 as often as the others (most unknown).

_returns:  index of action to take _
> 在这里得到可能的下一步的动作的在动作列表里面编号值， Index

#### __getActionsQValues
```csharp
Microsoft.VisualBasic.DataMining.QLearning.QTable`1.__getActionsQValues(`0)
```
The getActionsQValues function returns an array of Q values for
 all the actions available at any state. Note that if the current
 map state does not already exist in the Q table (never visited
 before), then it is initiated with Q values of 0 for all of the
 available actions.

|Parameter Name|Remarks|
|--------------|-------|
|map| current map (state) |


_returns:  an array of Q values for all the actions available at any state _

#### __getBestAction
```csharp
Microsoft.VisualBasic.DataMining.QLearning.QTable`1.__getBestAction(`0)
```
The getBestAction function uses a greedy approach for finding
 the best action to take. Note that if all Q values for the current
 state are equal (such as all 0 if the state has never been visited
 before), then getBestAction will always choose the same action.
 If such an action is invalid, this may lead to a deadlock as the
 map state never changes: for situations like these, exploration
 can get the algorithm out of this deadlock.

|Parameter Name|Remarks|
|--------------|-------|
|map| current map (state) |


_returns:  the action with the highest Q value _

#### __getMapString
```csharp
Microsoft.VisualBasic.DataMining.QLearning.QTable`1.__getMapString(`0)
```
This helper function is used for entering the map state into the
 HashMap

|Parameter Name|Remarks|
|--------------|-------|
|map|-|


_returns:  String used as a key for the HashMap _

#### GetValues
```csharp
Microsoft.VisualBasic.DataMining.QLearning.QTable`1.GetValues(`0)
```
Helper function to find the Q-values of a given map state.

|Parameter Name|Remarks|
|--------------|-------|
|map| current map (state) |


_returns:  the Q-values stored of the Qtable entry of the map state, otherwise null if it is not found _

#### NextAction
```csharp
Microsoft.VisualBasic.DataMining.QLearning.QTable`1.NextAction(`0)
```
For this example, the getNextAction function uses an e-greedy
 approach, having exploration happen if the exploration chance
 is rolled.
 ( **** 请注意，这个函数所返回的值为最佳选择的Index编号，所以可能还需要进行一些转换 **** )

|Parameter Name|Remarks|
|--------------|-------|
|map| current map (state) |


_returns:  the action to be taken by the calling program _

#### UpdateQvalue
```csharp
Microsoft.VisualBasic.DataMining.QLearning.QTable`1.UpdateQvalue(System.Int32,`0)
```
The updateQvalue is the heart of the Q-learning algorithm. Based on
 the reward gained by taking the action prevAction while being in the
 state prevState, the updateQvalue must update the Q value of that
 {prevState, prevAction} entry in the Q table. In order to do that,
 the Q value of the best action of the current map state must also
 be calculated.

|Parameter Name|Remarks|
|--------------|-------|
|reward| at the current map state |
|map| current map state (for finding the best action of the
 current map state) |



### Properties

#### __randomGenerator
for creating random numbers
#### _prevAction
Since in Q-learning the updates to the Q values are made ONE STEP
 LATE, the index of the action which resulted in the reward must be
 stored.
#### _prevState
Since in Q-learning the updates to the Q values are made ONE STEP
 LATE, the state of the world when the action resulting in the reward
 was made must be stored.
#### ActionRange
the actionRange variable determines the number of actions available
 at any map state, and therefore the number of Q values in each entry
 of the Q-table.
#### ExplorationChance
for e-greedy Q-learning, when taking an action a random number is
 checked against the explorationChance variable: if the number is
 below the explorationChance, then exploration takes place picking
 an action at random. Note that the explorationChance is not a final
 because it is customary that the exploration chance changes as the
 training goes on.
#### GammaValue
the discount factor is saved as the gammaValue variable. The
 discount factor determines the importance of future rewards.
 If the gammaValue is 0 then the AI will only consider immediate
 rewards, while with a gammaValue near 1 (but below 1) the AI will
 try to maximize the long-term reward even if it is many moves away.
#### LearningRate
the learningRate determines how new information affects accumulated
 information from previous instances. If the learningRate is 1, then
 the new information completely overrides any previous information.
 Note that the learningRate is not a final because it is
 customary that the learningRate changes as the
 training goes on.
#### Table
the table variable stores the Q-table, where the state is saved
 directly as the actual map. Each map state has an array of Q values
 for all the actions available for that state.
