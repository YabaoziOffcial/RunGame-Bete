using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestChildItem : RecyclingListViewItem
{
    public VerticalLayoutGroup verticalLayoutGroup;
    public Text leftText;
    public Text rightText1;
    public Text rightText2;

    private TestChildData childData;
    public TestChildData ChildData
    {
        get { return childData; }
        set
        {
            childData = value;
            leftText.text = childData.Title;
            rightText1.text = childData.Note1;
            rightText2.text = childData.Note2;
        }
    }

    [InspectorButton]
    public override float GetPerferenceHeight()
    {
        return leftText.preferredHeight + verticalLayoutGroup.padding.top + verticalLayoutGroup.padding.bottom;
    }
}
