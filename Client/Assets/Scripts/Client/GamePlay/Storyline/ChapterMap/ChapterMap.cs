﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ChapterMap : PoolObject
{
    #region  MouseControlMove

    Vector2 mouseWheelPos_Last;
    private bool startDrag = false;
    [SerializeField] private RectTransform Root;

    void Update()
    {
        if (IsMouseHoverChapterMap)
        {
            if (!startDrag)
            {
                if (Input.GetMouseButtonDown(2))
                {
                    startDrag = true;
                    mouseWheelPos_Last = Input.mousePosition;
                }
            }
            else
            {
                if (Input.GetMouseButton(2))
                {
                    Vector2 diff = (Vector2) Input.mousePosition - mouseWheelPos_Last;
                    mouseWheelPos_Last = (Vector2) Input.mousePosition;
                    Root.anchoredPosition += diff;
                }

                if (Input.GetMouseButtonUp(2))
                {
                    startDrag = false;
                }
            }

            float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            if ((!(mouseScroll > 0) || !(Root.localScale.x >= 2)) && (!(mouseScroll < 0) || !(Root.localScale.x <= 0.5f)))
            {
                Root.localScale += Vector3.one * 0.3f * mouseScroll;
            }
        }
        else
        {
            startDrag = false;
        }
    }

    public bool IsMouseHoverChapterMap = false;

    public void SetMouseHover(bool isHover)
    {
        IsMouseHoverChapterMap = isHover;
    }

    #endregion

    private Vector2[] nodeLocations;

    [SerializeField] private Transform ChapterMapRoutesTransform;
    [SerializeField] private Transform ChapterMapNodesTransform;

    private int routeIndex = 0;
    private Dictionary<int, ChapterMapRoute> ChapterMapRoutes = new Dictionary<int, ChapterMapRoute>(); // key: routeIndex
    private Dictionary<int, ChapterMapNode> ChapterMapNodes = new Dictionary<int, ChapterMapNode>(); // key: nodeLocationIndex

    private Dictionary<NodeTypes, List<int>> NodeCategory = new Dictionary<NodeTypes, List<int>>();
    private Dictionary<RouteTypes, List<int>> RouteCategory = new Dictionary<RouteTypes, List<int>>();

    public override void PoolRecycle()
    {
        Reset();
        base.PoolRecycle();
    }

    internal void Reset()
    {
        foreach (KeyValuePair<int, ChapterMapRoute> kv in ChapterMapRoutes)
        {
            kv.Value.PoolRecycle();
        }

        ChapterMapRoutes.Clear();

        foreach (KeyValuePair<NodeTypes, List<int>> kv in NodeCategory)
        {
            kv.Value.Clear();
        }

        foreach (KeyValuePair<int, ChapterMapNode> kv in ChapterMapNodes)
        {
            kv.Value.PoolRecycle();
        }

        ChapterMapNodes.Clear();

        foreach (KeyValuePair<RouteTypes, List<int>> kv in RouteCategory)
        {
            kv.Value.Clear();
        }

        Cur_SelectedNode = null;
    }

    private int RoundCount = 2;
    public Chapter Cur_Chapter;

    internal void Initialize(Chapter chapter)
    {
        if (NodeCategory.Count == 0)
        {
            foreach (string str in Enum.GetNames(typeof(NodeTypes)))
            {
                NodeTypes type = (NodeTypes) Enum.Parse(typeof(NodeTypes), str);
                NodeCategory.Add(type, new List<int>());
            }
        }

        if (RouteCategory.Count == 0)
        {
            foreach (string str in Enum.GetNames(typeof(RouteTypes)))
            {
                RouteTypes type = (RouteTypes) Enum.Parse(typeof(RouteTypes), str);
                RouteCategory.Add(type, new List<int>());
            }
        }

        Reset();

        Cur_Chapter = chapter;

        DrawMap(chapter.ChapterMapRoundCount);

        SetPicForNodes();

        SetLevels(Cur_Chapter.Levels);
    }

    private void DrawMap(int roundCount)
    {
        RoundCount = roundCount;

        //float routeLength = 520f / (roundCount + 2);
        float routeLength = 200f;
        routeIndex = 0;

        Vector2 a = new Vector2(1, 0);
        Vector2 b = new Vector2(0.5f, 0.866f);
        Vector2 c = new Vector2(-0.5f, 0.866f);
        nodeLocations = new Vector2[(roundCount + 1) * roundCount * 3 + 1 + 12];
        Vector2[] directions = new[] {a, b, c, -a, -b, -c, a};
        int index = 0;
        nodeLocations[index++] = Vector2.zero;

        // 画点
        NodeCategory[NodeTypes.All].Add(0);

        for (int round = 1; round <= roundCount; round++)
        {
            for (int i = 0; i < 6; i++)
            {
                NodeCategory[NodeTypes.All].Add(index);
                NodeCategory[NodeTypes.Common].Add(index);
                NodeCategory[NodeTypes.Corner].Add(index);
                if (round == roundCount)
                {
                    NodeCategory[NodeTypes.FinalRoundCommon].Add(index);
                    NodeCategory[NodeTypes.FinalRoundCorner].Add(index);
                }

                nodeLocations[index++] = round * directions[i] * routeLength;
                for (int middle = 1; middle <= round - 1; middle++)
                {
                    NodeCategory[NodeTypes.All].Add(index);
                    NodeCategory[NodeTypes.Common].Add(index);
                    NodeCategory[NodeTypes.Edge].Add(index);
                    if (round == roundCount)
                    {
                        NodeCategory[NodeTypes.FinalRoundCommon].Add(index);
                        NodeCategory[NodeTypes.FinalRoundEdge].Add(index);
                    }

                    nodeLocations[index++] = ((middle) * directions[i + 1] + (round - middle) * directions[i]) * routeLength;
                }
            }
        }

        //六角点BOSS
        for (int i = 0; i < 6; i++)
        {
            NodeCategory[NodeTypes.All].Add(index);
            NodeCategory[NodeTypes.Boss].Add(index);
            nodeLocations[index++] = (roundCount + 1) * directions[i] * routeLength * 1.1f;
        }

        //六边中点宝藏
        for (int i = 0; i < 6; i++)
        {
            NodeCategory[NodeTypes.All].Add(index);
            NodeCategory[NodeTypes.Treasure].Add(index);
            nodeLocations[index++] = (((roundCount + 1) / 2.0f) * directions[i + 1] + ((roundCount + 1) / 2.0f) * directions[i]) * routeLength * 1.1f;
        }

        for (int i = 0; i < nodeLocations.Length; i++)
        {
            GenerateNode(i);
        }

        // 画线
        for (int i = 1; i <= 6; i++)
        {
            GenerateRoute(0, i, new List<RouteTypes> {RouteTypes.All, RouteTypes.Common, RouteTypes.Chord});
        }

        int start = 1;
        for (int round = 1; round <= roundCount; round++)
        {
            List<RouteTypes> res = new List<RouteTypes> {RouteTypes.All, RouteTypes.Common, RouteTypes.Circumferential};
            if (round == roundCount) res.AddRange(new List<RouteTypes> {RouteTypes.FinalRoundCommon, RouteTypes.FinalRoundCircumferential});
            start += 6 * (round - 1);
            for (int i = 0; i < round * 6 - 1; i++)
            {
                if (i % round == 0 || (i + 1) % round == 0) // if corner
                {
                    if (round == roundCount)
                    {
                        List<RouteTypes> res_plus = res.ToArray().ToList();
                        res_plus.AddRange(new List<RouteTypes> {RouteTypes.Corner, RouteTypes.FinalRoundCorner});
                        GenerateRoute(start + i, start + i + 1, res_plus);
                    }
                    else
                    {
                        List<RouteTypes> res_plus = res.ToArray().ToList();
                        res_plus.AddRange(new List<RouteTypes> {RouteTypes.Corner});
                        GenerateRoute(start + i, start + i + 1, res_plus);
                    }
                }
                else
                {
                    if (round == roundCount)
                    {
                        List<RouteTypes> res_plus = res.ToArray().ToList();
                        res_plus.AddRange(new List<RouteTypes> {RouteTypes.BeyondCorner, RouteTypes.FinalRoundBeyondCorner});
                        GenerateRoute(start + i, start + i + 1, res_plus);
                    }
                    else
                    {
                        List<RouteTypes> res_plus = res.ToArray().ToList();
                        res_plus.AddRange(new List<RouteTypes> {RouteTypes.BeyondCorner});
                        GenerateRoute(start + i, start + i + 1, res_plus);
                    }
                }
            }

            if (round == roundCount)
            {
                List<RouteTypes> res_plus = res.ToArray().ToList();
                res_plus.AddRange(new List<RouteTypes> {RouteTypes.Corner, RouteTypes.FinalRoundCorner});
                GenerateRoute(start + round * 6 - 1, start, res);
            }
            else
            {
                List<RouteTypes> res_plus = res.ToArray().ToList();
                res_plus.AddRange(new List<RouteTypes> {RouteTypes.Corner});
                GenerateRoute(start + round * 6 - 1, start, res);
            }
        }

        for (int i = 1; i < nodeLocations.Length; i++)
        {
            for (int round = 1; round < roundCount; round++)
            {
                int last_end = (round - 1) * round * 6 / 2;
                int this_end = (round + 1) * round * 6 / 2;
                int next_end = (round + 2) * (round + 1) * 6 / 2;
                if (i <= this_end)
                {
                    int cornerIndex = (i - last_end - 1) / round;
                    bool isCornerIndex = (i - last_end - 1) % round == 0;

                    List<RouteTypes> res = new List<RouteTypes> {RouteTypes.All, RouteTypes.Common, RouteTypes.Chord};
                    if (round == roundCount) res.AddRange(new List<RouteTypes> {RouteTypes.FinalRoundCommon, RouteTypes.FinalRoundChord});

                    if (isCornerIndex)
                    {
                        if (cornerIndex == 0) // 第一个点
                        {
                            GenerateRoute(i, i + round * 6, res);
                            GenerateRoute(i, i + round * 6 + 1, res);
                            GenerateRoute(i, next_end, res);
                        }
                        else //其他角点
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                GenerateRoute(i, i + round * 6 + j + cornerIndex - 1, res);
                            }
                        }
                    }
                    else //边点
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            GenerateRoute(i, i + round * 6 + j + cornerIndex, res);
                        }
                    }

                    break;
                }
            }
        }

        // 六角点 BOSS
        int end1 = (roundCount - 1) * roundCount * 6 / 2;
        int end2 = (roundCount + 1) * roundCount * 6 / 2;
        for (int i = 0; i < 6; i++)
        {
            int node_index = end1 + 1 + roundCount * i;
            GenerateRoute(node_index, end2 + (i + 1), new List<RouteTypes> {RouteTypes.All, RouteTypes.ToBoss});
        }

        // 六边中点宝藏
        for (int i = 0; i < 6; i++)
        {
            int node_index = end1 + 1 + roundCount * i + roundCount / 2;
            GenerateRoute(node_index, end2 + i + 6 + 1, new List<RouteTypes> {RouteTypes.All, RouteTypes.ToTreasure});
        }
    }

    private void SetLevels(SortedDictionary<int, Level> levels)
    {
        foreach (KeyValuePair<int, Level> kv in levels)
        {
            SetNodeLevel(kv.Key, kv.Value);
        }
    }

    private enum RouteTypes
    {
        All,
        Common,
        FinalRoundCommon,
        Circumferential,
        FinalRoundCircumferential,
        Chord,
        FinalRoundChord,
        Corner,
        FinalRoundCorner,
        BeyondCorner,
        FinalRoundBeyondCorner,
        ToTreasure,
        ToBoss,
    }

    private enum NodeTypes
    {
        All,
        Common,
        FinalRoundCommon,
        Corner,
        FinalRoundCorner,
        Edge,
        FinalRoundEdge,
        Treasure,
        Boss
    }

    private void GenerateNode(int nodeLocationIndex)
    {
        ChapterMapNode cmn = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.ChapterMapNode].AllocateGameObject<ChapterMapNode>(ChapterMapNodesTransform);
        cmn.AdjacentRoutes.Clear();
        ChapterMapNodes.Add(nodeLocationIndex, cmn);
        cmn.transform.localPosition = nodeLocations[nodeLocationIndex];
    }

    private void GenerateRoute(int startIndex, int endIndex, List<RouteTypes> routeTypes)
    {
        if (Cur_Chapter.Routes.Count != 0)
        {
            if (Cur_Chapter.Routes.ContainsKey(startIndex))
            {
                HashSet<int> endNodeIndices = Cur_Chapter.Routes[startIndex];
                if (endNodeIndices.Contains(endIndex))
                {
                    DrawRoute(startIndex, endIndex, routeTypes);
                }
            }
        }
        else
        {
            if (!Cur_Chapter.AllRoutes.ContainsKey(startIndex))
            {
                Cur_Chapter.AllRoutes.Add(startIndex, new HashSet<int>());
            }

            if (!Cur_Chapter.AllRoutes.ContainsKey(endIndex))
            {
                Cur_Chapter.AllRoutes.Add(endIndex, new HashSet<int>());
            }

            Cur_Chapter.AllRoutes[startIndex].Add(endIndex);
            Cur_Chapter.AllRoutes[endIndex].Add(startIndex);
            DrawRoute(startIndex, endIndex, routeTypes);
        }
    }

    private void DrawRoute(int startIndex, int endIndex, List<RouteTypes> routeTypes)
    {
        int index = routeIndex++;
        ChapterMapRoute r = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.ChapterMapRoute].AllocateGameObject<ChapterMapRoute>(ChapterMapRoutesTransform);
        r.Refresh(nodeLocations[startIndex], nodeLocations[endIndex], index, startIndex, endIndex);
        ChapterMapNodes[startIndex].AdjacentRoutes.Add(index);
        ChapterMapNodes[endIndex].AdjacentRoutes.Add(index);
        ChapterMapRoutes.Add(index, r);
        foreach (RouteTypes routeType in routeTypes)
        {
            RouteCategory[routeType].Add(index);
        }
    }

    private float shopRatio = 0.1f;
    private float restRatio = 0.1f;

    private void OnHoverNode(ChapterMapNode node)
    {
        foreach (KeyValuePair<int, ChapterMapNode> kv in ChapterMapNodes)
        {
            if (kv.Value != node)
            {
                kv.Value.IsHovered = false;
            }
        }
    }

    private void SetPicForNodes()
    {
        foreach (int i in NodeCategory[NodeTypes.All])
        {
            ChapterMapNodes[i].Initialize(i, SelectNode, OnHoverNode, LevelTypes.Start);
        }

//        int shopNumMax = Mathf.CeilToInt(shopRatio * NodeCategory[NodeTypes.Common].Count);
//        int restNumMax = Mathf.CeilToInt(restRatio * NodeCategory[NodeTypes.Common].Count);
//
//        int finalRoundEdgeShopNumber = Mathf.Min(shopNumMax, 3);
//        int otherShopNumber = shopNumMax - finalRoundEdgeShopNumber;
//        List<int> shops = Utils.GetRandomFromList(NodeCategory[NodeTypes.FinalRoundEdge], finalRoundEdgeShopNumber);
//        shops.AddRange(Utils.GetRandomFromList(NodeCategory[NodeTypes.Common], otherShopNumber, shops));
//
//        int finalRoundEdgeRestNumber = Mathf.Min(shopNumMax, 6);
//        int otherRestNumber = restNumMax - finalRoundEdgeRestNumber;
//        List<int> rests = Utils.GetRandomFromList(NodeCategory[NodeTypes.FinalRoundCorner], finalRoundEdgeRestNumber);
//        rests.AddRange(Utils.GetRandomFromList(NodeCategory[NodeTypes.Common], otherRestNumber, rests));
//
//        foreach (int i in shops)
//        {
//            ChapterMapNodes[i].Initialize(i, SelectNode, OnHoverNode, LevelType.Shop);
//        }
//
//        foreach (int i in rests)
//        {
//            ChapterMapNodes[i].Initialize(i, SelectNode, OnHoverNode, LevelType.Rest);
//        }
//
//        foreach (int i in NodeCategory[NodeTypes.Treasure])
//        {
//            ChapterMapNodes[i].Initialize(i, SelectNode, OnHoverNode, LevelType.Treasure);
//        }
//
//        foreach (int i in NodeCategory[NodeTypes.Boss])
//        {
//            ChapterMapNodes[i].Initialize(i, SelectNode, OnHoverNode, LevelType.Enemy, EnemyType.Boss);
//        }
    }

    internal ChapterMapNode Cur_SelectedNode;

    public void UnSelectAllNode()
    {
        foreach (KeyValuePair<int, ChapterMapNode> kv in ChapterMapNodes)
        {
            kv.Value.IsSelected = false;
        }

        Cur_SelectedNode = null;
    }

    private void SelectNode(int nodeIndex)
    {
        if (ChapterMapNodes[nodeIndex].IsBeated)
        {
            return;
        }

        foreach (KeyValuePair<int, ChapterMapNode> kv in ChapterMapNodes)
        {
            kv.Value.IsSelected = false;
        }

        ChapterMapNodes[nodeIndex].IsSelected = true;
        Cur_SelectedNode = ChapterMapNodes[nodeIndex];
        OnSelectChapterNode?.Invoke(Cur_SelectedNode);
    }

    internal UnityAction<ChapterMapNode> OnSelectChapterNode;

    private void SetNodeLevel(int nodeIndex, Level level)
    {
        ChapterMapNodes[nodeIndex].SetLevel(level);
    }

    #region LevelEditor

    public void SetCurrentNodeLevel(Level level)
    {
        if (Cur_SelectedNode != null)
        {
            Cur_SelectedNode.SetLevel(level);
        }
    }

    // 仅用于剧情编辑器
    public void SaveChapter()
    {
        Cur_Chapter.Levels.Clear();
        foreach (KeyValuePair<int, ChapterMapNode> kv in ChapterMapNodes)
        {
            if (kv.Value.Cur_Level != null)
            {
                Cur_Chapter.Levels.Add(kv.Key, kv.Value.Cur_Level.Clone());
            }
        }
    }

    #endregion

    #region GameStory

    public void RefreshKnownLevels()
    {
        foreach (KeyValuePair<int, ChapterMapNode> kv in ChapterMapNodes)
        {
            kv.Value.IsSelected = false;
            kv.Value.IsHovered = false;
            kv.Value.IsBeated = false;
        }

        foreach (KeyValuePair<int, ChapterMapNode> kv in ChapterMapNodes)
        {
            kv.Value.gameObject.SetActive(false);
        }

        foreach (KeyValuePair<int, ChapterMapRoute> kv in ChapterMapRoutes)
        {
            kv.Value.SetRouteState(ChapterMapRoute.RouteStates.None);
        }

        foreach (KeyValuePair<int, bool> kv in Cur_Chapter.LevelBeatedDictionary)
        {
            ChapterMapNodes[kv.Key].IsBeated = kv.Value;
            ChapterMapNodes[kv.Key].gameObject.SetActive(kv.Value);
        }

        foreach (KeyValuePair<int, bool> kv in Cur_Chapter.LevelBeatedDictionary)
        {
            foreach (int ri in ChapterMapNodes[kv.Key].AdjacentRoutes)
            {
                ChapterMapRoute route = ChapterMapRoutes[ri];

                bool hasLevel_0 = Cur_Chapter.LevelBeatedDictionary.ContainsKey(route.NodeIndex_0);
                bool beated_0 = hasLevel_0 && Cur_Chapter.LevelBeatedDictionary[route.NodeIndex_0];
                bool hasLevel_1 = Cur_Chapter.LevelBeatedDictionary.ContainsKey(route.NodeIndex_1);
                bool beated_1 = hasLevel_1 && Cur_Chapter.LevelBeatedDictionary[route.NodeIndex_1];

                if (beated_0 && beated_1)
                {
                    ChapterMapNodes[route.NodeIndex_0].gameObject.SetActive(hasLevel_0);
                    ChapterMapNodes[route.NodeIndex_1].gameObject.SetActive(hasLevel_1);
                    route.SetRouteState(ChapterMapRoute.RouteStates.Conquered);
                }
                else if (beated_0 || beated_1)
                {
                    ChapterMapNodes[route.NodeIndex_0].gameObject.SetActive(hasLevel_0);
                    ChapterMapNodes[route.NodeIndex_1].gameObject.SetActive(hasLevel_1);

                    if (hasLevel_0 && hasLevel_1)
                    {
                        route.SetRouteState(ChapterMapRoute.RouteStates.NextStep);
                    }
                    else
                    {
                        route.SetRouteState(ChapterMapRoute.RouteStates.None);
                    }
                }
                else
                {
                    if (hasLevel_0 && hasLevel_1)
                    {
                        route.SetRouteState(ChapterMapRoute.RouteStates.Dashed);
                    }
                    else
                    {
                        route.SetRouteState(ChapterMapRoute.RouteStates.None);
                    }
                }
            }
        }

        ChapterMapNodes[0].gameObject.SetActive(true);
    }

    #endregion
}