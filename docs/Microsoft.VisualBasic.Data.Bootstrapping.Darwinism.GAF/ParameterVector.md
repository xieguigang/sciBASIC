# ParameterVector
_namespace: [Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF](./index.md)_

Parameters that wait for bootstrapping estimates



### Methods

#### Clone
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ParameterVector.Clone
```
按值复制

#### Crossover
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ParameterVector.Crossover(Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ParameterVector)
```
Clone and crossover and last assign the vector value.(结果是按值复制的)

|Parameter Name|Remarks|
|--------------|-------|
|anotherChromosome|-|


#### Mutate
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ParameterVector.Mutate
```
Clone and mutation a bit and last assign the vector value.(会按值复制)

#### ToString
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ParameterVector.ToString
```
这个函数生成的字符串是和@``M:Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ParameterVector.Parse(System.String)``解析函数所使用的格式是相对应的
> 
>  ###### 2016.12.2
>  
>  ```
>  var=value,....
>  ```
>  
>  使用这种形式方便在R之中进行测试
>  


### Properties

#### radicals
突变的激进程度，假若这个值越高的话，会有越高的概率突变当前数位，反之较高的概率突变当前的-1数位
#### vars
The function variable parameter that needs to fit, not includes the ``y0``.
 (只需要在这里调整参数就行了，y0初始值不需要)
#### Vector
Transform as a vector for the mutation and crossover function.
