# ODESolver
_namespace: [Microsoft.VisualBasic.Mathematical.Calculus](./index.md)_

Solving the Ordinary differential equation(ODE) by using trapezoidal method.(使用梯形法求解常微分方程)

> http://www.oschina.net/code/snippet_76_4433


### Methods

#### Eluer
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.ODESolver.Eluer(Microsoft.VisualBasic.Mathematical.Calculus.ODE@,System.Int32,System.Double,System.Double)
```
欧拉法解微分方程，分块数量为n, 解的区间为[a,b], 解向量为(x,y),方程初值为(x0,y0)，ODE的结果会从x和y这两个数组指针返回

|Parameter Name|Remarks|
|--------------|-------|
|n|-|
|a|-|
|b|-|


#### RK2
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.ODESolver.RK2(Microsoft.VisualBasic.Mathematical.Calculus.ODE@,System.Int32,System.Double,System.Double)
```
二阶龙格库塔法解解微分方程，分块数量为n, 解的区间为[a,b], 解向量为(x,y),方程初值为(x0, y0)
 参考http://blog.sina.com.cn/s/blog_698c6a6f0100lp4x.html

|Parameter Name|Remarks|
|--------------|-------|
|df|-|
|n|-|
|a|-|
|b|-|


#### RK4
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.ODESolver.RK4(Microsoft.VisualBasic.Mathematical.Calculus.ODE@,System.Int32,System.Double,System.Double)
```
四阶龙格库塔法解解微分方程，分块数量为n, 解的区间为[a,b], 解向量为(x,y),方程初值为(x0, y0)
 参考http://blog.sina.com.cn/s/blog_698c6a6f0100lp4x.html 和维基百科

|Parameter Name|Remarks|
|--------------|-------|
|df|-|
|n|-|
|a|-|
|b|-|



### Properties

#### sqr2
把根号2算出来，不在循环体内反复执行根号2，减少计算负担
