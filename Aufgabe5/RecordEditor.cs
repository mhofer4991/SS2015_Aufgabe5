//-----------------------------------------------------------------------
// <copyright file="RecordEditor.cs" company="Markus Hofer">
//     Copyright (c) Markus Hofer
// </copyright>
// <summary>This class draws a screen on the console, where the user can edit existing records.</summary>
//-----------------------------------------------------------------------
namespace Aufgabe5
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class draws a screen on the console, where the user can edit existing records.
    /// </summary>
    public static class RecordEditor
    {
        /// <summary>
        /// Gets the default foreground color for the page data.
        /// </summary>
        public const ConsoleColor DefaultForeGroundColor = ConsoleColor.Gray;

        /// <summary>
        /// Gets the default background color for the page data.
        /// </summary>
        public const ConsoleColor DefaultBackGroundColor = ConsoleColor.Black;

        /// <summary>
        /// Returns a modified record by prompting the user to enter new values.
        /// </summary>
        /// <param name="record">The record, which will be modified.</param>
        /// <returns>A modified record.</returns>
        public static Record GetModifiedRecord(Record record)
        {
            string input = string.Empty;

            DateTime newTimestamp = record.Timestamp;
            int newDuration = record.Duration;

            // The user can keep old values by entering only whitespaces.
            Console.WriteLine("\n > Info: To apply the current values, enter only whitespaces.\n");

            // Prompt the user to enter a new timestamp.
            Console.WriteLine(" > Current timestamp: {0}\n", newTimestamp.ToString());

            do
            {
                Console.Write(" New timestamp (Format: DD.MM.YYYY HH:MM:SS): ");

                if (string.IsNullOrWhiteSpace(input = Console.ReadLine()))
                {
                    newTimestamp = record.Timestamp;
                }
            }
            while (!string.IsNullOrWhiteSpace(input) && !DateTime.TryParse(input, out newTimestamp));

            // Prompt the user to enter a new duration.
            Console.WriteLine("\n > Current duration: {0}\n", newDuration);

            do
            {
                Console.Write(" New duration (in seconds): ");

                if (string.IsNullOrWhiteSpace(input = Console.ReadLine()))
                {
                    newDuration = record.Duration;
                }
            }
            while (!string.IsNullOrWhiteSpace(input) && !int.TryParse(input, out newDuration));

            // Apply the new values.
            record.SetTimestamp(newTimestamp);
            record.SetDuration(newDuration);

            return ModifyPagedata(record);
        }

        /// <summary>
        /// Modifies the page data of a record by prompting the user to enter new values.
        /// </summary>
        /// <param name="record">The record, which will be modified.</param>
        /// <returns>A modified record.</returns>
        private static Record ModifyPagedata(Record record)
        {
            bool exiting = false;
            ConsoleKeyInfo keyInfo;

            int cursorX = 0;
            int cursorY = 0;

            ConsoleColor foreColor = RecordEditor.DefaultForeGroundColor;
            ConsoleColor backColor = RecordEditor.DefaultBackGroundColor;

            while (!exiting)
            {
                Console.Clear();

                Console.Write("[F1] Clear [Up / Down] Navigate    [F2 / F3] Next/Previous foreground color [");

                // Shows the currently selected foreground color.
                Console.ForegroundColor = foreColor;
                Console.Write("##");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("]");

                Console.Write("[F6] Exit  [Right / Left] Navigate [F4 / F5] Next/Previous background color [");

                // Shows the currently selected background color.
                Console.BackgroundColor = backColor;
                Console.Write("  ");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write("]");

                Console.Write("--------------------------------------------------------------------------------");

                record.RenderPageData(3);

                Console.SetCursorPosition(cursorX, cursorY + 3); // The first 3 rows are the menu.

                keyInfo = Console.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.F6:
                        exiting = true;
                        break;
                    case ConsoleKey.F1:
                        Record.ClearPagedata(record.Pagedata);
                        break;
                    case ConsoleKey.F2:
                        foreColor = IncrementColor(foreColor);
                        break;
                    case ConsoleKey.F3:
                        foreColor = DecrementColor(foreColor);
                        break;
                    case ConsoleKey.F4:
                        backColor = IncrementColor(backColor);
                        break;
                    case ConsoleKey.F5:
                        backColor = DecrementColor(backColor);
                        break;
                    case ConsoleKey.LeftArrow:
                        if (cursorX > 0)
                        {
                            cursorX--;
                        }
                        else
                        {
                            cursorX = (Record.DefaultColumns - 1);
                        }

                        break;
                    case ConsoleKey.RightArrow:
                        if (cursorX < (Record.DefaultColumns - 1))
                        {
                            cursorX++;
                        }
                        else
                        {
                            cursorX = 0;
                        }

                        break;
                    case ConsoleKey.UpArrow:
                        if (cursorY > 0)
                        {
                            cursorY--;
                        }
                        else
                        {
                            cursorY = (Record.DefaultRows - 1);
                        }

                        break;
                    case ConsoleKey.DownArrow:
                        if (cursorY < (Record.DefaultRows - 1))
                        {
                            cursorY++;
                        }
                        else
                        {
                            cursorY = 0;
                        }

                        break;
                    default:
                        // Accepting all letters, digits and the spacebar.
                        if (char.IsLetterOrDigit(keyInfo.KeyChar) || keyInfo.KeyChar == 32)
                        {
                            record.ModifyCell(new Cell(cursorX, cursorY, keyInfo.KeyChar, foreColor, backColor));
                        }

                        break;
                }
            }

            return record;
        }

        /// <summary>
        /// Returns the next value from an instance of ConsoleColor.
        /// </summary>
        /// <param name="color">The instance of ConsoleColor.</param>
        /// <returns>The next value from an instance of ConsoleColor.</returns>
        private static ConsoleColor IncrementColor(ConsoleColor color)
        {
            if ((int)color == 15)
            {
                return (ConsoleColor)0;
            }
            else
            {
                return (ConsoleColor)((int)color + 1);
            }
        }

        /// <summary>
        /// Returns the previous value from an instance of ConsoleColor.
        /// </summary>
        /// <param name="color">The instance of ConsoleColor.</param>
        /// <returns>The previous value from an instance of ConsoleColor.</returns>
        private static ConsoleColor DecrementColor(ConsoleColor color)
        {
            if ((int)color == 0)
            {
                return (ConsoleColor)15;
            }
            else
            {
                return (ConsoleColor)((int)color - 1);
            }
        }
    }
}
