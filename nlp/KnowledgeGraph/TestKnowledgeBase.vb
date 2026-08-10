#Region "Microsoft.VisualBasic::cbd17021f578e1047f5e43e7a4e8b408, nlp\KnowledgeGraph\TestKnowledgeBase.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 250
    '    Code Lines: 147 (58.80%)
    ' Comment Lines: 81 (32.40%)
    '    - Xml Docs: 11.11%
    ' 
    '   Blank Lines: 22 (8.80%)
    '     File Size: 14.65 KB


    ' Module TestKnowledgeBase
    ' 
    '     Function: BuildTestGraph
    ' 
    '     Sub: AddAttrs
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' TestKnowledgeBase.vb - 测试知识库构建
'
' 基于网络搜索结果和百科知识，为 12 个测试词条构建属性知识图谱。
'
' 测试词条:
'   water, 水, H2O               → 期望判定为同义（水的三种表述）
'   apple, 苹果                   → 期望判定为同义（水果）
'   苹果公司                      → 期望与非同义（科技公司，与水果苹果无属性交集）
'   老虎, 狮子                    → 期望 Sibling（同属猫科豹属）
'   哺乳动物                      → 期望 Is-A 上位词（老虎/狮子 is-a 哺乳动物）
'   芬达, 可乐                    → 期望 Sibling（同属碳酸饮料）
'   解渴                          → 期望 Related-To（与 water/芬达/可乐 功能性关联）
'
' 属性数据来源：
'   - PubChem (https://pubchem.ncbi.nlm.nih.gov/compound/Water)
'   - Wikipedia (Properties of water, Tiger, Lion, Apple, Fanta)
'   - SeaWorld (Tiger classification)
'   - Coca-Cola 官网 (Fanta ingredients)
'   - Britannica (Apple fruit)
' ============================================================================

Imports System.Collections.Generic

