# HashMap
_namespace: [Microsoft.VisualBasic.Mathematical](./index.md)_

VB.NET常用的哈希算法集.其中包括了著名的暴雪的哈希,T33哈希.......
 不同的哈希算法在分布式,布降过滤器,位图MAP等等应用得比较多...

> 
>  http://bbs.csdn.net/topics/391950537
>  


### Methods

#### HashBKDR
```csharp
Microsoft.VisualBasic.Mathematical.HashMap.HashBKDR(System.Byte[],System.Int64)
```
BKDR 哈希

|Parameter Name|Remarks|
|--------------|-------|
|KeyByte|-|
|seed|种子数|


#### HashBlizzard
```csharp
Microsoft.VisualBasic.Mathematical.HashMap.HashBlizzard(System.Byte[],System.Int64)
```
暴雪公司著名的 HashMap .
 测试了 二千万 GUID, 没有重复.但运算量比较大。

|Parameter Name|Remarks|
|--------------|-------|
|KeyByte|-|
|HasType|HasType =[0 ,1 ,2] |


#### HashCMyMap
```csharp
Microsoft.VisualBasic.Mathematical.HashMap.HashCMyMap(System.Byte[])
```
经典times33算法。简单高效。[这个使用移位代替*33]
 测试一千万。没有重复哈希值。

|Parameter Name|Remarks|
|--------------|-------|
|KeyByte|-|


#### HashDJB
```csharp
Microsoft.VisualBasic.Mathematical.HashMap.HashDJB(System.Byte[])
```
和 HashCMyMap 基本一样.

|Parameter Name|Remarks|
|--------------|-------|
|KeyByte|-|


#### HashTimeMap
```csharp
Microsoft.VisualBasic.Mathematical.HashMap.HashTimeMap(System.Byte[],System.UInt32)
```
经典的Time算法。简单，高效。
 Ngix使用的是 time31，Tokyo Cabinet使用的是 time37
 小写英文词汇适合33, 大小写混合使用65。time33比较适合的是英文词汇的hash.

|Parameter Name|Remarks|
|--------------|-------|
|KeyByte|-|
|seed|种子质数。 31，33，37 。。。|



