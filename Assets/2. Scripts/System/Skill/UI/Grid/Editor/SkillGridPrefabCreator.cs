#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using SlayerLegend.Skill.UI.Grid;

namespace SlayerLegend.Skill.UI.Grid.Editor
{
    // 스킬 그리드 시스템 프리팹 생성 에디터 도구
    // 메뉴: Tools > Skill Grid > Create Prefabs
    public static class SkillGridPrefabCreator
    {
        private const string PREFAB_PATH = "Assets/2. Scripts/System/Skill/UI/Grid/Prefabs";

        #region 메뉴 항목

        [MenuItem("Tools/Skill Grid/Create All Prefabs", false, 0)]
        public static void CreateAllPrefabs()
        {
            EnsureDirectoryExists();

            CreateGridCellPrefab();
            CreateDraggableItemPrefab();
            CreateInventorySlotPrefab();
            CreateGridSystemPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[SkillGridPrefabCreator] 모든 프리팹 생성 완료!");
        }

        [MenuItem("Tools/Skill Grid/Create Grid Cell Prefab", false, 1)]
        public static void CreateGridCellPrefab()
        {
            EnsureDirectoryExists();

            string path = $"{PREFAB_PATH}/SkillGridCell.prefab";

            // 기존 프리팹이 있으면 삭제
            DeleteExistingPrefab(path);

            // 게임오브젝트 생성
            GameObject cellObj = new GameObject("SkillGridCell");

            // RectTransform 설정
            RectTransform rectTransform = cellObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(80f, 80f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // 배경 이미지
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(cellObj.transform);
            bgObj.transform.localPosition = Vector3.zero;

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.3f, 1f);
            bgImage.raycastTarget = true;

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // 하이라이트 오버레이
            GameObject highlightObj = new GameObject("HighlightOverlay");
            highlightObj.transform.SetParent(cellObj.transform);
            highlightObj.transform.localPosition = Vector3.zero;

            Image highlightImage = highlightObj.AddComponent<Image>();
            highlightImage.color = new Color(0f, 1f, 0f, 0f);
            highlightImage.raycastTarget = false;

            RectTransform highlightRect = highlightObj.GetComponent<RectTransform>();
            highlightRect.anchorMin = Vector2.zero;
            highlightRect.anchorMax = Vector2.one;
            highlightRect.sizeDelta = Vector2.zero;

            // SkillGridCell 컴포넌트 추가
            SkillGridCell cell = cellObj.AddComponent<SkillGridCell>();

            // SerializedObject로 필드 설정 (SkillGridCell.cs의 실제 필드명과 일치해야 함)
            SerializedObject serializedCell = new SerializedObject(cell);
            serializedCell.FindProperty("background").objectReferenceValue = bgImage;
            serializedCell.FindProperty("highlightOverlay").objectReferenceValue = highlightImage;
            serializedCell.FindProperty("normalColor").colorValue = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            serializedCell.FindProperty("darkColor").colorValue = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            serializedCell.FindProperty("hoverColor").colorValue = new Color(0.3f, 0.5f, 0.3f, 0.8f);
            serializedCell.FindProperty("validColor").colorValue = new Color(0.2f, 0.8f, 0.2f, 0.6f);
            serializedCell.FindProperty("invalidColor").colorValue = new Color(0.8f, 0.2f, 0.2f, 0.6f);
            serializedCell.ApplyModifiedProperties();

            // 프리팹 저장
            SavePrefab(cellObj, path);
        }

        [MenuItem("Tools/Skill Grid/Create Draggable Item Prefab", false, 2)]
        public static void CreateDraggableItemPrefab()
        {
            EnsureDirectoryExists();

            string path = $"{PREFAB_PATH}/SkillDraggableItem.prefab";

            // 기존 프리팹이 있으면 삭제
            DeleteExistingPrefab(path);

            // 게임오브젝트 생성
            GameObject itemObj = new GameObject("SkillDraggableItem");

            // RectTransform 설정
            RectTransform rectTransform = itemObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(80f, 80f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // CanvasGroup 추가 (드래그 시 투명도 제어)
            CanvasGroup canvasGroup = itemObj.AddComponent<CanvasGroup>();

            // 배경 이미지
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(itemObj.transform);
            bgObj.transform.localPosition = Vector3.zero;

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.6f, 0.9f, 1f);
            bgImage.raycastTarget = true;

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // 아이콘 이미지
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(itemObj.transform);
            iconObj.transform.localPosition = Vector3.zero;

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;
            iconImage.raycastTarget = false;

            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.sizeDelta = Vector2.zero;

            // 셀 이미지 컨테이너 (비정형 스킬용)
            GameObject containerObj = new GameObject("CellImagesContainer");
            containerObj.transform.SetParent(itemObj.transform);
            containerObj.transform.localPosition = Vector3.zero;

            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.sizeDelta = Vector2.zero;

            // SkillDraggableItem 컴포넌트 추가
            SkillDraggableItem item = itemObj.AddComponent<SkillDraggableItem>();

            // SerializedObject로 필드 설정
            SerializedObject serializedItem = new SerializedObject(item);
            serializedItem.FindProperty("iconImage").objectReferenceValue = iconImage;
            serializedItem.FindProperty("background").objectReferenceValue = bgImage;
            serializedItem.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            serializedItem.FindProperty("cellImagesContainer").objectReferenceValue = containerObj.transform;
            serializedItem.FindProperty("allowRotation").boolValue = true;
            serializedItem.ApplyModifiedProperties();

            // 프리팹 저장
            SavePrefab(itemObj, path);
        }

