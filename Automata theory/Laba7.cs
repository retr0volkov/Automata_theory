using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata_theory
{
    public partial class Laba7
    {
        public static List<TriggerFunction> TriggerFunctions = new List<TriggerFunction>();

        public static string MakeBinary(int num, int maxnum, bool forStates = false)
        {
            if (forStates)
            {
                string result = "";
                for (int i = 0; i < maxnum; i++)
                {
                    result += $"{num & 1} ";
                    num >>= 1;
                }
                return string.Join("", result.Substring(0, result.Length - 1).Reverse());
            }
            string mn = Convert.ToString(maxnum-1, 2);
            int power = (int)Math.Pow(2, mn.Length);
            string n = Convert.ToString(power - num - 1, 2);
            int zeros = mn.Length - n.Length;
            for (int i = 0; i < zeros; i++)
                n = "0" + n;
            return n;
        }
    }

    public class TriggerFunction
    {
        public TriggerFunction()
        {
            _items = new List<List<TriggerFunctionItem>>();
        }

        public void AddNewRule(string row, string col)  
        {
            _items.Add(new List<TriggerFunctionItem>());
            int inc = 1;
            for (int i = 0; i < row.Length; i++)
            {
                if (row[i] == ' ') continue;
                if (row[i] == '1') AddItem(new TriggerFunctionItem('a', inc, false)); //res += $"a{Common.SubScripts[inc]} \u22c5 ";
                else AddItem(new TriggerFunctionItem('a', inc, true));  //$"a\u0305{Common.SubScripts[inc]} \u22c5 "; // \u22c5 - ⋅
                inc++;
            }
            inc = 1;
            for (int i = 0; i < col.Length; i++)
            {
                if (col[i] == ' ') continue;
                if (col[i] == '2') { inc++; continue; }
                if (col[i] == '1') AddItem(new TriggerFunctionItem('t', inc, false)); //res += $"t{Common.SubScripts[inc]} \u22c5 ";
                else AddItem(new TriggerFunctionItem('t', inc, true));//res += $"t\u0305{Common.SubScripts[inc]} \u22c5 "; // \u22c5 - ⋅
                inc++;
            }
        }

        private void AddItem(TriggerFunctionItem item)
        {
            _items[_items.Count - 1].Add(item);
        }

        public string ToString(int layer = 0, int num = 0)
        {
            if (layer == 0)
            {
                string res = "F = ";
                for (int i = 0; i < _items.Count; i++)
                {
                    for (int j = 0; j < _items[i].Count; j++)
                    {
                        res += $"{_items[i][j].Symbol}{(_items[i][j].Inverse ? "\u0305" : "")}{Common.SubScripts[_items[i][j].Number]} \u22c5 ";
                    }
                    res = res.Remove(res.Length - 3, 3);
                    res += " \u22c1 ";
                }
                res = res.Remove(res.Length - 3, 3);
                return res;
            }
            else if (layer == 1)
            {
                string res = "";
                for (int j = 0; j < _items[num].Count; j++)
                {
                    res += $"{_items[num][j].Symbol}{(_items[num][j].Inverse ? "\u0305" : "")}{Common.SubScripts[_items[num][j].Number]}";
                }
                return res;
            }
            else
                throw new ArgumentOutOfRangeException();
        }

        private List<List<TriggerFunctionItem>> _items;
        private List<int> _names;
        public List<List<TriggerFunctionItem>> Items { get { return _items; } }
        public List<int> Names { get { return _names; } } 
    }

    public class TriggerFunctionItem
    {
        public TriggerFunctionItem(char sym, int num, bool inv)
        {
            _symbol = sym;
            _number = num;
            _inverse = inv;
        }

        public TriggerFunctionItem(TriggerFunctionItem item)
        {
            _symbol = item._symbol;
            _number = item._number;
            _inverse = item._inverse;
        }

        public static bool operator == (TriggerFunctionItem item1, TriggerFunctionItem item2)
        {
            if (item1.Inverse == item2.Inverse && item1.Number == item2.Number && item1.Symbol == item2.Symbol)
                return true;
            return false;
        }

        public static bool operator != (TriggerFunctionItem item1, TriggerFunctionItem item2)
        {
            if (item1.Inverse == item2.Inverse && item1.Number == item2.Number && item1.Symbol == item2.Symbol)
                return false;
            return true;
        }

        private char _symbol;
        private int _number;
        private bool _inverse;

        public char Symbol { get { return _symbol; } }
        public int Number { get { return _number; } }
        public bool Inverse { get { return _inverse; } }
    }
}
