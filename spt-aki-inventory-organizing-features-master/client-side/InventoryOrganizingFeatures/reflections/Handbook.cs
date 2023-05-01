using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryOrganizingFeatures.Reflections
{
    internal class Handbook : ReflectionBase
    {
        public Dictionary<string, HandbookNode> NodesTree { get; }

        public Handbook(object instance)
        {
            ReflectedInstance = instance;
            ReflectedType = instance.GetType();
            NodesTree = CreateNodesTree();
        }

        // Could be made public, but the nodes tree shouldn't change anyway, constructor assigned property suffices.
        private Dictionary<string, HandbookNode> CreateNodesTree()
        {
            var result = new Dictionary<string, HandbookNode>();
            foreach (DictionaryEntry node in InvokeMethod<IDictionary>("CreateNodesTree", new object[] {Type.Missing}))
            {
                result.Add((string)node.Key, new HandbookNode(node.Value)); // HandbookNode constructor is recursive.
            }
            return result;
        }

        public HandbookNode FindNode(string findId)
        {
            return RecursiveSearch(NodesTree, findId);
        }

        private HandbookNode RecursiveSearch(Dictionary<string, HandbookNode> tree, string findId)
        {
            foreach (var element in tree)
            {
                if (element.Key == findId) return element.Value;
                var recursionResult = RecursiveSearch(element.Value.ChildrenDict, findId);
                if (recursionResult != null) return recursionResult;
            }
            return null;
        }




    }
}
