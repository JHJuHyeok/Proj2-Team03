using System.Collections.Generic;
using UnityEngine;

/*
[승문]
SkillBarView
-스킬 메인 메뉴의 "한 줄(Bar)" 뷰
-Bar 하위의 Bundle들을 자동 수집해 리스트로 제공
*/
public class SkillBarView : MonoBehaviour
{
    [SerializeField] private Transform bundleRoot;//Bundle들의 부모(없으면 자기 자신)
    private readonly List<SkillBundleView> bundles = new List<SkillBundleView>(8);

    private void Awake()
    {
        if (bundleRoot == null)
        {
            bundleRoot = transform;
        }

        CollectBundles();
    }

    //번들 자동 수집
    public void CollectBundles()
    {
        bundles.Clear();

        int childCount = bundleRoot.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform c = bundleRoot.GetChild(i);
            if (c == null) continue;

            SkillBundleView b = c.GetComponent<SkillBundleView>();
            if (b == null) continue;

            bundles.Add(b);
        }

        if (bundles.Count <= 0)
        {
            SkillBundleView[] found = bundleRoot.GetComponentsInChildren<SkillBundleView>(true);
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] == null) continue;
                if (bundles.Contains(found[i])) continue;
                bundles.Add(found[i]);
            }
        }
    }

    //번들 목록 반환
    public IReadOnlyList<SkillBundleView> GetBundles()
    {
        return bundles;
    }
}