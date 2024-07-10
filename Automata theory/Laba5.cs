using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace Automata_theory
{
    public partial class Laba5
    {
        public static AutomataTableBase AutomataA { get; set; }
        public static AutomataTableBase AutomataB { get; set; }
        public static AutomataTableCombined SeriesTable { get; set; }
        public static AutomataTableCombined ParallelTable { get; set; }
        public static OutputTranslation TranslationParallel { get; set; }
        public static AutomataTableCombined FeedbackTable { get; set; }
        public static OutputTranslation TranslationFeedback { get; set; }

        public static AutomataTableCombined CombineSeries(AutomataTableBase A, AutomataTableBase B)
        {
            AutomataTableCombined combined = new AutomataTableCombined(A.States.Length * B.States.Length, A.Inputs);

            for (int i = 0; i < A.States.Length; i++) // for each A state
            {
                for (int j = 0; j < B.States.Length; j++) // for each B state
                {
                    combined.Titles.Add(new int[] { i, j });
                    AutomataStateCombined state = new AutomataStateCombined(A.Inputs, new int[] { i, j });
                    for (int k = 0; k < A.Inputs; k++) // for each input
                    {
                        state.Cells[k] = 
                            new AutomataCellCombined(new int[] { A.States[i].Cells[k].State, B.States[j].Cells[A.States[i].Cells[k].Output - 1].State },
                                B.States[j].Cells[A.States[i].Cells[k].Output - 1].Output);
                    }
                    combined.States[i * B.States.Length + j] = state; 
                }
            }

            return combined;
        }

        public static int ReverseState(AutomataTableCombined table, int[] state)
        {
            for (int i = 0; i < table.Titles.Count; i++)
                if (table.Titles[i][0] == state[0] && table.Titles[i][1] == state[1])
                    return i;
            return -1;
        }

        public static AutomataTableCombined CombineParallel(AutomataTableBase A, AutomataTableBase B)
        {
            AutomataTableCombined combined = new AutomataTableCombined(A.States.Length * B.States.Length, A.Inputs);
            TranslationParallel = new OutputTranslation(new int[] { A.Outputs, B.Outputs });

            for (int i = 0; i < A.States.Length; i++) // for each A state
            {
                for (int j = 0; j < B.States.Length; j++) // for each B state
                {
                    combined.Titles.Add(new int[] { i, j });
                    AutomataStateCombined state = new AutomataStateCombined(A.Inputs, new int[] { i, j });

                    for (int k = 0; k < A.Inputs; k++) // for each input
                    {
                        state.Cells[k] =
                            new AutomataCellCombined(new int[] { A.States[i].Cells[k].State, B.States[j].Cells[k].State },
                                TranslationParallel.Translation[A.States[i].Cells[k].Output - 1, B.States[j].Cells[k].Output - 1]);
                    }
                    combined.States[i * B.States.Length + j] = state;
                }
            }
            return combined;
        }

        public static AutomataTableCombined CombineFeedback(AutomataTableBase A, AutomataTableBase B, int inputs, int iterations)
        {
            AutomataTableCombined combined = new AutomataTableCombined(A.States.Length * B.States.Length, inputs);
            TranslationFeedback = new OutputTranslation(new int[] { inputs, B.Outputs }, A.Inputs);

            for (int i = 0; i < A.States.Length; i++) // for each A state
            {
                for (int j = 0; j < B.States.Length; j++) // for each B state
                {
                    combined.Titles.Add(new int[] { i, j });
                    AutomataStateCombined state = new AutomataStateCombined(inputs, new int[] { i, j });
                    for (int k = 0; k < inputs; k++) // for each input
                    {
                        state.Cells[k] = CalculateFeedback(A, B, i, j, k, iterations);
                    }
                    combined.States[i * B.States.Length + j] = state;
                }
            }

            return combined;
        }

        private static AutomataCellCombined CalculateFeedback(AutomataTableBase A, AutomataTableBase B, int stateA, int stateB, int input, int iterations)
        {
            int finalInput = TranslationFeedback.Translation[input, 0] - 1;
            int output = A.States[stateA].Cells[finalInput].Output;
            int newStateA = A.States[stateA].Cells[finalInput].State;
            int newStateB = B.States[stateB].Cells[ A.States[stateA].Cells[finalInput].Output-1 ].State;

            int finalOutput = output - 1, finalStateA = newStateA, finalStateB = newStateB;
            for (int i = 0; i < iterations; i++)
            {
                finalInput = TranslationFeedback.Translation[input, B.States[finalStateB].Cells[ finalOutput ].Output - 1] - 1;
                finalOutput = A.States[finalStateA].Cells[finalInput].Output - 1;
                finalStateB = B.States[finalStateB].Cells[ finalOutput ].State;
                finalStateA = A.States[finalStateA].Cells[finalInput].State;
            }
            //int newFinalInput = TranslationFeedback.Translation[input, B.States[stateB].Cells[ A.States[stateA].Cells[finalInput].Output - 1 ].Output - 1] - 1;
            //int output = A.States[stateA].Cells[finalInput].Output;
            //newStateB = B.States[newStateB].Cells[ A.States[newStateA].Cells[newFinalInput].Output - 1 ].State;
            //newStateA = A.States[newStateA].Cells[newFinalInput].State;
            return new AutomataCellCombined(new int[] { finalStateA, finalStateB }, finalOutput + 1);
        }
    }

    public class OutputTranslation
    {
        public OutputTranslation(int[] inputs, int outputs = -1)
        {
            _translation = new int[inputs[0], inputs[1]];
            for (int i = 0; i < inputs[0]; i++)
            {       
                for (int j = 0; j < inputs[1]; j++)
                {
                    _translation[i, j] = outputs == -1 ? i + j + 1 : outputs - ((i + j + 1) % outputs);
                }
            }
        }

        private int[,] _translation;
        public int[,] Translation { get { return _translation; } }
    }

    public class AutomataTableCombined
    {
        public AutomataTableCombined(int statesAmount, int inputAmount)
        {
            _states = new AutomataStateCombined[statesAmount];
            for (int i = 0; i < statesAmount; i++)
                _states[i] = new AutomataStateCombined(inputAmount, new int[] {i});
            Inputs = inputAmount;
        }

        private AutomataStateCombined[] _states;
        private List<int[]> _titles = new List<int[]>();

        public int Inputs;
        public AutomataStateCombined[] States { get { return _states; } }
        public List<int[]> Titles { get {  return _titles; } }
    }

    public class AutomataStateCombined
    {
        public AutomataStateCombined(int inputs, int[] number)
        {
            _cells = new AutomataCellCombined[inputs];
            Number = number;
        }

        public int[] Number { get; private set; }
        private AutomataCellCombined[] _cells;
        public AutomataCellCombined[] Cells { get { return _cells; } }
    }

    public class AutomataCellCombined
    {
        public AutomataCellCombined(int[] state, int output)
        {
            _state = state;
            _output = output;
        }

        private int[] _state = new int[2];
        private int _output = -1;

        public int[] State { get { return _state; } }
        public int Output { get { return _output; } }
    }
}
