// <copyright file="MainForm.cs" company="www.PublicDomain.tech">All rights waived.</copyright>

// Programmed by Victor L. Senior (VLS) <support@publicdomain.tech>, 2016
//
// Web: http://publicdomain.tech
//
// Sources: http://github.com/publicdomaintech/
//
// This software and associated documentation files (the "Software") is
// released under the CC0 Public Domain Dedication, version 1.0, as
// published by Creative Commons. To the extent possible under law, the
// author(s) have dedicated all copyright and related and neighboring
// rights to the Software to the public domain worldwide. The Software is
// distributed WITHOUT ANY WARRANTY.
//
// If you did not receive a copy of the CC0 Public Domain Dedication
// along with the Software, see
// <http://creativecommons.org/publicdomain/zero/1.0/>

/// <summary>
/// Wheel Colors Tracker.
/// </summary>
namespace Wheel_32_Colors_32_Tracker
{
    // Directives
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using PdBets;

    /// <summary>
    /// Main form.
    /// </summary>
    [Export(typeof(IPdBets))]
    public partial class MainForm : Form, IPdBets
    {
        /// <summary>
        /// The history list.
        /// </summary>
        private List<int> historyList = new List<int>();

        /// <summary>
        /// The number appearances dictionary.
        /// </summary>
        private Dictionary<string, int> numberAppearancesDictionary = new Dictionary<string, int>();

        /// <summary>
        /// The number color list.
        /// </summary>
        private List<Color> numberColorList = new List<Color>();

        /// <summary>
        /// The default number color list.
        /// </summary>
        private List<Color> defaultNumberColorList = new List<Color>();

        /// <summary>
        /// The roulette class instance.
        /// </summary>
        private Roulette roulette = new Roulette();

        /// <summary>
        /// The last reset spin.
        /// </summary>
        private int lastResetSpin = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Wheel_32_Colors_32_Tracker.MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            this.InitializeComponent();

            // Set default number color list
            for (int i = 0; i < 37; i++)
            {
                // Set in appearances dictionary
                this.numberAppearancesDictionary.Add(i.ToString(), 0);

                // Add to default number color list
                this.defaultNumberColorList.Add(this.roulette.GetNumberColor(i));

                // TODO Invert label fore color [Use HSV]
                this.wheelChart.Series[0].Points[i].LabelForeColor = Color.FromArgb(this.wheelChart.Series[0].Points[i].Color.ToArgb() ^ 0xffffff);
            }

            // Set number color list
            for (int i = 0; i <= 10; i++)
            {
                // Populate number colors 
                switch (i)
                {
                // Single
                    case 1:

                        // Yellow
                        this.numberColorList.Add(Color.Yellow);

                        // Halt flow
                        break;

                // Double
                    case 2:

                        // Set cyan
                        this.numberColorList.Add(Color.Cyan);

                        // Halt flow
                        break;

                // Triple
                    case 3:

                        // Set light green
                        this.numberColorList.Add(Color.LightGreen);

                        // Halt flow
                        break;

                // The rest
                    default:

                        // Set light pink
                        this.numberColorList.Add(Color.LightPink);

                        // Halt flow
                        break;
                }
            }
        }