        [MenuItem("Tools/Skill Grid/Create Inventory Slot Prefab", false, 3)]
        public static void CreateInventorySlotPrefab()
        {
            EnsureDirectoryExists();

            string path = $"{PREFAB_PATH}/InventorySlot.prefab";

            // 기존 프리팹이 있으면 삭제
            DeleteExistingPrefab(path);

            // 게임오브젝트 생성
            GameObject slotObj = new GameObject("InventorySlot");

            // RectTransform 설정
            RectTransform rectTransform = slotObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(160f, 60f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // 배경 이미지 + 버튼
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(slotObj.transform);
            bgObj.transform.localPosition = Vector3.zero;

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            bgImage.raycastTarget = true;

            Button button = bgObj.AddComponent<Button>();

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // 아이콘 이미지
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slotObj.transform);
            iconObj.transform.localPosition = new Vector3(-55f, 0f, 0f);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;
            iconImage.raycastTarget = false;

            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(45f, 45f);

            // 이름 텍스트
            GameObject nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(slotObj.transform);
            nameObj.transform.localPosition = new Vector3(15f, 10f, 0f);

            TMPro.TextMeshProUGUI nameText = nameObj.AddComponent<TMPro.TextMeshProUGUI>();
            nameText.text = "Skill Name";
            nameText.fontSize = 14f;
            nameText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            nameText.color = Color.white;

            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.sizeDelta = new Vector2(100f, 20f);

            // 크기 텍스트
            GameObject sizeObj = new GameObject("SizeText");
            sizeObj.transform.SetParent(slotObj.transform);
            sizeObj.transform.localPosition = new Vector3(15f, -12f, 0f);

            TMPro.TextMeshProUGUI sizeText = sizeObj.AddComponent<TMPro.TextMeshProUGUI>();
            sizeText.text = "1x1";
            sizeText.fontSize = 11f;
            sizeText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            sizeText.color = new Color(0.7f, 0.7f, 0.7f, 1f);

            RectTransform sizeRect = sizeObj.GetComponent<RectTransform>();
            sizeRect.sizeDelta = new Vector2(80f, 16f);

            // 모양 표시기
            GameObject shapeObj = new GameObject("ShapeIndicator");
            shapeObj.transform.SetParent(slotObj.transform);
            shapeObj.transform.localPosition = new Vector3(60f, -10f, 0f);

            Image shapeImage = shapeObj.AddComponent<Image>();
            shapeImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            shapeImage.raycastTarget = false;

            RectTransform shapeRect = shapeObj.GetComponent<RectTransform>();
            shapeRect.sizeDelta = new Vector2(30f, 30f);

            // InventorySlotUI 컴포넌트 추가
            InventorySlotUI slot = slotObj.AddComponent<InventorySlotUI>();

            // SerializedObject로 필드 설정
            SerializedObject serializedSlot = new SerializedObject(slot);
            serializedSlot.FindProperty("iconImage").objectReferenceValue = iconImage;
            serializedSlot.FindProperty("background").objectReferenceValue = bgImage;
            serializedSlot.FindProperty("shapeIndicator").objectReferenceValue = shapeImage;
            serializedSlot.FindProperty("nameText").objectReferenceValue = nameText;
            serializedSlot.FindProperty("sizeText").objectReferenceValue = sizeText;
            serializedSlot.FindProperty("slotButton").objectReferenceValue = button;
            serializedSlot.FindProperty("activeColor").colorValue = new Color(0.3f, 0.6f, 0.9f, 1f);
            serializedSlot.FindProperty("passiveColor").colorValue = new Color(0.6f, 0.4f, 0.8f, 1f);
            serializedSlot.FindProperty("selectedColor").colorValue = new Color(0.9f, 0.7f, 0.2f, 1f);
            serializedSlot.ApplyModifiedProperties();

            // 프리팹 저장
            SavePrefab(slotObj, path);
        }

