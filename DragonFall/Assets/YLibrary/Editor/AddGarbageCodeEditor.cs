
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;


/// <summary>
/// 最后编辑人：富强
/// 最后编辑时间：2024年8月13日19点26分
/// 修改内容：把写死的放越界范围改成随机生成
/// </summary>

public class AddGarbageCodeEditor : EditorWindow
{
	static string BodyCanUse = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890_";
	static string HeadCanUse = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";

	static char GetChar(string pool)
	{
		char[] data = pool.ToCharArray();
		return data[Random.Range(0, data.Length)];
	}

	static char GetHeadChar()
	{
		return GetChar(HeadCanUse);
	}

	static char GetBodyChar()
	{
		return GetChar(BodyCanUse);
	}

	static string GetRandomName(int num)
	{
		List<char> allName = new List<char>();
		allName.Add(GetHeadChar());
		for (int i = 0; i < num; i++)
		{
			allName.Add(GetBodyChar());
		}
		return string.Concat(allName);
	}

	static string GetRandomType()
	{
		List<string> allKey = new List<string>(Type_Default.Keys);
		return allKey[Random.Range(0, allKey.Count)];
	}

	static string GetRandomPublic()
	{
		switch (Random.Range(0, 3))
		{
			case 0:
				return "public static ";
			case 1:
				return "private static ";
			default:
				return "static ";
		}
	}

	static Dictionary<string, string> Type_Default = new Dictionary<string, string> {
		{"bool","false"},
		{"int","0"},
		{"float","0.0f"},
		{"double","0.0"},
		{"string",""},
	};

