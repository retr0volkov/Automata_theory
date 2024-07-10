using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// this is all good, no need to touch anything here
namespace Automata_theory
{
    public partial class Laba4
    {
        public static AutomataTableBase CurrentTable { get; set; }
        public static AngerPoll CurrentAngerPoll { get; set; }
        public static AutomataTableBase NewTable { get; set; }  

        public static AngerPoll CalculateAngerPoll(AutomataTableBase table)
        {
            AngerPoll angerPoll = new AngerPoll(table);
            return angerPoll;
        }

        public static void DisplayAngerPoll(AutomataTableBase table, AngerPoll angerPoll)
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox($"Куда вставить треугольную таблицу:", Type: 8);
            for (int i = 2; i < table.States.Length + 1; i++)
            {
                RG.Columns[1].Rows[i - 1].Value2 = "s" + Common.SubScripts[i];
                //RG.Columns[1].Rows[i - 1].Value2 = "s" + i;
                for (int j = i; j < table.States.Length + 1; j++)
                {

                    RG.Columns[i].Rows[j - 1].Value2 = 
                        angerPoll.States1[i - 2, j - 1] == -1 ? ("X " + angerPoll.States2[i - 2, j - 1]).TrimEnd() : 
                        angerPoll.States[i - 2, j - 1] == 1 ? "V" : (angerPoll.States[i - 2, j - 1] == -1 ? "X\n" + angerPoll.States2[i - 2, j - 1] : angerPoll.States[i - 2, j - 1].ToString());
                        
                    if (angerPoll.States[i - 2, j - 1] == 1) RG.Columns[i].Rows[j - 1].Interior.Color = Color.FromArgb(198, 239, 206);
                    else if (angerPoll.States[i - 2, j - 1] == -1) RG.Columns[i].Rows[j - 1].Interior.Color = Color.FromArgb(255, 199, 206);
                    else RG.Columns[i].Rows[j - 1].Interior.Color = Color.FromArgb(255, 235, 156);
                }
            }
            for (int i = 1; i < table.States.Length; i++)
                RG.Columns[i + 1].Rows[table.States.Length] = "s" + Common.SubScripts[i];
        }

        public static AutomataTableBase CalculateNew(AngerPoll angerPoll, AutomataTableBase table)
        {
            AutomataTableBase newTable = new AutomataTableBase(angerPoll.Sets.Count, table.States[0].Cells.Length);
            for (int i = 0; i < newTable.States.Length; i++)
            {
                AutomataStateBase automataState = new AutomataStateBase(table.States[0].Cells.Length, i);
                for (int j = 0; j < newTable.States[i].Cells.Length; j++)
                {
                    int forState = 0, forOutput = 0;
                    AutomataCellBase automataCell = 
                        new AutomataCellBase(ReverseState(angerPoll.Sets,
                        table.States[angerPoll.Sets[i][forState]].Cells[j].State),
                        table.States[angerPoll.Sets[i][forOutput]].Cells[j].Output);

                    for (; forState < angerPoll.Sets[i].Count && automataCell.State == -1 && angerPoll.Sets[i].Count > 1; forState++)
                    {
                        automataCell = 
                            new AutomataCellBase(ReverseState(angerPoll.Sets,
                            table.States[angerPoll.Sets[i][forState]].Cells[j].State),
                            table.States[angerPoll.Sets[i][forOutput]].Cells[j].Output);
                    }
                    for (; forOutput < angerPoll.Sets[i].Count && automataCell.Output == -1 && angerPoll.Sets[i].Count > 1; forOutput++)
                    {
                        automataCell = 
                            new AutomataCellBase(ReverseState(angerPoll.Sets,
                            table.States[angerPoll.Sets[i][forState]].Cells[j].State),
                            table.States[angerPoll.Sets[i][forOutput]].Cells[j].Output);
                    }
                    automataState.Cells[j] = automataCell;
                }
                newTable.States[i] = automataState;
            }
            newTable.Outputs = table.Outputs;
            return newTable;
        }