        [MenuItem("Tools/Skill Grid/Create Grid System Prefab", false, 4)]
        public static void CreateGridSystemPrefab()
        {
            EnsureDirectoryExists();

            string path = $"{PREFAB_PATH}/SkillGridSystem.prefab";
            string cellPrefabPath = $"{PREFAB_PATH}/SkillGridCell.prefab";
            string itemPrefabPath = $"{PREFAB_PATH}/SkillDraggableItem.prefab";
            string slotPrefabPath = $"{PREFAB_PATH}/InventorySlot.prefab";

            // 의존 프리팹 확인
            GameObject cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(cellPrefabPath);
            GameObject itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(itemPrefabPath);
            GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(slotPrefabPath);

            if (cellPrefab == null || itemPrefab == null || slotPrefab == null)
            {
                Debug.LogError("[SkillGridPrefabCreator] 먼저 개별 프리팹들을 생성하세요. (Create All Prefabs)");
                return;
            }

            // 기존 프리팹이 있으면 삭제
            DeleteExistingPrefab(path);

            // 루트 오브젝트 생성
            GameObject rootObj = new GameObject("SkillGridSystem");

            RectTransform rootRect = rootObj.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;

            // 그리드 컨테이너
            GameObject gridContainer = new GameObject("GridContainer");
            gridContainer.transform.SetParent(rootObj.transform);
            gridContainer.transform.localPosition = Vector3.zero;

            RectTransform gridContainerRect = gridContainer.AddComponent<RectTransform>();
            gridContainerRect.anchorMin = new Vector2(0f, 0f);
            gridContainerRect.anchorMax = new Vector2(0.6f, 1f);
            gridContainerRect.offsetMin = new Vector2(10f, 10f);
            gridContainerRect.offsetMax = new Vector2(-10f, -10f);

            Image gridBg = gridContainer.AddComponent<Image>();
            gridBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // 그리드 영역 (셀들이 배치될 곳)
            GameObject gridArea = new GameObject("GridArea");
            gridArea.transform.SetParent(gridContainer.transform);
            gridArea.transform.localPosition = Vector3.zero;

            RectTransform gridAreaRect = gridArea.AddComponent<RectTransform>();
            gridAreaRect.anchorMin = Vector2.zero;
            gridAreaRect.anchorMax = Vector2.one;
            gridAreaRect.sizeDelta = new Vector2(-20f, -20f);

            SkillGridManager gridManager = gridArea.AddComponent<SkillGridManager>();

            // SerializedObject로 그리드 매니저 설정
            // cellPrefab은 GameObject 타입이므로 직접 할당
            SerializedObject serializedManager = new SerializedObject(gridManager);
            serializedManager.FindProperty("cellPrefab").objectReferenceValue = cellPrefab;
            serializedManager.FindProperty("cellContainer").objectReferenceValue = gridArea.transform;
            serializedManager.FindProperty("gridWidth").intValue = 6;
            serializedManager.FindProperty("gridHeight").intValue = 6;
            serializedManager.FindProperty("cellSize").floatValue = 80f;
            serializedManager.ApplyModifiedProperties();

            // 인벤토리 컨테이너
            GameObject inventoryContainer = new GameObject("InventoryContainer");
            inventoryContainer.transform.SetParent(rootObj.transform);
            inventoryContainer.transform.localPosition = Vector3.zero;

            RectTransform invContainerRect = inventoryContainer.AddComponent<RectTransform>();
            invContainerRect.anchorMin = new Vector2(0.6f, 0f);
            invContainerRect.anchorMax = new Vector2(1f, 1f);
            invContainerRect.offsetMin = new Vector2(10f, 10f);
            invContainerRect.offsetMax = new Vector2(-10f, -10f);

            Image invBg = inventoryContainer.AddComponent<Image>();
            invBg.color = new Color(0.1f, 0.1f, 0.12f, 0.9f);

            // 인벤토리 스크롤 뷰
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(inventoryContainer.transform);
            scrollView.transform.localPosition = Vector3.zero;

            RectTransform scrollRectTransform = scrollView.AddComponent<RectTransform>();
            scrollRectTransform.anchorMin = Vector2.zero;
            scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.sizeDelta = new Vector2(-20f, -60f);

            Image scrollBg = scrollView.AddComponent<Image>();
            scrollBg.color = new Color(0.05f, 0.05f, 0.08f, 1f);

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();

            // 뷰포트 생성 (Mask 컴포넌트 포함)
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform);
            viewport.transform.localPosition = Vector3.zero;

            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;