	static string Type_Fun(string type, string a, string b, string c)
	{
		string fun = "";
		switch (type)
		{
			case "bool":
				int bool_V = Random.Range(0, 5);
				switch (bool_V)
				{
					case 0:
						fun = "\t" + "\t" + a + " = " + b + " && " + c + ";" + "\n";
						break;
					case 1:
						fun = "\t" + "\t" + "if(" + a + ") " + "\n" +
							 "\t" + "\t" + "{" + "\n" +
							 "\t" + "\t" + "    " + b + " = !" + c + ";" + "\n" + "\n" +
							 "\t" + "\t" + "}" + "\n";
						break;
					case 2:
						fun = "\t" + "\t" + "if(" + a + " && " + c + ") " + "\n" +
							  "\t" + "\t" + "{" + "\n" +
							  "\t" + "\t" + "    " + b + " = !" + b + ";" + "\n" +
							  "\t" + "\t" + "}" + "\n";
						break;
					case 3:
						fun = "\t" + "\t" + "if(" + a + " || " + b + ") " + "\n" +
							  "\t" + "\t" + "{" + "\n" +
							  "\t" + "\t" + "    " + b + " = !" + b + ";" + "\n" +
							  "\t" + "\t" + "}" + "\n";
						break;
					case 4:
						fun = "\t" + "\t" + a + " = " + b + " || " + c + ";" + "\n";
						break;
					default:
						fun = "\t" + "\t" + a + " = " + b + " && " + c + ";" + "\n";
						break;
				}
				break;
			case "int":
				int int_V = Random.Range(0, 6);
				switch (int_V)
				{
					case 0:
						fun = "\t" + "\t" + a + " = " + b + " + " + c + ";" + "\n" +
							  "\t" + "\t" + a + " = " + a + " > " + Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n" +
							  "\t" + "\t" + a + " = " + a + " < " + -Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n";
						break;
					case 1:
						fun = "\t" + "\t" + a + " = " + b + " - " + c + ";" + "\n" +
							  "\t" + "\t" + a + " = " + a + " > " + Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n" +
							  "\t" + "\t" + a + " = " + a + " < " + -Random.Range(10000, 1000000) + "? " + Random.Range(0, 10) + " : " + a + " ;" + "\n";
						break;
					case 2:
						fun = "\t" + "\t" + a + " = " + b + " * " + c + ";" + "\n" +
							  "\t" + "\t" + a + " = " + a + " > " + Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n" +
							  "\t" + "\t" + a + " = " + a + " < " + -Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n";
						break;
					case 3:
						fun = "\t" + "\t" + a + " = " + b + " / (" + c + "==0?10:" + c + ");" + "\n" +
							  "\t" + "\t" + a + " = " + a + " > " + Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n" +
							  "\t" + "\t" + a + " = " + a + " < " + -Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n";
						break;
					case 4:
						fun = "\t" + "\t" + a + " = " + Random.Range(1, 10) + ";" + "\n" +
							  "\t" + "\t" + b + " = " + Random.Range(1, 10) + ";" + "\n" +
							  "\t" + "\t" + c + " = " + Random.Range(1, 10) + ";" + "\n";
						break;
					default:
						fun = "\t" + "\t" + b + " = " + a + ";" + "\n" +
							  "\t" + "\t" + c + " = " + a + ";" + "\n";
						break;
				}
				break;
			case "float":
				int float_V = Random.Range(0, 6);
				switch (float_V)
				{
					case 0:
						fun = "\t" + "\t" + a + " = " + b + " + " + c + ";" + "\n" +
							  "\t" + "\t" + a + " = " + a + " > " + Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n" +
							  "\t" + "\t" + a + " = " + a + " < " + -Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n";
						break;
					case 1:
						fun = "\t" + "\t" + a + " = " + b + " - " + c + ";" + "\n" +
							  "\t" + "\t" + a + " = " + a + " > " + Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n" +
							  "\t" + "\t" + a + " = " + a + " < " + -Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n";
						break;
					case 2:
						fun = "\t" + "\t" + a + " = " + b + " * " + c + ";" + "\n" +
							  "\t" + "\t" + a + " = " + a + " > " + Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n" +
							  "\t" + "\t" + a + " = " + a + " < " + -Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n";
						break;
					case 3:
						fun = "\t" + "\t" + a + " = " + b + " / (" + c + "==0?10:" + c + ");" + "\n" +
							  "\t" + "\t" + a + " = " + a + " > " + Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n" +
							  "\t" + "\t" + a + " = " + a + " < " + -Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n";
						break;
					case 4:
						fun = "\t" + "\t" + a + " = " + Random.Range(1, 10) + ".0f;" + "\n" +
							  "\t" + "\t" + b + " = " + Random.Range(1, 10) + ".0f;" + "\n" +
							  "\t" + "\t" + c + " = " + Random.Range(1, 10) + ".0f;" + "\n";
						break;
					default:
						fun = "\t" + "\t" + b + " = " + a + ";" + "\n" +
							  "\t" + "\t" + c + " = " + a + ";" + "\n";
						break;
				}
				break;
			case "double":
				int double_V = Random.Range(0, 6);
				switch (double_V)
				{
					case 0:
						fun = "\t" + "\t" + a + " = " + b + " + " + c + ";" + "\n" +
							  "\t" + "\t" + a + " = " + a + " > " + Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n" +
							  "\t" + "\t" + a + " = " + a + " < " + -Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n";
						break;
					case 1:
						fun = "\t" + "\t" + a + " = " + b + " - " + c + ";" + "\n" +
							  "\t" + "\t" + a + " = " + a + " > " + Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n" +
							  "\t" + "\t" + a + " = " + a + " < " + -Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n";
						break;
					case 2:
						fun = "\t" + "\t" + a + " = " + b + " * " + c + ";" + "\n" +
							  "\t" + "\t" + a + " = " + a + " > " + Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n" +
							  "\t" + "\t" + a + " = " + a + " < " + -Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n";
						break;
					case 3:
						fun = "\t" + "\t" + a + " = " + b + " / (" + c + "==0?10:" + c + ");" + "\n" +
							  "\t" + "\t" + a + " = " + a + " > " + Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n" +
							  "\t" + "\t" + a + " = " + a + " < " + -Random.Range(10000, 1000000) + " ? " + Random.Range(0, 10) + " : " + a + " ;" + "\n";
						break;
					case 4:
						fun = "\t" + "\t" + a + " = " + Random.Range(1, 10) + ".0;" + "\n" +
							  "\t" + "\t" + b + " = " + Random.Range(1, 10) + ".0;" + "\n" +
							  "\t" + "\t" + c + " = " + Random.Range(1, 10) + ".0;" + "\n";
						break;
					default:
						fun = "\t" + "\t" + b + " = " + a + ";" + "\n" +
							  "\t" + "\t" + c + " = " + a + ";" + "\n";
						break;
				}
				break;
			case "string":
				int string_V = Random.Range(0, 3);
				switch (string_V)
				{
					case 0:
						fun = "\t" + "\t" + a + " = " + b + " + " + c + ";" + "\n" +
							  "\t" + "\t" + a + " = " + a + ".Length > " + Random.Range(300, 400) + " ? " + a + ".Substring(0, " + Random.Range(5, 15) + "): " + a + " ;" + "\n";
						break;
					case 1:
						fun = "\t" + "\t" + a + " = string.Format(" + c + "," + b + ");" + "\n" +
							  "\t" + "\t" + a + " = " + a + ".Length > " + Random.Range(300, 400) + " ? " + a + ".Substring(0, " + Random.Range(5, 15) + "): " + a + " ;" + "\n";

						break;
					default:
						fun = "\t" + "\t" + b + " = " + a + ";" + "\n" +
							  "\t" + "\t" + c + " = " + a + ";" + "\n";
						break;
				}
				break;

		}
		return fun;
	}


