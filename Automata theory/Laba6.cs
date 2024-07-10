using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace Automata_theory
{
    partial class Laba6
    {
        public static Partitions Partitions { get; set; }

        public static int GetPartitionsCount(int statesAmount)
        {
            if (statesAmount <= 4)
                return 2;
            if (statesAmount <= 8)
                return 3;
            else if (statesAmount <= 16)
                return 4;
            throw new ArgumentOutOfRangeException();
        }
    }

    public class Partitions
    {
        public Partitions(AutomataTableBase table, int partitions, bool doPi, List<List<List<int>>> P = null) 
        {
            this.partitions = partitions;
            do
            {
                MakePiPartitions(table, doPi, P);
            }
            while (GetIntersectionOfAllSets(pi).Count < table.States.Length);
            MakeTables(table);
            MakeTauPartitions(table);
            MakeEtaPartitions(table);
            CalculateDependancies();
            CalculateTranslations(table);
            CalculateFunctionG(table);
        }

        private void MakePiPartitions(AutomataTableBase table, bool doPi, List<List<List<int>>> P)
        {
            if (doPi)
            {
                this.pi = P;
                return;
            }
            this.pi = GenerateLists(partitions, table.States.Length);
        }

        static List<List<List<int>>> GenerateLists(int a, int b)
        {
            List<List<List<int>>> result = new List<List<List<int>>>();
            Random rand = new Random();

            for (int i = 0; i < a; i++)
            {
                List<List<int>> list1 = new List<List<int>>();
                List<int> numbers = new List<int>();

                for (int j = 0; j < b; j++)
                {
                    numbers.Add(j);
                }

                Shuffle(numbers, rand);

                int midPoint = numbers.Count / 2;
                List<int> subset1 = numbers.GetRange(0, midPoint);
                List<int> subset2 = numbers.GetRange(midPoint, b - midPoint);

                list1.Add(subset1);
                list1.Add(subset2);

                result.Add(list1);
            }

            return result;
        }

        static void Shuffle(List<int> list, Random rand)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                int value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private List<List<int>> GetIntersectionOfAllSets(List<List<List<int>>> listOfSets)
        {
            if (listOfSets == null || listOfSets.Count == 0)
            {
                // Handle invalid input, return an empty list or throw an exception as needed
                return new List<List<int>>();
            }

            // Initialize the result with the first set
            List<List<int>> result = listOfSets[0];

            // Iterate through the rest of the sets and find the intersection
            for (int i = 1; i < listOfSets.Count; i++)
            {
                result = IntersectSets(result, listOfSets[i]);
            }

            return result;
        }

        private List<List<int>> IntersectSets(List<List<int>> set1, List<List<int>> set2)
        {
            var intersection = new List<List<int>>();

            foreach (var subset1 in set1)
            {
                foreach (var subset2 in set2)
                {
                    var commonElements = subset1.Intersect(subset2).ToList();
                    if (commonElements.Count > 0)
                    {
                        intersection.Add(commonElements);
                    }
                }
            }

            return intersection;
        }

        private void MakeTables(AutomataTableBase table)
        {
            for (int i = 0; i < partitions; i++) // for each partition
            {
                AutomataTableBase newTable = new AutomataTableBase(table.States.Length, table.Inputs);
                for (int j = 0; j < table.States.Length; j++) // for each state
                {
                    newTable.States[j] = new AutomataStateBase(table.Inputs, j);
                    for (int k = 0; k < table.Inputs; k++) // for each input
                    {
                        newTable.States[j].Cells[k] = new AutomataCellBase(ReverseState(pi[i], table.States[j].Cells[k].State), -1);
                    }
                }
                tables.Add(newTable);
            }
        }

        private int ReverseState(List<List<int>> sets, int state, bool invert = false)
        {
            for (int i = 0; i < sets.Count; i++)
            {
                for (int j = 0; j < sets[i].Count; j++)
                {
                    if (sets[i][j] == state) return invert ? Math.Abs(i - 1) : i;
                }
            }
            return -1;
        }

        private void MakeTauPartitions(AutomataTableBase table)
        {
            for (int i = 0; i < partitions; i++) // for each partition
            {
                tau.Add(new List<List<int>>());
                List<int> skip = new List<int>();
                for (int j = 0; j < tables[i].States.Length; j++) // for each state
                {
                    if (skip.Contains(j)) continue;
                    List<int> set = new List<int>() { j };
                    List<int> input = new List<int>();

                    for (int k = 0; k < tables[i].Inputs; k++) // for each input
                    {
                        input.Add(tables[i].States[j].Cells[k].State);
                    }

                    for (int k = j + 1; k < tables[i].States.Length; k++) // for each state after current
                    {
                        bool flag = true;
                        for (int u = 0; u < tables[i].Inputs; u++) // for each input
                        {
                            if (tables[i].States[k].Cells[u].State != tables[i].States[j].Cells[u].State)
                            { flag = false; break; }
                        }
                        if (flag) { set.Add(k); skip.Add(k); }
                    }
                    tau[i].Add(set);
                }
            }
        }

        private void MakeEtaPartitions(AutomataTableBase table)
        {
            for (int i = 0; i < partitions; i++) // for each partition
            {
                eta.Add(new List<List<int>>());
                List<int> skip = new List<int>();
                for (int j = 0; j < tables[i].Inputs; j++) // for each state -> input
                {
                    if (skip.Contains(j)) continue;
                    List<int> set = new List<int>() { j };
                    List<int> input = new List<int>();

                    for (int k = 0; k < tables[i].States.Length; k++) // for each input -> state
                    {
                        input.Add(tables[i].States[k].Cells[j].State);
                    }

                    for (int k = j + 1; k < tables[i].Inputs; k++) // for each state after current -> input after
                    {
                        bool flag = true;
                        for (int u = 0; u < tables[i].Inputs; u++) // for each input -> state
                        {
                            if (tables[i].States[u].Cells[k].State != tables[i].States[u].Cells[j].State)
                            { flag = false; break; }
                        }
                        if (flag) { set.Add(k); skip.Add(k); }
                    }
                    eta[i].Add(set);
                }
            }
        }

        private void CalculateDependancies()
        {
            //for (int i = 0; i < partitions; i++)
            //{
            //    for (int j = 0; j < partitions; j++)
            //    {
            //        if (j == i) continue;
            //        List<List<int>> intersections = IntersectSets(pi[i], pi[j]);
            //        if (IsSetOfSetsContained())
            //    }
            //}
            // TODO: Make the algorithm properly
            for (int i = 0; i < partitions; i++)
            {
                dependancies.Add(new List<int>());
                for (int j = 0; j < partitions; j++)
                {
                    if (i == j) continue;
                    dependancies[i].Add(j);
                }
            }
        }

        private void CalculateTranslations(AutomataTableBase table)
        {
            //---
            Laba7.TriggerFunctions = new List<TriggerFunction>();
            //---
            for (int i = 0; i < partitions; i++)
            {
                Laba7.TriggerFunctions.Add(new TriggerFunction());
                relativeTables.Add(new AutomataTableRelative());
                relativeTables[i].StateCount = 2;

                List<int> ints = new List<int>();
                for (int j = 0; j < Math.Pow(2, dependancies[i].Count); j++)
                    ints.Add(j);

                for (int j = 0; j < ints.Count; j++) // для каждой комбинации зависимостей
                {
                    List<int> states = new List<int>(); // состояния пи разбиения (после итерации должно остаться одно)
                    states.AddRange(pi[i][0]); states.AddRange(pi[i][1]);

                    int currentState = ints[j];
                    for (int k = 0; k < dependancies[i].Count; k++)
                    {
                        states = states.Intersect(pi[dependancies[i][k]][currentState & 1]).ToList();
                        currentState >>= 1;
                    } // вычислили все состояния (должен быть длины 1)

                    for (int k = 0; k < eta[i].Count; k++)
                    {
                        List<AutomataCellRelative> cells = new List<AutomataCellRelative>(); // создаем новый ряд для двух состояний
                        for (int u = 0; u < 2; u++) // для каждого (двух) состояний
                        {
                            List<int> newStates = new List<int>(states);
                            newStates = newStates.Intersect(pi[i][u]).ToList();
                            
                            if (newStates.Count == 0)
                                cells.Add(new AutomataCellRelative(-2));
                            else
                            {
                                int st = ReverseState(pi[i], table.States[newStates[0]].Cells[eta[i][k][0]].State);
                                cells.Add(new AutomataCellRelative(st));
                                if (st == 0 && u == 1)
                                    Laba7.TriggerFunctions[i].AddNewRule($"{Laba7.MakeBinary(k, eta[i].Count)}", 
                                        $"{ReturnCorrectBinary(Convert.ToString(ints[j], 2).PadLeft(partitions - 1, '0'), $"{u}", i)}");
                            }
                        }
                        relativeTables[i].Rows.Add(
                            new AutomataRowRelative(GenerateUserFriendlyName(dependancies[i].Count, 98 + i, k, ints[j]),
                            $"{Laba7.MakeBinary(ints[j], partitions - 1, true)} {Laba7.MakeBinary(k, eta[i].Count)}", cells));
                    }
                }
            }
        }

        private string ReturnCorrectBinary(string a, string b, int c)
        {
            string result = ""; bool flag = false;
            for (int i = 0; i < a.Length; i++)
            {
                if (i == c) { result += b; flag = true; }
                result += a[i];
            }
            if (!flag) result += b;
            return result;
        }

        private string GenerateUserFriendlyName(int numOfStates, int skipLetter, int input, int state)
        {
            string result = ""; int nextLetter = 98;
            for (int i = 0; i < numOfStates; i++)
            {
                if (nextLetter == skipLetter) nextLetter++;
                result += $"{(char)(nextLetter)}{Common.SubScripts[Math.Abs((state & 1) + 1)]} * ";
                nextLetter++; state >>= 1;
            }   
            result = result.Substring(0, result.Length - 3);
            result += $", x{input + 1}";
            return result;
        }

        private void CalculateFunctionG(AutomataTableBase table)
        {
            tableG = new AutomataTableG(table);
            Laba7.TriggerFunctions.Add(new TriggerFunction());
            for (int i = 0; i < table.States.Length; i++)
            {
                List<int> ints = new List<int>();
                for (int j = 0; j < pi.Count; j++)
                {
                    if (pi[j][0].Contains(i)) ints.Add(1);
                    else ints.Add(0);
                } // foud where is this state number in each pi partition
                string title = "";
                for (int j = 0; j < ints.Count; j++)
                    title += $"{(char)(98 + j)}{Common.SubScripts[ints[j] == 0 ? 2 : 1]} * ";
                title = title.Substring(0, title.Length - 3);
                tableG.Titles[i] = title;
                tableG.BinTitles[i] = string.Join(" ", ints.Select(x => Math.Abs(x -1)));
                for (int j = 0; j < tableG.Inputs; j++)
                    if (table.States[i].Cells[j].Output == 2)
                        Laba7.TriggerFunctions[partitions].AddNewRule(Laba7.MakeBinary(j, tableG.Inputs), tableG.BinTitles[i]);
            }
        }

        public static List<int> IntersectSets(List<List<int>> list)
        {
            if (list == null || list.Count == 0)
            {
                return new List<int>();
            }

            List<int> commonNumbers = new List<int>(list[0]);

            foreach (List<int> intList in list)
            {
                commonNumbers = commonNumbers.Intersect(intList).ToList();
            }

            return commonNumbers;
        }

        private List<List<int>> dependancies = new List<List<int>>();
        private int partitions;
        private List<List<List<int>>> pi = new List<List<List<int>>>();
        private List<List<List<int>>> tau = new List<List<List<int>>>();
        private List<List<List<int>>> eta = new List<List<List<int>>>();
        private List<AutomataTableBase> tables = new List<AutomataTableBase>();
        private List<AutomataTableRelative> relativeTables = new List<AutomataTableRelative>();
        private AutomataTableG tableG;
            
        public int Count { get { return partitions; } }
        public List<List<List<int>>> Pi { get { return pi; } set { pi = value; } }
        public List<List<List<int>>> Tau { get { return tau; } }
        public List<List<List<int>>> Eta { get { return eta; } }
        public List<AutomataTableBase> Tables { get { return tables; } }
        public List<AutomataTableRelative> RelativeTables { get { return relativeTables; } }
        public AutomataTableG TableG { get { return tableG; } }
    }

    public class AutomataTableG
    {
        public AutomataTableG(AutomataTableBase table)
        {
            _states = new AutomataStateBase[table.States.Length];
            for (int i = 0; i < table.States.Length; i++)
            {
                _states[i] = new AutomataStateBase(table.Inputs, i);
                for (int j = 0; j < table.Inputs; j++)
                {
                    _states[i].Cells[j] = new AutomataCellBase(table.States[i].Cells[j].State, table.States[i].Cells[j].Output);
                }
            }
            Inputs = table.Inputs;
            Outputs = table.Outputs;
            _titles = new string[table.States.Length];
            _binTitles = new string[table.States.Length];
        }

        private AutomataStateBase[] _states;
        private string[] _titles;
        private string[] _binTitles;

        public int Outputs;
        public int Inputs;
        public AutomataStateBase[] States { get { return _states; } }
        public string[] Titles { get { return _titles; } set { _titles = value; } }
        public string[] BinTitles { get { return _binTitles; } set { _binTitles = value; } }
    }

    public class AutomataTableRelative
    {
        public AutomataTableRelative()
        {
            _rows = new List<AutomataRowRelative>();
        }
        public int StateCount;
        private List<AutomataRowRelative> _rows;
        public List<AutomataRowRelative> Rows {  get { return _rows; } set { _rows = value; } }
    }

    public class AutomataRowRelative
    {
        public AutomataRowRelative(string name)
        {
            _name = name;
            _cells = new List<AutomataCellRelative> ();
        }

        public AutomataRowRelative(string name, string binName, List<AutomataCellRelative> cells)
        {
            _name = name;
            _binName = binName;
            _cells = cells;
        }

        private string _name = string.Empty;
        public string Name { get { return _name; } }
        private string _binName = string.Empty;
        public string BinName { get { return _binName; } }
        private List<AutomataCellRelative> _cells;
        public List<AutomataCellRelative> Cells { get { return _cells; } }
    }

    public class AutomataCellRelative
    {
        public AutomataCellRelative(int state)
        {
            _state = state;
        }

        private int _state = -1;

        public int State { get { return _state; } }
    }
}
