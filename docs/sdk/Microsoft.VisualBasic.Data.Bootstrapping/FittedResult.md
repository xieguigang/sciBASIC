# FittedResult
_namespace: [Microsoft.VisualBasic.Data.Bootstrapping](./index.md)_





### Methods

#### GetY
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.FittedResult.GetY(System.Double)
```
根据x获取拟合方程的y值

|Parameter Name|Remarks|
|--------------|-------|
|x|-|



### Properties

#### Factor
拟合后的方程系数，根据阶次获取拟合方程的系数，如getFactor(2),就是获取y=a0+a1*x+a2*x^2+……+apoly_n*x^poly_n中a2的值
#### FactorSize
获取拟合方程系数的个数
#### FitedYlist
保存拟合后的y值，在拟合时可设置为不保存节省内存
#### Intercept
获取截距
#### R_square
确定系数，系数是0~1之间的数，是数理上判定拟合优度的一个量
#### RMSE
RMSE均方根误差
#### Slope
获取斜率
#### SSE
(剩余平方和)
#### SSR
回归平方和
