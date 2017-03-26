//-----------------------------------------------------------------------
// <copyright file="Cell.cs" company="Markus Hofer">
//     Copyright (c) Markus Hofer
// </copyright>
// <summary>This class represents a single cell of the page data.</summary>
//-----------------------------------------------------------------------
namespace Aufgabe5
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class represents a single cell of the page data.
    /// </summary>
    public class Cell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cell"/> class.
        /// </summary>
        /// <param name="x">X - coordinate of the cell.</param>
        /// <param name="y">Y - coordinate of the cell.</param>
        /// <param name="sign">Sign of the cell.</param>
        /// <param name="foreColor">Fore color of the cell.</param>
        /// <param name="backColor">Back color of the cell.</param>
        public Cell(int x, int y, char sign, ConsoleColor foreColor, ConsoleColor backColor)
        {
            this.X = x;
            this.Y = y;
            this.Sign = sign;
            this.ForeColor = foreColor;
            this.BackColor = backColor;
            this.IsEmpty = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cell"/> class.
        /// </summary>
        /// <param name="x">X - coordinate of the cell.</param>
        /// <param name="y">Y - coordinate of the cell.</param>
        public Cell(int x, int y)
            : this(x, y, ' ', ConsoleColor.Gray, ConsoleColor.Black)
        {
        }

        /// <summary>
        /// Gets the x - coordinate of the cell.
        /// </summary>
        /// <value>The x - coordinate of the cell.</value>
        public int X { get; private set; }

        /// <summary>
        /// Gets the y - coordinate of the cell.
        /// </summary>
        /// <value>The y - coordinate of the cell.</value>
        public int Y { get; private set; }

        /// <summary>
        /// Gets the sign of the cell.
        /// </summary>
        /// <value>The sign of the cell.</value>
        public char Sign { get; private set; }

        /// <summary>
        /// Gets the fore color of the cell.
        /// </summary>
        /// <value>The fore color of the cell.</value>
        public ConsoleColor ForeColor { get; private set; }

        /// <summary>
        /// Gets the back color of the cell.
        /// </summary>
        /// <value>The back color of the cell.</value>
        public ConsoleColor BackColor { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the cell is empty or not.
        /// </summary>
        /// <value>A value indicating whether the cell is empty or not.</value>
        public bool IsEmpty { get; private set; }

        /// <summary>
        /// Parses the given string to a valid cell.
        /// </summary>
        /// <param name="s">The given string.</param>
        /// <returns>A valid cell parsed from a string.</returns>
        public static Cell Parse(string s)
        {
            int x;
            int y;
            int foreground;
            int background;

            string[] splitted = s.Split(',');

            if (!int.TryParse(splitted[0], out x))
            {
                return null;
            }
            else if (!int.TryParse(splitted[1], out y))
            {
                return null;
            }
            else if (!int.TryParse(splitted[2], out foreground))
            {
                return null;
            }
            else if (!int.TryParse(splitted[3], out background))
            {
                return null;
            }
            else if (foreground < 0 || foreground > 15)
            {
                return null;
            }
            else if (background < 0 || background > 15)
            {
                return null;
            }
            else
            {
                Cell c = new Cell(x, y, splitted[4][0], (ConsoleColor)foreground, (ConsoleColor)background);
                c.IsEmpty = false;

                return c;
            }
        }

        /// <summary>
        /// Changes the sign to the given sign.
        /// </summary>
        /// <param name="sign">The given sign.</param>
        public void SetSign(char sign)
        {
            this.Sign = sign;
            this.IsEmpty = false;
        }

        /// <summary>
        /// Changes the fore color to the given fore color.
        /// </summary>
        /// <param name="foreColor">The given fore color.</param>
        public void SetForeColor(ConsoleColor foreColor)
        {
            this.ForeColor = foreColor;
            this.IsEmpty = false;
        }

        /// <summary>
        /// Changes the back color to the given back color.
        /// </summary>
        /// <param name="backColor">The given back color.</param>
        public void SetBackColor(ConsoleColor backColor)
        {
            this.BackColor = backColor;
            this.IsEmpty = false;
        }

        /// <summary>
        /// Exports the cell to a string.
        /// </summary>
        /// <returns>A string which contains the cell.</returns>
        public string Export()
        {
            string export = string.Empty;

            export += this.X.ToString() + ",";
            export += this.Y.ToString() + ",";
            export += ((int)this.ForeColor).ToString() + ",";
            export += ((int)this.BackColor).ToString() + ",";
            export += this.Sign;

            return export;
        }
    }
}