        public static int ReverseState(List<List<int>> states, int inputState)
        {
            if (inputState == -1) return -1;
            for (int i = 0; i < states.Count; i++)
                for (int j = 0; j < states[i].Count; j++)
                    if (states[i][j] == inputState)
                        return i;
            throw new Exception();
        }
    }

    public class AngerPoll
    {
        public AngerPoll(AutomataTableBase table)
        {
            InitTable(table.States.Length);
            CheckConflict(table);
            CheckSimilarity(table);
            CheckConditional(table);
            CalculateSets();
        }

        private void InitTable(int size)
        {
            _states = new int[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    _states[i, j] = -2;
        }

        private void CheckConflict(AutomataTableBase table)
        {
            for (int i = 0; i < _states.GetLength(0); i++)
            {
                for (int j = 0; j < _states.GetLength(1); j++)
                {
                    bool flag = true;
                    for (int k = 0; k < table.States[0].Cells.Length; k++)
                    {
                        if (!flag) break;
                        if (!(table.States[i].Cells[k].Output ==
                            table.States[j].Cells[k].Output ||

                            table.States[i].Cells[k].Output == -1 ||
                            table.States[j].Cells[k].Output == -1))
                            flag = false;
                    }
                    if (!flag) _states[i, j] = -1;
                }
            }
            _1 = _states;
        }

        private void CheckSimilarity(AutomataTableBase table)
        {
            for (int i = 0; i < _states.GetLength(0); i++)
            {
                for (int j = 0; j < _states.GetLength(1); j++)
                {
                    if (_states[i, j] == -1) continue;

                    bool flag = true;
                    for (int k = 0; k < table.States[0].Cells.Length; k++)
                    {
                        if (!flag) break;
                        if (!(table.States[i].Cells[k].State ==
                            table.States[j].Cells[k].State ||

                            table.States[i].Cells[k].State == -1 ||
                            table.States[j].Cells[k].State == -1))
                            flag = false;
                    }
                    _states[i, j] = flag ? 1 : 0;
                }
            }
        }

        private void CheckConditional(AutomataTableBase table)
        {
            _2 = new string[_states.GetLength(0), _states.GetLength(1)];
            bool isDone = false;
            int counter = 0;
            while (!isDone && counter < 100)
            {
                isDone = true;
                for (int i = 0; i < _states.GetLength(0); i++)
                {
                    for (int j = 0; j < _states.GetLength(1); j++)
                    {

                        if (_states[i, j] != 0) continue;

                        bool flag = true; int answer = 0;
                        for (int k = 0; k < table.States[0].Cells.Length; k++)
                        {
                            if (counter == 0 && table.States[i].Cells[k].State + 1 != table.States[j].Cells[k].State + 1)
                            {
                                _2[i, j] += $"{table.States[i].Cells[k].State + 1}, {table.States[j].Cells[k].State + 1}\n";
                            }

                            if (!flag) break;
                            if (table.States[i].Cells[k].State != -1 &&
                                table.States[j].Cells[k].State != -1 &&
                                _states[table.States[i].Cells[k].State,
                                table.States[j].Cells[k].State] == -1)
                                flag = false;
                            if (table.States[i].Cells[k].State != -1 &&
                                table.States[j].Cells[k].State != -1 &&
                                _states[table.States[i].Cells[k].State,
                                table.States[j].Cells[k].State] == 0)
                            {
                                isDone = false;
                                counter++;
                                answer = Convert.ToInt32($"{table.States[i].Cells[k].State + 1}" +
                                    $"{table.States[j].Cells[k].State + 1}");
                            }
                        }

                        _states[i, j] = flag ? (counter < 99 ? 0 : (answer == 0 ? 1 : answer)) : -1;
                    }
                }
            }
        }

        private void CalculateSets()
        {
            List<int[]> skipCell = new List<int[]>();
            for (int i = 0; i < _states.GetLength(0); i++)
            {
                for (int j = _states.GetLength(1) - 1; j > i; j--)
                {
                    if (Contains(skipCell, new int[] { i, j })) continue;
                    if (_states[i, j] == 1)
                    {
                        int vertical = -1, horizontal = -1;
                        bool hasVertical = false, hasHorizontal = false;
                        for (int k = j - 1; k > i; k--)
                            if (_states[i, k] == 1)
                            {
                                hasVertical = true;
                                vertical = i;
                                break;
                                //skipCell.Add(new int[] { i, k });
                            }
                        for (int k = i + 1; k < j; k++)
                            if (_states[k, j] == 1)
                            {
                                hasHorizontal = true;
                                horizontal = k;
                                break;
                                //skipCell.Add(new int[] { k, j });
                            }
                        if (hasVertical && hasHorizontal)
                        {
                            skipCell.Add(new int[] { horizontal, j });
                            skipCell.Add(new int[] { i, horizontal });
                            _sets.Add(new List<int> { vertical, horizontal, j });
                            _setsDisp.Add(new List<int> { vertical + 1, horizontal + 1, j + 1 });
                        }
                        else
                        {
                            _sets.Add(new List<int> { i, j });
                            _setsDisp.Add(new List<int> { i + 1, j + 1 });
                        }
                    }
                }
            }
            for (int i = 0; i < _states.GetLength(0); i++)
            {
                bool skip = false;
                for (int j = 0; j < _sets.Count; j++)
                {
                    for (int k = 0; k < _sets[j].Count; k++)
                        if (_sets[j][k] == i)
                        {
                            skip = true;
                            break;
                        }
                    if (skip) break;
                }

                if (skip) continue;
                else
                {
                    _sets.Add(new List<int> { i });
                    _setsDisp.Add(new List<int> { i + 1 });
                }
            }
        }

        private bool Contains(List<int[]> cells, int[] cell)
        {
            for (int i = 0; i < cells.Count; i++)
                if (cells[i][0] == cell[0] && cells[i][1] == cell[1])
                    return true;
            return false;
        }

        private int[,] _states;
        private int[,] _1;
        private string[,] _2;
        private List<List<int>> _sets = new List<List<int>>();
        private List<List<int>> _setsDisp = new List<List<int>>();

        public int[,] States { get { return _states; } }
        public int[,] States1 { get { return _1; } }
        public string[,] States2 { get { return _2; } }
        public List<List<int>> Sets { get { return _sets; } }
        public List<List<int>> SetsDisp { get { return _setsDisp; } }
    }

    public class AutomataTableBase
    {
        public AutomataTableBase(int statesAmount, int inputAmount)
        {
            _states = new AutomataStateBase[statesAmount];
            for (int i = 0; i < statesAmount; i++)
                _states[i] = new AutomataStateBase(inputAmount, i);
            Inputs = inputAmount;
        }
        private AutomataStateBase[] _states;

        public int Outputs;
        public int Inputs;
        public AutomataStateBase[] States { get { return _states; } }
    }

    public class AutomataStateBase
    {
        public AutomataStateBase(int inputs, int number)
        {
            _cells = new AutomataCellBase[inputs];
            Number = number;
        }

        public int Number { get; private set; }
        private AutomataCellBase[] _cells;
        public AutomataCellBase[] Cells { get { return _cells; } }  
    }

    public class AutomataCellBase
    {
        public AutomataCellBase(int state, int output)
        {
            _state = state;
            _output = output;
        }

        private int _state = -1;
        private int _output = -1;

        public int State { get { return _state; } }
        public int Output { get { return _output; } }
    }
}
