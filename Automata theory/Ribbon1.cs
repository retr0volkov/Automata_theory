using Microsoft.Office.Tools.Ribbon;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace Automata_theory
{
    public partial class Ribbon1
    {
        public static bool IsPiEnteredExternally = false;
        public static List<List<List<int>>> ExternalPi;
        public static bool IsLaba4Ready = false;

        // stuff opener 

        private void OpenButton_Click(object sender, RibbonControlEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "C:\\Program Files\\Microsoft Office\\root\\Office16\\WINWORD.EXE",
                Arguments = $"Automata\\{dropDown1.SelectedItemIndex}.docx"
            };

            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.Exited += (object ssender, EventArgs ee) => 
            {
                process.Close();
                process.Dispose();
            };
        }

        #region Laba4

        private void createAngerPoll_Click(object sender, RibbonControlEventArgs e)
        {
            Laba4.CurrentTable = Common.CreateTable("Выберите исходную таблицу:");
            Laba4.CurrentAngerPoll = Laba4.CalculateAngerPoll(Laba4.CurrentTable);
            Laba4.NewTable = Laba4.CalculateNew(Laba4.CurrentAngerPoll, Laba4.CurrentTable);
            setsButton.Enabled = true;
            combinedButton.Enabled = true;
            IsLaba4Ready = true;
            enterLab4DataButton.Label = "Разбить";
            enterLab4DataButton.OfficeImageId = "TableAutoFormat";
            enterLab4DataButton.ScreenTip = "Выполнить разбиения автомата";
            enterLab4DataButton.SuperTip = "Выполняет разбиение автомата, полученного в результате минимизации и выводит \u03c0, \u03c4 и \u03b7 разбиения.";
        }

        private void outputAngerPoll_Click(object sender, RibbonControlEventArgs e)
        {
            Laba4.DisplayAngerPoll(Laba4.CurrentTable, Laba4.CurrentAngerPoll);
        }

        private void setsButton_Click(object sender, RibbonControlEventArgs e)
        {
            Range RG = (Range)Globals.ThisAddIn.Application.InputBox("Куда вставить множества:", Type: 8);
            for (int i = 0; i < Laba4.CurrentAngerPoll.Sets.Count; i++)
            {
                RG.Columns[1].Rows[i + 1].Value2 = string.Join(", ", Laba4.CurrentAngerPoll.SetsDisp[i]);
            }
        }

        private void statesButton_Click(object sender, RibbonControlEventArgs e)
        {
            Common.DisplayTable(Laba4.NewTable, Common.DisplayMode.OnlyStates, "b");
        }

        private void outputButton_Click(object sender, RibbonControlEventArgs e)
        {
            Common.DisplayTable(Laba4.NewTable, Common.DisplayMode.OnlyOutput, "b");
        }

        private void combinedButton_Click(object sender, RibbonControlEventArgs e)
        {
            Common.DisplayTable(Laba4.NewTable, Common.DisplayMode.Everything, "b");
        }

        private void group6_DialogLauncherClick(object sender, RibbonControlEventArgs e)
        {
            Laba6Pi form = new Laba6Pi();
            form.ShowDialog();
        }
        #endregion

        #region Laba 5.1
        private void blocksSPButton_Click(object sender, RibbonControlEventArgs e)
        {
            Laba5_1.CurrentTable = Common.CreateTable("Выберите автомат:");
            Laba5_1.CurrentPartition = new SPPartition(Laba5_1.CurrentTable);
            if (Laba5_1.CurrentPartition.IsGood)
            {
                Range RG = (Range)Globals.ThisAddIn.Application.InputBox("Куда вставить произведение автоматов А1 и А2:", Type: 8);
                for (int i = 0; i < Laba5_1.CurrentPartition.States.GetLength(0); i++)
                {
                    for (int j = 0; j < Laba5_1.CurrentPartition.States.GetLength(1); j++)
                    {
                        RG.Columns[1 + j + i * Laba5_1.CurrentPartition.States.GetLength(1)].Rows[1].Value2 = $"b{i + 1} x c{j + 1}";
                        RG.Columns[1 + j + i * Laba5_1.CurrentPartition.States.GetLength(1)].Rows[2].Value2 = $"s{Laba5_1.CurrentPartition.States[i, j] + 1}";
                    }
                }
                A1SetsButton.Enabled = true;
                A2SetsButton.Enabled = true;
                A1StatesButton.Enabled = true;
                A2StatesButton.Enabled = true;
                A2OutputButton.Enabled = true;
            }
        }


        private void A1SetsButton_Click(object sender, RibbonControlEventArgs e)
        {
            Common.DisplayList(Laba5_1.CurrentPartition.BBlocksDisplay);
        }

        private void A1StatesButton_Click(object sender, RibbonControlEventArgs e)
        {
            Common.DisplayTable(Laba5_1.CurrentSP1, Common.DisplayMode.OnlyStates, "b");
        }

        private void A2SetsButton_Click(object sender, RibbonControlEventArgs e)
        {
            Common.DisplayList(Laba5_1.CurrentPartition.CBlocksDisplay);
        }

        private void A2StatesButton_Click(object sender, RibbonControlEventArgs e)
        {
            Common.DisplayTable(Laba5_1.CurrentSP2, Common.DisplayMode.OnlyStates, "c");
        }

        private void A2OutputButton_Click(object sender, RibbonControlEventArgs e)
        {
            Common.DisplayTable(Laba5_1.CurrentSP2, Common.DisplayMode.OnlyOutput, "c");
        }
        #endregion

        #region Laba 5
        int iter = -1, inp = -1;
        private void selectAutomatasButton_Click(object sender, RibbonControlEventArgs e)
        {
            Laba5.AutomataA = Common.CreateTable("Выберите автомат А:");
            Laba5.AutomataB = Common.CreateTable("Выберите автомат В:");
            seriesButton.Enabled = true;
            parallelButton.Enabled = true;
            iterationsBox.Enabled = true;
            inputsBox.Enabled = true;
        }

        private void seriesButton_Click(object sender, RibbonControlEventArgs e)
        {
            Laba5.SeriesTable = Laba5.CombineSeries(Laba5.AutomataA, Laba5.AutomataB);
            Common.DisplayTable(Laba5.SeriesTable, Common.DisplayMode.Everything);
        }

        private void parallelButton_Click(object sender, RibbonControlEventArgs e)
        {
            Laba5.ParallelTable = Laba5.CombineParallel(Laba5.AutomataA, Laba5.AutomataB);
            Common.DisplayTable(Laba5.ParallelTable, Common.DisplayMode.Everything, false);
            Common.DisplayTable(Laba5.TranslationParallel);
        }

        private void feedbackButton_Click(object sender, RibbonControlEventArgs e)
        {
            Laba5.FeedbackTable = Laba5.CombineFeedback(Laba5.AutomataA, Laba5.AutomataB, inp, iter);
            Common.DisplayTable(Laba5.FeedbackTable, Common.DisplayMode.Everything, false);
            Common.DisplayTable(Laba5.TranslationFeedback, "x");
        }

        private void iterationsBox_TextChanged(object sender, RibbonControlEventArgs e)
        {
            if (int.TryParse(iterationsBox.Text, out iter))
            {
                iterationsBox.ShowImage = false;
                iterationsBox.OfficeImageId = "";
                if (iter > -1 && inp > 0)
                    feedbackButton.Enabled = true;
                else feedbackButton.Enabled = false;
            }
            else
            {
                iterationsBox.ShowImage = true;
                iterationsBox.OfficeImageId = "MailDelete";
            }
        }

        private void inputsBox_TextChanged(object sender, RibbonControlEventArgs e)
        {
            if (int.TryParse(inputsBox.Text, out inp))
            {
                inputsBox.ShowImage = false;
                inputsBox.OfficeImageId = "";
                if (iter > -1 && inp > 0)
                    feedbackButton.Enabled = true;
                else feedbackButton.Enabled = false;
            }
            else
            {
                inputsBox.ShowImage = true;
                inputsBox.OfficeImageId = "MailDelete";
            }
        }
        #endregion

        #region Laba 6
        private void enterLab4DataButton_Click(object sender, RibbonControlEventArgs e)
        {
            if (IsLaba4Ready)
            {
                Combine();
            }
            else
            {
                Laba4.NewTable = Common.CreateTable("Введите таблицу полученную в результате лабы 4");
                Combine();
            }
        }

        private void Combine()
        {

            Laba6.Partitions = new Partitions(Laba4.NewTable, Laba6.GetPartitionsCount(Laba4.NewTable.States.Length), IsPiEnteredExternally, ExternalPi);
            Common.DisplayList(Laba6.Partitions.Pi, '\u03c0');
            Common.DisplayList(Laba6.Partitions.Tau, '\u03c4');
            Common.DisplayList(Laba6.Partitions.Eta, '\u03b7');
            statesPartitionsButton.Enabled = true;
            outputsLaba6Button.Enabled = true;
            transPartitionsButton.Enabled = true;
            //----
            encodeButton.Enabled = true;
            triggerFunctionButton.Enabled = true;
            //---
            implicatorsButton.Enabled = true;
        }

        private void outputsLaba6Button_Click(object sender, RibbonControlEventArgs e)
        {
            Common.DisplayTable(Laba6.Partitions.TableG);
        }

        private void statesPartitionsButton_Click(object sender, RibbonControlEventArgs e)
        {
            Common.DisplayTables(Laba6.Partitions.Tables, Common.DisplayMode.OnlyStates);
        }

        private void transPartitionsButton_Click(object sender, RibbonControlEventArgs e)
        {
            Common.DisplayTables(Laba6.Partitions.RelativeTables);
        }
        #endregion
        
        #region Laba 7
        private void encodeButton_Click(object sender, RibbonControlEventArgs e)
        {
            for (int i = 0; i < Laba6.Partitions.Count; i++)
                Common.DisplayTable(Laba6.Partitions.RelativeTables[i], i, true);
            Common.DisplayTable(Laba6.Partitions.TableG, true);
        }

        private void triggerFunctionButton_Click(object sender, RibbonControlEventArgs e)
        {
            //MessageBox.Show(Laba7.Encode("1 1 011", "0"));
            Common.DisplayList(Laba7.TriggerFunctions);
        }
        #endregion

        #region Laba 8
        private void implicatorsButton_Click(object sender, RibbonControlEventArgs e)
        {
            Laba8.SimplifyFunctions();

            for (int i = 0; i < Laba8.ImplicatorsTables.Count; i++)
                Common.DisplayTable(Laba8.ImplicatorsTables[i]);
            calcButton.Enabled = true;
            simplifiedButton.Enabled = true;
        }

        private void calcButton_Click(object sender, RibbonControlEventArgs e)
        {
            for (int i = 0; i < Laba8.IntersectionsTables.Count; i++)
                Common.DisplayTable(Laba8.IntersectionsTables[i]);
        }

        private void simplifiedButton_Click(object sender, RibbonControlEventArgs e)
        {
            Common.DisplayList(Laba8.SimpleTriggersFunctions);
        }
        #endregion
    }
}