        /// <summary>
        /// Processes incoming input and bet strings.
        /// </summary>
        /// <param name="inputString">Input string.</param>
        /// <param name="betString">Bet string.</param>
        /// <returns>>The processed input string.</returns>
        public string Input(string inputString, string betString)
        {
            // Check if must process undo
            if (inputString == "-U")
            {
                /* Process UNDO */

                // Check there's something to remove
                if (this.historyList.Count > 0)
                {
                    // Hold previous number
                    int prevNumber = this.historyList[this.historyList.Count - 1];

                    // Decrement appearances
                    this.numberAppearancesDictionary[prevNumber.ToString()]--;

                    // Remove last from history list
                    this.historyList.RemoveAt(this.historyList.Count - 1);

                    // Colorize numbers
                    this.ColorizeWheelNumbers();
                }
            }
            else
            {
                // Holds last number
                int lastNumber;

                // Try to parse last number
                if (!int.TryParse(inputString, out lastNumber))
                {
                    // Halt flow
                    return betString;
                }

                /* Process SPIN */

                // Validate range
                if (lastNumber >= 0 && lastNumber <= 36)
                {
                    // Rise appearances
                    this.numberAppearancesDictionary[inputString]++;

                    // Add to history list
                    this.historyList.Add(lastNumber);

                    // Colorize wheel numbers
                    this.ColorizeWheelNumbers();
                }
            }

            // Update history count
            this.historyCountToolStripStatusLabel.Text = this.historyList.Count.ToString();
                       
            // Return passed bet string
            return betString;
        }

        /// <summary>
        /// Colorizes the wheel numbers.
        /// </summary>
        private void ColorizeWheelNumbers()
        {
            // Colorize
            for (int n = 0; n < 37; n++)
            {
                // Set number label
                string numberLabel = this.wheelChart.Series[0].Points[n].Label;

                // Update number color
                if (this.numberAppearancesDictionary[numberLabel] > 0)
                {
                    try
                    {
                        // Set number color
                        this.wheelChart.Series[0].Points[n].Color = this.numberColorList[this.numberAppearancesDictionary[numberLabel]];
                    }
                    catch (Exception)
                    {
                        // TODO Use plain blue [Perhaps add color for 10+]
                        this.wheelChart.Series[0].Points[n].Color = Color.Blue;
                    }
                }
               
                // Reset color
                if (this.numberAppearancesDictionary[numberLabel] == 0)
                {
                    // Set default back color
                    this.wheelChart.Series[0].Points[n].Color = this.defaultNumberColorList[Convert.ToInt32(numberLabel)];
                }

                // TODO Invert label fore color [Use HSV]
                this.wheelChart.Series[0].Points[n].LabelForeColor = Color.FromArgb(this.wheelChart.Series[0].Points[n].Color.ToArgb() ^ 0xffffff);
            }

            // Compute reset difference
            int resetDifference = this.historyList.Count - this.lastResetSpin;

            // Update reset button count
            this.resetButton.Text = "Reset" + ((resetDifference < 1) ? string.Empty : " (" + resetDifference.ToString() + ")");
        }

        /// <summary>
        /// Raises the set colors tool strip menu item drop down item clicked event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSetColorsToolStripMenuItemDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // Hold clicked times
            int times;

            // Try to parse x times integer
            if (!int.TryParse(e.ClickedItem.Text.Replace("&", string.Empty).Replace("x", string.Empty), out times))
            {
                // Halt flow
                return;
            }

            // Set current color dialog color
            this.mainColorDialog.Color = this.numberColorList[times];

            // Open color dialog and check dialog result
            if (this.mainColorDialog.ShowDialog() == DialogResult.OK)
            {
                // Set current times color
                this.numberColorList[times] = this.mainColorDialog.Color;
            }
        }

        /// <summary>
        /// Raises the about tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnAboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            // About message
            MessageBox.Show("Programmed by Victor L. Senior (VLS)" + Environment.NewLine + "(www.publicdomain.tech / support@publicdomain.tech)" + Environment.NewLine + Environment.NewLine + "Version 0.1 - September 2016.", "About...");
        }

        /// <summary>
        /// Raises the reset button click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnResetButtonClick(object sender, EventArgs e)
        {
            // Reset number appearances dictionary
            for (int i = 0; i < 37; i++)
            {
                // Set appearances
                this.numberAppearancesDictionary[i.ToString()] = 0;
            }

            // Set last reset spins
            this.lastResetSpin = this.historyList.Count;

            // Colorize number numbers
            this.ColorizeWheelNumbers();
        }
    }
}