using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Automata_theory.Common;

namespace Automata_theory
{
    public static class Common
    {
        public enum DisplayMode
        {
            OnlyStates,
            OnlyOutput,
            Everything
        }

        public static Dictionary<int, string> SubScripts = new Dictionary<int, string>()
        {
            { 0, "\u2080" },
            { 1, "\u2081" },
            { 2, "\u2082" },
            { 3, "\u2083" },
            { 4, "\u2084" },
            { 5, "\u2085" },
            { 6, "\u2086" },
            { 7, "\u2087" },
            { 8, "\u2088" },
            { 9, "\u2089" },
            { 10, "\u2081\u2080" }
        };

        public static AutomataTableBase CreateTable(string msg = "Выберите совмещенную таблицу:")
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox(msg, Type: 8);
            List<int> outputs = new List<int>();
            AutomataTableBase table = new AutomataTableBase(RG.Columns.Count - 1, RG.Rows.Count - 1);
            for (int i = 2; i < RG.Columns.Count + 1; i++)
            {
                AutomataStateBase state = new AutomataStateBase(RG.Rows.Count - 1, i - 1);
                for (int j = 2; j < RG.Rows.Count + 1; j++)
                {
                    string text = RG.Columns[i].Rows[j].Value2;
                    string toState = text.Split('/')[0].Trim().Trim('S').Trim('b');
                    int ToState = -1; if (int.TryParse(toState, out _)) ToState = Convert.ToInt32(toState) - 1;
                    string output = text.Split('/')[1].Trim().Trim('y');
                    int Output = -1; if (Char.IsNumber(output[0])) Output = Convert.ToInt32(output);
                    if (!outputs.Contains(Output) && Output != -1) outputs.Add(Output);
                    AutomataCellBase cell = new AutomataCellBase(ToState, Output);
                    state.Cells[j - 2] = cell;
                }
                table.States[i - 2] = state;
            }
            table.Outputs = outputs.Count;
            return table;
        }

