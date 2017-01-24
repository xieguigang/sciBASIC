# LinqWhere`1
_namespace: [Microsoft.VisualBasic.Data.csv.IO.Linq](./index.md)_





### Methods

#### __compile
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.LinqWhere`1.__compile(Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.Data.csv.IO.Linq.ExprToken}@,System.Func{`0,System.Boolean})
```
编译查询条件表达式

|Parameter Name|Remarks|
|--------------|-------|
|tokens|-|
|stack|-|


#### __eq
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.LinqWhere`1.__eq(System.Object,System.String,System.Type)
```
a = b

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|test|-|
|type|-|


#### __gt
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.LinqWhere`1.__gt(System.Object,System.String,System.Type)
```
a > b

#### __instr
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.LinqWhere`1.__instr(System.Object,System.String,System.Type)
```
InStr(a, b) > 0

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|test|-|
|type|-|


#### __lt
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.LinqWhere`1.__lt(System.Object,System.String,System.Type)
```
a < b

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|test|-|
|type|-|


#### __neq
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.LinqWhere`1.__neq(System.Object,System.String,System.Type)
```
a != b

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|test|-|
|type|-|


#### __regex
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.LinqWhere`1.__regex(System.Object,System.String,System.Type)
```
Regex.Match(a, b).Success

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|test|-|
|type|-|


#### Compile
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.LinqWhere`1.Compile
```
编译LINQ数据库查询引擎之中的条件表达式
 OR 运算的级别是最低的
 a and b and c or (d and e and f or (g and h))

#### Test
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.LinqWhere`1.Test(`0)
```
LINQ WHERE TEST(x) = TRUE

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### TryParse
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.LinqWhere`1.TryParse(System.String)
```
column <opr> value

|Parameter Name|Remarks|
|--------------|-------|
|expr|-|



### Properties

#### _operations
操作符代码
