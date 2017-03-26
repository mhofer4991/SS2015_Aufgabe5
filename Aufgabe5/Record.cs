//-----------------------------------------------------------------------
// <copyright file="Record.cs" company="Markus Hofer">
//     Copyright (c) Markus Hofer
// </copyright>
// <summary>This class represents a record, which can be exported and imported and transferred to a server.</summary>
//-----------------------------------------------------------------------
namespace Aufgabe5
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class represents a record, which can be exported and imported and transferred to a server.
    /// </summary>
    public class Record
    {
        /// <summary>
        /// Gets the default amount of columns.
        /// </summary>
        public const int DefaultColumns = 80;

        /// <summary>
        /// Gets the default amount of rows.
        /// </summary>
        public const int DefaultRows = 25;

        /// <summary>
        /// Gets the default duration of rows.
        /// </summary>
        public const int DefaultDuration = 60;

        /// <summary>
        /// Initializes a new instance of the <see cref="Record"/> class.
        /// </summary>
        /// <param name="id">ID of the record.</param>
        /// <param name="timestamp">Timestamp of the record.</param>
        /// <param name="duration">Duration of the record.</param>
        /// <param name="pagedata">Page data of the record.</param>
        public Record(int id, DateTime timestamp, int duration, Cell[,] pagedata)
        {
            this.ID = id;
            this.Timestamp = timestamp;
            this.Duration = duration;
            this.Pagedata = pagedata;
        }

        /// <summary>
        /// Gets the ID of the record.
        /// </summary>
        /// <value>The ID of the record.</value>
        public int ID { get; private set; }

        /// <summary>
        /// Gets the timestamp of the record.
        /// </summary>
        /// <value>The timestamp of the record.</value>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Gets the duration of the record.
        /// </summary>
        /// <value>The duration of the record.</value>
        public int Duration { get; private set; }

        /// <summary>
        /// Gets the page data of the record.
        /// </summary>
        /// <value>The page data of the record.</value>
        public Cell[,] Pagedata { get; private set; }

        /// <summary>
        /// Gets an empty record with the given id.
        /// </summary>
        /// <param name="id">The given ID.</param>
        /// <returns>An empty record with the given ID.</returns>
        public static Record GetEmptyRecord(int id)
        {
            Cell[,] pagedata = new Cell[Record.DefaultColumns, Record.DefaultRows];

            Record.ClearPagedata(pagedata);

            return new Record(id, DateTime.Now, Record.DefaultDuration, pagedata);
        }

        /// <summary>
        /// Clears the page data by resetting all containing cells.
        /// </summary>
        /// <param name="pagedata">The page data, which will be cleared.</param>
        public static void ClearPagedata(Cell[,] pagedata)
        {
            for (int i = 0; i < pagedata.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < pagedata.GetUpperBound(1) + 1; j++)
                {
                    pagedata[i, j] = new Cell(i, j);
                }
            }
        }

        /// <summary>
        /// Parses the given string to a valid record.
        /// </summary>
        /// <param name="s">The given string.</param>
        /// <returns>A valid record parsed from a string.</returns>
        public static Record Parse(string s)
        {
            int id;
            DateTime timestamp;
            int duration;
            Cell[,] pagedata = new Cell[Record.DefaultColumns, Record.DefaultRows];

            string[] splitted = s.Split(';');

            if (!int.TryParse(splitted[0], out id))
            {
                return null;
            }
            else if (!int.TryParse(splitted[2], out duration))
            {
                return null;
            }
            else if (!DateTime.TryParse(splitted[1], out timestamp))
            {
                return null;
            }
            else
            {
                Record r = Record.GetEmptyRecord(id);
                string[] splittedCells = splitted[3].Split('#');

                r.SetDuration(duration);
                r.SetTimestamp(timestamp);

                for (int i = 0; i < splittedCells.Length; i++)
                {
                    Cell c = Cell.Parse(splittedCells[i]);

                    if (c != null)
                    {
                        r.Pagedata[c.X, c.Y] = c;
                    }
                }

                return r;
            }
        }

        /// <summary>
        /// Changes the timestamp to the given timestamp.
        /// </summary>
        /// <param name="timestamp">The given timestamp.</param>
        public void SetTimestamp(DateTime timestamp)
        {
            this.Timestamp = timestamp;
        }

        /// <summary>
        /// Changes the duration to the given duration.
        /// </summary>
        /// <param name="duration">The given duration.</param>
        public void SetDuration(int duration)
        {
            this.Duration = duration;
        }

        /// <summary>
        /// Draws the page data on the console.
        /// </summary>
        /// <param name="offsetY">Offset of the Y - coordinate.</param>
        public void RenderPageData(int offsetY)
        {
            ConsoleColor tempForeColor = Console.ForegroundColor;
            ConsoleColor tempBackColor = Console.BackgroundColor;

            for (int i = 0; i < this.Pagedata.GetUpperBound(1) + 1; i++)
            {
                for (int j = 0; j < this.Pagedata.GetUpperBound(0) + 1; j++)
                {
                    Console.ForegroundColor = this.Pagedata[j, i].ForeColor;
                    Console.BackgroundColor = this.Pagedata[j, i].BackColor;

                    // Check if it's useful to set the cursor and draw the sign.
                    // It would make no sense to draw an empty sign (' ') with the same backgroundcolor.
                    if (!(this.Pagedata[j, i].Sign == ' ' && this.Pagedata[j, i].BackColor == tempBackColor))
                    {
                        Console.SetCursorPosition(j, i + offsetY);
                        Console.Write(this.Pagedata[j, i].Sign);
                    }
                }
            }

            Console.ForegroundColor = tempForeColor;
            Console.BackgroundColor = tempBackColor;
        }

        /// <summary>
        /// Exports the record to a string.
        /// </summary>
        /// <returns>A string which contains the record.</returns>
        public string Export()
        {
            string export = string.Empty;

            export += this.ID.ToString() + ";";
            export += this.Timestamp.ToString() + ";";
            export += this.Duration.ToString() + ";";

            for (int i = 0; i < this.Pagedata.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < this.Pagedata.GetUpperBound(1) + 1; j++)
                {
                    if (!this.Pagedata[i, j].IsEmpty)
                    {
                        export += this.Pagedata[i, j].Export() + "#";
                    }
                }
            }

            // It can be possible that all cells are empty, so you have to check if the exports ends with a '#'.
            if (export.EndsWith("#"))
            {
                export = export.Substring(0, export.Length - 1);
            }
            
            return export;
        }

        /// <summary>
        /// Replaces a cell by a given new cell. It will be identified by X and Y coordinates.
        /// </summary>
        /// <param name="c">The given new cell.</param>
        public void ModifyCell(Cell c)
        {
            this.Pagedata[c.X, c.Y].SetSign(c.Sign);
            this.Pagedata[c.X, c.Y].SetForeColor(c.ForeColor);
            this.Pagedata[c.X, c.Y].SetBackColor(c.BackColor);
        }

        /// <summary>
        /// Checks if a record from the given list has the same timestamp.
        /// </summary>
        /// <param name="records">The given list of records.</param>
        /// <returns>A boolean indicating whether a record from the given list has the same timestamp or not.</returns>
        public bool DuplicateTimestampInList(List<Record> records)
        {
            foreach (Record r in records)
            {
                if (r.Timestamp.Ticks == this.Timestamp.Ticks)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a boolean indicating whether the record is equal with the given object.
        /// </summary>
        /// <param name="obj">The given object.</param>
        /// <returns>A boolean indicating whether the record is equal with the given object.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Record)
            {
                return ((Record)obj).ID == this.ID;
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