        public static void DisplayTable(AutomataTableBase table, DisplayMode displayMode = DisplayMode.Everything, string stateLetter = "s", bool displayLetter = true)
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox($"Куда вставить таблицу ({table.States[0].Cells.Length}х{table.States.Length}):", Type: 8);
            for (int i = 0; i < table.States[0].Cells.Length; i++)
                RG.Columns[1].Rows[i + 2].Value2 = "x" + SubScripts[i + 1];
            for (int i = 0; i < table.States.Length; i++)
            {
                RG.Columns[i + 2].Rows[1].Value2 = (displayLetter ? stateLetter : "") + (displayLetter ? SubScripts[i + 1].ToString() : (i + 1).ToString());
                for (int j = 0; j < table.States[i].Cells.Length; j++)
                    switch (displayMode)
                    {
                        case DisplayMode.OnlyStates:
                            RG.Columns[i + 2].Rows[j + 2].Value2 = table.States[i].Cells[j].State != -1 ? stateLetter + (table.States[i].Cells[j].State + 1) : "--";
                            break;
                        case DisplayMode.OnlyOutput:
                            RG.Columns[i + 2].Rows[j + 2].Value2 = table.States[i].Cells[j].Output != -1 ? "y" + table.States[i].Cells[j].Output : "--";
                            break;
                        case DisplayMode.Everything:
                            RG.Columns[i + 2].Rows[j + 2].Value2 = (table.States[i].Cells[j].State != -1 ? stateLetter + (table.States[i].Cells[j].State + 1) : "--") + "/"
                                + (table.States[i].Cells[j].Output != -1 ? "y" + table.States[i].Cells[j].Output : "--");
                            break;
                    }
            }
        }

        public static void DisplayTables(List<AutomataTableBase> tables, DisplayMode displayMode = DisplayMode.Everything)
        {
            for (int i = 0; i < tables.Count; i++)
            {
                DisplayTable(tables[i], displayMode, $"{(char)(98 + i)}", false);
            }
        }

        public static void DisplayTable(AutomataTableSP table, DisplayMode displayMode = DisplayMode.Everything, string stateLetter = "с")
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox($"Куда вставить таблицу ({table.States[0].Cells.Length}х{table.States.Length}):", Type: 8);
            for (int i = 0; i < table.States[0].Cells.Length / 2; i++)
                for (int j = 0; j < Laba5_1.CurrentPartition.BBlocks.Count; j++)
                    RG.Columns[1].Rows[2 + j + i * Laba5_1.CurrentPartition.BBlocks.Count].Value2 = $"x{SubScripts[i+1]} x b{SubScripts[j+1]}";

            for (int i = 0; i < table.States.Length; i++)
            {
                RG.Columns[i + 2].Rows[1].Value2 = stateLetter + SubScripts[(i + 1)];
                for (int j = 0; j < table.States[i].Cells.Length; j++)
                    switch (displayMode)
                    {
                        case DisplayMode.OnlyStates:
                            RG.Columns[i + 2].Rows[j + 2].Value2 =
                                table.States[i].Cells[j].State != -1 ? 
                                stateLetter + (table.States[i].Cells[j].State + 1) : "--";
                            break;
                        case DisplayMode.OnlyOutput:
                            RG.Columns[i + 2].Rows[j + 2].Value2 =
                                table.States[i].Cells[j].Output != -1 ? 
                                "y" + table.States[i].Cells[j].Output : "--";
                            break;
                        case DisplayMode.Everything:
                            RG.Columns[i + 2].Rows[j + 2].Value2 = 
                                (table.States[i].Cells[j].State != -1 ? 
                                stateLetter + (table.States[i].Cells[j].State + 1) : "--") + "/"
                                + (table.States[i].Cells[j].Output != -1 ? 
                                "y" + table.States[i].Cells[j].Output : "--");
                            break;
                    }
            }
        }

        public static void DisplayTable(AutomataTableCombined table, DisplayMode displayMode = DisplayMode.Everything, bool displayFirst = true)
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox($"Куда вставить таблицу ({table.States[0].Cells.Length+1}х{table.States.Length+1}):", Type: 8);
            for (int i = 0; i < table.States[0].Cells.Length; i++)
                RG.Columns[1].Rows[i + 2].Value2 = $"x{SubScripts[i + 1]}{(displayFirst ? $"(x1{i + 1})" : "")}";

            for (int i = 0; i < table.States.Length; i++)
            {
                RG.Columns[i + 2].Rows[1].Value2 = $"S{i + 1}(S1{table.States[i].Number[0] + 1}, S2{table.States[i].Number[1] + 1})";
                for (int j = 0; j < table.States[i].Cells.Length; j++)
                    switch (displayMode)
                    {
                        case DisplayMode.OnlyStates:
                            RG.Columns[i + 2].Rows[j + 2].Value2 =  
                                $"S{Laba5.ReverseState(table, table.States[i].Cells[j].State) + 1}(S1{table.States[i].Cells[j].State[0] + 1}, S2{table.States[i].Cells[j].State[1] + 1})";
                            break;
                        case DisplayMode.OnlyOutput:
                            RG.Columns[i + 2].Rows[j + 2].Value2 = 
                                "y" + table.States[i].Cells[j].Output;
                            break;
                        case DisplayMode.Everything:
                            RG.Columns[i + 2].Rows[j + 2].Value2 =
                                $"S{Laba5.ReverseState(table, table.States[i].Cells[j].State) + 1}(S1{table.States[i].Cells[j].State[0] + 1}, S2{table.States[i].Cells[j].State[1] + 1})" +
                                "/y" + table.States[i].Cells[j].Output;
                            break;
                    }
            }
        }

        public static void DisplayTable(OutputTranslation table, string upperRow = "y1")
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox($"Куда вставить таблицу переводов ({table.Translation.GetLength(0) + 1}х{table.Translation.GetLength(1) + 1}):", Type: 8);
            for (int i = 0; i < table.Translation.GetLength(1); i++)
                RG.Columns[1].Rows[i + 2].Value2 = $"y2{i + 1}";

            for (int i = 0; i < table.Translation.GetLength(0); i++)
            {
                RG.Columns[i + 2].Rows[1].Value2 = $"{upperRow}{i + 1}";
                for (int j = 0; j < table.Translation.GetLength(1); j++)
                {
                    RG.Columns[i + 2].Rows[j + 2].Value2 =
                        upperRow[0].ToString() + table.Translation[i, j];
                }
            }
        }

        public static void DisplayTable(AutomataTableRelative table, int num, bool bin = false, bool delay = false)
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox($"Куда вставить таблицу переходов автомата {(char)(66 + num)} ({table.Rows.Count}х{table.StateCount}):", Type: 8);
            for (int i = 0; i < table.StateCount; i++)
                RG.Columns[2 + i].Rows[1].Value2 = bin ? $"{i}" : $"{(char)(98 + num)}{SubScripts[i + 1]}";

            for (int i = 0; i < table.Rows.Count; i++)
            {
                RG.Columns[1].Rows[2 + i].Value2 = bin ? table.Rows[i].BinName : table.Rows[i].Name;
                for (int j = 0; j < table.Rows[i].Cells.Count; j++)
                {
                    RG.Columns[2 + j].Rows[2 + i].Value2 = table.Rows[i].Cells[j].State == -1 ? "--" : (table.Rows[i].Cells[j].State == -2 ? "*" : 
                        (bin ? $"{Math.Abs(table.Rows[i].Cells[j].State - 1)}" : $"{(char)(98 + num)}{table.Rows[i].Cells[j].State + 1}"));
                }
            }
        }

        public static void DisplayTables(List<AutomataTableRelative> tables)
        {
            for (int i = 0; i < tables.Count; i++)
            {
                DisplayTable(tables[i], i, false);
            }
        }

        public static void DisplayTable(AutomataTableG table, bool bin = false)
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox($"Куда вставить таблицу функции g:", Type: 8);
            for (int i = 0; i < table.States.Length; i++)
            {
                RG.Columns[2 + i].Rows[1].Value2 = $"{table.Titles[i]}";
                RG.Columns[2 + i].Rows[2].Value2 = bin ? $"{table.BinTitles[i]}" : $"{i + 1}";
            }

            for (int i = 0; i < table.Inputs; i++)
            {
                RG.Columns[1].Rows[i + 3].NumberFormat = "";
                RG.Columns[1].Rows[i + 3].Value2 = bin ? $"{Laba7.MakeBinary(i, table.Inputs)}" : $"x{SubScripts[i+1]}";
                for (int j = 0; j < table.States.Length; j++)
                    RG.Columns[j + 2].Rows[i + 3].Value2 = table.States[j].Cells[i].Output != -1 ? 
                        (bin ? $"{table.States[j].Cells[i].Output - 1}" : "y" + table.States[j].Cells[i].Output) : "--";
            }
        }

        private static void DisplayTable(ImplicatorsTable table, Range RG, int order)
        {
            RG.Rows[1].Columns[order * 4 + 2].Value2 = "№";
            if (order == 0)
                RG.Rows[1].Columns[3].Value2 = "Члены СДНФ";
            else RG.Rows[1].Columns[order * 4 + 3].Value2 = "Импликанты";
            for (int i = 0; i < table.Implicants.Count; i++)
            {
                RG.Rows[i + 2].Columns[order * 4 + 1].Value2 = $"{i+1}";
                RG.Rows[i + 2].Columns[order * 4 + 2].NumberFormat = "@";
                RG.Rows[i + 2].Columns[order * 4 + 2].Value2 = string.Join("-", table.Levels[i].Distinct());
                RG.Rows[i + 2].Columns[order * 4 + 3].Value2 = table.ToString(i);
            }
        }

        public static void DisplayTable(List<ImplicatorsTable> table)
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox($"Куда вставить таблицы испликаторов:", Type: 8);
            for (int i = 0; i < table.Count; i++)
            {

                DisplayTable(table[i], RG, i);
            }
        }

        public static void DisplayTable(IntersectionTable table)
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox($"Куда вставить таблицы испликаторов:", Type: 8);
            RG.Rows[1].Columns[1].Value2 = "Простые импликанты";
            RG.Rows[1].Columns[2].Value2 = "Члены СДНФ";
            for (int i = 0; i < table.CDNFAmount; i++)
                RG.Rows[2].Columns[2 + i].Value2 = i + 1;
            for (int i = 0; i < table.Implicants.Count; i++)
            {
                RG.Rows[3 + i].Columns[1].Value2 = ToString(table.Implicants[i]);
                for (int j = 0; j < table.CDNFAmount; j++)
                    RG.Rows[3 + i].Columns[2 + j].Value2 = table.Bools[i][j] ? "X" : "";
            }
        }

        private static string ToString(List<TriggerFunctionItem> items)
        {
            string res = "";
            for (int j = 0; j < items.Count; j++)
            {
                res += $"{items[j].Symbol}{(items[j].Inverse ? "\u0305" : "")}{Common.SubScripts[items[j].Number]}";
            }
            return res;
        }

        public static void DisplayList(List<int> ints, string msg = "Куда вставить список:")
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox(msg, Type: 8);
            for (int i = 0; i < ints.Count; i++)
            {
                RG.Columns[1].Rows[i+1].Value2 = ints[i];
            }
        }

        public static void DisplayList(List<List<int>> ints, string msg = "Куда вставить список:")
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox(msg, Type: 8);
            for (int i = 0; i < ints.Count; i++)
            {
                RG.Columns[1].Rows[i + 1].Value2 = string.Join(", ", ints[i]);
            }
        }

        public static void DisplayList(List<List<List<int>>> ints, char ch, string msg = null)
        {
            if (msg == null) msg = $"Куда вставить {ch} разбиения:";

            Range RG = (Range)Globals.ThisAddIn.Application.InputBox(msg, Type: 8);
            for (int i = 0; i < ints.Count; i++)
            {
                RG.Columns[1].Rows[i + 1].Value2 = ch.ToString() + SubScripts[i + 1].ToString();
                for (int j = 0; j < ints[i].Count; j++)
                {
                    for (int k = 0; k < ints[i][j].Count; k++) ints[i][j][k]++;
                    RG.Columns[j + 2].Rows[i + 1].Value2 = string.Join("", ints[i][j]);
                    for (int k = 0; k < ints[i][j].Count; k++) ints[i][j][k]--;
                }
            }
        }

        public static void DisplayList(List<TriggerFunction> list)
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox("Куда вставить функции возбуждения:", Type: 8);
            for (int i = 0; i < list.Count; i++)
            {
                RG.Columns[1].Rows[i + 1].Value2 = list[i].ToString();
            }
        }

        public static void DisplayList(List<IntersectionTable> list)
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox("Куда вставить таблицы Квайна:", Type: 8);
            for (int i = 0; i < list.Count; i++)
            {
                RG.Columns[1].Rows[i + 1].Value2 = list[i].ToString();
            }
        }

        public static void DisplayList(List<SimpleTriggerFunction> list)
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox("Куда вставить упрощенные функции возбуждения:", Type: 8);
            for (int i = 0; i < list.Count; i++)
            {
                RG.Columns[1].Rows[i + 1].Value2 = list[i].ToString();
            }
        }
    }
}