''' <summary>
''' 测试知识库构建器。
''' </summary>
Public Module TestKnowledgeBase

    ''' <summary>
    ''' 构建测试知识图谱。
    ''' </summary>
    Public Function BuildTestGraph() As KnowledgeGraph
        Dim g As New KnowledgeGraph()

        ' ================================================================
        ' 添加实体节点
        ' ================================================================
        Dim waterId As Integer = g.AddEntity("water", "en", "substance")
        Dim shuiId As Integer = g.AddEntity("水", "zh", "substance")
        Dim h2oId As Integer = g.AddEntity("H2O", "formula", "substance")

        Dim appleId As Integer = g.AddEntity("apple", "en", "fruit")
        Dim pingguoId As Integer = g.AddEntity("苹果", "zh", "fruit")
        Dim appleIncId As Integer = g.AddEntity("苹果公司", "zh", "company")

        Dim tigerId As Integer = g.AddEntity("老虎", "zh", "organism")
        Dim lionId As Integer = g.AddEntity("狮子", "zh", "organism")
        Dim mammalId As Integer = g.AddEntity("哺乳动物", "zh", "concept")

        Dim fantaId As Integer = g.AddEntity("芬达", "zh", "beverage")
        Dim colaId As Integer = g.AddEntity("可乐", "zh", "beverage")
        Dim thirstId As Integer = g.AddEntity("解渴", "zh", "concept")

        ' ================================================================
        ' water 的属性（22 个）
        ' 来源: PubChem, Wikipedia - Properties of water
        ' ================================================================
        AddAttrs(g, waterId, {
            ("chem_formula_h2o", "chemical"), ("state_liquid", "physical"),
            ("colorless", "physical"), ("transparent", "physical"),
            ("odorless", "physical"), ("tasteless", "physical"),
            ("boiling_point_100c", "physical"), ("freezing_point_0c", "physical"),
            ("density_1g_cm3", "physical"), ("molecular_weight_18", "chemical"),
            ("polar_molecule", "chemical"), ("universal_solvent", "chemical"),
            ("essential_for_life", "biological"), ("covers_71pct_earth", "geographical"),
            ("hydrogen_bonds", "chemical"), ("high_specific_heat", "physical"),
            ("surface_tension_high", "physical"), ("phase_ice", "physical"),
            ("phase_steam", "physical"), ("quenches_thirst", "functional"),
            ("provides_hydration", "functional"), ("cooling_sensation", "functional")
        })

        ' ================================================================
        ' 水 的属性（20 个）
        ' 与 water 共享 18 个，另有 2 个中文独有属性
        ' ================================================================
        AddAttrs(g, shuiId, {
            ("chem_formula_h2o", "chemical"), ("state_liquid", "physical"),
            ("colorless", "physical"), ("transparent", "physical"),
            ("odorless", "physical"), ("tasteless", "physical"),
            ("boiling_point_100c", "physical"), ("freezing_point_0c", "physical"),
            ("density_1g_cm3", "physical"), ("molecular_weight_18", "chemical"),
            ("polar_molecule", "chemical"), ("universal_solvent", "chemical"),
            ("essential_for_life", "biological"), ("covers_71pct_earth", "geographical"),
            ("hydrogen_bonds", "chemical"), ("quenches_thirst", "functional"),
            ("provides_hydration", "functional"), ("cooling_sensation", "functional"),
            ("wuxing_element", "cultural"), ("chinese_char_water", "cultural")
        })

        ' ================================================================
        ' H2O 的属性（18 个）
        ' 与 water/水 共享 14 个化学物理属性，另有 4 个化学专业属性
        ' ================================================================
        AddAttrs(g, h2oId, {
            ("chem_formula_h2o", "chemical"), ("state_liquid", "physical"),
            ("colorless", "physical"), ("transparent", "physical"),
            ("odorless", "physical"), ("tasteless", "physical"),
            ("boiling_point_100c", "physical"), ("freezing_point_0c", "physical"),
            ("density_1g_cm3", "physical"), ("molecular_weight_18", "chemical"),
            ("polar_molecule", "chemical"), ("universal_solvent", "chemical"),
            ("essential_for_life", "biological"), ("hydrogen_bonds", "chemical"),
            ("molecular_geometry_bent", "chemical"), ("covalent_bonds", "chemical"),
            ("cas_number_7732_18_5", "chemical"), ("two_h_one_o", "chemical")
        })

        ' ================================================================
        ' apple (水果) 的属性（16 个）
        ' 来源: Wikipedia - Apple, Britannica, NC State Plant Toolbox
        ' ================================================================
        AddAttrs(g, appleId, {
            ("type_fruit", "botanical"), ("color_red_green_yellow", "physical"),
            ("sweet_taste", "functional"), ("crunchy_texture", "physical"),
            ("grows_on_tree", "botanical"), ("family_rosaceae", "botanical"),
            ("genus_malus", "botanical"), ("contains_vitamin_c", "nutritional"),
            ("contains_pectin", "nutritional"), ("contains_fiber", "nutritional"),
            ("origin_central_asia", "geographical"), ("temperate_climate", "environmental"),
            ("deciduous_tree", "botanical"), ("edible_skin", "botanical"),
            ("ripens_autumn", "botanical"), ("stored_cold", "commercial")
        })

        ' ================================================================
        ' 苹果 (水果) 的属性（14 个）
        ' 与 apple 共享 13 个，另有 1 个中文独有属性
        ' ================================================================
        AddAttrs(g, pingguoId, {
            ("type_fruit", "botanical"), ("color_red_green_yellow", "physical"),
            ("sweet_taste", "functional"), ("crunchy_texture", "physical"),
            ("grows_on_tree", "botanical"), ("family_rosaceae", "botanical"),
            ("genus_malus", "botanical"), ("contains_vitamin_c", "nutritional"),
            ("contains_fiber", "nutritional"), ("origin_central_asia", "geographical"),
            ("temperate_climate", "environmental"), ("deciduous_tree", "botanical"),
            ("edible_skin", "botanical"), ("variety_fuji", "botanical")
        })

        ' ================================================================
        ' 苹果公司 的属性（16 个）
        ' 与 apple/苹果(水果) 零共享——虽然名称包含"苹果"
        ' ================================================================
        AddAttrs(g, appleIncId, {
            ("type_technology_company", "commercial"), ("founded_1976", "commercial"),
            ("founder_steve_jobs", "commercial"), ("founder_steve_wozniak", "commercial"),
            ("founder_ronald_wayne", "commercial"), ("hq_cupertino_california", "geographical"),
            ("product_iphone", "commercial"), ("product_mac", "commercial"),
            ("product_ipad", "commercial"), ("product_apple_watch", "commercial"),
            ("product_airpods", "commercial"), ("stock_nasdaq_aapl", "commercial"),
            ("logo_bitten_apple", "commercial"), ("ceo_tim_cook", "commercial"),
            ("ecosystem_ios", "commercial"), ("app_store", "commercial")
        })

        ' ================================================================
        ' 老虎 的属性（21 个）
        ' 来源: Wikipedia - Tiger, SeaWorld classification
        ' ================================================================
        AddAttrs(g, tigerId, {
            ("class_mammalia", "biological"), ("order_carnivora", "biological"),
            ("family_felidae", "biological"), ("genus_panthera", "biological"),
            ("species_tigris", "biological"), ("color_orange_black_stripes", "physical"),
            ("largest_cat_species", "biological"), ("habitat_asia", "geographical"),
            ("solitary_predator", "behavioral"), ("endangered_species", "conservation"),
            ("apex_predator", "behavioral"), ("weight_100_300kg", "physical"),
            ("carnivorous_diet", "behavioral"), ("powerful_swimmer", "behavioral"),
            ("warm_blooded", "biological"), ("has_hair_or_fur", "biological"),
            ("vertebrate", "biological"), ("live_birth", "biological"),
            ("mammary_glands", "biological"), ("diaphragm", "biological"),
            ("four_chambered_heart", "biological")
        })

        ' ================================================================
        ' 狮子 的属性（21 个）
        ' 与老虎共享 13 个生物分类属性
        ' ================================================================
        AddAttrs(g, lionId, {
            ("class_mammalia", "biological"), ("order_carnivora", "biological"),
            ("family_felidae", "biological"), ("genus_panthera", "biological"),
            ("species_leo", "biological"), ("color_golden_yellow", "physical"),
            ("mane_in_males", "physical"), ("pride_social_structure", "behavioral"),
            ("habitat_africa", "geographical"), ("apex_predator", "behavioral"),
            ("weight_150_250kg", "physical"), ("carnivorous_diet", "behavioral"),
            ("king_of_beasts", "cultural"), ("cooperative_hunting", "behavioral"),
            ("warm_blooded", "biological"), ("has_hair_or_fur", "biological"),
            ("vertebrate", "biological"), ("live_birth", "biological"),
            ("mammary_glands", "biological"), ("diaphragm", "biological"),
            ("four_chambered_heart", "biological")
        })

        ' ================================================================
        ' 哺乳动物 的属性（12 个）
        ' 作为老虎/狮子的上位词，其属性应大部分出现在老虎/狮子中
        ' ================================================================
        AddAttrs(g, mammalId, {
            ("class_mammalia", "biological"), ("warm_blooded", "biological"),
            ("has_hair_or_fur", "biological"), ("mammary_glands", "biological"),
            ("live_birth", "biological"), ("vertebrate", "biological"),
            ("three_middle_ear_bones", "biological"), ("neocortex_brain", "biological"),
            ("diaphragm", "biological"), ("four_chambered_heart", "biological"),
            ("sweat_glands", "biological"), ("heterodont_teeth", "biological")
        })

        ' ================================================================
        ' 芬达 的属性（12 个）
        ' 来源: Coca-Cola 官网, Wikipedia - Fanta
        ' ================================================================
        AddAttrs(g, fantaId, {
            ("type_carbonated_drink", "commercial"), ("flavor_orange", "physical"),
            ("owned_by_coca_cola", "commercial"), ("originated_germany_1940", "historical"),
            ("fruit_flavored", "physical"), ("caffeine_free", "chemical"),
            ("color_orange", "physical"), ("sweet_taste", "functional"),
            ("thirst_quenching", "functional"), ("contains_sugar", "nutritional"),
            ("provides_hydration", "functional"), ("refreshing", "functional")
        })

        ' ================================================================
        ' 可乐 的属性（12 个）
        ' 与芬达共享 6 个饮料属性
        ' ================================================================
        AddAttrs(g, colaId, {
            ("type_carbonated_drink", "commercial"), ("flavor_cola", "physical"),
            ("contains_caffeine", "chemical"), ("caramel_color", "physical"),
            ("owned_by_coca_cola", "commercial"), ("kola_nut_extract", "chemical"),
            ("sweet_taste", "functional"), ("dark_color", "physical"),
            ("thirst_quenching", "functional"), ("contains_sugar", "nutritional"),
            ("provides_hydration", "functional"), ("phosphoric_acid", "chemical")
        })

        ' ================================================================
        ' 解渴 的属性（10 个）
        ' 作为功能性概念，与 water/芬达/可乐 共享功能属性
        ' ================================================================
        AddAttrs(g, thirstId, {
            ("thirst_quenching", "functional"), ("relieves_thirst", "functional"),
            ("refreshing", "functional"), ("provides_hydration", "functional"),
            ("associated_with_beverages", "functional"), ("associated_with_water", "functional"),
            ("oral_dryness_relief", "functional"), ("cooling_sensation", "functional"),
            ("summer_association", "cultural"), ("quenches_thirst", "functional")
        })

        Return g
    End Function

    ''' <summary>
    ''' 批量添加实体属性。
    ''' </summary>
    Private Sub AddAttrs(g As KnowledgeGraph, entityId As Integer,
                        attrs As (name As String, category As String)())
        For Each attr In attrs
            g.AddEntityAttribute(entityId, attr.name, attr.category)
        Next
    End Sub

End Module