            Image viewportMask = viewport.AddComponent<Image>();
            viewportMask.color = new Color(1f, 1f, 1f, 0.01f); // 거의 투명
            viewportMask.raycastTarget = true;

            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // 스크롤 콘텐츠
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform);
            content.transform.localPosition = Vector3.zero;

            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0f, 1f);
            contentRect.sizeDelta = new Vector2(0f, 0f);

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = 5f;
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ScrollRect 설정
            scroll.content = contentRect;
            scroll.viewport = viewportRect;
            scroll.horizontal = false;
            scroll.vertical = true;

            // 인벤토리 헤더
            GameObject header = new GameObject("Header");
            header.transform.SetParent(inventoryContainer.transform);
            header.transform.localPosition = new Vector3(0f, -10f, 0f);

            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(-20f, 40f);

            TMPro.TextMeshProUGUI headerText = header.AddComponent<TMPro.TextMeshProUGUI>();
            headerText.text = "스킬 인벤토리";
            headerText.fontSize = 18f;
            headerText.alignment = TMPro.TextAlignmentOptions.Center;
            headerText.color = Color.white;

            // 컨트롤러 설정
            SkillGridController controller = rootObj.AddComponent<SkillGridController>();

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("gridManager").objectReferenceValue = gridManager;
            serializedController.FindProperty("inventoryContainer").objectReferenceValue = content.transform;
            serializedController.FindProperty("draggableItemPrefab").objectReferenceValue = itemPrefab;
            serializedController.ApplyModifiedProperties();

            // 인벤토리 UI 설정
            SkillInventoryUI inventoryUI = inventoryContainer.AddComponent<SkillInventoryUI>();

            SerializedObject serializedInvUI = new SerializedObject(inventoryUI);
            serializedInvUI.FindProperty("gridController").objectReferenceValue = controller;
            serializedInvUI.FindProperty("slotContainer").objectReferenceValue = content.transform;
            serializedInvUI.FindProperty("slotPrefab").objectReferenceValue = slotPrefab;
            serializedInvUI.ApplyModifiedProperties();

            // 초기화기 설정
            SkillGridInitializer initializer = rootObj.AddComponent<SkillGridInitializer>();

            SerializedObject serializedInit = new SerializedObject(initializer);
            serializedInit.FindProperty("gridController").objectReferenceValue = controller;
            serializedInit.FindProperty("inventoryUI").objectReferenceValue = inventoryUI;
            serializedInit.ApplyModifiedProperties();

            // 프리팹 저장
            SavePrefab(rootObj, path);
        }

        #endregion

        #region 유틸리티

        private static void EnsureDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/2. Scripts"))
            {
                AssetDatabase.CreateFolder("Assets", "2. Scripts");
            }
            if (!AssetDatabase.IsValidFolder("Assets/2. Scripts/System"))
            {
                AssetDatabase.CreateFolder("Assets/2. Scripts", "System");
            }
            if (!AssetDatabase.IsValidFolder("Assets/2. Scripts/System/Skill"))
            {
                AssetDatabase.CreateFolder("Assets/2. Scripts/System", "Skill");
            }
            if (!AssetDatabase.IsValidFolder("Assets/2. Scripts/System/Skill/UI"))
            {
                AssetDatabase.CreateFolder("Assets/2. Scripts/System/Skill", "UI");
            }
            if (!AssetDatabase.IsValidFolder("Assets/2. Scripts/System/Skill/UI/Grid"))
            {
                AssetDatabase.CreateFolder("Assets/2. Scripts/System/Skill/UI", "Grid");
            }
            if (!AssetDatabase.IsValidFolder(PREFAB_PATH))
            {
                AssetDatabase.CreateFolder("Assets/2. Scripts/System/Skill/UI/Grid", "Prefabs");
            }
        }

        private static void DeleteExistingPrefab(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

        private static void SavePrefab(GameObject obj, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);

            if (prefab == null)
            {
                Debug.LogError($"[SkillGridPrefabCreator] 프리팹 저장 실패: {path}");
                Object.DestroyImmediate(obj);
                return;
            }

            Object.DestroyImmediate(obj);

            Debug.Log($"[SkillGridPrefabCreator] 프리팹 생성 완료: {path}");
        }

        #endregion
    }
}
#endif
