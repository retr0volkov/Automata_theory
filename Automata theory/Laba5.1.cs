using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Automata_theory
{
    partial class Laba5_1
    {
        public static AutomataTableBase CurrentTable { get; set; }

        public static SPPartition CurrentPartition { get; set; }

        public static AutomataTableBase CurrentSP1 { get; set; }

        public static AutomataTableSP CurrentSP2 { get; set; }
    }

    public class SPPartition
    {
        public SPPartition(AutomataTableBase table)
        {
            if (CalculateBBlocks(table))
            {
                CalculateCBlocks();
                ReadyBlocks();
                CalculateProduct();
                CalculateTable();
                isGood = true;
            }
            else
            {
                MessageBox.Show("Нет СП разбиений", "Лаба 5.1", MessageBoxButtons.OK, MessageBoxIcon.Information);
                isGood = false;
            }
        }

        private bool CalculateBBlocks(AutomataTableBase table)
        {
            for (int i = 0; i < table.States.Length; i++)
                for (int j = i + 1; j < table.States.Length; j++)
                {
                    List<List<int>> blocks = CalculateSuggestion(table, 0, 2);
                    if (blocks.Count > 1)
                    {
                        _bBlocks = blocks;
                        return true;
                    }
                }
            return false;
        }

        private List<List<int>> CalculateSuggestion(AutomataTableBase table, int first, int second)
        {
            List<List<int>> result = new List<List<int>> { new List<int> { first, second } };
            List<List<int>> toSkip = new List<List<int>> { new List<int> { first, second } };
            result = CalculateState(result, table);
            while (!CheckStop(result, table))
            {
                for (int i = 0; i < result.Count; i++)
                {
                    List<int> block = new List<int>(result[i]);
                    if (Contains(toSkip, result[i])) continue;
                    result = CalculateState(result, table, i);
                    toSkip.Add(block);
                    i = 0;
                    if (CheckStop(result, table)) break;
                }
            }

            return result;
        }

        private bool Contains(List<List<int>> list, List<int> item)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Count == item.Count)
                {
                    bool flag = true;
                    for (int j = 0; j < item.Count; j++)
                        if (list[i][j] != item[j])
                        {
                            flag = false;
                            break;
                        }
                    if (flag) return true;
                }
            }
            return false;
        }

        private List<List<int>> CalculateState(List<List<int>> blocks, AutomataTableBase table, int index = 0)
        {
            List<List<int>> result = new List<List<int>>(blocks);
            for (int i = 0; i < table.Inputs; i++)
            {
                List<int> block = new List<int> { };
                for (int j = 0; j < blocks[index].Count; j++)
                    if (!block.Contains(table.States[blocks[index][j]].Cells[i].State))
                        block.Add(table.States[blocks[index][j]].Cells[i].State);
                block.Sort();
                result.Insert(index + 1, block);
            }

            result = CleanBlocks(result);

            return result;
        }

        private List<List<int>> CleanBlocks(List<List<int>> blocks)
        {
            blocks.Sort((a, b) => a[0].CompareTo(b[0]));
            for (int i = 0; i < blocks.Count; i++)
                for (int j = i + 1; j < blocks.Count; j++)
                    for (int k = 0; k < blocks[j].Count; k++)
                        if (blocks[i].Contains(blocks[j][k]))
                        {
                            for (int t = 0; t < blocks[j].Count; t++)
                                if (!blocks[i].Contains(blocks[j][t]))
                                    blocks[i].Add(blocks[j][t]);
                            blocks.RemoveAt(j); i = 0; j = 0; break;
                        }

            for (int i = 0; i < blocks.Count; i++)
                blocks[i].Sort();
            blocks.Sort((a, b) => a[0].CompareTo(b[0]));

            return blocks;
        }

        private bool CheckStop(List<List<int>> blocks, AutomataTableBase table)
        {
            List<int> checks = new List<int>();
            for (int i = 0; i < table.States.Length; i++)
                checks.Add(i);
            for (int i = 0; i < blocks.Count; i++)
                for (int j = 0; j < blocks[i].Count; j++)
                    checks.Remove(blocks[i][j]);

            for (int i = 1; i < blocks.Count; i++)
                if (blocks[i].Count != blocks[0].Count) return false;

            return checks.Count == 0;
        }

        private void ReadyBlocks()
        {
            for (int i = 0; i < _bBlocks.Count; i++)
                _bBlocks[i].Sort();
            _bBlocks.Sort((a, b) => a[0].CompareTo(b[0]));

            _bBlocksDisplay = new List<List<int>>();
            for (int i = 0; i < _bBlocks.Count; i++)
            {
                List<int> block = new List<int>();
                for (int j = 0; j < _bBlocks[i].Count; j++)
                    block.Add(i);
                _bBlocksDisplay.Add(block);
            }

            for (int i = 0; i < _bBlocks.Count; i++)
                for (int j = 0; j < _bBlocks[i].Count; j++)
                    _bBlocksDisplay[i][j] = _bBlocks[i][j] + 1;

            for (int i = 0; i < _cBlocks.Count; i++)
                _cBlocks[i].Sort();
            _cBlocks.Sort((a, b) => a[0].CompareTo(b[0]));

            _cBlocksDisplay = new List<List<int>>();
            for (int i = 0; i < _cBlocks.Count; i++)
            {
                List<int> block = new List<int>();
                for (int j = 0; j < _cBlocks[i].Count; j++)
                    block.Add(i);
                _cBlocksDisplay.Add(block);
            }
            for (int i = 0; i < _cBlocks.Count; i++)
                for (int j = 0; j < _cBlocks[j].Count; j++)
                    _cBlocksDisplay[i][j] = _cBlocks[i][j] + 1;
        }

        private void CalculateCBlocks()
        {
            for (int i = 0; i < _bBlocks[0].Count; i++)
            {
                List<int> block = new List<int>();
                for (int j = 0; j < _bBlocks.Count; j++)
                {
                    block.Add(_bBlocks[j][i]);
                }
                _cBlocks.Add(block);
            }
        }

        private void CalculateProduct()
        {
            _states = new int[_bBlocks.Count, _cBlocks.Count];

            for (int i = 0; i < _bBlocks.Count; i++)
                for (int j = 0; j < _cBlocks.Count; j++)
                    _states[i, j] = GetMutual(_bBlocks[i], _cBlocks[j]);
        }

        private int GetMutual(List<int> first, List<int> second)
        {
            for (int i = 0; i < first.Count; i++)
                for (int j = 0; j < second.Count; j++)
                    if (first[i] == second[j]) return first[i];
            throw new Exception();
        }

        private void CalculateTable()
        {
            Laba5_1.CurrentSP1 = new AutomataTableBase(_bBlocks.Count, Laba5_1.CurrentTable.Inputs);
            for (int i = 0; i < _bBlocks.Count; i++)
            {
                for (int j = 0; j < Laba5_1.CurrentSP1.Inputs; j++)
                {
                    Laba5_1.CurrentSP1.States[i].Cells[j] =
                        new AutomataCellBase(ReverseState(_bBlocks, Laba5_1.CurrentTable.States[_bBlocks[i][0]].Cells[j].State), 0);
                }
            }

            Laba5_1.CurrentSP2 = new AutomataTableSP(_cBlocks.Count, Laba5_1.CurrentTable.Inputs * 2);
            for (int i = 0; i < _cBlocks.Count; i++) // for each c state
            {
                for (int j = 0; j < Laba5_1.CurrentTable.Inputs; j++) // for each input
                {
                    for (int k = 0; k < _bBlocks.Count; k++) // for each b state
                    {
                        if (i == 0) Laba5_1.CurrentSP2.Header.Add(new int[] { j+1, k+1 }); // add title if first pass of c state

                        // access (x and b)th input of sp2 and set state and output
                        Laba5_1.CurrentSP2.States[i].Cells[j * 2 + k] =
                            new AutomataCellBase(
                                ReverseState(_cBlocks, Laba5_1.CurrentTable.States[_states[k, i]].Cells[j].State),
                                Laba5_1.CurrentTable.States[_states[k, i]].Cells[j].Output );
                    }
                }
            }
        }

        private int ReverseState(List<List<int>> states, int state)
        {
            for (int i = 0; i < states.Count; i++)
            {
                for (int j = 0; j < states[i].Count; j++)
                {
                    if (states[i][j] == state) return i;
                }
            }
            return -1;
        }

        private List<List<int>> _bBlocks = new List<List<int>>();
        private List<List<int>> _bBlocksDisplay;
        private List<List<int>> _cBlocks = new List<List<int>>();
        private List<List<int>> _cBlocksDisplay;
        private int[,] _states;
        private bool isGood;

        public List<List<int>> BBlocks { get { return _bBlocks; } }
        public List<List<int>> BBlocksDisplay { get { return _bBlocksDisplay; } }
        public List<List<int>> CBlocks { get { return _cBlocks; } }
        public List<List<int>> CBlocksDisplay { get { return _cBlocksDisplay; } }
        public int[,] States { get { return _states; } }
        public bool IsGood { get { return isGood; } }
    }

    public class AutomataTableSP
    {
        public AutomataTableSP(int statesAmount, int inputAmount)
        {
            _states = new AutomataStateBase[statesAmount];
            for (int i = 0; i < statesAmount; i++)
                _states[i] = new AutomataStateBase(inputAmount, i);
            Inputs = inputAmount;
        }
        private AutomataStateBase[] _states;
        private List<int[]> _header = new List<int[]>();

        public int Inputs;
        public List<int[]> Header { get { return _header; } }
        public AutomataStateBase[] States { get { return _states; } }
    }
}
