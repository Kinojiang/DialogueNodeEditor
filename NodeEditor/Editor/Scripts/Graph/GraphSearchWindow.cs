using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using CustomNodeTree;
using System.Reflection;
using System;
using CustomNodeTree.Core;

namespace CustomNodeTreeEditor
{
    public class GraphSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private NodeTreeViewer nodeTreeViewer;

        private List<Type> nodeTypes = new List<Type>();

        private Texture2D indentationIcon;

        public void Configure(NodeTreeViewer nodeTreeViewer)
        {
            this.nodeTreeViewer = nodeTreeViewer;

            //当前运行状态中的所有程序集
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            DefaultNodeTypeAttribute typeAttribute = nodeTreeViewer.tree.GetType().GetCustomAttribute<DefaultNodeTypeAttribute>();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (typeAttribute != null && (type.IsAssignableFrom(typeAttribute.type) == true || type.IsSubclassOf(typeAttribute.type))
                        && type.IsSubclassOf(typeof(CustomNode)) == true
                        && type.IsAbstract == false&&!typeof(StartingNode).IsAssignableFrom(type))
                    {
                        //这里有一个小tips:asassembly.GetType()得到的Type数组会从基类开始以字母排序，小字母排前面，其子类亦是如此
                        //例如类A{子类ab,子类bc，子类ga}->类B{子类ab，子类cd，子类gf}
                        nodeTypes.Add(type);
                    }
                }
            }

            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            //字典用来存储下面搜索列表的路径分类
            Dictionary<string, int> pahtCategory = new Dictionary<string, int>();
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
            };

            foreach (var type in nodeTypes)
            {
                string display_name = "";
                int display_level = 1;
                bool isExistPath = true;
                string[] display_split;
                string currentPath = "";
                if (type.GetCustomAttribute<NodeCategoryAttribute>() != null)
                {
                    display_name = type.GetCustomAttribute<NodeCategoryAttribute>().Category;
                    display_split = HelpTool.SplitString(display_name, new string[] { "/" });
                    if (display_split.Length > 1)
                    {
                        for (int i = 0; i < display_split.Length - 1; i++)
                        {
                            currentPath += display_split[i] + "/";
                            if (pahtCategory.ContainsKey(currentPath)) continue;
                            else
                            {
                                //字典中不存在当前路径时，在搜索列表中新增一个搜索树组群(新的搜索路径)
                                //并将该路径和它在搜索列表中位置存入字典中
                                isExistPath = false;
                                tree.Add(new SearchTreeGroupEntry(new GUIContent(display_split[i]), i + 1));
                                pahtCategory[currentPath] = tree.Count - 1;
                            }
                        }
                    }
                    else{
                        isExistPath=false;
                    }
                    //display_split数组中最后一个就是将要创建的可搜索条目
                    display_name = display_split[display_split.Length - 1];
                    display_level = display_split.Length;
                }
                else
                {
                    display_name = type.Name;
                    isExistPath = false;
                }

                if (isExistPath)
                {
                    //路径存在时，将新的搜索条目插入已有路径下即可
                    int insertIndex = pahtCategory[currentPath] + 1;
                    tree.Insert(insertIndex, new SearchTreeEntry(new GUIContent(display_name, indentationIcon))
                    {
                        level = display_level,
                        userData = type
                    });

                    //由于新插入了一个搜索条目到搜索树列表中，原来的搜索列表结构发生了变化
                    //这里要把字典存储的路径位置与改变后的搜索列表中的位置同步
                    List<string> reviseKey=new List<string>();

                    foreach (var path in pahtCategory)
                    {
                        if (path.Value >= insertIndex)
                        {
                            reviseKey.Add(path.Key);
                        }
                    }

                    reviseKey.ForEach(r=>pahtCategory[r]=pahtCategory[r]+1);
                }
                else
                {
                    tree.Add(new SearchTreeEntry(new GUIContent(display_name, indentationIcon))
                    {
                        level = display_level,
                        userData = type
                    });
                }
            }

            //tree.Add(new SearchTreeEntry(new GUIContent("Group", indentationIcon))
            //{
            //    level = 1,
            //    userData = new Group()
            //});
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            switch (SearchTreeEntry.userData)
            {
                case Type nodeData:
                    {
                        nodeTreeViewer.CreateNode(nodeData);
                        return true;
                    }
                    //case Group group:
                    //    graphView.CreateGroupBlock(graphMousePosition);
                    //    return true;
            }
            return false;
        }
    }

}
