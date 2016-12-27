# ColorMap
_namespace: [Microsoft.VisualBasic.Imaging](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Imaging.ColorMap.#ctor(System.Int32,System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|colorLength%|-|
|alpha%|@``P:System.Drawing.Color.A``: color alpha value|


#### GetMaps
```csharp
Microsoft.VisualBasic.Imaging.ColorMap.GetMaps(System.String,System.Boolean)
```
If failure, then this function will returns @``M:Microsoft.VisualBasic.Imaging.ColorMap.Jet`` by default, 
 or nothing if parameter **`noDefault`** is set True.

|Parameter Name|Remarks|
|--------------|-------|
|name|大小写不敏感|


#### Jet
```csharp
Microsoft.VisualBasic.Imaging.ColorMap.Jet
```
*


### Properties

#### Alpha
Alpha value in the RBG color function.
#### PatternAutumn
Autumn
#### PatternCool
Cool
#### PatternGray
Gray
#### PatternHot
Hot
#### PatternJet
Jet
#### PatternSpring
Spring
#### PatternSummer
Summer
#### PatternWinter
Winter
