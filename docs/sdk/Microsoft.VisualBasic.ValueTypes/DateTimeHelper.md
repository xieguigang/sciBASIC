# DateTimeHelper
_namespace: [Microsoft.VisualBasic.ValueTypes](./index.md)_





### Methods

#### DateSeq
```csharp
Microsoft.VisualBasic.ValueTypes.DateTimeHelper.DateSeq(System.DateTime,System.DateTime)
```
枚举出在**`start`**到**`ends`**这个时间窗里面的所有日期

|Parameter Name|Remarks|
|--------------|-------|
|start|-|
|ends|-|


_returns: 返回值里面包含有起始和结束的日期_

#### FillDateZero
```csharp
Microsoft.VisualBasic.ValueTypes.DateTimeHelper.FillDateZero(System.Int32)
```
00

|Parameter Name|Remarks|
|--------------|-------|
|d|-|


#### GetMonthInteger
```csharp
Microsoft.VisualBasic.ValueTypes.DateTimeHelper.GetMonthInteger(System.String)
```
从全称或者简称解析出月份的数字

|Parameter Name|Remarks|
|--------------|-------|
|mon|大小写不敏感|


#### YYMMDD
```csharp
Microsoft.VisualBasic.ValueTypes.DateTimeHelper.YYMMDD(System.DateTime)
```
yyyy-mm-dd

|Parameter Name|Remarks|
|--------------|-------|
|x|-|



### Properties

#### MonthList
List of month names and its @``T:System.Int32`` value in a year
