using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameSocket
{
    class Node<T> : IComparable
    {
        private T value;
        public Node<T> rightLeaf;
        public Node<T> leftLeaf;

        public T Value { get; }
        public Node(T value)
        {
            this.value = value;
            rightLeaf = null;
            leftLeaf = null;
        }

        public bool isLeaf(ref Node<T> node)
        {
            return (node.rightLeaf == null && node.leftLeaf == null);

        }

        public void insertData(ref Node<T> node, T data)
        {
            if (node == null)
            {
                node = new Node<T>(data);

            }
            else if (node.CompareTo(data) == -1)
            {
                insertData(ref node.rightLeaf, data);
            }

            else if (node.CompareTo(data) == 1)
            {
                insertData(ref node.leftLeaf, data);
            }
        }

        public bool search(Node<T> node, T s)
        {
            if (node == null)
                return false;

            if (node.CompareTo(s) == 0)
            {
                return true;
            }
            else if (node.CompareTo(s) == -1)
            {
                return search(node.rightLeaf, s);
            }
            else if (node.CompareTo(s) == 1)
            {
                return search(node.leftLeaf, s);
            }

            return false;
        }

        public void display(Node<T> node)
        {
            if (node == null)
                return;

            display(node.leftLeaf);
            Console.Write(" " + node.Value);
            display(node.rightLeaf);
        }

        public int CompareTo(T value)
        {
            if (Value.Equals(value))
                return 0;
            if (Comparer.Default.Compare(Value, value) > 0)
                return 1;
            if (Comparer.Default.Compare(Value, value) < 0)
                return -1;
            return 2;
        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
    class BinaryTree<T>
    {
        private Node<T> root;
        private int count;

        public BinaryTree()
        {
            root = null;
            count = 0;
        }
        public bool isEmpty()
        {
            return root == null;
        }

        public void insert(T data)
        {
            if (isEmpty())
            {
                root = new Node<T>(data);
            }
            else
            {
                root.insertData(ref root, data);
            }

            count++;
        }

        public bool search(T search)
        {
            return root.search(root, search);
        }

        public bool isLeaf()
        {
            if (!isEmpty())
                return root.isLeaf(ref root);

            return true;
        }

        public void display()
        {
            if (!isEmpty())
                root.display(root);
        }

        public int Count()
        {
            return count;
        }
    }
}
