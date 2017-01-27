# QLearning`1
_namespace: [Microsoft.VisualBasic.DataMining.QLearning](./index.md)_

Q Learning sample class The goal of this code sample is for the character @ to reach the goal area G
 compile using "javac QLearning.java" 
 test using "java QLearning" 
 
 @author A.Liapis (Original author), A. Hartzen (2013 modifications)



### Methods

#### __finishLearn
```csharp
Microsoft.VisualBasic.DataMining.QLearning.QLearning`1.__finishLearn
```
You can save you q table by overrides at here.

#### __reset
```csharp
Microsoft.VisualBasic.DataMining.QLearning.QLearning`1.__reset(System.Int32)
```
If the @``P:Microsoft.VisualBasic.DataMining.QLearning.QLearning`1.GoalReached`` then reset and continute learning.

|Parameter Name|Remarks|
|--------------|-------|
|i|机器学习的当前的迭代次数|


#### __run
```csharp
Microsoft.VisualBasic.DataMining.QLearning.QLearning`1.__run(System.Int32)
```
Takes a action for the agent.

|Parameter Name|Remarks|
|--------------|-------|
|i|Iteration counts.|



### Properties

#### ActionRange
The size of the @``T:Microsoft.VisualBasic.DataMining.QLearning.QTable`1``
#### GoalPenalty
目标没有达成的罚分
#### GoalRewards
目标达成所得到的奖励
