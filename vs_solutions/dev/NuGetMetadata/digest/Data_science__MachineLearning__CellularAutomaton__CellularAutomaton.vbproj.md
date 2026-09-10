# Data_science/MachineLearning/CellularAutomaton/CellularAutomaton.vbproj

- RootNamespace : Microsoft.VisualBasic.MachineLearning.CellularAutomaton
- AssemblyName  : Microsoft.VisualBasic.MachineLearning.CellularAutomaton
- TargetFramework: net10.0
- Source files  : 10
- Existing Title: Cellular Automaton Grid Simulator with netCDF Snapshot Debugging
- Existing Desc : A generic cellular automaton engine for sciBASIC#: runs tick-and-commit generations over a 2D grid of typed cells with configurable neighbourhood and boundary modes, and exports per-cell state snapshots to netCDF for analysis.
- Existing Tags : scibasic;cellular-automaton;simulation;netcdf;grid-model

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.MachineLearning.CellularAutomaton)

## Public types
- Class Debugger (Debugger\Debugger.vb)
- Module WriteCDF (Debugger\WriteCDF.vb)
- Module Extensions (Extensions.vb)
- Class ConwayCell (GameOfLife.WinForms\ConwayCell.vb) - 康威生命游戏（Conway's Game of Life）的细胞实现。 规则 B3/S23：存活细胞在 2 或 3 个活邻居时存活，死亡细胞在恰好 3 个活邻居时出生。 邻居数量由所配置的 <see cref="NeighborhoodType"/> 决定（冯·诺依曼 4 / 摩尔 8 / 扩展摩尔 24）。
- Class MainForm (GameOfLife.WinForms\MainForm.vb) - 康威生命游戏（Conway's Game of Life）可视化演示。 直接复用本仓库 CellularAutomaton 类库的 <see cref="Simulator(Of T)"/> 与 <see cref="Individual"/>， 可在冯·诺依曼型 / 摩尔型 / 扩展摩尔型三种邻居拓扑及有界/环面边界下运行，
- Module Program (GameOfLife.WinForms\Program.vb) - 应用程序入口。禁用应用框架（UseApplicationFramework=false），以 Sub Main 显式启动窗体。
- Class CellEntity (SimulatorModel\CellEntity.vb)
- Interface Individual (SimulatorModel\Individual.vb)
- Enum NeighborhoodType (SimulatorModel\Neighborhood.vb) - 元胞自动机的邻居拓扑类型。
- Enum BoundaryMode (SimulatorModel\Neighborhood.vb) - 网格边界处理模式。
- Module Neighborhoods (SimulatorModel\Neighborhood.vb) - 邻居拓扑偏移表的生成器。各拓扑的偏移集合集中维护，新增拓扑仅需扩充本模块，符合开闭原则。
- Class Simulator (SimulatorModel\Simulator.vb)

## Notable public members
- Public Sub TakeSnapshots()
- Public Overrides Function ToString() As String
- Protected Overridable Sub Dispose(disposing As Boolean)
- Public Sub Dispose() Implements IDisposable.Dispose
- Public Sub Flush(path As String, cache As Integer()()(), type As Type)
- Public Sub TakeSnapshots(Of T As Individual)(simulator As Simulator(Of T), getKey As Func(Of T, String), counts As Dictionary(Of String, List(Of Integ…
- Public Function CreateCountSnapshotBuckets(Of T As Structure)() As Dictionary(Of String, List(Of Integer))
- Public Function CreateSnapshotMatrix(Of T As {New, INamedValue, IDynamicMeta(Of Double)})(snapshots As Dictionary(Of String, List(Of Integer))) As T()
- Public Property State As Boolean
- Public Sub New(Optional alive As Boolean = False)
- Public Sub Tick(adjacents As IEnumerable(Of Individual)) Implements Individual.Tick
- Public Sub Commit() Implements Individual.Commit
- Public Sub New()
- Friend Sub config(grid As Simulator(Of T), type As NeighborhoodType)
- Public ReadOnly Property Value As T
- Public Delegate Function ToInteger(Of T As Individual)(a As T) As Integer
- Public Function Offsets(type As NeighborhoodType) As Point()
- Public Overloads Sub Run(Optional random As Boolean = True)
- Public Iterator Function RandomCells() As IEnumerable(Of CellEntity(Of T))
- Public Iterator Function Snapshot() As IEnumerable(Of T)
- Public Function CellData(i As Integer, j As Integer) As T

## Imports
- Microsoft.VisualBasic.ComponentModel.Collection
- Microsoft.VisualBasic.ComponentModel.Collection.Generic
- Microsoft.VisualBasic.ComponentModel.DataSourceModel
- Microsoft.VisualBasic.Data.GraphTheory.GridGraph
- Microsoft.VisualBasic.DataStorage.netCDF
- Microsoft.VisualBasic.DataStorage.netCDF.Components
- Microsoft.VisualBasic.DataStorage.netCDF.Data
- Microsoft.VisualBasic.DataStorage.netCDF.DataVector
- Microsoft.VisualBasic.Language
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.MachineLearning.CellularAutomaton
- Microsoft.VisualBasic.Math.Framework
- randf = Microsoft.VisualBasic.Math.RandomExtensions
- System
- System.Drawing
- System.Runtime.CompilerServices
- System.Windows.Forms

## File tree
- Debugger\Debugger.vb
- Debugger\WriteCDF.vb
- Extensions.vb
- GameOfLife.WinForms\ConwayCell.vb
- GameOfLife.WinForms\MainForm.vb
- GameOfLife.WinForms\Program.vb
- SimulatorModel\CellEntity.vb
- SimulatorModel\Individual.vb
- SimulatorModel\Neighborhood.vb
- SimulatorModel\Simulator.vb