	//开头是这些的就不加垃圾代码
	static List<string> allNeedCheck = new List<string> {
		"using",
		"public",
		"{",
		"[",
		"private",
		"public class",
		"class",
		"//",
		"return",
		"protected",
		"Debug",
		"\t",
		" "
	};

	//开头是这些的就不加垃圾代码
	static List<string> allNeedSkipFun = new List<string> {
		"void Awake()",
		"void Start()",
		"void Update()",
		"public void Update()",
		"void FixedUpdate()",
		"public void FixedUpdate()",
		"private void Awake()",
		"private void Start()",
		"private void Update()",
		"private void FixedUpdate()",
		"protected void Awake()",
		"protected void Start()",
		"protected void Update()",
		"protected void FixedUpdate()"
	};


	[MenuItem("混淆工具/插入垃圾代码")]
	static void InsertCommentForAllScripts()
	{
		// 获取选中的文件夹路径
		string selectedFolderPath = AssetDatabase.GetAssetPath(Selection.activeObject);


		int itemNum = 1;
		int funNum = 2;

		if (Directory.Exists(selectedFolderPath))
		{
			// 遍历文件夹下的所有 .cs 文件
			string[] csFiles = Directory.GetFiles(selectedFolderPath, "*.cs", SearchOption.AllDirectories);
			if (csFiles.Length > 0)
			{
				Debug.Log("开始插入共" + csFiles.Length + "个脚本");
				foreach (string csFilePath in csFiles)
				{
					// 读取文件内容

					string dealEnd = "";
					string[] allLine = File.ReadAllLines(csFilePath, Encoding.GetEncoding("utf-8"));

					List<int> allFunStartIndex = new List<int>();
					List<int> allFunEndIndex = new List<int>();

					for (int i = 0; i < allLine.Length; i++)
					{
						foreach (var item2 in allNeedSkipFun)
						{
							if (allLine[i].IndexOf(item2) != -1)
							{
								allFunStartIndex.Add(i);
								Debug.Log("找到了包含" + item2 + "的代码在第" + i + "行");
							}
						}
					}
					foreach (var item in allFunStartIndex)
					{
						int tagIndex = 0;
						bool canJump = false;
						for (int i = item; i < allLine.Length; i++)
						{
							foreach (var item2 in allLine[i])
							{
								if (item2 == '{')
								{
									tagIndex++;
									canJump = true;
								}
								if (item2 == '}')
								{
									tagIndex--;
								}
							}
							if (tagIndex == 0 && canJump)
							{
								allFunEndIndex.Add(i);
								Debug.Log("找到了第" + item + "行的代码结尾在第" + i + "行");
								break;
							}
						}
					}


					List<string> allBool = new List<string>();
					List<string> allInt = new List<string>();
					List<string> allFloat = new List<string>();
					List<string> allDouble = new List<string>();
					List<string> allString = new List<string>();

					for (int i = 0; i < itemNum; i++)
					{
						allBool.Add(GetRandomName(Random.Range(15, 25)));
						allInt.Add(GetRandomName(Random.Range(15, 25)));
						allFloat.Add(GetRandomName(Random.Range(15, 25)));
						allDouble.Add(GetRandomName(Random.Range(15, 25)));
						allString.Add(GetRandomName(Random.Range(15, 25)));
					}

					bool haveNameSpace = false;

					for (int k = 0; k < allLine.Length; k++)
					{
						string line = allLine[k].Trim();
						if (line.StartsWith("namespace"))
						{
							haveNameSpace = true;
							break;
						}
					}


					int addAtIndex = haveNameSpace ? 2 : 1;
					for (int i = allLine.Length - 1; i >= 0; i--)
					{
						string line = allLine[i].Trim();
						if (line.StartsWith("}") && line.EndsWith("}"))
						{
							if (addAtIndex > 0)
							{
								addAtIndex--;
							}
							else
							{
								allLine[i] = allLine[i] + "\n";
								for (int K = 0; K < itemNum; K++)
								{
									allLine[i] += "\t" + "\t" + GetRandomPublic() + "bool " + allBool[K] + " = " + Type_Default["bool"] + ";//SAFELINE" + "\n";
									allLine[i] += "\t" + "\t" + GetRandomPublic() + "int " + allInt[K] + " = " + Random.Range(1, 10) + ";//SAFELINE" + "\n";
									allLine[i] += "\t" + "\t" + GetRandomPublic() + "float " + allFloat[K] + " = " + Random.Range(1, 10) + ";//SAFELINE" + "\n";
									allLine[i] += "\t" + "\t" + GetRandomPublic() + "double " + allDouble[K] + " = " + Random.Range(1, 10) + ";//SAFELINE" + "\n";
									allLine[i] += "\t" + "\t" + GetRandomPublic() + "string " + allString[K] + " = \"" + GetRandomName(Random.Range(15, 25)) + "\";//SAFELINE" + "\n";
								}
								break;
							}
						}
					}


					for (int s = 0; s < allLine.Length; s++)
					{
						string line = allLine[s].Trim();
						if (line.EndsWith(";"))
						{
							bool canAdd = true;
							foreach (var item2 in allNeedCheck)
							{
								if (line.StartsWith(item2))
								{
									canAdd = false;
									break;
								}
							}

							for (int F = 0; F < allFunStartIndex.Count; F++)
							{
								if (s >= allFunStartIndex[F] && s <= allFunEndIndex[F])
								{
									canAdd = false;
									Debug.Log("当前行数在跳过区间里");
								}
							}


							if (canAdd)
							{
								int wantNumL = 0;
								int wantNumR = 0;
								for (int t = s; t >= 0; t--)
								{
									wantNumL += new List<char>(allLine[t].ToCharArray()).FindAll(e => e == '{').Count;
									wantNumR += new List<char>(allLine[t].ToCharArray()).FindAll(e => e == '}').Count;
								}
								if ((wantNumL - wantNumR) == (haveNameSpace ? 2 : 1))
								{
									canAdd = false;
								}
							}
							if (canAdd)
							{
								dealEnd += allLine[s] + "\n";
								string type = GetRandomType();
								switch (type)
								{
									case "bool":
										for (int P = 0; P < funNum; P++)
										{
											dealEnd += Type_Fun(type, allBool[Random.Range(0, allBool.Count)], allBool[Random.Range(0, allBool.Count)], allBool[Random.Range(0, allBool.Count)]);
										}
										break;
									case "int":
										for (int P = 0; P < funNum; P++)
										{
											dealEnd += Type_Fun(type, allInt[Random.Range(0, allInt.Count)], allInt[Random.Range(0, allInt.Count)], allInt[Random.Range(0, allInt.Count)]);
										}
										break;
									case "float":
										for (int P = 0; P < funNum; P++)
										{
											dealEnd += Type_Fun(type, allFloat[Random.Range(0, allFloat.Count)], allFloat[Random.Range(0, allFloat.Count)], allFloat[Random.Range(0, allFloat.Count)]);
										}
										break;
									case "double":
										for (int P = 0; P < funNum; P++)
										{
											dealEnd += Type_Fun(type, allDouble[Random.Range(0, allDouble.Count)], allDouble[Random.Range(0, allDouble.Count)], allDouble[Random.Range(0, allDouble.Count)]);
										}
										break;
									case "string":
										for (int P = 0; P < funNum; P++)
										{
											dealEnd += Type_Fun(type, allString[Random.Range(0, allString.Count)], allString[Random.Range(0, allString.Count)], allString[Random.Range(0, allString.Count)]);
										}
										break;
								}

							}
							else
							{
								dealEnd += allLine[s] + "\n";
							}
						}
						else
						{
							dealEnd += allLine[s] + "\n";
						}
					}

					foreach (var item in allLine)
					{

					}

					dealEnd = dealEnd.Replace("//SAFELINE", "");
					File.WriteAllText(csFilePath, dealEnd, Encoding.GetEncoding("utf-8"));
				}
				Debug.Log("处理完成");
				AssetDatabase.Refresh();
			}
			else
			{
				Debug.LogError("选中的文件夹中没有cs脚本");
			}



		}
		else
		{
			Debug.LogError("选中的对象不是文件夹");
		}
	}



}