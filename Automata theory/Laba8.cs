using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata_theory
{
    public partial class Laba8
    {
        public static List<List<ImplicatorsTable>> ImplicatorsTables;
        public static List<IntersectionTable> IntersectionsTables;
        public static List<SimpleTriggerFunction> SimpleTriggersFunctions;

        public static void SimplifyFunctions()
        {
            ImplicatorsTables = new List<List<ImplicatorsTable>>(Laba7.TriggerFunctions.Count);
            IntersectionsTables = new List<IntersectionTable>();
            SimpleTriggersFunctions = new List<SimpleTriggerFunction>();
            CalculateSimplerFunctions();
        }

        private static void CalculateSimplerFunctions()
        {
            for (int i = 0; i < Laba7.TriggerFunctions.Count; i++) // for each function
            {
                ImplicatorsTables.Add(new List<ImplicatorsTable>());
                ImplicatorsTable currentTable = new ImplicatorsTable();
                for (int j = 0; j < Laba7.TriggerFunctions[i].Items.Count; j++)
                {
                    currentTable.AddEntry(Laba7.TriggerFunctions[i].Items[j], new List<int>() { j + 1 }, new List<int>() { j });
                }
                ImplicatorsTables[i].Add(currentTable);
                while (currentTable.Implicants.Count != 0)
                {
                    currentTable = Iterate(currentTable);
                    ImplicatorsTables[i].Add(currentTable);
                }
                ImplicatorsTables[i].RemoveAt(ImplicatorsTables[i].Count - 1);
                IntersectionTable intersectionTable = new IntersectionTable(ImplicatorsTables[i]);
                IntersectionsTables.Add(intersectionTable);
                SimpleTriggersFunctions.Add(new SimpleTriggerFunction(IntersectionsTables[i], ImplicatorsTables[i]));
            }
        }

        public static ImplicatorsTable Iterate(ImplicatorsTable table)
        {
            ImplicatorsTable result = new ImplicatorsTable();
            for (int i = 0; i < table.Implicants.Count; i++)
            {
                for (int j = i + 1; j < table.Implicants.Count; j++)
                {
                    List<TriggerFunctionItem> list1 = new List<TriggerFunctionItem>();
                    List<TriggerFunctionItem> list2 = new List<TriggerFunctionItem>();
                    list1.AddRange(table.Implicants[i]);
                    list2.AddRange(table.Implicants[j]);
                    int counter = 0, index = -1;
                    for (int k = 0; k < list1.Count; k++)
                    {
                        if (list1[k].Number == list2[k].Number && list1[k].Symbol == list2[k].Symbol && list1[k].Inverse == !list2[k].Inverse)
                        { counter++; index = k; }
                        if (list1[k].Number != list2[k].Number || list1[k].Symbol != list2[k].Symbol)
                        { counter = -1; break; }
                    }
                    if (counter == 1)
                    {
                        List<TriggerFunctionItem> res = new List<TriggerFunctionItem>();
                        res.AddRange(NewList(table.Implicants[i]));
                        res = RemoveItem(res, list1[index]);
                        List<int> ints = new List<int>();
                        ints.AddRange(NewList(table.Level[i]));
                        ints.AddRange(NewList(table.Level[j]));
                        if (!ContainsEqual(result, res, new List<int> { i + 1, j + 1 }, ints))
                            result.AddEntry(res, new List<int> { i+1, j+1 }, ints);

                    }
                    if (result.Implicants.Count > 50) return result;
                }
            }
            return result;
        }

        public static List<TriggerFunctionItem> RemoveItem(List<TriggerFunctionItem> list, TriggerFunctionItem item)
        {
            for (int i = 0; i < list.Count;i++)
            {
                if (list[i] == item)
                {
                    list.RemoveAt(i);
                }
            }
            return list;
        }

        private static List<TriggerFunctionItem> NewList(List<TriggerFunctionItem> list)
        {
            List<TriggerFunctionItem> result = new List<TriggerFunctionItem>();
            for (int i = 0; i < list.Count;i++)
            {
                result.Add(new TriggerFunctionItem(list[i]));
            }
            return result;
        }
        private static List<int> NewList(List<int> list)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < list.Count; i++)
            {

                result.Add(list[i]);
            }
            return result;
        }

        private static bool ContainsEqual(ImplicatorsTable list, List<TriggerFunctionItem> item, List<int> levels, List<int> level)
        {
            for (int i = 0; i < list.Implicants.Count; i++)
            {
                if (Equals(list.Implicants[i], item))
                {
                    list.Level[i].AddRange(level);
                    list.Levels[i].AddRange(levels);
                    return true;
                }
            }
            return false;
        }

        public static bool Equals(List<TriggerFunctionItem> a, List<TriggerFunctionItem> b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
    }

    public class ImplicatorsTable
    {
        public ImplicatorsTable()
        {
            implicants = new List<List<TriggerFunctionItem>>();
            levels = new List<List<int>>();
            level = new List<List<int>>();
        }

        public void AddEntry(List<TriggerFunctionItem> implicant, List<int> levels, List<int> level)
        {
            implicants.Add(implicant);
            this.levels.Add(levels);
            this.level.Add(level);
        }

        public string ToString(int index)
        {
            string res = "";
            for (int j = 0; j < implicants[index].Count; j++)
            {
                res += $"{implicants[index][j].Symbol}{(implicants[index][j].Inverse ? "\u0305" : "")}{Common.SubScripts[implicants[index][j].Number]}";
            }
            return res;
        }

        private List<List<TriggerFunctionItem>> implicants;
        private List<List<int>> levels;
        private List<List<int>> level;  
        public List<List<TriggerFunctionItem>> Implicants { get { return implicants; } }
        public List<List<int>> Levels { get { return levels; } }
        public List<List<int>> Level { get { return level; } }
    }

    public class IntersectionTable
    {
        public IntersectionTable(List<ImplicatorsTable> table)
        {
            _implicants = new List<List<TriggerFunctionItem>>();
            _bools = new List<List<bool>>();
            _CDNF = table[0].Implicants.Count;
            List<int> ignore = new List<int>(), ignoreOld = new List<int>();
            for (int i = table.Count - 1; i > 0; i--)
            {
                ignoreOld = ignore;
                ignore = new List<int>();
                for (int j = 0; j < table[i].Implicants.Count; j++)
                {
                    if (ignoreOld.Contains(j))
                    {
                        List<int> ints = new List<int>();
                        for (int h = 0; h < table[i].Levels[j].Count; h++)
                            ints.Add(table[i].Levels[j][h] - 1);
                        ignore.AddRange(ints);
                        continue;
                    }

                    _implicants.Add(table[i].Implicants[j]);
                    List<bool> bools = new List<bool>();
                    for (int k = 0; k < table[0].Implicants.Count; k++)
                    {
                        if (table[i].Level[j].Contains(k))
                        {
                            bools.Add(true);
                            List<int> ints = new List<int>();
                            for (int h = 0; h < table[i].Levels[j].Count; h++)
                                ints.Add(table[i].Levels[j][h] - 1);
                            ignore.AddRange(ints);
                        }
                        else bools.Add(false);
                    }
                    _bools.Add(bools);
                }
            }
        }

        public int Intersections(int cdnfAddress)
        {
            int result = 0;
            for (int i = 0; i < _bools.Count; i++)
            {
                result += Convert.ToInt32(_bools[i][cdnfAddress]);
            }
            return result;
        }

        public override string ToString()
        {
            return "";
        }

        private int _CDNF;
        public int CDNFAmount { get { return _CDNF; } }
        private List<List<bool>> _bools; // first - implicants, second - CDNF
        public List<List<bool>> Bools { get { return _bools; } }
        private List<List<TriggerFunctionItem>> _implicants;
        public List<List<TriggerFunctionItem>> Implicants { get { return _implicants; } }
    }

    public class SimpleTriggerFunction
    {
        public SimpleTriggerFunction(IntersectionTable table, List<ImplicatorsTable> implicators)
        {
            _items = new List<List<TriggerFunctionItem>> ();
            List<int> done = new List<int> ();
            List<int> items = new List<int> ();
            for (int i = 0; i < table.CDNFAmount; i++) // for every state in intersections table
                if (table.Intersections(i) == 1) // if only one intersection in the state
                    for (int j = 0; j < table.Implicants.Count; j++) // for every implicant in table to check for the one intersecting with the state
                        if (table.Bools[j][i]) // if intersection detected, thats the one
                        {
                            items.Add(j); // add to list of implicants to add rules about
                            done.AddRange(ReturnLevelsOfImplicant(implicators, table.Implicants[j]).Select(x => x - 1)); // add to list of states that are covered by implicants
                        }

            for (int i = 0; i < items.Count; i++) // for every item in list of implicant numbers
                AddNewRule(table.Implicants[items[i]]); // add them to the list of rules

            done = done.Distinct().ToList(); // leave only distinct 
            for (int i = 0; i < table.CDNFAmount; i++) // for every state in intersections table
                if (done.Contains(i)) continue; // if state is covered by implicant, skip
                else AddNewRule(implicators[0].Implicants[i]); // if not, add base rule to the list of rules
        }

        private List<int> ReturnLevelsOfImplicant(List<ImplicatorsTable> implicators, List<TriggerFunctionItem> item)
        {
            for (int i = implicators.Count - 1; i > 0; i--)
            {
                for (int j = 0; j < implicators[i].Levels.Count; j++)
                {
                    if (Laba8.Equals(implicators[i].Implicants[j], item))
                    {
                        return implicators[i].Levels[j];
                    }
                }
            }
            return null;
        }

        private void AddNewRule(List<TriggerFunctionItem> item)
        {
            List<TriggerFunctionItem> n = new List<TriggerFunctionItem>();
            for (int i = 0; i < item.Count; i++)
            {
                n.Add(new TriggerFunctionItem(item[i].Symbol, item[i].Number, item[i].Inverse));
            }
            _items.Add(n);
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
}