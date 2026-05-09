using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public struct TestChildData {
    public string Title;
    public string Note1;
    public string Note2;

    public TestChildData(string t, string n1, string n2) {
        Title = t;
        Note1 = n1;
        Note2 = n2;
    }
}

public class TestPanel : MonoBehaviour
{
    public RecyclingListView theList;
    private List<TestChildData> data = new List<TestChildData>();

    [SerializeField] InputField create_input;   // 
    [SerializeField] InputField delete_input;
    [SerializeField] InputField to_input;

    [SerializeField] InputField update_content_input;
    [SerializeField] InputField update_row_input;

    private void Start()
    {
        theList.ItemCallback = PopulateItem;
        // RetrieveData();
        // This will resize the list and cause callbacks to PopulateItem for
        // items that are needed for the view
    }

    public void createList()
    {
        data.Clear();
        int row = 0;

        // You'd obviously load real data here
        string[] randomTitles = new[] {
            "Hello World You look nice today",
            "This is fine You look nice today",
            "You look nice today You look nice today",
            "Recycling is good You look nice today",
            "Why not",
            "Go outside You look nice today",
            "And do something You look nice today",
            "Less boring instead You look nice today"
        };
        int number = int.Parse(create_input.text);
        for(int i = 0; i < number; ++i)
        {
            data.Add(new TestChildData(randomTitles[Random.Range(0, randomTitles.Length)], $"Row {row++}", Random.Range(0, 256).ToString()));
        }
        theList.RowCount = data.Count;
    }

    /// <summary>
    /// 通知更新
    /// </summary>
    /// <param name="item"></param>
    /// <param name="rowIndex"></param>
    private void PopulateItem(RecyclingListViewItem item, int rowIndex)
    {
        var child = item as TestChildItem;
        child.ChildData = data[rowIndex];
    }

    public void ClearList()
    {
        theList.Clear();
    }

    public void AddRow()
    {
        data.Add(new TestChildData()
        {
            Title = "这是新添加的数据",
            Note1 = data.Count.ToString()
        });
        theList.RowCount = data.Count;
    }

    public void DeleteRow()
    {
        if(!string.IsNullOrEmpty(delete_input.text))
        {
            Debug.Log("需要删除的行数为: " + delete_input.text);
            string rowIndex = delete_input.text;
            data.RemoveAll(item => item.Note1 == "Row " + rowIndex);
            theList.RowCount = data.Count;
            theList.Refresh();
        }
    }

    public void MoveToRow(int type)
    {
        if(string.IsNullOrEmpty(to_input.text))
        {
            Debug.LogError("请输入行号");
            return;
        }
        var rowIndex = int.Parse(to_input.text);
        // theList.ScrollToRow(rowIndex, (RecyclingListViewTest.ScrollPosType) type);
    }

    public void UpdateRow()
    {
        string content = update_content_input.text;
        int row = int.Parse(update_row_input.text);
        data[row] = new TestChildData(content, data[row].Note1, data[row].Note2);
        theList.Refresh(row, 1);    // 需要知道对方的row
    }
}
